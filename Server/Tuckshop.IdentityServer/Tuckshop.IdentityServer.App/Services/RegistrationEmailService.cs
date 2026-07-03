namespace Tuckshop.IdentityServer.App.Services
{
  using System;
  using System.Text;
  using System.Threading.Tasks;
  using Microsoft.AspNetCore.Http;
  using Microsoft.AspNetCore.Identity;
  using Microsoft.AspNetCore.WebUtilities;
  using Neo.IdentityServer.Providers;
  using Neo.NotificationServer.Services;
  using Tuckshop.IdentityServer.App.Notifications;
  using Tuckshop.IdentityServer.Models;
  using Tuckshop.IdentityServer.Models.UserManagement;

  /// <summary>
  /// A class for sending email verifications.
  /// </summary>
  public class RegistrationEmailService : IRegistrationEmailService
  {
    private readonly UserManager<TuckshopApplicationUser> userManager;
    private readonly INotificationService notificationService;
    private readonly IDataProtectionOptionsProvider dataProtectionOptionsProvider;
    private readonly HttpContext httpContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="RegistrationEmailService"/> class.
    /// </summary>
    /// <param name="userManager">The User Manger.</param>
    /// <param name="notificationService">The Notification Service.</param>
    /// <param name="httpContextAccessor">The http context accessor.</param>
    /// <param name="dataProtectionOptionsProvider">The data protection options provider.</param>
    public RegistrationEmailService(
        UserManager<TuckshopApplicationUser> userManager,
        INotificationService notificationService,
        IHttpContextAccessor httpContextAccessor,
        IDataProtectionOptionsProvider dataProtectionOptionsProvider)
    {
      this.userManager = userManager;
      this.notificationService = notificationService;
      this.dataProtectionOptionsProvider = dataProtectionOptionsProvider;
      this.httpContext = httpContextAccessor.HttpContext ?? throw new ArgumentException($"{nameof(httpContextAccessor.HttpContext)} is required.");
    }

    /// <inheritdoc />
    public async Task SendVerificationEmailAsync(TuckshopApplicationUser user)
    {
      var code = await this.userManager.GenerateEmailConfirmationTokenAsync(user);
      code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

      var request = this.httpContext.Request;
      var verificationUrl = $"{request.Scheme}://{request.Host}{request.PathBase}/identity/account/ConfirmEmail?userId={user.Id}&code={code}";

      var model = new EmailVerification(user.FirstName, user.Email ?? throw new InvalidOperationException("Missing user email"), verificationUrl)
      {
        LinkExpiryHours = this.dataProtectionOptionsProvider.GetOptions(DataProtectionPurposes.EmailConfirmation).TokenLifespan.TotalHours,
      };

      await this.notificationService.SendNotificationAsync(TemplateTypes.EmailVerification, model);
    }

    /// <inheritdoc />
    public Task SendUserInviteEmailAsync(UserInvite userInvite)
    {
      var request = this.httpContext.Request;
      var registerUrl = $"{request.Scheme}://{request.Host}{request.PathBase}/identity/account/register?email={userInvite.EmailAddress}";

      return this.notificationService.SendNotificationAsync(
          TemplateTypes.NewUserInvite,
          NewUserInvite.Create(userInvite.EmailAddress, registerUrl));
    }
  }
}