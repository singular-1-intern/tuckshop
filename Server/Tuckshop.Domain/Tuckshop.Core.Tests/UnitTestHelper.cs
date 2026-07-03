namespace Tuckshop.Core.Tests
{
  using System;
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using Microsoft.EntityFrameworkCore;
  using Neo.Model.Metadata;
  using Neo.Model.Processing;
  using Neo.Model.Validation;
  using Neo.Testing;
  using Neo.Testing.Identity;
  using Tuckshop.Core.Models;
  using Tuckshop.Core.Models.Identity;
  using Tuckshop.Core.Models.Initializers;

  /// <summary>
  /// The unit test helper class.
  /// </summary>
  public class UnitTestHelper
  {
    private ModelDbContext? dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="UnitTestHelper"/> class.
    /// </summary>
    public UnitTestHelper()
    {
      this.ModelValidator = new ModelValidator(this.MetadataService);
      this.NotificationService = new();
    }

    /// <summary>
    /// Gets the metadata service.
    /// </summary>
    public IMetadataService MetadataService { get; } = new MetadataService();

    /// <summary>
    /// Gets the model validator.
    /// </summary>
    public ModelValidator ModelValidator { get; }

    /// <summary>
    /// Gets the user resolver.
    /// </summary>
    public TestUserResolver<User> UserResolver { get; } = new TestUserResolver<User>(1);

    /// <summary>
    /// Gets the notification service.
    /// </summary>
    public MockNotificationService NotificationService { get; }

    /// <summary>
    /// Gets the db context. You must call <see cref="InitContextAsync"/> before accessing this property.
    /// </summary>
    public ModelDbContext DbContext
    {
      get
      {
        if (this.dbContext == null)
        {
          throw new InvalidOperationException("You must call InitContextAsync before accessing the DbContext property.");
        }

        return this.dbContext;
      }
    }

    /// <summary>
    /// Create the test helper and initialise the db context.
    /// </summary>
    /// <param name="generateSeedData">A value indicating whether to generate seed data.</param>
    /// <returns>The unit test helper.</returns>
    public static async Task<UnitTestHelper> InitWithContextAsync(bool generateSeedData = false)
    {
      var testHelper = new UnitTestHelper();
      var dbContext = testHelper.InitContext();
      if (generateSeedData)
      {
        await testHelper.PopulateSeedDataAsync(dbContext);
      }
      return testHelper;
    }

    /// <summary>
    /// Create the test helper and initialise the db context.
    /// </summary>
    /// <returns>The unit test helper.</returns>
    public static UnitTestHelper InitWithContext()
    {
      var testHelper = new UnitTestHelper();
      testHelper.InitContext();
      return testHelper;
    }

    /// <summary>
    /// Initialise the model database context.
    /// </summary>
    /// <returns>The model database context.</returns>
    public ModelDbContext InitContext()
    {
      DbContextOptionsBuilder<ModelDbContext> builder =
        new DbContextOptionsBuilder<ModelDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString());

      var processingOptions = new DbContextProcessingOptions<ModelDbContext>(
      new List<IDbContextProcessor>() { new Neo.Model.AuditTrail.AuditTrailProcessor<User>(this.UserResolver) });

      ModelDbContext context = new(builder.Options, processingOptions);
      context.Database.EnsureCreated();

      this.dbContext = context;
      return context;
    }

    /// <summary>
    /// Populates the dbContext with seed data using the main projects seed data initializer.
    /// </summary>
    /// <param name="context">The model database context.</param>
    public async Task PopulateSeedDataAsync(ModelDbContext context)
    {
      var systemUserService = new SystemUserServiceMock<User>(this.UserResolver);
      await new SeedDataAsyncInitializer(context, systemUserService, null, null).InitializeAsync(default);

      context.ChangeTracker.Clear();
    }
  }
}