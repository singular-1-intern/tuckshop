namespace Tuckshop.IdentityServer.Areas.Identity.Pages.Account
{
  using System.Collections.Generic;
  using System.ComponentModel.DataAnnotations;
  using System.Linq;
  using System.Threading.Tasks;
  using Microsoft.AspNetCore.Authentication;
  using Microsoft.AspNetCore.Authorization;
  using Microsoft.AspNetCore.Identity;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.Extensions.Configuration;
  using Microsoft.Extensions.Logging;
  using Neo.IdentityServer.App.Services.IdentityProviders;
  using Neo.IdentityServer.Models.IdentityProviders;
  using Neo.Model.Metadata;
  using Tuckshop.IdentityServer.App.Services;

  /// <summary>
  /// The LoginModel class.
  /// </summary>
  [AllowAnonymous]
  public class LoginModel : AccountPageModelBase<LoginModel>
  {
    private readonly SignInManager signInManager;
    private readonly ILogger<LoginModel> logger;
    private readonly IIdentityProviderCache identityProviderCache;
    private List<IdentityProviderLookup>? externalProviders;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoginModel"/> class.
    /// </summary>
    /// <param name="signInManager">The sign in manager.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="urlProvider">The url provider.</param>
    /// <param name="configuration">The configuration.</param>
    /// <param name="identityProviderCache">the identity provider cache.</param>
    public LoginModel(
      SignInManager signInManager,
      ILogger<LoginModel> logger,
      IUrlProvider urlProvider,
      IConfiguration configuration,
      IIdentityProviderCache identityProviderCache)
      : base(logger, urlProvider, configuration)
    {
      this.signInManager = signInManager;
      this.logger = logger;
      this.identityProviderCache = identityProviderCache;
    }

    /// <summary>
    /// Gets or sets the input model.
    /// </summary>
    [BindProperty]
    public InputModel Input { get; set; } = new InputModel();

    /// <summary>
    /// Gets or sets the list of external login authentication schemes.
    /// </summary>
    public IList<AuthenticationScheme>? ExternalLogins { get; set; }

    /// <summary>
    /// Gets or sets the Return Url.
    /// </summary>
    public string? ReturnUrl { get; set; }

    /// <summary>
    /// Gets or sets the Error Message.
    /// </summary>
    [TempData]
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Executes when the login page is fetched.
    /// </summary>
    /// <param name="returnUrl">The return url query parameter.</param>
    /// <returns>A task awaiting the get operation.</returns>
    public async Task<IActionResult> OnGetAsync(string? returnUrl = null)
    {
      if (!string.IsNullOrEmpty(this.ErrorMessage))
      {
        this.ModelState.AddModelError(string.Empty, this.ErrorMessage);
      }

      returnUrl ??= this.Url.Content("~/");

      if (this.User.Identity?.IsAuthenticated == true)
      {
        // Clear the existing external cookie to ensure a clean login process
        await this.HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
        // This is done to make sure the correct QRCode is displayed when using 2fa
        await this.HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);

        // Redirect to self
        return this.RedirectToPage(pageName: null, routeValues: new { returnUrl });
      }

      this.ExternalLogins = await this.GetExternalLoginsAsync();

      this.ReturnUrl = returnUrl;

      return this.Page();
    }

    /// <summary>
    /// Executes when the login page is posted.
    /// </summary>
    /// <param name="returnUrl">The return url query parameter.</param>
    /// <returns>A task awaiting the post operation.</returns>
    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
      returnUrl ??= this.Url.Content("~/");

      if (this.ModelState.IsValid)
      {
        var result = await this.signInManager.PasswordSignInAsync(this.Input.Email!, this.Input.Password!, this.Input.RememberMe, lockoutOnFailure: true);
        if (result.Succeeded && this.signInManager.SignedInUser != null)
        {
          this.logger.LogInformation("User logged in.");

          if (this.signInManager.MustConfigureMFA(result, this.signInManager.SignedInUser))
          {
            return this.RedirectToPage("./EnableAuthenticator", new { ReturnUrl = returnUrl, TenantRequiresMFA = true });
          }
          else
          {
            return this.AuthenticatedRedirect(returnUrl);
          }
        }

        if (result.RequiresTwoFactor)
        {
          return this.RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = this.Input.RememberMe });
        }

        if (result.IsLockedOut)
        {
          this.logger.LogWarning("User account locked out.");
          return this.RedirectToPage("./Lockout");
        }
        else
        {
          this.ModelState.AddModelError(string.Empty, "Invalid login attempt.");
          return this.Page();
        }
      }

      // If we got this far, something failed, redisplay form
      return this.Page();
    }

    public string? GetProviderImageUrl(string identityProviderName)
    {
      if (this.externalProviders != null)
      {
        IdentityProviderLookup? provider = this.externalProviders.FirstOrDefault(identityProvider => identityProvider.Name == identityProviderName);
        return this.GenerateButtonImageUrl(provider);
      }

      return null;
    }

    /// <summary>
    /// Gets the URL to use for the Identity Provider's button image.
    /// </summary>
    /// <param name="provider">The identity provider lookup.</param>
    /// <returns>A URL.</returns>
    public string GenerateButtonImageUrl(IdentityProviderLookup? provider)
    {
      if (provider == null)
      {
        return string.Empty;
      }

      if (!string.IsNullOrWhiteSpace(provider.ButtonImageUrl))
      {
        return provider.ButtonImageUrl;
      }
      else
      {
        return $"~/images/IdentityProviders/{((IdentityProviderType)provider.IdentityProviderType).GetIdentityProviderNamePrefix()}.png";
      }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0305:Simplify collection initialization", Justification = "Readability")]
    private async Task<IList<AuthenticationScheme>> GetExternalLoginsAsync()
    {
      this.externalProviders = (await this.identityProviderCache.GetAsync() ?? throw new InvalidOperationException("No external providers returned from IdentityProviderCache"))
        .OrderBy(identityProvider => identityProvider.Name)
        .ToList();

      var externalLogins = (await this.signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
      externalLogins = externalLogins.Where(authScheme =>
        this.externalProviders.Any(identityProvider => identityProvider.Name == authScheme.Name)).OrderBy(identityProvider => identityProvider.Name).ToList();

      return externalLogins;
    }

    /// <summary>
    /// Login input model.
    /// </summary>
    public class InputModel
    {
      /// <summary>
      /// Gets or sets the Email.
      /// </summary>
      [Required]
      [EmailAddress]
      public string? Email { get; set; }

      /// <summary>
      /// Gets or sets the Password.
      /// </summary>
      [Required]
      [DataType(System.ComponentModel.DataAnnotations.DataType.Password)]
      public string? Password { get; set; }

      /// <summary>
      /// Gets or sets a value indicating whether the user wants their login session remembered.
      /// </summary>
      [Display(Name = "Remember me?")]
      public bool RememberMe { get; set; }
    }
  }
}
