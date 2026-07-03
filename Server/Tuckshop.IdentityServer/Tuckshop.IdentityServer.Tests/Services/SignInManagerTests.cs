namespace Tuckshop.IdentityServer.Tests.Services
{
  using System.Threading.Tasks;
  using Microsoft.EntityFrameworkCore;
  using Neo.Extensions;
  using Neo.IdentityServer.App.Services.IdentityProviders;
  using Neo.IdentityServer.SignInRules;
  using Tuckshop.IdentityServer;
  using Tuckshop.IdentityServer.Models;
  using Tuckshop.IdentityServer.Tests.Mocks;
  using Xunit;

  public class SignInManagerTests
  {
    private readonly IdentityDbContext identityDbContext;
    private readonly IdentityProviderService<IdentityDbContext> identityProviderService;

    public SignInManagerTests()
    {
      var result = UnitTestHelper.CreateIdentityProviderServices();
      this.identityDbContext = result.IdentityDbContext;
      this.identityProviderService = result.IdentityProviderService;
    }

    [Fact]
    public async Task PasswordSignInAsync_UnknownUser_Expect_SignResultNotAllowed()
    {
      // Arrange
      var signInManager = new FakeSignInManager(new TuckshopApplicationUser() { Email = "unknown@test.com" }, this.identityDbContext, this.identityProviderService);

      // Act
      var signinResult = await signInManager.PasswordSignInAsync(string.Empty, string.Empty, false, false);

      // Assert        
      this.identityDbContext.ChangeTracker.Clear();
      var signInAudits = await this.identityDbContext.SignInAudits.ToListAsync(TestContext.Current.CancellationToken);

      Assert.True(signinResult.IsNotAllowed);
      Assert.False(signinResult.Succeeded);
      Assert.Contains(signInAudits, signInAudit => signInAudit.Details == SignInReason.UnknownUser.Description());
    }

    [Fact]
    public async Task PasswordSignInAsync_BlockedUser_ExpectFalse()
    {
      // Arrange
      var user = new TuckshopApplicationUser()
      {
        Email = "test@singular.co.za",
        IsActive = false,
        UserName = "test@singular.co.za",
      };
      var signInManager = new FakeSignInManager(user, this.identityDbContext, this.identityProviderService);

      // Act
      var signinResult = await signInManager.PasswordSignInAsync(user.UserName, "Password", isPersistent: true, lockoutOnFailure: true);

      // Assert
      this.identityDbContext.ChangeTracker.Clear();
      var signInAudits = await this.identityDbContext.SignInAudits.ToListAsync(TestContext.Current.CancellationToken);

      Assert.True(signinResult.IsNotAllowed);
      Assert.False(signinResult.Succeeded);
      Assert.Contains(signInAudits, signInAudit => signInAudit.Details == SignInReason.BlockedUser.Description());
    }

    [Fact]
    public async Task PasswordSignInAsync_InValidPassword_ExpectFalse()
    {
      // Arrange
      var user = new TuckshopApplicationUser()
      {
        Email = "test@singular.co.za",
        IsActive = true,
        UserName = "test@singular.co.za",
      };
      var signInManager = new FakeSignInManager(user, this.identityDbContext, this.identityProviderService);

      // Act
      var signinResult = await signInManager.PasswordSignInAsync(user.UserName, string.Empty, true, true);

      // Assert
      this.identityDbContext.ChangeTracker.Clear();
      var signInAudits = await this.identityDbContext.SignInAudits.ToListAsync(TestContext.Current.CancellationToken);

      Assert.False(signinResult.Succeeded);
      Assert.Contains(signInAudits, signInAudit => signInAudit.Details.Contains("Invalid sign in attempt"));
    }

    [Fact]
    public async Task PasswordSignInAsync_ValidPassword_ExpectTrue()
    {
      // Arrange
      var user = new TuckshopApplicationUser()
      {
        Email = "test@singular.co.za",
        IsActive = true,
        UserName = "test@singular.co.za",
      };
      var signInManager = new FakeSignInManager(user, this.identityDbContext, this.identityProviderService);

      // Act
      var signinResult = await signInManager.PasswordSignInAsync(user.UserName, "Password", true, true);

      // Assert
      this.identityDbContext.ChangeTracker.Clear();
      var signInAudits = await this.identityDbContext.SignInAudits.ToListAsync(TestContext.Current.CancellationToken);

      Assert.True(signinResult.Succeeded);
      Assert.Contains(signInAudits, signInAudit => signInAudit.Details == SignInReason.SucessfullSignIn.Description());
    }
  }
}
