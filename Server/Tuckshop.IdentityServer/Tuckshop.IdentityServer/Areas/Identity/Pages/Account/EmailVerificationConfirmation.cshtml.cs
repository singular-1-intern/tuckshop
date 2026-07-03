#pragma warning disable SA1649 // File name should match first type name
#pragma warning disable CA1056 // URI-like properties should not be strings
namespace Tuckshop.IdentityServer.Areas.Identity.Pages.Account
{
  using Microsoft.AspNetCore.Mvc.RazorPages;
  using Neo.IdentityServer.Providers;
  using Tuckshop.IdentityServer.App.Services;

  /// <summary>
  /// An Email Verification Confirmation class.
  /// </summary>
  public class EmailVerificationConfirmationModel : PageModel
  {
    private readonly IDataProtectionOptionsProvider dataProtectionOptionsProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailVerificationConfirmationModel"/> class.
    /// </summary>
    /// <param name="dataProtectionOptionsProvider">The data protection options provider.</param>
    public EmailVerificationConfirmationModel(IDataProtectionOptionsProvider dataProtectionOptionsProvider)
    {
      this.dataProtectionOptionsProvider = dataProtectionOptionsProvider;
    }

    /// <summary>
    /// Gets or sets the home page url.
    /// </summary>
    public string ReturnUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets the link expiry hours.
    /// </summary>
    public double LinkExpiryHours { get; private set; }

    /// <summary>
    /// The OnGet method.
    /// </summary>
    public void OnGet()
    {
      this.ReturnUrl = this.Url.Content("~/");
      this.LinkExpiryHours = this.dataProtectionOptionsProvider.GetOptions(DataProtectionPurposes.EmailConfirmation).TokenLifespan.TotalHours;
    }
  }
}
