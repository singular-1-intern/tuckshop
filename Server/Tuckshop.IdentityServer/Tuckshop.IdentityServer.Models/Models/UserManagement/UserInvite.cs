namespace Tuckshop.IdentityServer.Models.UserManagement
{
  using System;
  using System.ComponentModel.DataAnnotations;
  using System.ComponentModel.DataAnnotations.Schema;
  using Microsoft.EntityFrameworkCore;
  using Neo.Model;
  using Neo.Model.AuditTrail;
  using Tuckshop.IdentityServer.Contracts.UserManagement.Queries;

  [Index(nameof(EmailAddress), IsUnique = true)]
  public class UserInvite : ModelBase<UserInvite>, IAuditTrailCreatedEntity
  {
    public int UserInviteId { get; set; }

    /// <summary>
    /// Gets or sets the email address the new user must use when registering.
    /// </summary>
    [Required]
    [StringLength(250)]
    [EmailAddress]
    public string EmailAddress { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the id of the user group the user must be added to in authorisation server.
    /// </summary>
    public int? AddToUserGroupId { get; set; }

    /// <inheritdoc />
    public int CreatedBy { get; set; }

    /// <inheritdoc />
    [Column(TypeName = "datetime")]
    public DateTime CreatedOn { get; set; }

    /// <summary>
    /// Converts the model to a lookup.
    /// </summary>
    /// <param name="user">User linked to the invite if any.</param>
    /// <returns>The user invite lookup.</returns>
    public UserInviteLookup ToLookup(TuckshopApplicationUser? user)
    {
      return new UserInviteLookup()
      {
        UserInviteId = this.UserInviteId,
        AddToGroupId = this.AddToUserGroupId,
        EmailAddress = this.EmailAddress,
        CreatedOn = this.CreatedOn,
        UserIdentifier = user?.Id,
      };
    }
  }
}