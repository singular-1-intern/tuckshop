namespace Tuckshop.IdentityServer.App.Notifications
{
  using Neo.NotificationServer;
  using Neo.NotificationServer.Models;

  /// <summary>
  /// Identity Notification Template Types.
  /// </summary>
  public static class TemplateTypes
  {
    /// <summary>
    /// Gets the existing user invite template type.
    /// </summary>
    public static TemplateType<EmailVerification> EmailVerification { get; } = new TemplateType<EmailVerification>()
    {
      NotificationTypes = NotificationType.Email,
      TemplateTypeKey = "EmailVerification",
      TemplateTypeName = "Email Verification",
      Description = "Registration email verification.",
      ExampleModel = Notifications.EmailVerification.GetExampleData(),
      DefaultTemplate = new TemplateDefinition()
      {
        Body = HtmlHelper.ReplaceNewLines(DefaultNotificationTemplates.EmailVerification),
        Subject = "Verify your email address",
      },
    };

    /// <summary>
    /// Gets the reset password email template type.
    /// </summary>
    public static TemplateType<ResetPassword> ResetPasswordEmail { get; } = new TemplateType<ResetPassword>()
    {
      NotificationTypes = NotificationType.Email,
      TemplateTypeKey = "ResetPassword",
      TemplateTypeName = "Reset Password",
      Description = "Reset Password Email.",
      ExampleModel = ResetPassword.GetExampleData(),
      DefaultTemplate = new TemplateDefinition()
      {
        Body = HtmlHelper.ReplaceNewLines(DefaultNotificationTemplates.ResetPassword),
        Subject = "Password Reset",
      },
    };

    /// <summary>
    /// Static property for the NewUserInvite default template type.
    /// </summary>
    public static TemplateType<NewUserInvite> NewUserInvite { get; } = new TemplateType<NewUserInvite>
    {
      NotificationTypes = NotificationType.Email,
      TemplateTypeKey = "NewUserInvite",
      TemplateTypeName = "New User Invite",
      Description = "User Invite email for when a new user is invited.",
      ExampleModel = Notifications.NewUserInvite.GetExampleData(),
      DefaultTemplate = new TemplateDefinition
      {
        Body = HtmlHelper.ReplaceNewLines(DefaultNotificationTemplates.UserInvite),
        Subject = "Tuckshop platform invite"
      }
    };
  }
}