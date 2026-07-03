namespace Tuckshop.IdentityServer.App.Services.UserManagement
{
  using System;
  using System.Threading.Tasks;
  using Neo.Model.Services;
  using Tuckshop.IdentityServer.Contracts.UserManagement.Commands;
  using Tuckshop.IdentityServer.Contracts.UserManagement.Queries;
  using Tuckshop.IdentityServer.Models.UserManagement;

  /// <summary>
  /// Interface for a user management service.
  /// </summary>
  public interface IUserManagementService
  {
    /// <summary>
    /// Finds users.
    /// </summary>
    /// <param name="request">The user lookup request.</param>
    /// <returns>A list of users.</returns>
    Task<PageResult<UserLookup>> FindUsersAsync(PageRequest<UserLookupCriteria> request);

    /// <summary>
    /// Performs a user action on a user.
    /// </summary>
    /// <param name="command">The command containing the action details.</param>
    /// <returns>A task awaiting the action completion.</returns>
    Task PerformUserActionAsync(PerformUserActionCommand command);

    /// <summary>
    /// Gets the log history matching the criteria.
    /// </summary>
    /// <param name="request">The page request.</param>
    /// <returns>A page result of action logs.</returns>
    Task<PageResult<UserManagementActionLogLookup>> UserManagementActionLogHistoryAsync(PageRequest<UserManagementActionLogLookupCriteria> request);

    /// <summary>
    /// Gets a list of user invites.
    /// </summary>
    /// <param name="pageRequest">The user invite criteria.</param>
    /// <returns>The list of user invites.</returns>
    Task<PageResult<UserInviteLookup>> GetUserInvitesAsync(PageRequest<UserInviteCriteria> pageRequest);

    /// <summary>
    /// Uses the tracking state to save the user invite.
    /// </summary>
    /// <param name="userInvite">User invite.</param>
    /// <returns>Lookup with id.</returns>
    Task<UserInviteLookup> SaveUserInviteAsync(UserInvite userInvite);

    /// <summary>
    /// Revokes a user invite if the user has not already registered.
    /// </summary>
    /// <param name="userInviteId">User invite id.</param>
    /// <returns>Task.</returns>
    Task RevokeUserInviteAsync(int userInviteId);

    /// <summary>
    /// If the user that matches the provided identifier is linked to a user invite, it will be returned.
    /// </summary>
    /// <param name="userIdentifier">User identifier.</param>
    /// <returns>User invite or null.</returns>
    Task<UserInviteLookup?> FindInvitedUserAsync(Guid userIdentifier);
  }
}