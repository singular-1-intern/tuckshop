namespace Tuckshop.IdentityServer.Models.Migrations.Initializers
{
  using System.Threading;
  using System.Threading.Tasks;
  using Extensions.Hosting.AsyncInitialization;
  using Microsoft.EntityFrameworkCore;
  using Tuckshop.IdentityServer;

  /// <summary>
  /// Will initialize the Identity Server db.
  /// </summary>
  public class DatabaseAsyncInitializer : IAsyncInitializer
  {
    private readonly IdentityDbContext identityDbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseAsyncInitializer"/> class.
    /// </summary>
    /// <param name="identityDbContext">The Identity db context.</param>
    /// <param name="seedData">The seed data used to populate the database at the time it is created.</param>
    public DatabaseAsyncInitializer(
      IdentityDbContext identityDbContext)
    {
      this.identityDbContext = identityDbContext;
    }

    /// <inheritdoc/>
    public Task InitializeAsync(CancellationToken cancellationToken)
    {
      // migrate the DBs
      return this.MigrateDatabasesAsync(cancellationToken);
    }

    private Task MigrateDatabasesAsync(CancellationToken cancellationToken)
    {
      return this.identityDbContext.Database.MigrateAsync(cancellationToken);
    }
  }
}
