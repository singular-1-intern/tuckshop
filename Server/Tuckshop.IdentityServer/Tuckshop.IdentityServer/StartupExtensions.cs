namespace Tuckshop.IdentityServer
{
  using System;
  using Azure.Identity;
  using Microsoft.AspNetCore.Builder;
  using Microsoft.AspNetCore.DataProtection;
  using Microsoft.AspNetCore.Hosting;
  using Microsoft.AspNetCore.Http;
  using Microsoft.AspNetCore.Identity;
  using Microsoft.AspNetCore.Mvc.Infrastructure;
  using Microsoft.AspNetCore.Mvc.Routing;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.Extensions.Configuration;
  using Microsoft.Extensions.DependencyInjection;
  using Microsoft.Extensions.DependencyInjection.Extensions;
  using Microsoft.Extensions.Hosting;
  using Neo.Extensions;
  using Neo.Extensions.DependencyInjection;
  using Neo.IdentityServer.App.OpenIddict;
  using Neo.IdentityServer.App.Services;
  using Neo.IdentityServer.App.Services.IdentityProviders;
  using Neo.IdentityServer.Providers;
  using Neo.Model.AuditTrail;
  using Neo.Model.Identity;
  using Neo.Model.Mappers;
  using Neo.Model.Metadata;
  using Neo.Model.Processing;
  using Neo.Model.Serilog.Enrichers;
  using Neo.SecretVault;
  using Tuckshop.IdentityServer.App.Services;
  using Tuckshop.IdentityServer.App.Services.Events;
  using Tuckshop.IdentityServer.App.Services.UserManagement;
  using Tuckshop.IdentityServer.Filters;
  using Tuckshop.IdentityServer.Initializers;
  using Tuckshop.IdentityServer.Models;
  using Tuckshop.IdentityServer.Models.Migrations.DesignTime;
  using Tuckshop.IdentityServer.Models.Migrations.Initializers;
  using Tuckshop.IdentityServer.Models.Options;
  using Serilog;
  using Serilog.Filters;

  /// <summary>
  /// The Identity Server Startup Extensions class.
  /// </summary>
  public static class StartupExtensions
  {
    /// <summary>
    /// The secret vaults provider. This has to be created explicitly because it is needed during startup (I.e. before the DI container is available).
    /// It is declared here so that all the startup methods in this class can access it if required.
    /// </summary>
    private static readonly ISecretVaultServiceProvider SecretVaultsProvider = new SecretVaultServiceProvider();

    /// <summary>
    /// Adds Identity Secret Vault services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="environment">The web host environment.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddIdentitySecretVaults(this IServiceCollection services, IWebHostEnvironment environment, IConfiguration configuration)
    {
      services.AddSingleton(SecretVaultsProvider);
      services.AddNeoKeyVaults(configuration, SecretVaultsProvider);
      return services;
    }

    /// <summary>
    /// Adds Always Encrypted services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="environment">The web host environment.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddAlwaysEncrypted(this IServiceCollection services, IWebHostEnvironment environment, IConfiguration configuration)
    {
      return services.AddNeoSqlAlwaysEncryptedWithKeyVaultAsKeyStoreProvider(configuration, "Main");
    }

    /// <summary>
    /// Adds Identity API authentication service providers.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="environment">The web host environment.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddIdentityAuthenticationProviders(this IServiceCollection services, IWebHostEnvironment environment, IConfiguration configuration)
    {
      services.AddOpenIddictInternalAuthenticationClient(configuration, environment);

      services.AddHttpClient<OpenIDVerificationClient>();

      return services;
    }

    /// <summary>
    /// Configures Identity API cookie policy.
    /// </summary>
    /// <param name="services">The services collection.</param>
    /// <param name="environment">The web host environment.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The services collection.</returns>
    public static IServiceCollection AddIdentityCookiePolicy(this IServiceCollection services, IWebHostEnvironment environment, IConfiguration configuration)
    {
      // ToDo: Confirm if we need this on all services or not
      services.Configure<CookiePolicyOptions>(options =>
      {
        // This lambda determines whether user consent for non-essential cookies is needed for a given request.
        options.CheckConsentNeeded = context => true;
        options.MinimumSameSitePolicy = SameSiteMode.Lax;
        options.Secure = CookieSecurePolicy.Always;
      });

      return services;
    }

    /// <summary>
    /// Adds Identity API authentication service providers.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="environment">The web host environment.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddIdentityUserDataServices(this IServiceCollection services, IWebHostEnvironment environment, IConfiguration configuration)
    {
      services.AddScoped<IDbContextProcessor, AuditTrailProcessor<TuckshopApplicationUser>>();
      services.AddScoped<IAuditTrailProcessor>(sp => sp.GetRequiredService<AuditTrailProcessor<TuckshopApplicationUser>>());

      services.AddScoped<IUsersDbContext<TuckshopApplicationUser>>(sp => sp.GetRequiredService<IdentityDbContext>());

      services.AddNeoClientUserResolver<TuckshopApplicationUser, TuckshopApplicationUserClaimMapper>();

      services.AddScoped<Neo.Identity.IUserStore<TuckshopApplicationUser>, UserStore>();

      if (!environment.IsDevelopment() && !environment.IsDevStaging())
      {
        var dataProtectionOptions = configuration.GetOptions<CustomDataProtectionOptions>();

        services.AddDataProtection()
            .PersistKeysToDbContext<IdentityDbContext>()
            .ProtectKeysWithAzureKeyVault(new Uri(dataProtectionOptions.KeyVaultKeyId), new DefaultAzureCredential());
      }

      return services;
    }

    /// <summary>
    /// Adds Identity API data services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="environment">The web host environment.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddIdentityDataServices(this IServiceCollection services, IWebHostEnvironment environment, IConfiguration configuration)
    {
      services.AddHttpContextAccessor();

      services.AddSingleton<IMetadataService, MetadataService>();

      return services;
    }

    /// <summary>
    /// Adds Identity API model services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="environment">The web host environment.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddIdentityModelServices(this IServiceCollection services, IWebHostEnvironment environment, IConfiguration configuration)
    {
      services.TryAddSingleton<INeoMapper, NeoMapper>();

      services.AddNeoModelUtilityServices();

      services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
      services.AddScoped(serviceProvider =>
      {
        var actionContext = serviceProvider.GetRequiredService<IActionContextAccessor>().ActionContext;
        var factory = serviceProvider.GetRequiredService<IUrlHelperFactory>();
        return factory.GetUrlHelper(actionContext ?? throw new ArgumentException("ActionContext cannot be null."));
      });

      // User Management
      services.AddScoped<IUserManagementService, UserManagementService>()
        .AddScoped<IRegistrationEmailService, RegistrationEmailService>();

      return services;
    }

    /// <summary>
    /// Adds Identity services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="environment">The web host environment.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddIdentityServices(this IServiceCollection services, IWebHostEnvironment environment, IConfiguration configuration)
    {
      services.AddTransient<IDataProtectionOptionsProvider, DataProtectionOptionsProvider>();

      services.AddIdentity<TuckshopApplicationUser, IdentityRole>(options =>
          {
            options.SignIn.RequireConfirmedEmail = true;
          })
         .AddEntityFrameworkStores<IdentityDbContext>()
         .AddDefaultTokenProviders()
         .AddDefaultUI()
         .AddSignInManager<SignInManager>()
         .AddClaimsPrincipalFactory<UserClaimsPrincipalFactory>();

      services
        .AddIdentityEmailSender(configuration)
        .AddScoped<IUrlProvider, UrlProvider>()
        .AddScoped<IMfaManager, MfaManager>();

      services.AddNeoMultiTenancyCustomTenantService();

      services.AddIdentityProviderSupport<IdentityDbContext, TuckshopApplicationUser>();

      services.AddMediatrDomainEventHandling(typeof(UserRegisteredEvent).Assembly);

      services.AddScoped<ISignInAuditService, SignInAuditService<IdentityDbContext>>();
      services.AddScoped<IRegistrationService, RegistrationService>();

      var passwordOptions = configuration.GetOptions<Options.PasswordOptions>();

      services.Configure<IdentityOptions>(options =>
      {
        // Default Password settings.
        options.Password.RequiredLength = passwordOptions.RequiredLength;
        options.Password.RequireLowercase = passwordOptions.RequireLowercase;
        options.Password.RequireUppercase = passwordOptions.RequireUppercase;
        options.Password.RequireNonAlphanumeric = passwordOptions.RequireNonAlphanumeric;
        options.Password.RequireDigit = passwordOptions.RequireDigit;
        options.Password.RequiredUniqueChars = passwordOptions.RequiredUniqueChars;
      });

      return services;
    }

    /// <summary>
    /// Adds Identity integration services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="environment">The web host environment.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddIdentityIntegrationServices(this IServiceCollection services, IWebHostEnvironment environment, IConfiguration configuration)
    {
      services.AddTemplatedNotificationsClient(configuration, notificationClientOptions =>
      {
        notificationClientOptions.RegisterMessagePublisher = false;
        notificationClientOptions.TemplateHolderTypes = new[] { typeof(App.Notifications.TemplateTypes) };
      });

      return services;
    }

    /// <summary>
    /// Adds Identity Server services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="environment">The web host environment.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddIdentityServerServices(this IServiceCollection services, IWebHostEnvironment environment, IConfiguration configuration)
    {
      var serverOptions = new NeoOpenIddictServerSetupOptions()
        .WithCustomTokenRequestValidator<TokenRequestValidator>()
        .WithServerConfiguration(options => options.AllowImplicitFlow())
        .WithDbContextConfiguration(dbOptions =>
        {
          dbOptions.UseSqlServer(
            configuration.GetConnectionString("Main"),
            builder => builder.MigrationsAssembly(typeof(IdentityDbContextDesignTimeDbContextFactory).Assembly.GetName().Name));
        });

      // Add OpenIddict
      services.AddNeoOpenIddictServices<IdentityDbContext, ProfileService>(
        configuration,
        environment,
        SecretVaultsProvider,
        secretVaultKey: "Main",
        serverOptions);

      // If in dev, show PII to ease debugging.
      if (environment.IsDevelopment())
      {
        Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;
      }

      return services;
    }

    /// <summary>
    /// Adds Identity API MVC services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="environment">The web host environment.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddIdentityMvcServices(this IServiceCollection services, IWebHostEnvironment environment, IConfiguration configuration)
    {
      NeoModelStartupOptions neoOptions = new NeoModelStartupOptions
      {
        AddFluentValidation = false,
      };

      IMvcBuilder builder = services.AddControllersWithViews()
        .AddMvcOptions(options =>
        {
          // add the exception filter
          options.Filters.Add(typeof(UnauthorizedAccessExceptionFilter));
        })
        .AddMvcNeoOptions(neoOptions)
        .AddNeoAuthorisationOptions();

      if (environment.IsDevelopment())
      {
        builder.AddRazorRuntimeCompilation();
      }

      return services;
    }

    /// <summary>
    /// Adds Identity API Async Initialisers.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="environment">The web host environment.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddIdentityAsyncInitialisers(this IServiceCollection services, IWebHostEnvironment environment, IConfiguration configuration)
    {
      services.AddAsyncInitialization();
      services.AddAsyncInitializer<DatabaseAsyncInitializer>();

      services.AddSystemUser<IdentityDbContext, TuckshopApplicationUser>(
        new Neo.Model.Identity.SystemUser.SystemUserOptions<TuckshopApplicationUser>()
        {
          SuppressInitializer = true,
          FindUserByIdPredicate = (Guid id) => appUser => appUser.Id == id.ToString(),
          CreateSystemUserHandler = (Guid id) => TuckshopApplicationUser.SystemUser(id),
        });

      services.AddAsyncInitializer<SeedDataAsyncInitializer>();

      services.AddIdentityConfigSyncServices(
        new Neo.IdentityServer.App.OpenIddict.Services.ConfigSyncServiceOptions() { ApiResourceName = "Tuckshop.Domain" });

      services.AddAsyncInitializer<IdentityProvidersAsyncInitializer>();

      return services;
    }

    /// <summary>
    /// Configures serilog from the app config.
    /// </summary>    
    /// <param name="services">services.</param>
    /// <param name="environment">environment.</param>
    /// <param name="configuration">configuration.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddIdentityLogging(
      this IServiceCollection services,
      IWebHostEnvironment environment,
      IConfiguration configuration)
    {
      LoggerConfiguration loggerConfig = new LoggerConfiguration()
          // serilog-aspnetcore picks up and logs exceptions, so we filter out logs from the standard middleware to prevent duplicate logs.
          .Filter.ByExcluding(Matching.FromSource<Microsoft.AspNetCore.Diagnostics.ExceptionHandlerMiddleware>())
          // Filters out 'info' logs from Microsoft.AspNetCore when the request path is a health check URL
          .Filter.ByExcluding(Matching.WithProperty<string>(
            propertyName: "RequestPath",
            requestPath => requestPath.EndsWith("/health/live", StringComparison.InvariantCultureIgnoreCase) || requestPath.EndsWith("/health/ready", StringComparison.InvariantCultureIgnoreCase)))
          .Enrich.FromLogContext()
          .ReadFrom.Configuration(configuration);

      var trimmingOptions = configuration.GetSection("Logging:ExceptionTrimming").Get<ExceptionTrimmingOptions>();
      if (trimmingOptions != null)
      {
        loggerConfig.Enrich.With(new ExceptionTrimmingEnricher(trimmingOptions));
      }

      Log.Logger = loggerConfig.CreateLogger();

      services.AddLogCleanupForSqlServer<IdentityDbContext>(configuration);

      return services;
    }
  }
}
