namespace Tuckshop.IdentityServer.App.Services.UserManagement
{
  using System;
  using System.Collections.Generic;
  using System.Globalization;
  using System.Linq;
  using System.Threading.Tasks;
  using Microsoft.AspNetCore.Identity;
  using Microsoft.EntityFrameworkCore;
  using Neo.AuthorisationServer.Client;
  using Neo.Extensions;
  using Neo.Model.Exceptions;
  using Neo.Model.Identity.SystemUser;
  using Neo.Model.Services;
  using Tuckshop.IdentityServer.Contracts.UserManagement;
  using Tuckshop.IdentityServer.Contracts.UserManagement.Commands;
  using Tuckshop.IdentityServer.Contracts.UserManagement.Queries;
  using Tuckshop.IdentityServer.Models;
  using Tuckshop.IdentityServer.Models.Security;
  using Tuckshop.IdentityServer.Models.UserManagement;

  /// <summary>
  /// User management service.
  /// </summary>
  public class UserManagementService : IUserManagementService
  {
    private readonly IdentityDbContext dbContext;
    private readonly IAuthorisationService authorisationService;
    private readonly IRegistrationEmailService registrationEmailService;
    private readonly IPageQueryService queryService;
    private readonly SystemUserOptions<TuckshopApplicationUser> systemUserOptions;
    private readonly IMfaManager mfaManager;
    private readonly UserManager<TuckshopApplicationUser> userManager;

    /// <summary>
    /// Generates an instance of a UserManagementService.
    /// </summary>
    /// <param name="dbContext">The db context.</param>
    /// <param name="authorisationService">The authorisation service.</param>
    /// <param name="registrationEmailService">The email sender for registration events.</param>
    /// <param name="queryService">The query service.</param>
    /// <param name="systemUserOptions">The system user options.</param>
    /// <param name="mfaManager">The Mfa Manager.</param>
    /// <param name="userManager">User manager.</param>
    public UserManagementService(
      IdentityDbContext dbContext,
      IAuthorisationService authorisationService,
      IRegistrationEmailService registrationEmailService,
      IPageQueryService queryService,
      SystemUserOptions<TuckshopApplicationUser> systemUserOptions,
      IMfaManager mfaManager,
      UserManager<TuckshopApplicationUser> userManager)
    {
      this.dbContext = dbContext;
      this.authorisationService = authorisationService;
      this.registrationEmailService = registrationEmailService;
      this.queryService = queryService;
      this.systemUserOptions = systemUserOptions;
      this.mfaManager = mfaManager;
      this.userManager = userManager;
    }

    /// <inheritdoc/>
    public async Task<PageResult<UserLookup>> FindUsersAsync(PageRequest<UserLookupCriteria> request)
    {
      await this.authorisationService.AssertUserHasRoleAsync(Roles.UserManagement.Access);

      var users = await (from user in this.dbContext.Users
                         join identityProvider in this.dbContext.IdentityProviders on user.IdentityProviderId equals identityProvider.IdentityProviderId
                         select new UserLookup()
                         {
                           Id = user.Id,
                           FirstName = user.FirstName,
                           LastName = user.LastName,
                           UserName = user.UserName!,
                           IsActive = user.IsActive,
                           EmailConfirmed = user.EmailConfirmed,
                           TwoFactorEnabled = user.TwoFactorEnabled,
                           TwoFactorConfigured = user.TwoFactorConfigured,
                           LockoutEnd = user.LockoutEnd.HasValue ? user.LockoutEnd.Value.DateTime : null,
                           IdentityProvider = identityProvider.DisplayName,
                           IsExternalIdentityProvider = identityProvider.IsExternalProvider,
                         })
                  .Where(request.Criteria.DbFilter(new[] { this.systemUserOptions.SystemUserGuid.ToString() })).ToListAsync();

      var query = users.Where(request.Criteria.FinalFilter()).AsQueryable();
      var pageResult = await this.queryService.EntitiesPaged(query, request);

      foreach (var user in pageResult.EntityList)
      {
        user.ProviderRequiresMFA = this.mfaManager.UserRequiresTwoFactor(user);
      }
      return pageResult;
    }

    /// <inheritdoc/>
    public async Task PerformUserActionAsync(PerformUserActionCommand command)
    {
      var requiredRole = command.Action.GetRole();

      if (requiredRole.HasValue)
      {
        await this.authorisationService.AssertUserHasRoleAsync(requiredRole.Value);
      }

      var user = (await this.FindUsersAsync(new PageRequest<UserLookupCriteria>(new UserLookupCriteria() { UserId = command.UserId, IsActive = null }))).EntityList.FirstOrDefault()
                    ?? throw new InvalidDomainOperationException(string.Format(CultureInfo.CurrentCulture, DomainExceptions.NoUserFoundForId, command.UserId));

      AssertCanPerformUserAction(user, command);

      await this.PerformUserActionAsync(user, command);
    }

    /// <inheritdoc/>
    public async Task<PageResult<UserManagementActionLogLookup>> UserManagementActionLogHistoryAsync(PageRequest<UserManagementActionLogLookupCriteria> request)
    {
      await this.authorisationService.AssertUserHasRoleAsync(Roles.UserManagement.Access);

      var query = from log in this.dbContext.UserManagementActionLogs
                  join user in this.dbContext.Users on log.CreatedBy equals user.UserId
                  where log.UserId == request.Criteria.UserId
                  select new UserManagementActionLogLookup()
                  {
                    Action = log.Action.Description(),
                    ActionedByFirstName = user.FirstName,
                    ActionedByLastName = user.LastName,
                    ActionedOn = log.CreatedOn,
                  };

      var result = await this.queryService.EntitiesPaged(query, request);
      return result;
    }

    private static void AssertCanPerformUserAction(UserLookup user, PerformUserActionCommand command)
    {
      switch (command.Action)
      {
        case UserManagementAction.ResendEmailVerificationLink:
          AssertUser(() => !user.IsExternalIdentityProvider, DomainExceptions.CannotPerformActionForExternalLoginUsers);
          AssertUser(() => !user.EmailConfirmed && user.IsActive, !user.IsActive ? DomainExceptions.CannotPerformActionUserIsInactive : DomainExceptions.CannotPerformActionUserMFANotConfigured);
          break;
        case UserManagementAction.EnableMFA:
          AssertUser(() => !user.IsExternalIdentityProvider, DomainExceptions.CannotPerformActionForExternalLoginUsers);
          AssertUser(() => !user.TwoFactorEnabled && user.IsActive, !user.IsActive ? DomainExceptions.CannotPerformActionUserIsInactive : DomainExceptions.CannotPerformActionTwoFactorEnabled);
          break;
        case UserManagementAction.DisableMFA:
          AssertUser(() => !user.IsExternalIdentityProvider, DomainExceptions.CannotPerformActionForExternalLoginUsers);
          AssertUser(() => !user.ProviderRequiresMFA, string.Format(CultureInfo.CurrentCulture, DomainExceptions.CannotPerformActionMFAIsRequired, user.IdentityProvider));
          AssertUser(() => user.TwoFactorEnabled && user.IsActive, !user.IsActive ? DomainExceptions.CannotPerformActionUserIsInactive : DomainExceptions.CannotPerformActionTwoFactorNotEnabled);
          break;
        case UserManagementAction.ResetMFA:
          AssertUser(() => !user.IsExternalIdentityProvider, DomainExceptions.CannotPerformActionForExternalLoginUsers);
          AssertUser(() => user.TwoFactorConfigured && user.IsActive, !user.IsActive ? DomainExceptions.CannotPerformActionUserIsInactive : DomainExceptions.CannotPerformActionUserMFANotConfigured);
          break;
        case UserManagementAction.ClearLockout:
          AssertUser(() => !user.IsExternalIdentityProvider, DomainExceptions.CannotPerformActionForExternalLoginUsers);
          AssertUser(() => user.LockoutEnd != null && user.IsActive, !user.IsActive ? DomainExceptions.CannotPerformActionUserIsInactive : DomainExceptions.CannotPerformActionUserMFANotConfigured);
          break;
        case UserManagementAction.Activate:
          AssertUser(() => !user.IsActive, DomainExceptions.CannotPerformActionUserAlreadyActive);
          break;
        case UserManagementAction.Deactivate:
          AssertUser(() => user.IsActive, DomainExceptions.CannotPerformActionUserAlreadyInactive);
          break;
      }
    }

    private static void AssertUser(Func<bool> assertPredicate, string failureMessage)
    {
      if (!assertPredicate())
      {
        throw new InvalidDomainOperationException(failureMessage);
      }
    }

    private async Task PerformUserActionAsync(UserLookup userLookup, PerformUserActionCommand command)
    {
      var user = this.dbContext.Users.FirstOrDefault(appUser => appUser.Id == userLookup.Id) ?? throw new InvalidDomainOperationException(string.Format(CultureInfo.CurrentCulture, DomainExceptions.NoUserFoundForId, command.UserId));
      this.dbContext.UserManagementActionLogs.Add(new UserManagementActionLog(command.Action, userLookup.Id));
      switch (command.Action)
      {
        case UserManagementAction.ResendEmailVerificationLink:
          await this.registrationEmailService.SendVerificationEmailAsync(user);
          break;
        case UserManagementAction.EnableMFA:
          user.EnableMFA();
          break;
        case UserManagementAction.DisableMFA:
          user.DisableMFA();
          break;
        case UserManagementAction.ResetMFA:
          user.ResetMFA();
          break;
        case UserManagementAction.ClearLockout:
          user.ClearLockout();
          break;
        case UserManagementAction.Activate:
          user.Activate();
          break;
        case UserManagementAction.Deactivate:
          user.Deactivate();
          break;
      }

      await this.dbContext.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public Task<PageResult<UserInviteLookup>> GetUserInvitesAsync(PageRequest<UserInviteCriteria> pageRequest)
    {
      var criteria = pageRequest.Criteria;
      var query = this.GetUserInviteQuery().Where(userInvite => criteria.IncludeRegistered || userInvite.UserIdentifier == null);

      return this.queryService.EntitiesPaged(query, pageRequest);
    }

    /// <inheritdoc/>
    public async Task<UserInviteLookup> SaveUserInviteAsync(UserInvite userInvite)
    {
      userInvite.EmailAddress = userInvite.EmailAddress.ToLowerInvariant().Trim();

      if (userInvite.TrackingState == TrackableEntities.Common.Core.TrackingState.Added)
      {
        this.dbContext.UserInvites.Add(userInvite);

        var user = await this.userManager.FindByEmailAsync(userInvite.EmailAddress);
        user?.UserInvite = userInvite;

        await this.dbContext.SaveChangesAsync();

        await this.registrationEmailService.SendUserInviteEmailAsync(userInvite);

        return userInvite.ToLookup(user);
      }
      else
      {
        throw new InvalidDomainOperationException(DomainExceptions.UserInviteUpdateNotAllowed);
      }
    }

    /// <inheritdoc/>
    public async Task RevokeUserInviteAsync(int userInviteId)
    {
      var existing = await this.GetUserInviteQuery().FirstAsync(userInvite => userInvite.UserInviteId == userInviteId);

      if (!existing.HasRegistered)
      {
        this.dbContext.UserInvites.Remove(new UserInvite() { UserInviteId = existing.UserInviteId });
        await this.dbContext.SaveChangesAsync();
      }
      else
      {
        throw new InvalidDomainOperationException(DomainExceptions.UserInviteAlreadyRegistered);
      }
    }

    /// <inheritdoc/>
    public Task<UserInviteLookup?> FindInvitedUserAsync(Guid userIdentifier)
    {
      return this.GetUserInviteQuery().Where(userInviteLookup => userInviteLookup.UserIdentifier == userIdentifier.ToString()).FirstOrDefaultAsync();
    }

    private IQueryable<UserInviteLookup> GetUserInviteQuery()
    {
#pragma warning disable IDE0031 // Use null propagation
      return from userInvite in this.dbContext.UserInvites
             join user in this.dbContext.Users on userInvite.UserInviteId equals user.UserInviteId into users
             from user in users.DefaultIfEmpty()
             select new UserInviteLookup()
             {
               UserInviteId = userInvite.UserInviteId,
               AddToGroupId = userInvite.AddToUserGroupId,
               EmailAddress = userInvite.EmailAddress,
               CreatedOn = userInvite.CreatedOn,
               UserIdentifier = user == null ? null : user.Id,
             };
#pragma warning restore IDE0031 // Use null propagation
    }
  }
}
