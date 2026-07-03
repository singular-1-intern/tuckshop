namespace Tuckshop.Reporting.App.Services
{
  using Neo.Reporting.Pdf.Options;

  /// <summary>
  /// Service used to configure global pdf options.
  /// </summary>
  public class PdfReportOptionsService : Neo.Reporting.Pdf.IPdfReportOptionsService
  {
    /// <inheritdoc/>
    public void ConfigureOptions(PdfReportOptions options)
    {
      // Set global options here.
      options.PostProcessingOptions = new PostProcessingOptions()
      {
        RemoveDuplicateStreams = true,
        // RemoveEmbeddedFonts = true, // Uncomment to reduce pdf size at the cost of portability.
      };

      options.DocumentOptions.RenderDelayType = RenderDelayType.JavascriptCallback;
    }
  }
}