namespace Tuckshop.IdentityServer.App.Services
{
  using System.Configuration;
  using System.Text;
  using Microsoft.Extensions.Configuration;
  using Neo.Extensions;

  /// <summary>
  /// Provides application Urls. 
  /// Note: Customize this class to meet your projects requirements.
  /// </summary>
  public class UrlProvider : IUrlProvider
  {
    private readonly IConfiguration configuration;

    /// <summary>
    /// Initializes a new instance of the <see cref="UrlProvider"/> class.
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    public UrlProvider(IConfiguration configuration)
    {
      this.configuration = configuration;
    }

    /// <inheritdoc />
    public string GetDashboardUrl()
    {
      return this.configuration.GetString("Urls:Dashboard") ??
        throw new ConfigurationErrorsException("Urls:Dashboard section not found in config.");
    }
  }
}
