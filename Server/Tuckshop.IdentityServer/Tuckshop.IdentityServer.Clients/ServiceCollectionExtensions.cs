#pragma warning disable IDE0060 // Remove unused parameter
namespace Neo.Extensions.DependencyInjection
{
  using System.Configuration;
  using Microsoft.Extensions.Configuration;
  using Microsoft.Extensions.DependencyInjection;
  using Microsoft.Extensions.Hosting;
  using Neo.Identity.Clients;
  using Tuckshop.IdentityServer.Clients;
  using Tuckshop.IdentityServer.Services;

  /// <summary>
  /// Extends the <see cref="IServiceCollection"/> class.
  /// </summary>
  public static class ServiceCollectionExtensions
  {
    /// <summary>
    /// Will add the Identity Api Client as itself and its interfaces.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The config.</param>
    /// <param name="environment">The host environment.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddIdentityClientServices(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
      services.AddSingleton(
       new Model.Services.Api.ApiClientOptions<IdentityServerApiClient>()
       {
         EndPoint = configuration["IdentityService:EndPointUrl"] ?? throw new ConfigurationErrorsException("Missing IdentityService:EndPointUrl configuration"),
         AuthenticationClientKey = AuthenticationClientKeys.IdentityServer,
       });

      services.AddHttpClient<IdentityServerApiClient>();
      services.AddScoped<IIdentityServerService>(sp => sp.GetRequiredService<IdentityServerApiClient>());

      return services;
    }
  }
}