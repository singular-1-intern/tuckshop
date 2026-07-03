namespace Tuckshop.Reporting
{
  using Microsoft.EntityFrameworkCore;
  using Neo.Model.MultiTenancy;
  using Neo.Model.Processing;
  using Neo.Reporting;
  using Neo.Reporting.Models;

  /// <summary>
  /// Reporting Db Context.
  /// </summary>
  /// <remarks>
  /// Initializes a new instance of the <see cref="ReportingDbContext"/> class.
  /// </remarks>
  /// <param name="options">The options.</param>
  /// <param name="processingOptions">The processing options.</param>
  /// <param name="tenantService">The tenant service.</param>
  public class ReportingDbContext(
    DbContextOptions<ReportingDbContext> options,
    DbContextProcessingOptions<ReportingDbContext> processingOptions,
    ITenantService tenantService) : PureReportingDbContextBase<ReportingDbContext>(options, processingOptions, tenantService), IUserLayoutsDbContext
  {
    /// <summary>
    /// Key of the connection string for the reporting database.
    /// </summary>
    public const string ConnectionStringKey = "Reporting";

    /// <inheritdoc />
    public DbSet<UserLayout> UserLayouts { get; set; }
  }
}