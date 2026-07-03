namespace Tuckshop.IdentityServer.Controllers.Custom
{
  using System;
  using System.Threading.Tasks;
  using Microsoft.AspNetCore.Authorization;
  using Microsoft.AspNetCore.Mvc;
  using Neo.Identity.Security;
  using Neo.Model.DataAnnotations;
  using Neo.Model.Services;
  using Tuckshop.IdentityServer.App.Services.UserManagement;
  using Tuckshop.IdentityServer.Contracts.UserManagement.Commands;
  using Tuckshop.IdentityServer.Contracts.UserManagement.Queries;
  using Tuckshop.IdentityServer.Models.Security;
  using Tuckshop.IdentityServer.Models.UserManagement;
  using static Neo.IdentityServer.Core.OpenIddict.NeoOpenIddictConstants;

  /// <summary>
  /// Controller for performing user management functions.
  /// </summary>
  [Route("api/user-management")]
  [Authorize(LocalApi.PolicyName)]
  public class UserManagementController : ControllerBase
  {
    private readonly IUserManagementService userManagementService;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserManagementController"/> class.
    /// </summary>
    /// <param name="userManagementService">The user management service.</param>
    public UserManagementController(IUserManagementService userManagementService)
    {
      this.userManagementService = userManagementService;
    }

    /// <summary>
    /// Finds users.
    /// </summary>
    /// <param name="request">The user lookup request.</param>
    /// <returns>A list of users.</returns>
    [HttpPost("find")]
    public Task<PageResult<UserLookup>> FindUsers([FromBody] PageRequest<UserLookupCriteria> request)
    {
      return this.userManagementService.FindUsersAsync(request);
    }

    /// <summary>
    /// Performs a user action on a user.
    /// </summary>
    /// <param name="command">The command containing the action details.</param>
    /// <returns>A task awaiting the action completion.</returns>
    [HttpPost("perform-action")]
    public Task PerformAction([FromBody] PerformUserActionCommand command)
    {
      return this.userManagementService.PerformUserActionAsync(command);
    }

    /// <summary>
    /// Gets the log history matching the criteria.
    /// </summary>
    /// <param name="request">The page request.</param>
    /// <returns>A page result of action logs.</returns>
    [HttpGet("log-history")]
    public Task<PageResult<UserManagementActionLogLookup>> LogHistory([FromQuery] PageRequest<UserManagementActionLogLookupCriteria> request)
    {
      return this.userManagementService.UserManagementActionLogHistoryAsync(request);
    }

    /// <summary>
    /// Gets a list of user invites.
    /// </summary>
    /// <param name="pageRequest">The user invite criteria.</param>
    /// <returns>The list of user invites.</returns>
    [HttpGet("user-invites")]
    [RequireRole(Roles.UserManagement.InviteUser)]
    public Task<PageResult<UserInviteLookup>> GetUserInvites([FromQuery] PageRequest<UserInviteCriteria> pageRequest)
    {
      return this.userManagementService.GetUserInvitesAsync(pageRequest);
    }

    /// <summary>
    /// Creates an invited user.
    /// </summary>
    /// <param name="invitedUser">Invited user.</param>
    /// <returns>Lookup with id.</returns>
    [HttpPost("user-invite")]
    [RequireRole(Roles.UserManagement.InviteUser)]
    public Task<UserInviteLookup> SaveUserInvite([FromBody] UserInvite invitedUser)
    {
      return this.userManagementService.SaveUserInviteAsync(invitedUser);
    }

    /// <summary>
    /// Revokes a user invite if the user has not already registered.
    /// </summary>
    /// <param name="userInviteId">User invite id.</param>
    /// <returns>Task.</returns>
    [HttpDelete("user-invite/{userInviteId}")]
    public Task RevokeUserInvite([FromRoute] int userInviteId)
    {
      return this.userManagementService.RevokeUserInviteAsync(userInviteId);
    }

    /// <summary>
    /// If the user that matches the provided identifier is an admin user, it will be returned.
    /// </summary>
    /// <param name="userIdentifier">User identifier.</param>
    /// <returns>Admin user or null.</returns>
    [HttpGet("find-invited-user/{userIdentifier}")]
    [Authorize(Policy = Policies.IsService)]
    public Task<UserInviteLookup?> FindInvitedUser([FromRoute] Guid userIdentifier)
    {
      return this.userManagementService.FindInvitedUserAsync(userIdentifier);
    }
  }
}