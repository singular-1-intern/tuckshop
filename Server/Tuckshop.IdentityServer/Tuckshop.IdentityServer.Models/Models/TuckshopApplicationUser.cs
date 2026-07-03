namespace Tuckshop.IdentityServer.Models
{
  using System;
  using System.ComponentModel.DataAnnotations;
  using System.Data;
  using Microsoft.AspNetCore.Mvc;
  using Neo.IdentityServer.Models;
  using Neo.IdentityServer.Models.IdentityProviders;
  using Neo.Model.DataAnnotations;
  using Neo.Model.SqlServer.AlwaysEncrypted;
  using Tuckshop.IdentityServer.Contracts.UserManagement.Queries;
  using Tuckshop.IdentityServer.Models.UserManagement;

  /// <summary>
  /// Overrides the default ApplicationUser from Neo.IdentityServer to provide an IsActive flag.
  /// </summary>
  [ModelMetadataType(type: typeof(TuckshopApplicationUserMetadata))]
  [Serializable]
  public class TuckshopApplicationUser : ApplicationUser, IApplicationUser, IIdentityProviderUser
  {
    /// <inheritdoc />
    public bool IsActive { get; set; } = true;

    /// <inheritdoc />
    public int UserId { get; private set; }

    /// <inheritdoc />
    public Guid? IdentityGuid => new Guid(this.Id);

    /// <summary>
    /// Gets a value indicating whether 2 factor has been configured.
    /// </summary>
    public bool TwoFactorConfigured { get; private set; }

    /// <summary>
    /// Gets or sets a value indicating whether a logout should be forced for the user (I.e. Do not refresh the user's access token).
    /// </summary>
    public bool ForceLogout { get; set; }

    /// <inheritdoc />
    [StringLength(30)]
    [EFIgnore]
    public string ClientId { get; set; } = string.Empty;

    /// <inheritdoc />
    public int IdentityProviderId { get; set; }

    /// <summary>
    /// Gets or sets the identity provider id.
    /// </summary>
    public IdentityProvider? IdentityProvider { get; set; }

    /// <summary>
    /// Gets or sets the id of the user invite matched to this email address.
    /// </summary>
    public int? UserInviteId { get; set; }

    /// <summary>
    /// Gets or sets the user invite navigation.
    /// </summary>
    public UserInvite? UserInvite { get; set; }

    /// <summary>
    /// Will create the system user with the provided Id.
    /// </summary>
    /// <param name="id">The system user id.</param>
    /// <returns>A new System User entity.</returns>
    public static TuckshopApplicationUser SystemUser(Guid id)
    {
      return new TuckshopApplicationUser()
      {
        Id = id.ToString(),
        FirstName = "System",
        LastName = "User",
        UserName = "system-user",
        IsActive = false,
        IdentityProviderId = (int)IdentityProviderType.LoginCredentials,
      };
    }

    /// <summary>
    /// Will set the TwoFactorConfigured property.
    /// </summary>
    /// <param name="twoFactorConfigured">Value indicating whether Two Factor is configured.</param>
    public void ConfigureTwoFactor(bool twoFactorConfigured)
    {
      this.TwoFactorConfigured = twoFactorConfigured;
    }

    /// <summary>
    /// Will enable the user's MFA.
    /// </summary>
    public void EnableMFA()
    {
      this.TwoFactorEnabled = true;
    }

    /// <summary>
    /// Will disable the user's MFA.
    /// </summary>
    public void DisableMFA()
    {
      this.TwoFactorEnabled = false;
      this.TwoFactorConfigured = false;
    }

    /// <summary>
    /// Will reset the user's MFA.
    /// </summary>
    public void ResetMFA()
    {
      this.TwoFactorConfigured = false;
      this.LockoutEnd = null;
    }

    /// <summary>
    /// Will clear the user's lockout.
    /// </summary>
    public void ClearLockout()
    {
      this.LockoutEnd = null;
    }

    /// <summary>
    /// Will set the fields required to block the user and force their logout.
    /// </summary>
    public void Deactivate()
    {
      this.IsActive = false;
      this.ForceLogout = true;
    }

    /// <summary>
    /// Will set the fields required to unblock the user.
    /// </summary>
    public void Activate()
    {
      this.IsActive = true;
      this.ForceLogout = false;
    }

    /// <summary>
    /// Gets a UserLookup model for this user.
    /// </summary>
    /// <returns>A UserLookup model.</returns>
    public UserLookup ToLookup()
    {
      var lookup = new UserLookup()
      {
        Id = this.Id,
        FirstName = this.FirstName,
        LastName = this.LastName,
        UserName = this.UserName ?? string.Empty,
        IsActive = this.IsActive,
        EmailConfirmed = this.EmailConfirmed,
        TwoFactorEnabled = this.TwoFactorEnabled,
        TwoFactorConfigured = this.TwoFactorConfigured,
        LockoutEnd = this.LockoutEnd?.DateTime,
        IdentityProvider = this.IdentityProvider?.Name ?? string.Empty,
        IsExternalIdentityProvider = this.IdentityProvider != null && this.IdentityProvider.IsExternalProvider,
      };

      return lookup;
    }
  }

  /// <summary>
  /// The Application User Metadata Class.
  /// </summary>
  public class TuckshopApplicationUserMetadata
  {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    /// <summary>
    /// Gets or sets the Email.
    /// </summary>
    [MaxLength(256)]
    [EncryptedColumn(SqlDbType.NVarChar)]
    public string Email { get; set; }

    /// <summary>
    /// Gets or sets the First Name.
    /// </summary>
    [EncryptedColumn(SqlDbType.NVarChar)]
    public string FirstName { get; set; }

    /// <summary>
    /// Gets or sets the Last Name of the User.
    /// </summary>
    [EncryptedColumn(SqlDbType.NVarChar)]
    public string LastName { get; set; }

    /// <summary>
    /// Gets or sets the Normalized Email.
    /// </summary>
    [MaxLength(256)]
    [EncryptedColumn(SqlDbType.NVarChar, EncryptionType = EncryptionType.Deterministic, Collation = "Latin1_General_BIN2")]
    public string NormalizedEmail { get; set; }

    /// <summary>
    /// Gets or sets the Normalized UserName.
    /// </summary>
    [MaxLength(256)]
    [EncryptedColumn(SqlDbType.NVarChar, EncryptionType = EncryptionType.Deterministic, Collation = "Latin1_General_BIN2")]
    public string NormalizedUserName { get; set; }

    /// <summary>
    /// Gets or sets the Phone Number.
    /// </summary>
    [EncryptedColumn(SqlDbType.NVarChar)]
    public string PhoneNumber { get; set; }

    /// <summary>
    /// Gets or sets the UserName.
    /// </summary>
    [MaxLength(256)]
    [EncryptedColumn(SqlDbType.NVarChar)]
    public string UserName { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
  }
}
