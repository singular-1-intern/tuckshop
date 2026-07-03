namespace Tuckshop.IdentityServer.App.Services
{
  using System;
  using System.Security.Claims;
  using System.Threading.Tasks;
  using Microsoft.AspNetCore.Identity;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.Extensions.Logging;
  using Neo.Extensions;
  using Neo.IdentityServer.App.OpenIddict.Services;
  using Tuckshop.IdentityServer.Contracts;
  using Tuckshop.IdentityServer.Models;

  /// <summary>
  /// The profile service.
  /// </summary>
  public class ProfileService : IProfileService
  {
    private readonly ILogger<ProfileService> logger;
    private readonly UserManager<TuckshopApplicationUser> userManager;
    private readonly IdentityDbContext dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProfileService"/> class.
    /// </summary>
    /// <param name="userManager">The user manager.</param>
    /// <param name="dbContext">The Identity DbContext.</param>
    /// <param name="logger">The logger.</param>
    public ProfileService(
      UserManager<TuckshopApplicationUser> userManager,
      IdentityDbContext dbContext,
      ILogger<ProfileService> logger)
    {
      this.userManager = userManager;
      this.dbContext = dbContext;
      this.logger = logger;
    }

    /// <inheritdoc/>
    public async Task GetProfileDataAsync(ProfileDataRequestContext context)
    {
      var user = context.User as TuckshopApplicationUser ??
        throw new InvalidOperationException("No user or user is incorrect type");

      context.IssuedClaims.AddRange(context.Subject.Claims);

      /* if additional claims are required that can be determined directly from the User entity, rather add them inside the UserClaimsPrincipalFactory */
      if (!context.IssuedClaims.HasClaimType(TuckshopClaimTypes.IsInvitedUser) &&
          await this.dbContext.UserInvites.AnyAsync(invite => invite.EmailAddress == user.Email))
      {
        context.IssuedClaims.Add(new Claim(TuckshopClaimTypes.IsInvitedUser, "true"));
      }
    }

    /// <inheritdoc/>
    public async Task IsActiveAsync(IsActiveContext context)
    {
      var sub = context.Subject.GetSubjectId();
      var user = await this.userManager.FindByIdAsync(sub);

      if (user != null && !user.IsActive)
      {
        this.logger.LogInformation($"User with id {sub} is not active.");
      }

      context.IsActive = user != null && user.IsActive;
    }
  }
}
