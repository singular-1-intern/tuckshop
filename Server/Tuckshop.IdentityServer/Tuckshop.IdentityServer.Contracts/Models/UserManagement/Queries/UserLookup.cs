namespace Tuckshop.IdentityServer.Contracts.UserManagement.Queries
{
  using System;

  /// <summary>
  /// User lookup.
  /// </summary>
  public class UserLookup
  {
    /// <summary>
    /// Gets or sets the user id.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the First Name starts.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Last Name starts.
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the User Name.
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the user Is Active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user has a Email is Confirmed.
    /// </summary>
    public bool EmailConfirmed { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user Two Factor is Enabled.
    /// </summary>
    public bool TwoFactorEnabled { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user Two Factor is Configured.
    /// </summary>
    public bool TwoFactorConfigured { get; set; }

    /// <summary>
    /// Gets or sets the users lockout end date.
    /// </summary>
    public DateTime? LockoutEnd { get; set; }

    /// <summary>
    /// Gets or sets the Identity Provider name.
    /// </summary>
    public string IdentityProvider { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the identity provider is external.
    /// </summary>
    public bool IsExternalIdentityProvider { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the tenant identity provider requires MFA.
    /// </summary>
    public bool ProviderRequiresMFA { get; set; }
  }
}
