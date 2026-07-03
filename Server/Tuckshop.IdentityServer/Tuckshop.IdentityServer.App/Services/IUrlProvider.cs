namespace Tuckshop.IdentityServer.App.Services
{
  /// <summary>
  /// Interface for the url provider.
  /// </summary>
  public interface IUrlProvider
  {
    /// <summary>
    /// Gets the dashboard url.
    /// </summary>
    /// <returns>The url string.</returns>
    string GetDashboardUrl();
  }
}