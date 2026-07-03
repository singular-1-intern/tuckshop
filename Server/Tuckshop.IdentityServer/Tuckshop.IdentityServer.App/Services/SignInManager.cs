namespace Tuckshop.IdentityServer.App.Services
{
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using Microsoft.AspNetCore.Authentication;
  using Microsoft.AspNetCore.Http;
  using Microsoft.AspNetCore.Identity;
  using Microsoft.Extensions.Options;
  using Neo.Extensions;
  using Neo.IdentityServer.App.Services;
  using Neo.IdentityServer.App.Services.IdentityProviders;
  using Neo.IdentityServer.Models.IdentityProviders;
  using Neo.IdentityServer.Models.SignIn;
  using Neo.IdentityServer.SignInRules;
  using Neo.Model.ValueObjects;
  using Tuckshop.IdentityServer.Models;

  /// <summary>
  /// The sign in manager override.
  /// </summary>
  public class SignInManager : SignInManager<TuckshopApplicationUser>
  {
    private readonly ILogger<SignInManager> logger;
    private readonly IAuthenticationSchemeProvider schemes;
    private readonly ISignInAuditService signInAuditService;
    private readonly IMfaManager mfaManager;
    private readonly IIdentityProviderLookupService identityProviderService;

    /// <summary>
    /// Initializes a new instance of the <see cref="SignInManager"/> class.
    /// </summary>
    /// <param name="userManager">The user manager.</param>
    /// <param name="contextAccessor">The context accessor.</param>
    /// <param name="claimsFactory">The claims factory.</param>
    /// <param name="optionsAccessor">The options accessor.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="schemes">The schemes.</param>
    /// <param name="confirmation">Used check whether a user account is confirmed.</param>
    /// <param name="signInAuditService">The sign in audit service.</param>
    /// <param name="mfaManager">The mfa manager.</param>
    /// <param name="identityProviderService">The identity provider service.</param>
    public SignInManager(
      UserManager<TuckshopApplicationUser> userManager,
      IHttpContextAccessor contextAccessor,
      IUserClaimsPrincipalFactory<TuckshopApplicationUser> claimsFactory,
      IOptions<IdentityOptions> optionsAccessor,
      ILogger<SignInManager> logger,
      IAuthenticationSchemeProvider schemes,
      IUserConfirmation<TuckshopApplicationUser> confirmation,
      ISignInAuditService signInAuditService,
      IMfaManager mfaManager,
      IIdentityProviderLookupService identityProviderService)
      : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
    {
      this.logger = logger;
      this.schemes = schemes;
      this.signInAuditService = signInAuditService;
      this.mfaManager = mfaManager;
      this.identityProviderService = identityProviderService;
    }

    /// <summary>
    /// Gets the signed in user (note, the user may still need to complete MFA before they can sign in).
    /// </summary>
    public TuckshopApplicationUser? SignedInUser { get; private set; }

    /// <inheritdoc/>
    public override Task<bool> CanSignInAsync(TuckshopApplicationUser user)
    {
      if (!user.IsActive)
      {
        return Task.FromResult(false);
      }
      return base.CanSignInAsync(user);
    }

    /// <inheritdoc/>
    public override async Task<SignInResult> PasswordSignInAsync(string userName, string password, bool isPersistent, bool lockoutOnFailure)
    {
      var user = await this.UserManager.FindByEmailAsync(userName);

      var identityProvider = (await this.identityProviderService.GetIdentityProvidersAsync((int)IdentityProviderType.LoginCredentials)).First();

      SignInCriteria signInCriteria = this.InitializeSignInAuditCriteria(userName, password, isPersistent, lockoutOnFailure, user, identityProvider.IdentityProviderId);
      List<ISignInRule> signInRules = this.GetSignInRules();

      var (result, signInAudit) = await this.ExecuteSignInRulesAsync(signInCriteria, signInRules);

      if (result.Succeeded || result.RequiresTwoFactor || signInAudit is null)
      {
        signInAudit = SignInAudit.SuccessSignInAudit(
          user?.UserIdentifier ?? "Unknown",
          identityProvider.IdentityProviderId,
          (result.RequiresTwoFactor ? SignInReason.SucessfullSignInRequiresMFA : SignInReason.SucessfullSignIn).Description(), signInCriteria.RequestHeaderDetails!);
        this.logger.LogInformation(SignInReason.SucessfullSignIn.Description());

        this.SignedInUser = user;
      }

      this.signInAuditService.LogSignIn(signInAudit);

      return result;
    }

    /// <summary>
    /// Gets a collection of <see cref="AuthenticationScheme"/>s for the known external login providers.
    /// </summary>
    /// <returns>A collection of <see cref="AuthenticationScheme"/>s for the known external login providers.</returns>
    public override async Task<IEnumerable<AuthenticationScheme>> GetExternalAuthenticationSchemesAsync()
    {
      var schemes = await this.schemes.GetAllSchemesAsync();
      return schemes.Where(authenticationScheme => !string.IsNullOrEmpty(authenticationScheme.DisplayName));
    }

    /// <inheritdoc/>
    public override async Task<SignInResult> TwoFactorAuthenticatorSignInAsync(string code, bool isPersistent, bool rememberClient)
    {
      var result = await base.TwoFactorAuthenticatorSignInAsync(code, isPersistent, rememberClient);

      var user = await this.GetTwoFactorAuthenticationUserAsync() ??
        throw new System.InvalidOperationException("GetTwoFactorAuthenticationUserAsync() returned null");

      if (result.Succeeded)
      {
        this.signInAuditService.LogSignIn(SignInAudit.SuccessSignInAudit(user.UserIdentifier, user.IdentityProviderId, SignInReason.SucessfullSignInWithMFA.Description(), RequestHeaderDetails.Create(this.Context)));
      }
      else
      {
        this.signInAuditService.LogSignIn(SignInAudit.FailedSignInAudit(user.UserIdentifier, user.IdentityProviderId, SignInReason.FailedMFAAttempt.Description(), RequestHeaderDetails.Create(this.Context)));
      }

      return result;
    }

    /// <summary>
    /// Gets a value indicating whether the user must configure MFA.
    /// </summary>
    /// <param name="result">The sign in result.</param>
    /// <param name="user">The signed in user.</param>
    /// <returns>A value indicating. </returns>
    public bool MustConfigureMFA(SignInResult result, TuckshopApplicationUser user)
    {
      return this.mfaManager.SignInRequiresMFA(result, user);
    }

    private async Task<(SignInResult signInResult, SignInAudit? signInAudit)> ExecuteSignInRulesAsync(SignInCriteria signInCriteria, List<ISignInRule> signInRules)
    {
      foreach (var signInRule in signInRules)
      {
        var result = await signInRule.CanSignInAsync(signInCriteria);
        if (result != SignInResult.Success)
        {
          var signInAudit = signInRule.SignInAudit;
          this.logger.LogInformation(signInRule.Reason);
          return (result, signInAudit);
        }
      }

      return (SignInResult.Success, null);
    }

    private List<ISignInRule> GetSignInRules()
    {
      List<ISignInRule> signInRules = new List<ISignInRule>()
      {
        new UnknownUserRule(),
        new BlockedUserRule(),
        new ValidPasswordRule<TuckshopApplicationUser>(this),
      };
      return signInRules;
    }

    private SignInCriteria InitializeSignInAuditCriteria(string userName, string password, bool isPersistent, bool lockoutOnFailure, TuckshopApplicationUser? user, int identityProviderId)
    {
      var requestHeaderDetails = RequestHeaderDetails.Create(this.Context);

      return new SignInCriteria(user, userName, password, requestHeaderDetails, isPersistent, lockoutOnFailure, identityProviderId);
    }
  }
}
