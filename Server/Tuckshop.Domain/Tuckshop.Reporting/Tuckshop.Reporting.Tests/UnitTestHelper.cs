namespace Tuckshop.Tests
{
  using System;
  using System.Collections.Generic;
  using Microsoft.EntityFrameworkCore;
  using Neo.Model.MultiTenancy;
  using Neo.Model.Processing;
  using Neo.Model.Validation;
  using Neo.Testing;
  using Tuckshop.Core.Models.Identity;
  using Tuckshop.Reporting;

  /// <summary>
  /// The unit test helper class.
  /// </summary>
  public class UnitTestHelper
  {
    private readonly TestUserResolver<User> userResolver = new(1);

    /// <summary>
    /// Gets the model validator.
    /// </summary>
    public ModelValidator ModelValidator { get; } = new ModelValidator(new Neo.Model.Metadata.MetadataService());

    /// <summary>
    /// Initialise the unit test helper and the reporting database context.
    /// </summary>
    /// <returns>The unit test helper.</returns>
    public static UnitTestHelper InitWithContext()
    {
      var testHelper = new UnitTestHelper();
      testHelper.InitContext();
      return testHelper;
    }

    /// <summary>
    /// Initialise the reporting database context.
    /// </summary>
    /// <returns>The reporting database context.</returns>
    public ReportingDbContext InitContext()
    {
      DbContextOptionsBuilder<ReportingDbContext> builder =
        new DbContextOptionsBuilder<ReportingDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString());

      var processingOptions = new DbContextProcessingOptions<ReportingDbContext>(
      new List<IDbContextProcessor>() { new Neo.Model.AuditTrail.AuditTrailProcessor<User>(this.userResolver) });

      var context = new ReportingDbContext(builder.Options, processingOptions, new CustomTenantService());

      this.PopulateDbContext(context);

      return context;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Future use")]
    private void PopulateDbContext(ReportingDbContext context)
    {
    }
  }
}
