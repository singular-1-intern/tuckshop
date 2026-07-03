namespace Tuckshop.IdentityServer.Views.ViewModels
{
  /// <summary>
  /// Represents the Redirect ViewModel.
  /// </summary>
  public class RedirectViewModel
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="RedirectViewModel"/> class.
    /// </summary>
    /// <param name="redirectUrl">The redirect Url.</param>
    public RedirectViewModel(string redirectUrl)
    {
      this.RedirectUrl = redirectUrl;
    }

    /// <summary>
    /// Gets or sets the redirect Url.
    /// </summary>
    public string RedirectUrl { get; set; }
  }
}