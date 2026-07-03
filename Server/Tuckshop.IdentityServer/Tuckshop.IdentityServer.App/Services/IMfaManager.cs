namespace Tuckshop.IdentityServer.App.Services
{
  using Microsoft.AspNetCore.Identity;
  using Tuckshop.IdentityServer.Contracts.Registration;
  using Tuckshop.IdentityServer.Contracts.UserManagement.Queries;
  using Tuckshop.IdentityServer.Models;

  /// <summary>
  /// Interface for the MFA Manager.
  /// </summary>
  public interface IMfaManager
  {
    /// <summary>
    /// Returns a value indicating whether the current sign in requires MFA.
    /// </summary>
    /// <param name="result">The result.</param>
    /// <param name="user">The user.</param>
    /// <returns>A value indicating whether the current sign in requires MFA.</returns>
    bool SignInRequiresMFA(SignInResult result, TuckshopApplicationUser user);

    /// <summary>
    /// Returns a value indicating whether the account requires MFA.
    /// </summary>
    /// <param name="user">The user account.</param>
    /// <param name="identityProviderId">The identity provider id.</param>
    /// <returns>A value indicating whether the user requires MFA.</returns>
    bool NewUserRequiresTwoFactor(IRegistrationUser user, int identityProviderId);

    /// <summary>
    /// Returns a value indicating whether the account requires MFA.
    /// </summary>
    /// <param name="user">The user lookup.</param>
    /// <returns>A value indicating whether the user requires MFA.</returns>
    bool UserRequiresTwoFactor(UserLookup user);
  }
}
