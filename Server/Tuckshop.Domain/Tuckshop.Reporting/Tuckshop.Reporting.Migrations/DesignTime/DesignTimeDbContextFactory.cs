namespace Tuckshop.Reporting.Migrations.DesignTime
{
  using System.IO;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.EntityFrameworkCore.Design;
  using Microsoft.Extensions.Configuration;
  using Neo.Model.MultiTenancy;

  /// <summary>
  /// Class to construct the DbContext at design time.
  /// </summary>
  public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ReportingDbContext>
  {
    /// <inheritdoc />
    public ReportingDbContext CreateDbContext(string[] args)
    {
      var configuration = new ConfigurationBuilder()
       .SetBasePath(Path.GetFullPath("..\\..\\Tuckshop.Core.Api", Directory.GetCurrentDirectory()))
       .AddJsonFile("appsettings.development.json")
       .Build();

      var builder = new DbContextOptionsBuilder<ReportingDbContext>();

      builder.UseSqlServer(
        configuration.GetConnectionString(ReportingDbContext.ConnectionStringKey),
        builder => builder.MigrationsAssembly(typeof(DesignTimeDbContextFactory).Assembly.GetName().Name));

      return new ReportingDbContext(
        builder.Options,
        new Neo.Model.Processing.DbContextProcessingOptions<ReportingDbContext>(),
        new CustomTenantService());
    }
  }
}
