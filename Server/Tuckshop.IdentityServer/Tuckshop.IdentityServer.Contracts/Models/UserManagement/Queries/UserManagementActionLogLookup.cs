namespace Tuckshop.IdentityServer.Contracts.UserManagement.Queries
{
  using System;
  using Newtonsoft.Json;

  /// <summary>
  /// User management log lookup.
  /// </summary>
  public class UserManagementActionLogLookup
  {
    /// <summary>
    /// Gets or sets the action string.
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the actioned by First Name.
    /// </summary>
    [JsonIgnore]
    public string ActionedByFirstName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the actioned by First Last Name.
    /// </summary>
    [JsonIgnore]
    public string ActionedByLastName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or actioned by full name.
    /// </summary>
    public string ActionedBy => $"{this.ActionedByFirstName} {this.ActionedByLastName}";

    /// <summary>
    /// Gets or sets the actioned on date and time.
    /// </summary>
    public DateTime ActionedOn { get; set; }
  }
}
