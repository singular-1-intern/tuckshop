namespace Tuckshop.IdentityServer.Tests.Mocks
{
  using System;
  using System.Threading.Tasks;
  using Microsoft.AspNetCore.Identity;
  using Microsoft.Extensions.Logging;
  using Microsoft.Extensions.Options;
  using Moq;
  using Tuckshop.IdentityServer.Models;

  public class FakeUserManager<TUser> : UserManager<TUser>
    where TUser : TuckshopApplicationUser
  {
    private readonly TUser applicationUser;

    public FakeUserManager(TUser applicationUser)
            : base(new Mock<IUserStore<TUser>>().Object,
                  new Mock<IOptions<IdentityOptions>>().Object,
                  new Mock<IPasswordHasher<TUser>>().Object,
                  new IUserValidator<TUser>[0],
                  new IPasswordValidator<TUser>[0],
                  new Mock<ILookupNormalizer>().Object,
                  new Mock<IdentityErrorDescriber>().Object,
                  new Mock<IServiceProvider>().Object,
                  new Mock<ILogger<UserManager<TUser>>>().Object)
    {
      this.applicationUser = applicationUser;
    }

    /// <inheritdoc />
    public override Task<TUser?> FindByEmailAsync(string? email)
    {
      if (email == this.applicationUser.Email)
      {
        return Task.FromResult<TUser?>(this.applicationUser);
      }
      else
      {
        return Task.FromResult<TUser?>(null);
      }
    }
  }
}
