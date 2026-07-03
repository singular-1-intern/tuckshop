#pragma warning disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Neo.AuthorisationServer.Client;
using Neo.Extensions;
using Neo.IdentityServer.App.Services;
using Neo.IdentityServer.App.Services.IdentityProviders;
using Neo.IdentityServer.Models;
using Neo.IdentityServer.Models.IdentityProviders;
using Neo.IdentityServer.Models.SignIn;
using Neo.Model.Exceptions;
using Neo.Model.MultiTenancy;
using Neo.Model.ValueObjects;
using Tuckshop.IdentityServer.App.Services;
using Tuckshop.IdentityServer.Models;

namespace Tuckshop.IdentityServer.Areas.Identity.Pages.Account
{
  [AllowAnonymous]
  public class ExternalLoginModel : AccountPageModelBase<ExternalLoginModel>
  {
    private readonly SignInManager<TuckshopApplicationUser> signInManager;
    private readonly UserManager<TuckshopApplicationUser> userManager;
    private readonly IHttpContextAccessor contextAccessor;
    private readonly IEmailSender emailSender;
    private readonly IAuthorisationService authorisationService;
    private readonly IRegistrationService registrationService;
    private readonly IdentityDbContext identityDbContext;
    private readonly ITenantService tenantService;
    private readonly IIdentityProviderLookupService identityProviderService;
    private readonly IIdentityProviderUserService<TuckshopApplicationUser> identityProviderUserService;
    private readonly ISignInAuditService signInAuditService;

    public ExternalLoginModel(
        SignInManager<TuckshopApplicationUser> signInManager,
        UserManager<TuckshopApplicationUser> userManager,
        ILogger<ExternalLoginModel> logger,
        IHttpContextAccessor contextAccessor,
        IEmailSender emailSender,
        IAuthorisationService authorisationService,
        IUrlProvider urlProvider,
        IConfiguration configuration,
        IRegistrationService registrationService,
        IdentityDbContext identityDbContext,
        ITenantService tenantService,
        IIdentityProviderLookupService identityProviderService,
        IIdentityProviderUserService<TuckshopApplicationUser> identityProviderUserService,
        ISignInAuditService signInAuditService)
      : base(logger, urlProvider, configuration)
    {
      this.signInManager = signInManager;
      this.userManager = userManager;
      this.contextAccessor = contextAccessor;
      this.emailSender = emailSender;
      this.authorisationService = authorisationService;
      this.registrationService = registrationService;
      this.identityDbContext = identityDbContext;
      this.tenantService = tenantService;
      this.identityProviderService = identityProviderService;
      this.identityProviderUserService = identityProviderUserService;
      this.signInAuditService = signInAuditService;
    }

    [BindProperty]
    public InputModel Input { get; set; }

    public string ProviderDisplayName { get; set; }

    public string ReturnUrl { get; set; }

    [TempData]
    public string ErrorMessage { get; set; }

    public class InputModel
    {
      [Required]
      [EmailAddress]
      public string Email { get; set; }
    }

    public IActionResult OnGetAsync()
    {
      return RedirectToPage("./Login");
    }

    [HttpPost()]
    public IActionResult OnPost(string provider, [FromQuery] string returnUrl = null)
    {
      // Request a redirect to the external login provider.
      var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
      var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
      return new ChallengeResult(provider, properties);
    }

    public async Task<IActionResult> OnGetCallbackAsync(string returnUrl = null, string remoteError = null)
    {
      LoginContext loginContext = new LoginContext();
      IdentityProviderLookup identityProvider = null;

      try
      {
        returnUrl = returnUrl ?? Url.Content("~/");
        if (remoteError != null)
        {
          loginContext.SetError($"Error from external provider: '{remoteError}'. Please contact the system administrator.");
          return await this.HandleLoginErrorAsync(loginContext);
        }

        // Extract information from the token(s) that came back from the external provider
        var info = await signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
          loginContext.SetError("Error loading external login information. Please contact the system administrator.");
          return await this.HandleLoginErrorAsync(loginContext);
        }

        identityProvider = await this.identityProviderService.GetIdentityProviderAsync(info.LoginProvider);

        ReturnUrl = returnUrl;
        ProviderDisplayName = info.ProviderDisplayName;

        // Get the claims from the ID Token
        string firstName = info.GetClaim(ClaimTypes.GivenName);
        string lastName = info.GetClaim(ClaimTypes.Surname);
        string email = info.GetClaim(ClaimTypes.Email);
        bool emailVerified = (info.GetClaim("email_verified")).ToBool(false);
        if (!emailVerified)
        {
          if (email.Equals(info.GetClaim("verified_primary_email"), StringComparison.InvariantCultureIgnoreCase))
          {
            emailVerified = true;
          }
        }

        loginContext
          .SetLoginProvider(info.LoginProvider)
          .SetReturnUrl(returnUrl)
          .SetTenantIdentityProvider(identityProvider)
          .AddExtraInfo("Username", info.ProviderKey);

        if (string.IsNullOrEmpty(email))
        {
          loginContext.SetError("Unable to retrieve your email from the identity provider. Please ensure your account is set up correctly.");
          return await this.HandleLoginErrorAsync(loginContext);
        }

        loginContext.SetUserId(email);

        if (!emailVerified)
        {
          loginContext.SetError("Please ensure your email address has been verified with the external identity provider before trying to log in again.");
          return await this.HandleLoginErrorAsync(loginContext);
        }

        var user = await this.userManager.FindByEmailAsync(email);
        if (user != null)
        {
          loginContext.SetUser(user);

          // Check that same identity provider
          if (user.IdentityProviderId != identityProvider.IdentityProviderId)
          {
            try
            {
              await this.identityProviderUserService.ConvertUserToExternalIdentityProviderAsync(user, identityProvider.IdentityProviderId, info);
            }
            catch (Exception exception)
            {
              string error = exception is AggregateException ?
                $"{exception.Message} ({string.Join(", ", ((AggregateException)exception).InnerExceptions.Select(e => e.Message))}" :
                exception.Message;

              loginContext.SetError(
                userError: "Error converting your account to external identity provider, please contact the System Administrator.",
                internalError: error);

              return await this.HandleLoginErrorAsync(loginContext);
            }
          }
        }

        // Signs in the user with this external login provider if the user already has a login.
        var result = await this.signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

        // If signin failed for an unspecified reason, but we have a user record, this means that the ProviderKey in AspNetUserLogins is out of date
        // (This typically happens if the user switches from one OIDC Identity Provider to another, E.g. Moving between Okta instances)
        if (user != null && result == Microsoft.AspNetCore.Identity.SignInResult.Failed)
        {
          if (await identityProviderUserService.TrySwitchExternalLoginSignInAsync(info, user))
          {
            // switch was successful, try sign in again
            result = await signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
          }
        }

        if (result.Succeeded)
        {
          // Ensure the force logout flag is cleared, otherwise the user may be logged out again after a short time
          if (user.ForceLogout)
          {
            this.identityDbContext.Users.Attach(user);
            user.ForceLogout = false;
            await this.identityDbContext.SaveChangesAsync();
          }

          return await this.HandleLoginSuccessAsync(loginContext, result);
        }

        if (result.IsLockedOut)
        {
          return RedirectToPage("./Lockout");
        }

        // Create the user and automatically log in
        var newUser = new TuckshopApplicationUser
        {
          UserName = email,
          Email = email,
          FirstName = firstName,
          LastName = lastName,
          IsActive = true,
          IdentityProviderId = identityProvider.IdentityProviderId,
          EmailConfirmed = true
        };

        var createResult = await this.userManager.CreateAsync(newUser);

        if (!createResult.Succeeded)
        {
          loginContext.SetError(
            userError: "An error occured while setting up your account. Please contact the System Administrator.",
            internalError: $"userManager.CreateAsync error: {string.Join(", ", createResult.Errors.Select(e => $"{e.Code}: {e.Description}"))}");
          return await this.HandleLoginErrorAsync(loginContext);
        }

        loginContext.SetUser(newUser);

        var addLoginResult = await userManager.AddLoginAsync(newUser, info);
        if (!addLoginResult.Succeeded)
        {
          loginContext.SetError(
            userError: "Unable to login. Please contact the System Administrator.",
            internalError: $"userManager.AddLoginAsync error: {string.Join(", ", addLoginResult.Errors.Select(e => $"{e.Code}: {e.Description}"))}");
          return await this.HandleLoginErrorAsync(loginContext);
        }

        this.Logger.LogInformation($"User created an account using {info.LoginProvider} provider.");

        var loginResult = await signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
        if (!loginResult.Succeeded)
        {
          loginContext.SetError(
            userError: "Unable to login. Please contact the System Administrator.",
            internalError: $"signInManager.ExternalLoginSignInAsync error: IsLockedOut: {loginResult.IsLockedOut}, IsNotAllowed: {loginResult.IsNotAllowed}, RequiresTwoFactor: {loginResult.RequiresTwoFactor})");
          return await this.HandleLoginErrorAsync(loginContext);
        }

        return await this.HandleLoginSuccessAsync(loginContext, result);
      }
      catch (Exception exception)
      {
        loginContext.SetError(
          userError: "Unable to Login. Please contact the System Administrator.",
          internalError: $"External Login Error: {exception.ToString()}");
        return await this.HandleLoginErrorAsync(loginContext);
      }
    }

    public async Task<IActionResult> OnPostConfirmationAsync(string returnUrl = null)
    {
      returnUrl = returnUrl ?? Url.Content("~/");

      // Get the information about the user from the external login provider
      var info = await signInManager.GetExternalLoginInfoAsync();
      if (info == null)
      {
        ErrorMessage = "Error loading external login information during confirmation.";
        return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
      }

      if (ModelState.IsValid)
      {
        var identityProvider = await this.identityProviderService.GetIdentityProviderAsync(info.LoginProvider)
            ?? throw new InvalidDomainOperationException($"No IdentityProvider found for name {info.LoginProvider}");

        var user = new TuckshopApplicationUser { UserName = Input.Email, Email = Input.Email, IdentityProviderId = identityProvider.IdentityProviderId };

        var result = await userManager.CreateAsync(user);
        if (result.Succeeded)
        {
          result = await userManager.AddLoginAsync(user, info);
          if (result.Succeeded)
          {
            this.Logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);

            var userId = await userManager.GetUserIdAsync(user);
            var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { area = "Identity", userId = userId, code = code },
                protocol: Request.Scheme);

            await emailSender.SendEmailAsync(
              Input.Email,
              "Confirm your email",
              $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

            // If account confirmation is required, we need to show the link if we don't have a real email sender
            if (userManager.Options.SignIn.RequireConfirmedAccount)
            {
              return RedirectToPage("./RegisterConfirmation", new { Email = Input.Email });
            }

            await signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);

            return LocalRedirect(returnUrl);
          }
        }
        foreach (var error in result.Errors)
        {
          ModelState.AddModelError(string.Empty, error.Description);
        }
      }

      ProviderDisplayName = info.ProviderDisplayName;
      ReturnUrl = returnUrl;
      return Page();
    }

    #region "Error Handling"

    private async Task<IActionResult> HandleLoginSuccessAsync(
      LoginContext loginContext,
      Microsoft.AspNetCore.Identity.SignInResult result)
    {
      this.Logger.LogInformation($"User {loginContext.User.Id} logged in with {loginContext.LoginProvider} provider.");

      this.signInAuditService.LogSignIn(
        SignInAudit.SuccessSignInAudit(
          loginContext.User.Id,
          loginContext.IdentityProvider.IdentityProviderId,
          "External Login",
          RequestHeaderDetails.Create(this.Request.HttpContext),
          loginContext.ExtraInfo));

      return AuthenticatedRedirect(loginContext.ReturnUrl);
    }

    private async Task<IActionResult> HandleLoginErrorAsync(LoginContext loginContext)
    {
      ErrorMessage = loginContext.UserError;
      this.signInAuditService.LogSignIn(
        SignInAudit.FailedSignInAudit(
          loginContext.UserId,
          loginContext.IdentityProvider.IdentityProviderId,
          loginContext.InternalError,
          RequestHeaderDetails.Create(this.Request.HttpContext),
          loginContext.ExtraInfo));

      return RedirectToPage("./Login", new { ReturnUrl = loginContext.ReturnUrl });
    }

    #endregion

    /// <summary>
    /// Contains information about a login attempt
    /// </summary>
    private class LoginContext
    {
      /// <summary>
      /// Gets the Login Provider name
      /// </summary>
      public string LoginProvider { get; private set; }

      /// <summary>
      /// Gets the User Id
      /// </summary>
      public string UserId { get; private set; }

      /// <summary>
      /// Gets the Return URL for the login
      /// </summary>
      public string ReturnUrl { get; private set; }

      /// <summary>
      /// Gets the User logging in
      /// </summary>
      public ApplicationUser User { get; private set; }

      /// <summary>
      /// Gets the Identity Provider
      /// </summary>
      public IdentityProviderLookup IdentityProvider { get; private set; }

      /// <summary>
      /// Gets the error to display to the user
      /// </summary>
      public string UserError { get; private set; }

      /// <summary>
      /// Gets the error to log internally
      /// </summary>
      public string InternalError { get; private set; }

      /// <summary>
      /// Gets the extra information for the login context
      /// </summary>
      public Dictionary<string, string> ExtraInfo { get; private set; } = new();

      /// <summary>
      /// Sets the Login Provider
      /// </summary>
      /// <param name="loginProvider">The login provider name</param>
      /// <returns>The Login Context</returns>
      public LoginContext SetLoginProvider(string loginProvider)
      {
        this.LoginProvider = loginProvider;
        return this;
      }

      /// <summary>
      /// Sets the User Id
      /// </summary>
      /// <param name="userId">The User Id</param>
      /// <returns>The Login Context</returns>
      public LoginContext SetUserId(string userId)
      {
        this.UserId = userId;
        return this;
      }

      /// <summary>
      /// Sets the Return URL
      /// </summary>
      /// <param name="returnUrl">The return URL to redirect the user to after login</param>
      /// <returns>The Login Context</returns>
      public LoginContext SetReturnUrl(string returnUrl)
      {
        this.ReturnUrl = returnUrl;
        return this;
      }

      /// <summary>
      /// Sets the User and User Id
      /// </summary>
      /// <param name="user">The user that is logging in</param>
      /// <returns>The Login Context</returns>
      public LoginContext SetUser(ApplicationUser user)
      {
        this.User = user;
        this.UserId = user.Id;
        return this;
      }

      /// <summary>
      /// Sets the Tenant Identity Provider
      /// </summary>
      /// <param name="tenantIdentityProvider">The Tenant Identity Provider</param>
      /// <returns>The Login Context</returns>
      public LoginContext SetTenantIdentityProvider(IdentityProviderLookup identityProvider)
      {
        this.IdentityProvider = identityProvider;
        return this;
      }

      /// <summary>
      /// Sets the error information
      /// </summary>
      /// <param name="userError">The error to display to the user</param>
      /// <param name="internalError">The error to log internally (Optional)</param>
      /// <returns>The Login Context</returns>
      public LoginContext SetError(string userError, string internalError = null)
      {
        this.UserError = userError;
        this.InternalError = internalError ?? userError;
        return this;
      }

      /// <summary>
      /// Adds extra information to the login context
      /// </summary>
      /// <param name="key">The key to store the information against</param>
      /// <param name="value">The extra information</param>
      /// <returns>The Login Context</returns>
      public LoginContext AddExtraInfo(string key, string value)
      {
        if (this.ExtraInfo.ContainsKey(key))
        {
          this.ExtraInfo[key] = value;
        }
        else
        {
          this.ExtraInfo.Add(key, value);
        }

        return this;
      }
    }
  }
}