namespace Tuckshop.IdentityServer.Initializers
{
  using System;
  using System.Linq;
  using System.Threading;
  using System.Threading.Tasks;
  using Extensions.Hosting.AsyncInitialization;
  using Microsoft.AspNetCore.Hosting;
  using Microsoft.AspNetCore.Identity;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.Extensions.Configuration;
  using Microsoft.Extensions.Hosting;
  using Microsoft.Extensions.Logging;
  using Neo.Cryptography;
  using Neo.Extensions;
  using Neo.Identity;
  using Neo.IdentityServer.Models.IdentityProviders;
  using Neo.Model.Identity.SystemUser;
  using Tuckshop.IdentityServer.Models;

  /// <summary>
  /// Will initialize the Identity Server db.
  /// </summary>
  public class SeedDataAsyncInitializer : IAsyncInitializer
  {
    /// <summary>
    /// Test User Alice's Id.
    /// </summary>
    public const string AliceId = "aaa7e131-5496-4c25-ae5f-2306e1160001";

    /// <summary>
    /// Test User Bob's Id.
    /// </summary>
    public const string BobId = "aaa7e131-5496-4c25-ae5f-2306e1160002";

    /// <summary>
    /// Super User's Id.
    /// </summary>
    public const string SuperUserId = "aaa7e131-5496-4c25-ae5f-2306e1160100";

    private readonly IConfiguration configuration;
    private readonly ILogger<SeedDataAsyncInitializer> logger;
    private readonly IdentityDbContext identityDbContext;
    private readonly IWebHostEnvironment environment;
    private readonly UserManager<TuckshopApplicationUser> userManager;
    private readonly IColumnEncryptionConfigurationService encryptionConfigurationService;
    private readonly IColumnEncryptionService columnEncryptionService;
    private readonly IOverridableUserResolver<TuckshopApplicationUser> userResolver;
    private readonly ISystemUserService<TuckshopApplicationUser> systemUserService;
    private readonly SystemUserInitializer<IdentityDbContext, TuckshopApplicationUser> systemUserInitializer;

    /// <summary>
    /// Initializes a new instance of the <see cref="SeedDataAsyncInitializer"/> class.
    /// </summary>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="identityDbContext">The identity db context.</param>
    /// <param name="environment">The web host environment.</param>
    /// <param name="userManager">The user manager.</param>
    /// <param name="columnEncryptionService">The column encryption service.</param>
    /// <param name="encryptionConfigurationService">The column encryption configuration service.</param>
    /// <param name="userResolver">The user resolver.</param>
    /// <param name="systemUserService">System user service.</param>
    /// <param name="systemUserInitializer">The system user initializer.</param>
    public SeedDataAsyncInitializer(
      IConfiguration configuration,
      ILogger<SeedDataAsyncInitializer> logger,
      IdentityDbContext identityDbContext,
      IWebHostEnvironment environment,
      UserManager<TuckshopApplicationUser> userManager,
      IColumnEncryptionConfigurationService encryptionConfigurationService,
      IColumnEncryptionService columnEncryptionService,
      IOverridableUserResolver<TuckshopApplicationUser> userResolver,
      ISystemUserService<TuckshopApplicationUser> systemUserService,
      SystemUserInitializer<IdentityDbContext, TuckshopApplicationUser> systemUserInitializer)
    {
      this.configuration = configuration;
      this.logger = logger;
      this.identityDbContext = identityDbContext;
      this.environment = environment;
      this.userManager = userManager;
      this.encryptionConfigurationService = encryptionConfigurationService;
      this.columnEncryptionService = columnEncryptionService;
      this.userResolver = userResolver;
      this.systemUserService = systemUserService;
      this.systemUserInitializer = systemUserInitializer;
    }

    /// <inheritdoc/>
    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
      var identityProvider = await this.SeedIdentityProvidersAsync(cancellationToken);
      await this.systemUserInitializer.InitializeAsync(cancellationToken);

      await this.systemUserService.RunWithSystemUserAsync(async () =>
      {
        await this.SeedUsersAsync(identityProvider);

        if (this.configuration.GetBoolean("AlwaysEncrypted:Enabled", false) ?? false)
        {
          await this.ConfigureDatabaseEncryptionAsync();
        }
      });
    }

    private async Task<IdentityProvider> SeedIdentityProvidersAsync(CancellationToken cancellationToken)
    {
      IdentityProvider identityProvider;

      if (!this.identityDbContext.IdentityProviders.Any())
      {
        identityProvider = IdentityProvider.LoginCredentials(setId: true);

        this.identityDbContext.IdentityProviders.Add(identityProvider);

        this.identityDbContext.SuppressAuditTrailProcessor = true;
        await this.identityDbContext.SaveChangesWithIdentityInsertAsync<IdentityProvider>();
        this.identityDbContext.SuppressAuditTrailProcessor = false;
      }
      else
      {
        identityProvider = await this.identityDbContext.IdentityProviders.FirstAsync(identityProvider => identityProvider.IdentityProviderType == (int)IdentityProviderType.LoginCredentials, cancellationToken);
      }

      return identityProvider;
    }

    private async Task SeedUsersAsync(IdentityProvider identityProvider)
    {
      if (!this.identityDbContext.Users.Any(neoTemplateApplicationUser => (new[] { AliceId, BobId, SuperUserId }).Contains(neoTemplateApplicationUser.Id)))
      {
        var seedUserOptions = this.configuration.GetOptions<SeedUserOptions>();

        if (this.environment.IsDevelopment() || this.environment.IsStaging() || this.environment.IsDevStaging())
        {
          // Dev and Test
          await this.CreateSeedUserAsync(AliceId, "Alice", "Test User", "alice@test.com", seedUserOptions.TestUserPassword, identityProvider);
          await this.CreateSeedUserAsync(BobId, "Bob", "Test User", "bob@test.com", seedUserOptions.TestUserPassword, identityProvider);
        }
        else
        {
          // Production
          var email = "superu@Tuckshop.com".ToLowerInvariant();
          await this.CreateSeedUserAsync(SuperUserId, "Super", "User", email, seedUserOptions.SuperUserPassword, identityProvider);
        }
      }
    }

    private async Task CreateSeedUserAsync(string id, string firstName, string lastName, string email, string password, IdentityProvider identityProvider)
    {
      var user = new TuckshopApplicationUser()
      {
        Id = id,
        FirstName = firstName,
        LastName = lastName,
        UserName = email,
        Email = email,
        EmailConfirmed = true,
        IsActive = true,
        IdentityProviderId = identityProvider.IdentityProviderId,
        UserInvite = new Models.UserManagement.UserInvite()
        {
          EmailAddress = email,
          CreatedOn = DateTime.UtcNow,
        },
      };

      var result = await this.userManager.CreateAsync(user, password);

      if (!result.Succeeded)
      {
        throw new InvalidOperationException($"Error creating seed user {email}: {string.Join("\n", result.Errors.Select(identityError => identityError.Description))}");
      }
    }

    private async Task ConfigureDatabaseEncryptionAsync()
    {
      // Initialise the database keys (CMK and CEK)
      await this.encryptionConfigurationService.ConfigureDatabaseAsync(this.identityDbContext, "Main");

      // Encrypt the Database Columns
      this.logger.LogInformation("Encrypting database columns");

      await this.columnEncryptionService.EncryptColumnsAsync<TuckshopApplicationUser>(
        this.identityDbContext,
        neoTemplateApplicationUser => neoTemplateApplicationUser.Id,
        columnEncryptionBuilder =>
        {
          columnEncryptionBuilder.EncryptAllDecoratedColumns();
        });
    }
  }
}
