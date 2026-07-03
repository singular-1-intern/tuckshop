namespace Tuckshop.IdentityServer.Areas.Identity.Pages.Account
{
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.Extensions.Configuration;
  using Microsoft.Extensions.Logging;
  using Tuckshop.IdentityServer.App.Services;

  public class ShowRecoveryCodesModel : AccountPageModelBase<ShowRecoveryCodesModel>
  {
    public ShowRecoveryCodesModel(ILogger<ShowRecoveryCodesModel> logger, IUrlProvider urlProvider, IConfiguration configuration)
      : base(logger, urlProvider, configuration)
    {
    }

    public string[]? RecoveryCodes { get; private set; }

    public string ReturnUrl { get; set; } = string.Empty;

    public IActionResult OnGet(string returnUrl, string[] recoveryCodes)
    {
      this.RecoveryCodes = recoveryCodes;
      if (string.IsNullOrEmpty(returnUrl) || returnUrl == @"/")
      {
        this.ReturnUrl = this.GetDashboardRedirectUrl();
      }
      else
      {
        this.ReturnUrl = returnUrl;
      }

      if (this.RecoveryCodes == null)
      {
        return this.RedirectToPage("TwoFactorAuthentication");
      }

      return this.Page();
    }
  }
}