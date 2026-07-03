namespace Tuckshop.IdentityServer.Contracts.UserManagement.Queries
{
  using System;
  using Newtonsoft.Json;

  /// <summary>
  /// The user invite lookup class.
  /// </summary>
  public class UserInviteLookup
  {
    /// <summary>
    /// Gets or sets the Id of the user invite.
    /// </summary>
    public int UserInviteId { get; set; }

    /// <summary>
    /// Gets or sets the email address the new user must use when registering.
    /// </summary>
    public string EmailAddress { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the id of the user group the user must be added to in authorisation server.
    /// </summary>
    public int? AddToGroupId { get; set; }

    /// <summary>
    /// Gets or sets the user identifier.
    /// </summary>
    [JsonIgnore]
    public string? UserIdentifier { get; set; }

    /// <summary>
    /// Gets or sets the date and time the user invite was created.
    /// </summary>
    public DateTime CreatedOn { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this user has registered.
    /// </summary>
    public bool HasRegistered => this.UserIdentifier != null;
  }
}