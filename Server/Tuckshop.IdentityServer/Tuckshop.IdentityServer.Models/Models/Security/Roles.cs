namespace Tuckshop.IdentityServer.Models.Security
{
  using System.ComponentModel.DataAnnotations;
  using Neo.AuthorisationServer.Client;

  public class Roles : IRoles
  {
    /// <inheritdoc />
    public string ResourceName => "Identity";

    /// <inheritdoc />
    public string DisplayName => "Security";

    /// <summary>
    /// The user management roles.
    /// </summary>
    public enum UserManagement
    {
      DeactivateUser,
      ActivateUser,
      Access,
      EmailVerificationLinkResend,
      [Display(Name = "Enable MFA")]
      EnableMFA,
      [Display(Name = "Disable MFA")]
      DisableMFA,
      [Display(Name = "Reset MFA")]
      ResetMFA,
      ClearLockout,
      InviteUser
    }

    /// <summary>
    /// The identity provider roles.
    /// </summary>
    public enum IdentityProviders
    {
      Setup,
    }
  }
}
