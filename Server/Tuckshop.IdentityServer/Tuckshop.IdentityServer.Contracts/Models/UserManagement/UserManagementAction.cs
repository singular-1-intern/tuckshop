#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Tuckshop.IdentityServer.Contracts.UserManagement
{
  using System.ComponentModel.DataAnnotations;
  using Neo.Model.DataAnnotations;

  /// <summary>
  /// Enum representing the types of user management actions
  /// </summary>
  [DbEnum]
  public enum UserManagementAction
  {
    [Display(Description = "Resend the email verification link for {User}")]
    ResendEmailVerificationLink,

    [Display(Description = "Reset MFA for {User}")]
    ResetMFA,

    [Display(Description = "Clear lockout for {User}")]
    ClearLockout,

    [Display(Description = "Activate {User}")]
    Activate,

    [Display(Description = "Deactivate {User}")]
    Deactivate,

    [Display(Description = "Enable MFA for {User}")]
    EnableMFA,

    [Display(Description = "Disable MFA for {User}")]
    DisableMFA,
  }
}
