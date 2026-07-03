namespace Tuckshop.IdentityServer.App.Services
{
  using Microsoft.AspNetCore.Identity;
  using Tuckshop.IdentityServer.Contracts.Registration;
  using Tuckshop.IdentityServer.Contracts.UserManagement.Queries;
  using Tuckshop.IdentityServer.Models;

  /// <summary>
  /// The MFA Manager.
  /// </summary>
  public class MfaManager : IMfaManager
  {
    /// <inheritdoc/>
    public bool SignInRequiresMFA(SignInResult result, TuckshopApplicationUser user)
    {
      // TODO: Change this if you wish to customize which users require MFA on Sign In (i.e. if MFA gets enabled post user creation).
      return user.TwoFactorEnabled;
    }

    /// <inheritdoc/>
    public bool NewUserRequiresTwoFactor(IRegistrationUser user, int identityProviderId)
    {
      // TODO: Change this if you wish to customize which users require MFA
      return true;
    }

    /// <inheritdoc/>
    public bool UserRequiresTwoFactor(UserLookup user)
    {
      // TODO: Change this if you wish to customize which users require MFA
      return true;
    }
  }
}
