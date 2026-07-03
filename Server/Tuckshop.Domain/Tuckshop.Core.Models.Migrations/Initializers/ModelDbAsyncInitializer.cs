namespace Tuckshop.Core.Models.Migrations.Initializers
{
  using System.Threading;
  using System.Threading.Tasks;
  using Extensions.Hosting.AsyncInitialization;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.Extensions.Configuration;
  using Microsoft.Extensions.Logging;
  using Neo.Cryptography;
  using Neo.Extensions;

  /// <summary>
  /// Will migrate the database and add test data if the environment is Development.
  /// </summary>
  /// <remarks>
  /// Initializes a new instance of the <see cref="ModelDbAsyncInitializer"/> class.
  /// </remarks>
  /// <param name="dbContext">Db context.</param>
  /// <param name="configuration">The application configuration.</param>
  /// <param name="logger">The logger.</param>
  /// <param name="columnEncryptionService">The column encryption service.</param>
  /// <param name="encryptionConfigurationService">The column encryption configuration service.</param>
  public class ModelDbAsyncInitializer(
    ModelDbContext dbContext,
    IConfiguration configuration,
    ILogger<ModelDbAsyncInitializer> logger,
    IColumnEncryptionConfigurationService encryptionConfigurationService,
    IColumnEncryptionService columnEncryptionService) : IAsyncInitializer
  {
    private readonly ModelDbContext dbContext = dbContext;
    private readonly IConfiguration configuration = configuration;
    private readonly ILogger<ModelDbAsyncInitializer> logger = logger;
    private readonly IColumnEncryptionConfigurationService encryptionConfigurationService = encryptionConfigurationService;
    private readonly IColumnEncryptionService columnEncryptionService = columnEncryptionService;

    /// <inheritdoc/>
    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
      await this.dbContext.Database.MigrateAsync(cancellationToken);

      if (this.configuration.GetBoolean("AlwaysEncrypted:Enabled", false) ?? false)
      {
        await this.ConfigureDatabaseEncryptionAsync(cancellationToken);
      }
    }

    private async Task ConfigureDatabaseEncryptionAsync(CancellationToken cancellationToken)
    {
      // Initialise the database keys (CMK and CEK)
      cancellationToken.ThrowIfCancellationRequested();
      await this.encryptionConfigurationService.ConfigureDatabaseAsync(this.dbContext, "Main");
      cancellationToken.ThrowIfCancellationRequested();

      // Add column encryption logic here, e.g:

      //// Encrypt the Database Columns
      //logger.LogInformation("Encrypting database columns");

      //await this.columnEncryptionService.EncryptColumnsAsync<ModelName>(
      //  this.dbContext,
      //  modelName => modelName.Id,
      //  columnEncryptionBuilder =>
      //  {
      //    columnEncryptionBuilder.EncryptAllDecoratedColumns();
      //  });
    }
  }
}
