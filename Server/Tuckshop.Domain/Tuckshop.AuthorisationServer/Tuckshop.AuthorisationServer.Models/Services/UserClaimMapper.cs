namespace Tuckshop.AuthorisationServer.Models
{
  using System;
  using Neo.Identity;
  using Neo.Model.Identity;
  using Tuckshop.IdentityServer.Contracts;

  /// <summary>
  /// Will map the identity claims to the User entity.
  /// </summary>
  /// <remarks>
  /// Initializes a new instance of the <see cref="UserClaimMapper"/> class.
  /// </remarks>
  /// <param name="user">The Claims Principal user.</param>
  public class UserClaimMapper(System.Security.Claims.ClaimsPrincipal user) : UserClaimMapperBase<TuckshopAuthorisationUser>(user)
  {
    /// <summary>
    /// Will create a user from the information in the claims.
    /// </summary>
    /// <returns>A user populated with information in the claims.</returns>
    public override TuckshopAuthorisationUser? CreateNewUser()
    {
      if (this.IsHumanUser)
      {
        return new TuckshopAuthorisationUser()
        {
          IdentityGuid = Guid.Parse(this.UserIdentifier),
          UserName = this.GetClaimValue(ClaimType.Email),
          PreferredName = this.GetClaimValue(ClaimType.FirstName),
          LastName = this.GetClaimValue(ClaimType.LastName),
          IsInvitedUser = this.GetClaimValue(TuckshopClaimTypes.IsInvitedUser) == "true",
        };
      }
      else if (this.IsClientAppUser)
      {
        return new TuckshopAuthorisationUser()
        {
          ClientId = this.UserIdentifier,
          UserName = $"{this.UserIdentifier}@client.com",
          PreferredName = "Client",
          LastName = this.UserIdentifier,
        };
      }
      return null;
    }

    /// <summary>
    /// Will update the user from the information in the claims.
    /// </summary>
    /// <param name="user">The user to be updated.</param>
    public override void UpdateUser(TuckshopAuthorisationUser user)
    {
      ArgumentNullException.ThrowIfNull(user);

      var claimUser = this.CreateNewUser() ?? throw new InvalidOperationException("A user is required but there is no user in scope. Override the user using the IOverridableUserResolver or the ISystemUserService services.");
      user.UserName = claimUser.UserName;
      user.PreferredName = claimUser.PreferredName;
      user.LastName = claimUser.LastName;
      user.IsInvitedUser = claimUser.IsInvitedUser;
    }
  }
}