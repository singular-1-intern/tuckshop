namespace Tuckshop.Reporting.Reports.Example
{
  using System;
  using System.Collections.Generic;
  using Neo.Reporting;
  using Neo.Reporting.Pdf.Options;
  using Tuckshop.Reporting.Security;

  /// <summary>
  /// The example report.
  /// </summary>
  public class ExampleReport : ReportBase<ExampleReportBuilder, ExampleCriteria, List<ExampleReportLookup>>
  {
    /// <inheritdoc/>
    public override Enum RequireRole => Roles.General.View;

    /// <inheritdoc/>
    public override Enum RequireRoleForExcelDownload => Roles.General.Download;

    /// <inheritdoc/>
    public override bool EnablePdfDownload => true;

    /// <inheritdoc/>
    public override string GetReportViewName(object? criteria) => "/Areas/Reports/Views/Example.cshtml";

    /// <inheritdoc/>
    public override void ConfigurePdfReportOptions(PdfReportOptions options, IReportInstance reportInstance)
    {
      /*
       * TODO: If all your reports will have similar headers, footers and margins, 
       * move these options to the PdfReportOptionsService in Tuckshop.Reporting.App.
       */
      options.Headers.HeaderType = PdfHeaderType.SourceHtml;
      options.Footers.FooterType = PdfFooterType.SourceHtml;
      options.Margins.Top = 27;
      options.Margins.Bottom = 15;
      options.Margins.Left = 10;
      options.Margins.Right = 10;
    }
  }
}