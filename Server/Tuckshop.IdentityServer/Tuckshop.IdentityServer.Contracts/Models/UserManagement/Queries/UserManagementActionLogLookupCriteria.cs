namespace Tuckshop.IdentityServer.Contracts.UserManagement.Queries
{
  /// <summary>
  /// Criteria for the user management action log lookup.
  /// </summary>
  public class UserManagementActionLogLookupCriteria
  {
    /// <summary>
    /// Gets or sets the user id.
    /// </summary>
    public string UserId { get; set; } = string.Empty;
  }
}
