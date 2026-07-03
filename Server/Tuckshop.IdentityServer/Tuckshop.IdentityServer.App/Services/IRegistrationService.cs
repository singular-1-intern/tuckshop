namespace Tuckshop.IdentityServer.App.Services
{
  using System.Threading.Tasks;
  using Tuckshop.IdentityServer.Contracts.Registration;

  /// <summary>
  /// An interface for registering user registrations.
  /// </summary>
  public interface IRegistrationService
  {
    /// <summary>
    /// Will try to register the user.
    /// </summary>
    /// <param name="user">The register user info.</param>
    /// <param name="identityProviderId">The identity provider id.</param>
    /// <returns>A register result.</returns>
    Task<RegisterResult> RegisterUserAsync(IRegistrationUser user, int identityProviderId);
  }
}
