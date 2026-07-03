namespace Tuckshop.AuthorisationServer.Models
{
  using System;
  using System.ComponentModel.DataAnnotations.Schema;
  using Neo.AuthorisationServer.Models;

  /// <summary>
  /// The TuckshopAuthorisationUser class.
  /// </summary>
  [Table("Users")]
  [Serializable]
  public class TuckshopAuthorisationUser : AuthorisationUserBase<TuckshopAuthorisationUser>
  {
    /// <summary>
    /// Gets or sets a value indicating whether this user is linked to a user invite.
    /// </summary>
    public bool IsInvitedUser { get; set; }
  }
}