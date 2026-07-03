namespace Tuckshop.Core.Models.Migrations.DesignTime
{
  using System.IO;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.EntityFrameworkCore.Design;
  using Microsoft.Extensions.Configuration;
  using Tuckshop.Core.Models;

  /// <summary>
  /// Class to construct the DbContext at design time.
  /// </summary>
  public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ModelDbContext>
  {
    /// <inheritdoc />
    public ModelDbContext CreateDbContext(string[] args)
    {
      var configuration = new ConfigurationBuilder()
       .SetBasePath(Path.GetFullPath("..\\Tuckshop.Core.Api", Directory.GetCurrentDirectory()))
       .AddJsonFile("appsettings.development.json")
       .Build();

      var builder = new DbContextOptionsBuilder<ModelDbContext>();

      builder.UseSqlServer(
        configuration.GetConnectionString("Main"),
        builder => builder.MigrationsAssembly(typeof(DesignTimeDbContextFactory).Assembly.GetName().Name));

      return new ModelDbContext(builder.Options, new Neo.Model.Processing.DbContextProcessingOptions<ModelDbContext>());
    }
  }
}