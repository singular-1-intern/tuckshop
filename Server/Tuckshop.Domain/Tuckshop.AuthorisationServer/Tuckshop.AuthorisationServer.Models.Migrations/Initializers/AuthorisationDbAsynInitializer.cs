namespace Tuckshop.AuthorisationServer.Models
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
  /// Initializes a new instance of the <see cref="AuthorisationDbAsyncInitializer"/> class.
  /// </remarks>
  /// <param name="context">The db context.</param>
  /// <param name="environment">The web host environment.</param>
  public class AuthorisationDbAsyncInitializer(AuthorisationDbContext context, IHostEnvironment environment) : IAsyncInitializer
  {
    /// <inheritdoc/>
    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
      if (environment.IsDevelopment())
      {
        var pendingOperations = context.GetMigrationOperations();

        if (pendingOperations.Any())
        {
          // To view the pending operations. Put a breakpoint on the throw line below, and add a watch for the pendingOperations variable.
          throw new InvalidOperationException($"Migrations are required on {nameof(AuthorisationDbContext)}.");
        }
      }

      await context.Database.MigrateAsync(cancellationToken);
    }
  }
}