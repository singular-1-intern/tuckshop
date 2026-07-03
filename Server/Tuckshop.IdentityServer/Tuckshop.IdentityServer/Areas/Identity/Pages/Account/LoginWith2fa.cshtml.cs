namespace Tuckshop.IdentityServer.Areas.Identity.Pages.Account
{
  using System;
  using System.ComponentModel.DataAnnotations;
  using System.Threading.Tasks;
  using Microsoft.AspNetCore.Authorization;
  using Microsoft.AspNetCore.Identity;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.Extensions.Configuration;
  using Microsoft.Extensions.Logging;
  using Tuckshop.IdentityServer.App.Services;
  using Tuckshop.IdentityServer.Models;

  [AllowAnonymous]
  public class LoginWith2faModel : AccountPageModelBase<LoginWith2faModel>
  {
    private readonly SignInManager<TuckshopApplicationUser> signInManager;
    private readonly ILogger<LoginWith2faModel> logger;

    public LoginWith2faModel(SignInManager<TuckshopApplicationUser> signInManager, ILogger<LoginWith2faModel> logger, IUrlProvider urlProvider, IConfiguration configuration)
      : base(logger, urlProvider, configuration)
    {
      this.signInManager = signInManager;
      this.logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new InputModel();

    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }

    public class InputModel
    {
      [Required]
      [StringLength(7, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
      [DataType(DataType.Text)]
      [Display(Name = "Authenticator code")]
      public string? TwoFactorCode { get; set; }

      [Display(Name = "Remember this machine")]
      public bool RememberMachine { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(bool rememberMe, string? returnUrl = null)
    {
      // Ensure the user has gone through the username & password screen first
      var user = await this.signInManager.GetTwoFactorAuthenticationUserAsync() ?? throw new InvalidOperationException($"Unable to load two-factor authentication user.");
      if (!user.TwoFactorConfigured)
      {
        // user needs to configure their 2FA
        return this.RedirectToPage("./EnableAuthenticator", new { ReturnUrl = returnUrl, TenantRequiresMFA = true });
      }

      this.ReturnUrl = returnUrl;
      this.RememberMe = rememberMe;

      return this.Page();
    }

    public async Task<IActionResult> OnPostAsync(bool rememberMe, string? returnUrl = null)
    {
      if (!this.ModelState.IsValid)
      {
        return this.Page();
      }

      returnUrl ??= this.Url.Content("~/");

      var user = await this.signInManager.GetTwoFactorAuthenticationUserAsync() ?? throw new InvalidOperationException($"Unable to load two-factor authentication user.");
      var authenticatorCode = this.Input.TwoFactorCode!.Replace(" ", string.Empty, StringComparison.InvariantCulture)
                                                      .Replace("-", string.Empty, StringComparison.InvariantCulture);

      var result = await this.signInManager.TwoFactorAuthenticatorSignInAsync(authenticatorCode, rememberMe, this.Input.RememberMachine);

      if (result.Succeeded)
      {
        this.logger.LogInformation("User with ID '{UserId}' logged in with 2fa.", user.Id);
        return this.AuthenticatedRedirect(returnUrl);
      }
      else if (result.IsLockedOut)
      {
        this.logger.LogWarning("User with ID '{UserId}' account locked out.", user.Id);
        return this.RedirectToPage("./Lockout");
      }
      else
      {
        this.logger.LogWarning("Invalid authenticator code entered for user with ID '{UserId}'.", user.Id);
        this.ModelState.AddModelError(string.Empty, "Invalid authenticator code.");
        return this.Page();
      }
    }
  }
}
