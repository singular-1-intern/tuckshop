namespace Tuckshop.IdentityServer.Contracts.UserManagement.Queries
{
  /// <summary>
  /// The user invite criteria class.
  /// </summary>
  public class UserInviteCriteria
  {
    /// <summary>
    /// Gets or sets a value indicating whether user invites where the user has registered should be included.
    /// </summary>
    public bool IncludeRegistered { get; set; }
  }
}