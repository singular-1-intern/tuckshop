namespace Tuckshop.IdentityServer.Tests.Services
{
  using System;
  using System.Collections.Generic;
  using System.Globalization;
  using System.Threading.Tasks;
  using Microsoft.AspNetCore.Identity;
  using Microsoft.EntityFrameworkCore;
  using Moq;
  using Neo.AuthorisationServer.Client;
  using Neo.IdentityServer.Models.IdentityProviders;
  using Neo.Model.Exceptions;
  using Neo.Model.Identity.SystemUser;
  using Neo.Model.Services;
  using Tuckshop.IdentityServer.App;
  using Tuckshop.IdentityServer.App.Services;
  using Tuckshop.IdentityServer.App.Services.UserManagement;
  using Tuckshop.IdentityServer.Contracts.UserManagement;
  using Tuckshop.IdentityServer.Contracts.UserManagement.Commands;
  using Tuckshop.IdentityServer.Contracts.UserManagement.Queries;
  using Tuckshop.IdentityServer.Models;
  using Tuckshop.IdentityServer.Models.UserManagement;
  using Xunit;

  public class UserManagementServiceTests
  {
    private readonly IdentityDbContext dbContext;
    private readonly UserManagementService userManagementService;
    private readonly string UserJohnId = Guid.NewGuid().ToString();
    private readonly string UserJeffId = Guid.NewGuid().ToString();
    private readonly string UserSarahId = Guid.NewGuid().ToString();
    private readonly string UserSusanId = Guid.NewGuid().ToString();

    private bool mfaRequired = true;

    public UserManagementServiceTests()
    {
      this.dbContext = UnitTestHelper.InitIdentityDbContext();

      var authorisationServiceMock = new Mock<IAuthorisationService>();
      authorisationServiceMock.Setup(authorisationService => authorisationService.AssertUserHasRoleAsync(It.IsAny<Enum>())).Returns(() => Task.FromResult(this.UserHasRole));

      var registrationEmailServiceMock = new Mock<IRegistrationEmailService>();
      registrationEmailServiceMock.Setup(registrationEmailService => registrationEmailService.SendVerificationEmailAsync(It.IsAny<TuckshopApplicationUser>()))
        .Returns((TuckshopApplicationUser user) =>
        {
          this.SentToUsers.Add(user);
          return Task.CompletedTask;
        });
      registrationEmailServiceMock.Setup(registrationEmailService => registrationEmailService.SendUserInviteEmailAsync(It.IsAny<UserInvite>()))
        .Returns((UserInvite userInvite) =>
        {
          this.UserInvitesSent.Add(userInvite);
          return Task.CompletedTask;
        });

      var mfaManagerMock = new Mock<IMfaManager>();
      mfaManagerMock.Setup(mfaManager => mfaManager.UserRequiresTwoFactor(It.IsAny<UserLookup>())).Returns(() => this.mfaRequired);

      var userStoreMock = new Mock<IUserStore<TuckshopApplicationUser>>();
      var userManagerMock = new Mock<UserManager<TuckshopApplicationUser>>(() => new UserManager<TuckshopApplicationUser>(userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!));
      userManagerMock.Setup(userManager => userManager.FindByEmailAsync(It.IsAny<string>()))
        .Returns((string email) =>
        {
          return this.dbContext.Users.FirstOrDefaultAsync(appUser => appUser.Email != null && appUser.Email.ToLowerInvariant() == email.ToLowerInvariant());
        });

      this.userManagementService = new UserManagementService(
        this.dbContext,
        authorisationServiceMock.Object,
        registrationEmailServiceMock.Object,
        new QueryService(),
        new SystemUserOptions<TuckshopApplicationUser>() { SystemUserGuid = Guid.NewGuid() },
        mfaManagerMock.Object,
        userManagerMock.Object);

      this.SetupTestData();
    }

    private List<TuckshopApplicationUser> SentToUsers { get; } = new List<TuckshopApplicationUser>();

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "NEO1731:Property should be plurally named", Justification = "Already plural")]
    private List<UserInvite> UserInvitesSent { get; } = new();

    private bool UserHasRole { get; set; } = true;

    [Fact]
    public async Task FindUsersAsync()
    {
      var criteria = new UserLookupCriteria()
      {
        LastName = "User",
      };

      var request = new PageRequest<UserLookupCriteria>(criteria);
      var results = await this.userManagementService.FindUsersAsync(request);

      // Should be 4 users
      Assert.Equal(4, results.EntityList.Count);

      Assert.Collection(
        results.EntityList,
        userLookup => Assert.Equal("John", userLookup.FirstName),
        userLookup => Assert.Equal("Jeff", userLookup.FirstName),
        userLookup => Assert.Equal("Sarah", userLookup.FirstName),
        userLookup => Assert.Equal("Susan", userLookup.FirstName));

      // should find 0 XUsers
      criteria.LastName = "XUser";
      results = await this.userManagementService.FindUsersAsync(request);
      Assert.Empty(results.EntityList);

      // should find 2 Users
      criteria.LastName = string.Empty;
      criteria.FirstName = "J";

      // should find 2 different users
      results = await this.userManagementService.FindUsersAsync(request);
      Assert.Equal(2, results.EntityList.Count);

      Assert.Collection(
        results.EntityList,
        userLookup => Assert.Equal("John", userLookup.FirstName),
        userLookup => Assert.Equal("Jeff", userLookup.FirstName));

      // search on "contains" username
      criteria.FirstName = "";
      criteria.LastName = "";
      criteria.UserName = "@test.com";
      results = await this.userManagementService.FindUsersAsync(request);
      Assert.Equal(4, results.EntityList.Count);
    }

    [Fact]
    public Task PerformUserAction_ResendEmailVerificationLink()
    {
      return this.AssertUserActionAsync(
        UserManagementAction.ResendEmailVerificationLink,
        this.UserJohnId,
        user =>
        {
          var sentToUser = Assert.Single(this.SentToUsers);
          Assert.Equal(this.UserJohnId, sentToUser.Id);
        });
    }

    [Fact]
    public async Task PerformUserAction_ResetMFA()
    {
      // now make sure MFA is enabled and configured
      var user = await this.dbContext.Users.FirstAsync(appUser => appUser.Id == this.UserJohnId, TestContext.Current.CancellationToken);
      user.TwoFactorEnabled = true;
      user.ConfigureTwoFactor(true);
      this.dbContext.SaveChanges();

      await this.AssertUserActionAsync(
        UserManagementAction.ResetMFA,
        this.UserJohnId,
        user =>
        {
          Assert.False(user.TwoFactorConfigured);
        });
    }

    [Fact]
    public async Task PerformUserAction_ClearLockout()
    {
      // now make sure MFA is enabled and configured
      var user = await this.dbContext.Users.FirstAsync(appUser => appUser.Id == this.UserJohnId, TestContext.Current.CancellationToken);
      user.LockoutEnabled = true;
      user.LockoutEnd = DateTime.UtcNow.AddMinutes(10);
      this.dbContext.SaveChanges();

      await this.AssertUserActionAsync(
        UserManagementAction.ClearLockout,
        this.UserJohnId,
        user =>
        {
          Assert.Null(user.LockoutEnd);
        });
    }

    [Fact]
    public async Task PerformUserAction_Unblock()
    {
      // now make sure MFA is enabled and configured
      var user = await this.dbContext.Users.FirstAsync(appUser => appUser.Id == this.UserJohnId, TestContext.Current.CancellationToken);
      user.Deactivate();
      this.dbContext.SaveChanges();

      await this.AssertUserActionAsync(
        UserManagementAction.Activate,
        this.UserJohnId,
        user =>
        {
          Assert.True(user.IsActive);
        });
    }

    [Fact]
    public async Task PerformUserAction_Block()
    {
      // now make sure MFA is enabled and configured
      var user = await this.dbContext.Users.FirstAsync(appUser => appUser.Id == this.UserJohnId, TestContext.Current.CancellationToken);
      user.Activate();
      this.dbContext.SaveChanges();

      await this.AssertUserActionAsync(
        UserManagementAction.Deactivate,
        this.UserJohnId,
        user =>
        {
          Assert.False(user.IsActive);
        });
    }

    [Fact]
    public async Task PerformUserAction_EnableMFA()
    {
      // now make sure MFA is enabled and configured
      var user = await this.dbContext.Users.FirstAsync(appUser => appUser.Id == this.UserJohnId, TestContext.Current.CancellationToken);
      user.TwoFactorEnabled = false;
      this.dbContext.SaveChanges();

      await this.AssertUserActionAsync(
        UserManagementAction.EnableMFA,
        this.UserJohnId,
        user =>
        {
          Assert.True(user.TwoFactorEnabled);
        });
    }

    [Fact]
    public async Task PerformUserAction_DisableMFA()
    {
      // now make sure MFA is enabled and configured
      var user = await this.dbContext.Users.FirstAsync(appUser => appUser.Id == this.UserJohnId, TestContext.Current.CancellationToken);
      user.TwoFactorEnabled = true;
      this.dbContext.SaveChanges();

      var ex = await Assert.ThrowsAsync<InvalidDomainOperationException>(() => this.AssertUserActionAsync(
        UserManagementAction.DisableMFA,
        this.UserJohnId,
        user =>
        {
          Assert.False(user.TwoFactorEnabled);
        }));

      Assert.Equal(
        string.Format(CultureInfo.CurrentCulture, DomainExceptions.CannotPerformActionMFAIsRequired, "Credentials"),
        ex.Message);

      this.mfaRequired = false;

      await this.AssertUserActionAsync(
        UserManagementAction.DisableMFA,
        this.UserJohnId,
        user =>
        {
          Assert.False(user.TwoFactorEnabled);
        });
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Assertions", "xUnit2023:Do not use collection methods for single-item collections", Justification = "Test collection")]
    public async Task UserInvite()
    {
      var savedInvite = await this.userManagementService.SaveUserInviteAsync(new UserInvite()
      {
        EmailAddress = "John_User@test.com",
        AddToUserGroupId = 1,
        TrackingState = TrackableEntities.Common.Core.TrackingState.Added,
      });

      var user = await this.dbContext.Users.FirstAsync(appUser => appUser.Email == "John_User@test.com", TestContext.Current.CancellationToken);

      Assert.Equal(savedInvite.UserInviteId, user.UserInviteId);
      Assert.Collection(this.UserInvitesSent, userInvite => Assert.Equal(userInvite.EmailAddress, savedInvite.EmailAddress));

      var ex = await Assert.ThrowsAsync<InvalidDomainOperationException>(() => this.userManagementService.RevokeUserInviteAsync(savedInvite.UserInviteId));

      Assert.Equal(DomainExceptions.UserInviteAlreadyRegistered, ex.Message);
    }

    private async Task AssertUserActionAsync(UserManagementAction action, string userId, Action<TuckshopApplicationUser> assertAction)
    {
      var command = new PerformUserActionCommand() { Action = action, UserId = userId };
      await this.userManagementService.PerformUserActionAsync(command);
      var user = await this.dbContext.Users.FirstAsync(appUser => appUser.Id == userId);
      assertAction(user);
    }

    private void SetupTestData()
    {
      this.dbContext.IdentityProviders.Add(
        new IdentityProvider()
        {
          IdentityProviderId = 1,
          IdentityProviderType = (int)IdentityProviderType.LoginCredentials,
          Name = "Credentials",
          DisplayName = "Credentials",
          NameSuffix = "creds",
        });

      this.AddUser(this.UserJohnId, "John", "User");
      this.AddUser(this.UserJeffId, "Jeff", "User");
      this.AddUser(this.UserSarahId, "Sarah", "User");
      this.AddUser(this.UserSusanId, "Susan", "User");

      this.dbContext.SaveChanges();
    }

    private void AddUser(string id, string firstName, string lastName)
    {
      var user = new TuckshopApplicationUser()
      {
        Id = id,
        FirstName = firstName,
        LastName = lastName,
        Email = $"{firstName}_{lastName}@test.com",
        UserName = $"{firstName}_{lastName}@test.com",
        IdentityProviderId = 1,
        IsActive = true,
      };

      this.dbContext.Users.Add(user);
    }
  }
}
