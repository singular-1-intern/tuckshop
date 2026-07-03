namespace Tuckshop.IdentityServer.Areas.Identity.Pages.Account
{
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.AspNetCore.Mvc.RazorPages;
  using Microsoft.Extensions.Configuration;
  using Microsoft.Extensions.Logging;
  using Tuckshop.IdentityServer.App.Services;

  /// <summary>
  /// A base class for Account Pages.
  /// </summary>
  /// <typeparam name="TPageModel">The page model type.</typeparam>
  public class AccountPageModelBase<TPageModel> : PageModel
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="AccountPageModelBase{TPageModel}"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="urlProvider">The url provider.</param>
    /// <param name="configuration">The configuration.</param>
    public AccountPageModelBase(
      ILogger<TPageModel> logger,
      IUrlProvider urlProvider,
      IConfiguration configuration)
    {
      this.Logger = logger;
      this.UrlProvider = urlProvider;
      this.Configuration = configuration;
    }

    /// <summary>
    /// Gets the logger.
    /// </summary>
    protected ILogger<TPageModel> Logger { get; }

    /// <summary>
    /// Gets the url provider.
    /// </summary>
    protected IUrlProvider UrlProvider { get; }

    /// <summary>
    /// Gets the config.
    /// </summary>
    protected IConfiguration Configuration { get; }

    /// <summary>
    /// Will get the Dashboard redirect url.
    /// </summary>
    /// <returns>The full url of the dashboard page.</returns>
    protected string GetDashboardRedirectUrl()
    {
      var dashboardUrl = this.UrlProvider.GetDashboardUrl()?.ToString() ?? string.Empty;
      if (string.IsNullOrEmpty(dashboardUrl))
      {
        dashboardUrl = new Uri($"{this.Request.Scheme}://{this.Request.Host.Host}").ToString();
        this.Logger.LogInformation($"UrlProvider.GetDashboardUrl() not set, using {dashboardUrl}");
      }
      return dashboardUrl;
    }

    /// <summary>
    /// Will work out the correct authenticated redirect url for Tuckshop.
    /// </summary>
    /// <param name="returnUrl">The return url.</param>
    /// <returns>The redirect result.</returns>
    protected IActionResult AuthenticatedRedirect(string? returnUrl)
    {
      string? pathBase = this.Configuration.GetValue<string>("Routing:PathBase");

      if (string.IsNullOrEmpty(returnUrl) || returnUrl == "/" || returnUrl == pathBase || returnUrl == pathBase + @"/")
      {
        returnUrl = this.GetDashboardRedirectUrl();
        this.Logger.LogDebug($"No login url specified, redirecting to {returnUrl}");
      }
      else
      {
        this.Logger.LogDebug($"Redirecting to provided url: {returnUrl}");
      }

      if (returnUrl.ToUpperInvariant().StartsWith("HTTP", StringComparison.InvariantCulture))
      {
        return this.Redirect(returnUrl);
      }
      else
      {
        if (!string.IsNullOrEmpty(pathBase))
        {
          if (!returnUrl.StartsWith(pathBase, StringComparison.InvariantCulture))
          {
            if (!returnUrl.StartsWith(@"/", StringComparison.InvariantCulture)) { returnUrl += @"/"; }
            returnUrl = pathBase + returnUrl;

            this.Logger.LogDebug($"Local url does not start with Path Base, Path Base inserted: {returnUrl}");
          }
          else
          {
            this.Logger.LogDebug($"Local url starts with Path Base, continuing.");
          }
        }
        else
        {
          this.Logger.LogDebug($"Path Base is null or empty, continuing.");
        }
        return this.LocalRedirect(returnUrl);
      }
    }
  }
}
