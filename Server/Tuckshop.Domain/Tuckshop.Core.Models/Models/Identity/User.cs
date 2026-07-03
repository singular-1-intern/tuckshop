namespace Tuckshop.Core.Models.Identity
{
  using System;
  using System.ComponentModel.DataAnnotations;
  using System.ComponentModel.DataAnnotations.Schema;
  using Neo.Identity;
  using Neo.Model;
  using Neo.Model.AuditTrail;

  /// <summary>
  /// Represents a client user of the service.
  /// </summary>
  [Serializable] // this class MUST be serializable because users get cached
  public class User : ModelBase<User>, IAuditTrailModifiedOnEntity, INamedUser
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="User"/> class.
    /// </summary>
    public User()
    {
    }

    /// <inheritdoc />
    public virtual int UserId { get; set; }

    /// <inheritdoc />
    public virtual Guid? IdentityGuid { get; set; }

    /// <inheritdoc/>
    public virtual string UserIdentifier
    {
      get
      {
        if (this.IdentityGuid != null && this.IdentityGuid.HasValue)
        {
          return this.IdentityGuid.ToString() ?? string.Empty;
        }
        else
        {
          return this.ClientId;
        }
      }
    }

    /// <inheritdoc />
    [StringLength(30)]
    public string ClientId { get; set; } = string.Empty;

    /// <inheritdoc />
    [Required]
    [StringLength(100)]
    public string? FirstName { get; set; }

    /// <inheritdoc />
    [Required]
    [StringLength(100)]
    public string? LastName { get; set; }

    /// <summary>
    /// Gets or sets the UserName.
    /// </summary>
    [Required]
    [StringLength(1000)]
    public string UserName { get; set; } = string.Empty;

    /// <inheritdoc />
    [Column(TypeName = "datetime")]
    public DateTime ModifiedOn { get; set; }
  }
}
