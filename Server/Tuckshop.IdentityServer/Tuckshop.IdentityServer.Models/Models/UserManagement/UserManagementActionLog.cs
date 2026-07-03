namespace Tuckshop.IdentityServer.Models.UserManagement
{
  using System;
  using System.ComponentModel.DataAnnotations;
  using System.ComponentModel.DataAnnotations.Schema;
  using Neo.Model;
  using Neo.Model.AuditTrail;
  using Tuckshop.IdentityServer.Contracts.UserManagement;

  /// <summary>
  /// Class representing a user management action log.
  /// </summary>
  public class UserManagementActionLog : ModelBase<UserManagementActionLog>, IAuditTrailCreatedEntity
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="UserManagementActionLog"/> class.
    /// </summary>
    /// <param name="action">The action to be performed.</param>
    /// <param name="userId">The user id this log applies to.</param>
    public UserManagementActionLog(UserManagementAction action, string userId)
    {
      this.Action = action;
      this.UserId = userId;
    }

    /// <summary>
    /// Gets or sets the id.
    /// </summary>
    public int UserManagementActionLogId { get; set; }

    /// <summary>
    /// Gets or sets the user id this log applies to;.
    /// </summary>
    [StringLength(450)]
    public string UserId { get; set; }

    /// <summary>
    /// Gets or sets the user this log applies to;.
    /// </summary>
    public TuckshopApplicationUser? User { get; set; }

    /// <summary>
    /// Gets or sets the user management action.
    /// </summary>
    public UserManagementAction Action { get; set; }

    /// <inheritdoc />
    public int CreatedBy { get; set; }

    /// <inheritdoc />
    [Column(TypeName = "datetime")]
    public DateTime CreatedOn { get; set; }
  }
}
