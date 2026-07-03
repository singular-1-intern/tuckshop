namespace Tuckshop.IdentityServer.Services
{
  using System.Threading.Tasks;
  using Neo.Identity;
  using Tuckshop.IdentityServer.Contracts.UserManagement.Queries;

  /// <summary>
  /// Identity server api.
  /// </summary>
  public interface IIdentityServerService
  {
    /// <summary>
    /// If the user is linked to a user invite, it will be returned.
    /// </summary>
    /// <param name="user">User.</param>
    /// <returns>User invite or null.</returns>
    Task<UserInviteLookup?> FindInvitedUserAsync(IClientUser user);
  }
}