namespace Tuckshop.Reporting.Reports.Example
{
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using Neo.Reporting;

  /// <summary>
  /// Will build an ExampleReport model, using the criteria and any required Scoped Services.
  /// </summary>
  public class ExampleReportBuilder : IReportModelBuilder<ExampleCriteria, List<ExampleReportLookup>>
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="ExampleReportBuilder"/> class.
    /// </summary>
    public ExampleReportBuilder()
    {
      // Bring in and initialize any dependencies here
    }

    /// <inheritdoc/>
    public Task<List<ExampleReportLookup>> BuildModelAsync(ExampleCriteria? criteria, ReportOptions reportOptions)
    {
      // Map domain models to the report model here.
      return Task.FromResult(
        new List<ExampleReportLookup>()
        {
          new ExampleReportLookup() { UserId = 1, FirstName = "System", LastName = "User", JoinDate = new System.DateTime(2000, 1, 1), Salary = 100000 },
          new ExampleReportLookup() { UserId = 2, FirstName = "Super", LastName = "User", JoinDate = new System.DateTime(2010, 2, 1), Salary = 70000 },
          new ExampleReportLookup() { UserId = 3, FirstName = "John", LastName = "Doe", JoinDate = new System.DateTime(2020, 3, 1), Salary = 25000 },
          new ExampleReportLookup() { UserId = 4, FirstName = "Mr", LastName = "Thief", JoinDate = new System.DateTime(2022, 1, 1), Salary = -10000 },
        });
    }
  }
}