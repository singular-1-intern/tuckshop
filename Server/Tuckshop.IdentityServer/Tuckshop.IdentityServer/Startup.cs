namespace Tuckshop.IdentityServer
{
  using Microsoft.AspNetCore.Builder;
  using Microsoft.AspNetCore.Hosting;
  using Microsoft.Extensions.Configuration;
  using Microsoft.Extensions.DependencyInjection;
  using Microsoft.Extensions.Hosting;
  using Neo.App;
  using Neo.Builders;
  using Neo.Extensions;
  using Neo.Extensions.DependencyInjection;
  using Neo.IdentityServer.Models;
  using Tuckshop.IdentityServer.Models;
  using Tuckshop.IdentityServer.Models.Security;

  /// <summary>
  /// Represents the Startup.
  /// </summary>
  public class Startup : NeoApiStartupBase
  {
    /// <summary>
    /// The config key for the primary db connection string.
    /// </summary>
    public const string MainConnectionStringKey = "Main";

    /// <summary>
    /// Initializes a new instance of the <see cref="Startup"/> class.
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    /// <param name="environment">The web host environment.</param>
    public Startup(IConfiguration configuration, IWebHostEnvironment environment)
      : base(configuration, environment)
    {
    }

    /// <summary>
    /// Configure the services.
    /// This method gets called by the runtime. Use this method to add services to the container.
    /// </summary>
    /// <param name="serviceCollection">The services collection.</param>
    public void ConfigureServices(IServiceCollection serviceCollection)
    {
      var startupOptions = new NeoModelStartupOptions()
      {
        FluentValidatorAssemblies = [typeof(IdentityDbContext).Assembly]
      };
      serviceCollection
        .AddNeoWebApiBase(this.Environment, this.Configuration, startupOptions, builder =>
        {
          builder
            .Add(NeoSetupActions.HealthChecks, services => services.AddNeoHealthChecks(this.Environment, this.Configuration))
            .Add(StartupActions.SecretVaults, services => services.AddIdentitySecretVaults(this.Environment, this.Configuration))
            .Add(StartupActions.AlwaysEncrypted, services => services.AddAlwaysEncrypted(this.Environment, this.Configuration))
            .Replace(NeoSetupActions.Mvc, services => services.AddIdentityMvcServices(this.Environment, this.Configuration))
            .Remove(NeoSetupActions.Authentication)
            .Remove(NeoSetupActions.AuthenticationProviders)
            .Add(NeoSetupActions.AuthenticationProviders, services => services.AddIdentityAuthenticationProviders(this.Environment, this.Configuration))
            .Add(NeoSetupActions.Authorisation, services => services.AddNeoAuthorisation<TuckshopApplicationUser>(
              this.Environment,
              new AuthorisationClientSetupOptions()
              {
                ApplicationName = "Identity",
                RoleTypes = [typeof(Roles)],
                ConfigureOptions = options =>
                {
                  this.Configuration.ConfigureOptions(options);
                  options.AuthenticationClientKey = AuthenticationClientKeys.Internal;
                },
              }))
            .Add(StartupActions.CookiePolicy, services => services.AddIdentityCookiePolicy(this.Environment, this.Configuration))
            .Add(StartupActions.DataServices, services => services.AddIdentityDataServices(this.Environment, this.Configuration))
            .Add(StartupActions.UserDataServices, services => services.AddIdentityUserDataServices(this.Environment, this.Configuration))
            .Add(StartupActions.ModelServices, services => services.AddIdentityModelServices(this.Environment, this.Configuration))
            .Add(StartupActions.IdentityServices, services => services.AddIdentityServices(this.Environment, this.Configuration))
            .Add(StartupActions.IntegrationServices, services => services.AddIdentityIntegrationServices(this.Environment, this.Configuration))
            .Add(StartupActions.IdentityServerServices, services => services.AddIdentityServerServices(this.Environment, this.Configuration))
            .Add(StartupActions.AsyncInitialisers, services => services.AddIdentityAsyncInitialisers(this.Environment, this.Configuration))
            .Replace(StartupActions.Logging, services => services.AddIdentityLogging(this.Environment, this.Configuration));

          serviceCollection.AddAuthorization(options => NeoIdentityExtensions.AddIsServicePolicy(options, serviceCollection));

          this.ConfigureServicesOverrides(builder);
        });
    }

    /// <summary>
    /// Configure.
    /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    /// </summary>
    /// <param name="app">The application builder.</param>
    public void Configure(IApplicationBuilder app)
    {
      app.UseNeoWebApi(this.Environment, this.Configuration, builder =>
      {
        builder
          .InsertAfter(NeoSetupActions.Cors, StartupActions.CookiePolicy, app => app.UseCookiePolicy())
          .Replace(NeoSetupActions.Authentication, app =>
          {
            app.UseAuthentication(); // added for OpenIddict
            app.UseAuthorization();
          })
          .Replace(NeoSetupActions.Endpoints, app =>
          {
            app.UseEndpoints(endpoints =>
            {
              endpoints.MapNeoHealthCheckEndpoints();
              endpoints.MapRazorPages();
              endpoints.MapControllerRoute(
                  name: "default",
                  pattern: "{controller=Home}/{action=Index}/{id?}");
            });
          });

        app.UseCsp(opt =>
        {
          opt
            .BlockAllMixedContent()
            .StyleSources(ss => ss.Self().CustomSources("https://cdn.jsdelivr.net", "https://cdnjs.cloudflare.com", "https://fonts.googleapis.com"))
            .ScriptSources(ss => ss.Self().CustomSources("https://ajax.aspnetcdn.com", "https://cdn.jsdelivr.net", "https://code.jquery.com", "https://cdnjs.cloudflare.com"))
            .FontSources(fs => fs.Self().CustomSources("https://cdnjs.cloudflare.com", "https://fonts.gstatic.com"))
            .FrameAncestors(fa => fa.Self())
            .ImageSources(img => img.Self().CustomSources("data:"));

          if (!this.Environment.IsDevelopment())
          {
            opt.DefaultSources(ds => ds.Self());
          }
        });
      });
    }

    /// <summary>
    /// Tests can override this method in order to change the application configuration.
    /// (We need to redeclare here because the tests need the Authorisation Server's ModelDbContext).
    /// </summary>
    /// <param name="builder">The Neo Application Builder.</param>
    protected override void ConfigureOverrides(NeoApplicationBuilder builder)
    {
    }
  }
}