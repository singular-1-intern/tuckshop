namespace Tuckshop.IdentityServer.Tests.Mocks
{
  using System.Net;
  using System.Threading.Tasks;
  using Microsoft.AspNetCore.Authentication;
  using Microsoft.AspNetCore.Http;
  using Microsoft.AspNetCore.Identity;
  using Microsoft.Extensions.Logging;
  using Microsoft.Extensions.Options;
  using Moq;
  using Neo.IdentityServer.App.Services;
  using Neo.IdentityServer.App.Services.IdentityProviders;
  using Tuckshop.IdentityServer;
  using Tuckshop.IdentityServer.App.Services;
  using Tuckshop.IdentityServer.Models;

  public class FakeSignInManager : SignInManager
  {
    private const string UserAgent = "User-Agent";

    public FakeSignInManager(TuckshopApplicationUser applicationUser, IdentityDbContext identityDbContext, IIdentityProviderLookupService identityProviderService)
          : base(new FakeUserManager<TuckshopApplicationUser>(applicationUser),
                new HttpContextAccessor(),
                new Mock<IUserClaimsPrincipalFactory<TuckshopApplicationUser>>().Object,
                new Mock<IOptions<IdentityOptions>>().Object,
                new Mock<ILogger<SignInManager>>().Object,
                new Mock<IAuthenticationSchemeProvider>().Object,
                new Mock<IUserConfirmation<TuckshopApplicationUser>>().Object,
                new SignInAuditService<IdentityDbContext>(identityDbContext),
                new MfaManager(),
                identityProviderService)
    {
      this.Context = new DefaultHttpContext();
      this.Context.Request.Headers[UserAgent] = "TestAgent";
      this.Context.Connection.RemoteIpAddress = IPAddress.Parse("127.0.0.1");
    }

    /// <inheritdoc />
    public override Task<SignInResult> PasswordSignInAsync(TuckshopApplicationUser user, string password, bool isPersistent, bool lockoutOnFailure)
    {
      if (password == "Password")
      {
        return Task.FromResult(SignInResult.Success);
      }
      return Task.FromResult(SignInResult.Failed);
    }
  }
}
