namespace Tuckshop.IdentityServer.App.Services
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using Microsoft.AspNetCore.Identity;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.Extensions.Hosting;
  using Neo.Model.DomainEvents;
  using Tuckshop.IdentityServer.App.Services.Events;
  using Tuckshop.IdentityServer.Contracts.Registration;
  using Tuckshop.IdentityServer.Models;

  /// <summary>
  /// The registration service.
  /// </summary>
  public class RegistrationService : IRegistrationService
  {
    // Pre verifiedParticipantIds Alice - Add emails in here that do not need to proceed with email verification/
    private static readonly HashSet<string> stagingPreVerifiedEmailAddresses = new HashSet<string>();
    private readonly IHostEnvironment environment;
    private readonly IDomainEventDispatcher domainEventDispatcher;
    private readonly IMfaManager mfaManager;
    private readonly UserManager<TuckshopApplicationUser> userManager;
    private readonly ILogger<RegistrationService> logger;
    private readonly IRegistrationEmailService registrationEmailService;
    private readonly IdentityDbContext dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="RegistrationService"/> class.
    /// </summary>
    /// <param name="environment">The environment.</param>
    /// <param name="domainEventDispatcher">The domainEventDispatcher.</param>
    /// <param name="mfaManager">The mfaManager.</param>
    /// <param name="userManager">The userManager.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="registrationEmailService">The registration email sender.</param>
    /// <param name="dbContext">Db context.</param>
    public RegistrationService(
      IHostEnvironment environment,
      IDomainEventDispatcher domainEventDispatcher,
      IMfaManager mfaManager,
      UserManager<TuckshopApplicationUser> userManager,
      ILogger<RegistrationService> logger,
      IRegistrationEmailService registrationEmailService,
      IdentityDbContext dbContext)
    {
      this.environment = environment;
      this.domainEventDispatcher = domainEventDispatcher;
      this.mfaManager = mfaManager;
      this.userManager = userManager;
      this.logger = logger;
      this.registrationEmailService = registrationEmailService;
      this.dbContext = dbContext;
    }

    /// <inheritdoc/>
    public async Task<RegisterResult> RegisterUserAsync(IRegistrationUser user, int identityProviderId)
    {
      if ((this.environment.IsDevelopment() || this.environment.IsStaging()) && stagingPreVerifiedEmailAddresses.Any(email => user.Email.Equals(email, StringComparison.OrdinalIgnoreCase)))
      {
        // in staging we will automatically verify the preVerifiedEmailAddresses
        user.EmailConfirmed = true;
      }

      // get the identity provider for this type.
      var newUser = new TuckshopApplicationUser()
      {
        UserName = user.Email,
        Email = user.Email,
        FirstName = user.FirstName,
        LastName = user.LastName,
        IsActive = true,
        IdentityProviderId = identityProviderId,
        EmailConfirmed = user.EmailConfirmed,
        TwoFactorEnabled = this.mfaManager.NewUserRequiresTwoFactor(user, identityProviderId),
      };

      var userInvite = await this.dbContext.UserInvites.FirstOrDefaultAsync(userInvite => userInvite.EmailAddress == user.Email.Trim());
      if (userInvite != null)
      {
        newUser.UserInviteId = userInvite.UserInviteId;
      }
      else
      {
        /* TODO:
         * We recommended that you only allow known users to register.
         * If you can match a user against a record in your domain service (e.g. employee email address), then add a call here to the domain service.
         * Otherwise, if you want to allow any user to register, remove this else block.
        */
        return RegisterResult.FailResult("Could not find your details, please contact the administrator.");
      }

      var result = await this.userManager.CreateAsync(newUser, user.Password);

      RegisterResult registerResult;
      if (result.Succeeded)
      {
        this.logger.LogDebug("User created a new account with password.");

        var newUserLookup = newUser.ToLookup();

        if (!newUser.EmailConfirmed)
        {
          this.logger.LogDebug("User email not verified, sending verification email.");
          await this.registrationEmailService.SendVerificationEmailAsync(newUser);

          registerResult = RegisterResult.PendingEmailVerification(newUserLookup);
        }
        else
        {
          if (newUser.TwoFactorEnabled)
          {
            if (newUser.TwoFactorConfigured)
            {
              registerResult = RegisterResult.SuccessConfigureMFA(newUserLookup);
            }
            else
            {
              registerResult = RegisterResult.SuccessRequiresMFA(newUserLookup);
            }
          }
          else
          {
            registerResult = RegisterResult.SuccessNoMFA(newUserLookup);
          }
        }

        await this.domainEventDispatcher.DispatchAsync(new UserRegisteredEvent(newUser.Id, user, registerResult.Type));
      }
      else
      {
        registerResult = RegisterResult.FailResult(string.Join(", ", result.Errors.Select(identityError => identityError.Description)));
      }

      return registerResult;
    }
  }
}
