namespace Tuckshop.IdentityServer.App.Services
{
  using System;
  using System.Security.Claims;
  using System.Threading.Tasks;
  using Microsoft.AspNetCore.Identity;
  using Microsoft.Extensions.Options;
  using Tuckshop.IdentityServer.Contracts;
  using Tuckshop.IdentityServer.Models;
  using static OpenIddict.Abstractions.OpenIddictConstants;

  /// <summary>
  /// Represents a class for adding claims for the application user
  /// Claims will be user information provided in the user info requests.
  /// </summary>
  public class UserClaimsPrincipalFactory : UserClaimsPrincipalFactory<TuckshopApplicationUser, IdentityRole>
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="UserClaimsPrincipalFactory"/> class.
    /// </summary>
    /// <param name="userManager">Provides an api for managing users in a persistent store.</param>
    /// <param name="roleManager">Provides an api for managing roles in a persistent store.</param>
    /// <param name="optionsAccessor">Used to retrieve Identity Options.</param>
    public UserClaimsPrincipalFactory(
            UserManager<TuckshopApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IOptions<IdentityOptions> optionsAccessor)
            : base(userManager, roleManager, optionsAccessor)
    {
    }

    /// <inheritdoc/>
    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(TuckshopApplicationUser user)
    {
      _ = user ?? throw new ArgumentNullException(nameof(user));

      var identity = await base.GenerateClaimsAsync(user);
      identity.AddClaim(new Claim(Claims.Name, user.UserName ?? string.Empty));
      identity.AddClaim(new Claim(Claims.GivenName, user.FirstName ?? string.Empty));
      identity.AddClaim(new Claim(Claims.FamilyName, user.LastName ?? string.Empty));
      identity.AddClaim(new Claim(Claims.EmailVerified, user.EmailConfirmed.ToString()));

      identity.AddClaim(new Claim("amr", user.TwoFactorEnabled ? "mfa" : "pwd"));

      if (user.UserInviteId != null)
      {
        identity.AddClaim(new Claim(TuckshopClaimTypes.IsInvitedUser, "true"));
      }

      return identity;
    }
  }
}