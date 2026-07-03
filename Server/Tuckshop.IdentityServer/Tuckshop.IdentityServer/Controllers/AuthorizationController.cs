namespace Tuckshop.IdentityServer.Controllers
{
  using Neo.IdentityServer.App.OpenIddict.Controllers;
  using Neo.IdentityServer.App.OpenIddict.Services;
  using Neo.IdentityServer.App.OpenIddict.Validators;
  using Tuckshop.IdentityServer.Models;
  using OpenIddict.Abstractions;

  /// <summary>
  /// Authorization controller to handle authorization requests.
  /// </summary>
  public class AuthorizationController : AuthorizationControllerBase<TuckshopApplicationUser>
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorizationController"/> class.
    /// </summary>
    /// <param name="applicationManager">The application manager.</param>
    /// <param name="authorizationManager">The authorization manager.</param>
    /// <param name="scopeManager">The scope manager.</param>
    /// <param name="signInManager">The sign in manager.</param>
    /// <param name="userManager">The user manager.</param>
    /// <param name="userClaimsPrincipalFactory">The user claims principal factory.</param>
    /// <param name="profileService">The profile service.</param>
    /// <param name="customTokenRequestValidator">The custom token request validator.</param>
    /// <param name="configuration">The configuration.</param>
    public AuthorizationController(
      IOpenIddictApplicationManager applicationManager,
      IOpenIddictAuthorizationManager authorizationManager,
      IOpenIddictScopeManager scopeManager,
      SignInManager<TuckshopApplicationUser> signInManager,
      UserManager<TuckshopApplicationUser> userManager,
      IUserClaimsPrincipalFactory<TuckshopApplicationUser> userClaimsPrincipalFactory,
      IProfileService profileService,
      ICustomTokenRequestValidator customTokenRequestValidator,
      IConfiguration configuration
     )
      : base(applicationManager, authorizationManager, scopeManager, signInManager, userManager, userClaimsPrincipalFactory, profileService, customTokenRequestValidator, configuration)
    {
    }
  }
}
