namespace Tuckshop.RazorReports
{
  using Microsoft.AspNetCore.Mvc.Razor;
  using Neo.Reporting;

  /// <summary>
  /// Base class for reports.
  /// </summary>
  /// <typeparam name="TCriteria">The criteria type.</typeparam>
  /// <typeparam name="TModel">The model type.</typeparam>
  public abstract class ReportView<TCriteria, TModel> : RazorPage<ReportInstance<TCriteria, TModel>>
    where TCriteria : notnull
    where TModel : class
  {
    /// <summary>
    /// Gets the report data.
    /// </summary>
    public TModel Data => this.Model.ReportModel;

    /// <summary>
    /// Gets the report criteria.
    /// </summary>
    public TCriteria Criteria => this.Model.Criteria;

    /// <summary>
    /// Gets the report instance.
    /// </summary>
    public ReportInstance<TCriteria, TModel> Report => this.Model;

    /// <summary>
    /// Gets the report options.
    /// </summary>
    public ReportOptions Options => this.Report.Options;
  }
}