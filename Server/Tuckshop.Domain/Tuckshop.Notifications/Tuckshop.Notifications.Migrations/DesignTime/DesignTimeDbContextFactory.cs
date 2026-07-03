namespace Tuckshop.Notifications.Migrations.DesignTime
{
  using System.IO;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.EntityFrameworkCore.Design;
  using Microsoft.Extensions.Configuration;
  using Neo.Model.MultiTenancy;
  using Tuckshop.Notifications;

  /// <summary>
  /// Class to construct the DbContext at design time.
  /// </summary>
  public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<NotificationsDbContext>
  {
    /// <inheritdoc />
    public NotificationsDbContext CreateDbContext(string[] args)
    {
      var configuration = new ConfigurationBuilder()
       .SetBasePath(Path.GetFullPath("..\\..\\Tuckshop.Core.Api", Directory.GetCurrentDirectory()))
       .AddJsonFile("appsettings.development.json")
       .Build();

      var builder = new DbContextOptionsBuilder<NotificationsDbContext>();

      builder.UseSqlServer(
        configuration.GetConnectionString(NotificationsDbContext.ConnectionStringKey),
        builder => builder.MigrationsAssembly(typeof(DesignTimeDbContextFactory).Assembly.GetName().Name));

      return new NotificationsDbContext(
        builder.Options,
        new Neo.Model.Processing.DbContextProcessingOptions<NotificationsDbContext>(),
        new CustomTenantService());
    }
  }
}
