namespace Tuckshop.AuthorisationServer.Migrations.DesignTime
{
  using System.IO;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.EntityFrameworkCore.Design;
  using Microsoft.Extensions.Configuration;
  using Neo.Model.MultiTenancy;
  using Tuckshop.AuthorisationServer.Models;

  /// <summary>
  /// Class to construct the DbContext at design time.
  /// </summary>
  public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AuthorisationDbContext>
  {
    /// <inheritdoc />
    public AuthorisationDbContext CreateDbContext(string[] args)
    {
      IConfigurationRoot configuration = new ConfigurationBuilder()
       .SetBasePath(Path.GetFullPath("..\\..\\Tuckshop.Core.Api", Directory.GetCurrentDirectory()))
       .AddJsonFile("appsettings.development.json")
       .Build();

      var builder = new DbContextOptionsBuilder<AuthorisationDbContext>();

      builder.UseSqlServer(
        configuration.GetConnectionString(AuthorisationDbContext.ConnectionStringKey),
        builder => builder.MigrationsAssembly(typeof(DesignTimeDbContextFactory).Assembly.GetName().Name));

      return new AuthorisationDbContext(
        builder.Options,
        new Neo.Model.Processing.DbContextProcessingOptions<AuthorisationDbContext>(),
        new CustomTenantService());
    }
  }
}