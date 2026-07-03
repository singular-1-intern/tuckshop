namespace Tuckshop.IdentityServer.Contracts.UserManagement.Queries
{
  using System;
  using System.Linq;
  using System.Linq.Expressions;

  /// <summary>
  /// User lookup criteria.
  /// </summary>
  public class UserLookupCriteria
  {
    /// <summary>
    /// Gets or sets the exact user id to match on.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the First Name starts with filter.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Last Name starts with filter.
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the User Name starts with filter.
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether only active or inactive users should be returned. Null = all;.
    /// </summary>
    public bool? IsActive { get; set; } = true;

    /// <summary>
    /// Db Filter expression to be used in initial Db call.
    /// Be careful of encrypted fields, these must be:
    ///  - Exact match on deterministic, or
    ///  - Filtered after loading with the Final filter method below.
    /// </summary>
    /// <param name="excludeUserIds">The users ids to exclude.</param>
    /// <returns>A Db filter expression.</returns>
    public Expression<Func<UserLookup, bool>> DbFilter(string[] excludeUserIds)
    {
      return user => (string.IsNullOrEmpty(this.UserId) || user.Id == this.UserId) &&
        (this.IsActive == null || user.IsActive == this.IsActive) &&
        !excludeUserIds.Contains(user.Id);
    }

    /// <summary>
    /// Final filter to be run after the data is loaded from the Db.
    /// </summary>
    /// <returns>The final filter function.</returns>
    public Func<UserLookup, bool> FinalFilter()
    {
      return user => user.FirstName.StartsWith(this.FirstName, StringComparison.CurrentCultureIgnoreCase)
        && user.LastName.StartsWith(this.LastName.ToUpperInvariant(), StringComparison.CurrentCultureIgnoreCase)
        && user.UserName.Contains(this.UserName.ToUpperInvariant(), StringComparison.CurrentCultureIgnoreCase);
    }
  }
}
