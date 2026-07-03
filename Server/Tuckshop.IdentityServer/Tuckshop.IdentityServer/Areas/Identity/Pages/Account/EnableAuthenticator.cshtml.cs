namespace Tuckshop.IdentityServer.Areas.Identity.Pages.Account
{
  using System;
  using System.ComponentModel.DataAnnotations;
  using System.Globalization;
  using System.Linq;
  using System.Text;
  using System.Text.Encodings.Web;
  using System.Threading.Tasks;
  using Microsoft.AspNetCore.Identity;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.Extensions.Configuration;
  using Microsoft.Extensions.Hosting;
  using Microsoft.Extensions.Logging;
  using Tuckshop.IdentityServer.App.Services;
  using Tuckshop.IdentityServer.Models;

  /// <summary>
  /// Page to enable the authenticator.
  /// </summary>
  public class EnableAuthenticatorModel : AccountPageModelBase<EnableAuthenticatorModel>
  {
    private const string AuthenticatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";
    private readonly SignInManager<TuckshopApplicationUser> signInManager;
    private readonly UserManager<TuckshopApplicationUser> userManager;
    private readonly ILogger<EnableAuthenticatorModel> logger;
    private readonly UrlEncoder urlEncoder;
    private readonly IHostEnvironment environment;

    /// <summary>
    /// Initializes a new instance of the <see cref="EnableAuthenticatorModel"/> class.
    /// </summary>
    /// <param name="signInManager">The sign in manager.</param>
    /// <param name="userManager">The user manager.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="urlEncoder">The url encoder.</param>
    /// <param name="urlProvider">The url provider.</param>
    /// <param name="configuration">The config.</param>
    /// <param name="environment">The host environment.</param>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public EnableAuthenticatorModel(
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        SignInManager<TuckshopApplicationUser> signInManager,
        UserManager<TuckshopApplicationUser> userManager,
        ILogger<EnableAuthenticatorModel> logger,
        UrlEncoder urlEncoder,
        IUrlProvider urlProvider,
        IConfiguration configuration,
        IHostEnvironment environment)
      : base(logger, urlProvider, configuration)
    {
      this.signInManager = signInManager;
      this.userManager = userManager;
      this.logger = logger;
      this.urlEncoder = urlEncoder;
      this.environment = environment;
    }

    public string SharedKey { get; set; }

    public string AuthenticatorUri { get; set; }

    [TempData]
    public string[] RecoveryCodes { get; set; }

    [TempData]
    public string StatusMessage { get; set; }

    [BindProperty]
    public InputModel Input { get; set; } = new InputModel();

    public string? ReturnUrl { get; set; }

    public bool RememberMe { get; set; }

    public bool TenantRequiresMFA { get; set; }

    public bool IsTwoFactorUser { get; private set; }

    public class InputModel
    {
      [Required]
      [StringLength(7, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
      [DataType(DataType.Text)]
      [Display(Name = "Verification Code")]
      public string? Code { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(bool tenantRequiresMFA = false, bool rememberMe = false, string? returnUrl = null)
    {
      this.TenantRequiresMFA = tenantRequiresMFA;
      this.ReturnUrl = returnUrl;
      this.RememberMe = rememberMe;

      TuckshopApplicationUser? user = await this.GetUserAsync();
      if (user == null)
      {
        this.logger.LogError("Cannot enable MFA, no user found");

        return this.RedirectToPage("./Login", new { ReturnUrl = this.Request.Path });
      }

      await this.LoadSharedKeyAndQrCodeUriAsync(user);

      return this.Page();
    }

    public async Task<IActionResult> OnPostAsync(bool tenantRequiresMFA = false, bool rememberMe = false, string? returnUrl = null)
    {
      if (returnUrl == "/Identity/Account/EnableAuthenticator")
      {
        returnUrl = string.Empty;
      }

      this.TenantRequiresMFA = tenantRequiresMFA;
      this.ReturnUrl = returnUrl;
      this.RememberMe = rememberMe;

      var user = await this.GetUserAsync();
      if (user == null)
      {
        return this.NotFound($"Unable to load user to enable authenticator.");
      }

      if (!this.ModelState.IsValid)
      {
        await this.LoadSharedKeyAndQrCodeUriAsync(user);
        return this.Page();
      }

      // Strip spaces and hyphens
      var verificationCode = this.Input.Code!.Replace(" ", string.Empty, StringComparison.InvariantCulture)
                                  .Replace("-", string.Empty, StringComparison.InvariantCulture);

      var is2faTokenValid = await this.userManager.VerifyTwoFactorTokenAsync(
          user, this.userManager.Options.Tokens.AuthenticatorTokenProvider, verificationCode);

      if (!is2faTokenValid)
      {
        this.ModelState.AddModelError("Input.Code", "Verification code is invalid.");
        await this.LoadSharedKeyAndQrCodeUriAsync(user);
        return this.Page();
      }

      user.ConfigureTwoFactor(true);
      await this.userManager.SetTwoFactorEnabledAsync(user, true);
      var userId = await this.userManager.GetUserIdAsync(user);
      this.logger.LogInformation("User with ID '{UserId}' has enabled 2FA with an authenticator app.", userId);

      this.StatusMessage = "Your authenticator app has been verified.";

      if (this.IsTwoFactorUser)
      {
        var result = await this.signInManager.TwoFactorAuthenticatorSignInAsync(verificationCode, rememberMe, rememberClient: false);

        if (!result.Succeeded)
        {
          this.logger.LogError("Unsuccessful TwoFactorAuthenticatorSignInAsync after validating user with ID '{UserId}'", user.Id);

          return this.RedirectToPage("./Login", new { ReturnUrl = this.Request.Path });
        }
      }

      // Decide if you want Recovery Codes, this is an extra burden for the user to worry about and it is much easier
      // for them to contact their admin and request a reset, however this depends on the number of users and the type of system.
      // If we set the user's TwoFactorConfigured to false, then they can re-configure on next login, this can be done in the User Management screen
      // which ships with the template.
      if (await this.userManager.CountRecoveryCodesAsync(user) == 0)
      {
        var recoveryCodes = await this.userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
        this.RecoveryCodes = (recoveryCodes ?? []).ToArray();
        return this.RedirectToPage("./ShowRecoveryCodes", new { ReturnUrl = returnUrl, this.RecoveryCodes });
      }

      return this.AuthenticatedRedirect(returnUrl);
    }

    private async Task<TuckshopApplicationUser?> GetUserAsync()
    {
      var user = await this.userManager.GetUserAsync(this.User);
      if (user == null)
      {
        // user has not yet authenticated.
        this.IsTwoFactorUser = true;
        user = await this.signInManager.GetTwoFactorAuthenticationUserAsync();
      }
      return user;
    }

    private async Task LoadSharedKeyAndQrCodeUriAsync(TuckshopApplicationUser user)
    {
      // Load the authenticator key & QR code URI to display on the form
      var unformattedKey = await this.userManager.GetAuthenticatorKeyAsync(user);
      if (string.IsNullOrEmpty(unformattedKey))
      {
        await this.userManager.ResetAuthenticatorKeyAsync(user);
        unformattedKey = await this.userManager.GetAuthenticatorKeyAsync(user);
      }

      this.SharedKey = FormatKey(unformattedKey!);

      var email = await this.userManager.GetEmailAsync(user);
      this.AuthenticatorUri = this.GenerateQrCodeUri(email!, unformattedKey!);
    }

    private static string FormatKey(string unformattedKey)
    {
      var result = new StringBuilder();
      int currentPosition = 0;
      while (currentPosition + 4 < unformattedKey.Length)
      {
        result.Append(unformattedKey.AsSpan(currentPosition, 4)).Append(' ');
        currentPosition += 4;
      }
      if (currentPosition < unformattedKey.Length)
      {
        result.Append(unformattedKey.AsSpan(currentPosition));
      }

      return result.ToString().ToLowerInvariant();
    }

    private string GenerateQrCodeUri(string email, string unformattedKey)
    {
      string appName = "Tuckshop";

      string host = this.Request.Host.Host;
      if (!this.environment.IsProduction())
      {
        // tweak the app name
        if (this.environment.IsDevelopment())
        {
          appName = $"Dev-{appName}";
        }
        else
        {
          if (host.Contains(".uat-", StringComparison.InvariantCultureIgnoreCase))
          {
            appName = $"UAT-{appName}";
          }
          else
          {
            appName = $"QA-{appName}";
          }
        }
      }
      else if (host.Contains(".pp-", StringComparison.InvariantCultureIgnoreCase))
      {
        appName = $"PP-{appName}";
      }

      // "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";
      return string.Format(
          CultureInfo.CurrentCulture,
          AuthenticatorUriFormat,
          this.urlEncoder.Encode(appName),
          this.urlEncoder.Encode(email),
          unformattedKey);
    }
  }
}
