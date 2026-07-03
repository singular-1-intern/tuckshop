namespace Tuckshop.IdentityServer.App.Services.UserManagement
{
  using Tuckshop.IdentityServer.Contracts.UserManagement;
  using Tuckshop.IdentityServer.Models.Security;

  /// <summary>
  /// Extensions for user management actions.
  /// </summary>
  public static class UserManagementActionExtensions
  {
    /// <summary>
    /// Gets the role required for a user to perform the specified action.
    /// </summary>
    /// <param name="action">The user management action.</param>
    /// <returns>The role required for a user to perform the specified action.</returns>
    public static Roles.UserManagement? GetRole(this UserManagementAction action)
    {
      switch (action)
      {
        case UserManagementAction.ResendEmailVerificationLink:
          return Roles.UserManagement.EmailVerificationLinkResend;
        case UserManagementAction.EnableMFA:
          return Roles.UserManagement.EnableMFA;
        case UserManagementAction.DisableMFA:
          return Roles.UserManagement.DisableMFA;
        case UserManagementAction.ResetMFA:
          return Roles.UserManagement.ResetMFA;
        case UserManagementAction.ClearLockout:
          return Roles.UserManagement.ClearLockout;
        case UserManagementAction.Activate:
          return Roles.UserManagement.ActivateUser;
        case UserManagementAction.Deactivate:
          return Roles.UserManagement.DeactivateUser;
      }
      return null;
    }
  }
}
