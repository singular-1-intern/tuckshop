namespace Tuckshop.IdentityServer.Models
{
  using Neo.Identity;
  using Neo.Model.Identity;

  /// <summary>
  /// Claim mapper to map token claims to a user object.
  /// </summary>
  public class TuckshopApplicationUserClaimMapper : UserClaimMapperBase<TuckshopApplicationUser>
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="TuckshopApplicationUserClaimMapper"/> class.
    /// </summary>
    /// <param name="user">The Claims Principal user.</param>
    public TuckshopApplicationUserClaimMapper(System.Security.Claims.ClaimsPrincipal user)
      : base(user)
    {
    }

    /// <summary>
    /// Will create a user from the information in the claims.
    /// </summary>
    /// <returns>A user populated with information in the claims.</returns>
    public override TuckshopApplicationUser? CreateNewUser()
    {
      // this returns the user from the claims
      if (this.IsHumanUser)
      {
        var user = new TuckshopApplicationUser()
        {
          Id = this.UserIdentifier,
          UserName = this.GetClaimValue(ClaimType.Email),
          FirstName = this.GetClaimValue(ClaimType.FirstName),
          LastName = this.GetClaimValue(ClaimType.LastName),
        };

        return user;
      }
      else if (this.IsClientAppUser)
      {
        return new TuckshopApplicationUser()
        {
          ClientId = this.UserIdentifier,
          UserName = $"{this.UserIdentifier}@client.com",
          FirstName = "Client",
          LastName = this.UserIdentifier,
        };
      }
      return null;
    }

    /// <summary>
    /// Will update the user from the information in the claims.
    /// </summary>
    /// <param name="user">The user to be updated.</param>
    public override void UpdateUser(TuckshopApplicationUser user)
    {
      // since IDS is the user source so user will always be up to date
    }
  }
}
