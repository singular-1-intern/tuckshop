namespace Tuckshop.Reporting.Migrations.Initializers
{
  using System;
  using System.Linq;
  using System.Threading;
  using System.Threading.Tasks;
  using Extensions.Hosting.AsyncInitialization;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.Extensions.Hosting;
  using Neo.Extensions;

  /// <summary>
  /// Will migrate the database and add test data if the environment is Development.
  /// </summary>
  /// <remarks>
  /// Initializes a new instance of the <see cref="ReportingDbAsyncInitializer"/> class.
  /// </remarks>
  /// <param name="environment">The web host environment.</param>
  /// <param name="dbContext">The reporting database context.</param>
  public class ReportingDbAsyncInitializer(IHostEnvironment environment, ReportingDbContext dbContext) : IAsyncInitializer
  {
    /// <inheritdoc/>
    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
      if (environment.IsDevelopment())
      {
        var pendingOperations = dbContext.GetMigrationOperations();

        if (pendingOperations.Any())
        {
          // To view the pending operations. Put a breakpoint on the throw line below, and add a watch for the pendingOperations variable.
          throw new InvalidOperationException($"Migrations are required on {nameof(ReportingDbContext)}.");
        }
      }

      await dbContext.Database.MigrateAsync(cancellationToken);
    }
  }
}
