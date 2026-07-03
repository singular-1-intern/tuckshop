namespace Tuckshop.Notifications
{
  using Microsoft.EntityFrameworkCore;
  using Neo.BulkNotifications.Models;
  using Neo.Extensions;
  using Neo.Model.MultiTenancy;
  using Neo.Model.Processing;
  using Neo.NotificationServer;

  /// <summary>
  /// Notifications db context.
  /// </summary>
  /// <remarks>
  /// Initializes a new instance of the <see cref="NotificationsDbContext"/> class.
  /// </remarks>
  /// <param name="options">DbContext Options.</param>
  /// <param name="processingOptions">Processing Options.</param>
  /// <param name="tenantService">Tenant service.</param>
  public class NotificationsDbContext(
    DbContextOptions<NotificationsDbContext> options,
    DbContextProcessingOptions<NotificationsDbContext> processingOptions,
    ITenantService tenantService) : NotificationsDbContextBase<NotificationsDbContext>(options, processingOptions, tenantService), IBulkNotificationsDbContext
  {
    /// <summary>
    /// Key of the connection string for the notifications database.
    /// </summary>
    public const string ConnectionStringKey = "Notifications";

    /// <inheritdoc/>
    public DbSet<BulkNotificationTemplate> BulkNotificationTemplates { get; set; }

    /// <inheritdoc/>
    public DbSet<BulkNotification> BulkNotifications { get; set; }

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      base.OnModelCreating(modelBuilder);

      modelBuilder.SetupBulkNotificationsModels();
    }
  }
}
