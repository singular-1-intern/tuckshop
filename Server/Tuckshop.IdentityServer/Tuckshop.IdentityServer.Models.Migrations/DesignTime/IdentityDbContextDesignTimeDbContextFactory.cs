namespace Tuckshop.IdentityServer.Models.Migrations.DesignTime
{
  using System.Collections.Generic;
  using System.IO;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.EntityFrameworkCore.Design;
  using Microsoft.Extensions.Configuration;
  using Neo.IdentityServer.Models.OpenIddict;
  using Neo.Model.Processing;
  using Tuckshop.IdentityServer;

  /// <summary>
  /// Class to construct the IdentityDbContext at design time.
  /// </summary>
  public class IdentityDbContextDesignTimeDbContextFactory : IDesignTimeDbContextFactory<IdentityDbContext>
  {
    /// <inheritdoc />
    public IdentityDbContext CreateDbContext(string[] args)
    {
      IConfigurationRoot configuration = new ConfigurationBuilder()
       .SetBasePath(Directory.GetCurrentDirectory())
       .AddJsonFile("appsettings.Development.json")
       .Build();
      var builder = new DbContextOptionsBuilder<IdentityDbContext>();
      var connectionString = configuration.GetConnectionString("Main");
      builder.UseSqlServer(connectionString, builder => builder.MigrationsAssembly(typeof(IdentityDbContextDesignTimeDbContextFactory).Assembly.GetName().Name));
      builder.UseOpenIddict<OpenIddictClientApplication, OpenIddictAuthorization, OpenIddictScope, OpenIddictToken, long>();
      return new IdentityDbContext(builder.Options, new List<IDbContextProcessor>());
    }
  }
}
