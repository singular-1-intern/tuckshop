namespace Tuckshop.IdentityServer.App.Services
{
  using System.Threading.Tasks;
  using Tuckshop.IdentityServer.Models;
  using Tuckshop.IdentityServer.Models.UserManagement;

  /// <summary>
  /// An interface for sending email verifications.
  /// </summary>
  public interface IRegistrationEmailService
  {
    /// <summary>
    /// Sends a verification notification to the user
    /// </summary>
    /// <param name="user">User</param>
    /// <returns>A <see cref="Task"/></returns>
    Task SendVerificationEmailAsync(TuckshopApplicationUser user);

    /// <summary>
    /// Sends an invite email.
    /// </summary>
    /// <param name="userInvite">User invite.</param>
    /// <returns>Task.</returns>
    Task SendUserInviteEmailAsync(UserInvite userInvite);
  }
}
