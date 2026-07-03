namespace Tuckshop.Notifications.App.Security
{
  using Microsoft.AspNetCore.Mvc.ApplicationModels;
  using Neo.NotificationServer.Api.Security;

  /// <summary>
  /// Convention for overriding the api security of notification server.
  /// </summary>
  public class NotificationsApiConvention : NotificationServerApiRoleConvention
  {
    /// <inheritdoc/>
    protected override void AddMergeTemplatesFilter(ActionModel action)
    {
      // By default, any authenticated user can merge templates.
      base.AddMergeTemplatesFilter(action);
    }

    /// <inheritdoc/>
    protected override void AddSendNotificationsFilter(ActionModel action)
    {
      // By default, only other services can call the api to send notifications.
      base.AddSendNotificationsFilter(action);
    }
  }
}