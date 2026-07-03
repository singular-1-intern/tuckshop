#pragma warning disable IDE0290 // Use primary constructor
namespace Tuckshop.RazorReports
{
  using System;
  using System.Configuration;
  using System.Globalization;
  using System.IO;
  using System.Reflection;
  using System.Text;
  using Microsoft.AspNetCore.Html;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.Extensions.Configuration;

  /// <summary>
  /// Service which provides styles for reports.
  /// </summary>
  public class ReportStyleService
  {
    private readonly string basePath;

    private string? utilsClasses;

    /// <summary>
    /// DI constructor for report style service.
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    public ReportStyleService(IConfiguration configuration)
    {
      this.basePath = configuration.GetValue<string>("Routing:RootInternal") ?? throw new ConfigurationErrorsException("Routing:RootInternal must have a value.");
    }

    /// <summary>
    /// Returns an absolute path for the provided relative path.
    /// </summary>
    /// <param name="urlHelper">Url helper from razor page.</param>
    /// <param name="isPreview">True if the report is being displayed in a browser.</param>
    /// <param name="pathFromWebRoot">Path in wwwroot folder in reporting project.</param>
    /// <returns>The absolute path.</returns>
    public string AbsolutePath(IUrlHelper urlHelper, bool isPreview, string pathFromWebRoot)
    {
      string relativePath = $"/_content/Tuckshop.RazorReports/{pathFromWebRoot}";

      if (isPreview)
      {
        var request = urlHelper.ActionContext.HttpContext.Request;
        var absolutePath = urlHelper.Content($"~{relativePath}") ?? throw new InvalidOperationException("Could not generate path.");
        return $"{request.Scheme}://{request.Host}{absolutePath}";
      }
      else
      {
        return $"{this.basePath}{relativePath}";
      }
    }

    /// <summary>
    /// Renders style tags with common reporting styles.
    /// </summary>
    /// <param name="isScreenDisplay">Is the report being viewed on the screen as a continuous layout (not paged).</param>
    /// <returns>Style text.</returns>
    public HtmlString RenderReportingStyles(bool isScreenDisplay)
    {
      var assembly = this.GetType().Assembly;

      var stringBuilder = new StringBuilder();

      stringBuilder.AppendLine("<style type=\"text/css\">");

      AppendResource(stringBuilder, assembly, "Tuckshop.RazorReports.wwwroot.css.reporting.css");

      this.AppendUtils(stringBuilder);

      if (isScreenDisplay)
      {
        AppendResource(stringBuilder, assembly, "Tuckshop.RazorReports.wwwroot.css.reporting.screen.css");
      }

      stringBuilder.AppendLine("</style>");

      return new HtmlString(stringBuilder.ToString());
    }

    private static void AppendResource(StringBuilder stringBuilder, Assembly assembly, string resourceName)
    {
      using var stream = assembly.GetManifestResourceStream(resourceName) ?? throw new ArgumentException($"{resourceName} not found.");

      using var reader = new StreamReader(stream);
      stringBuilder.AppendLine(reader.ReadToEnd());
    }

    /// <summary>
    /// Creates bootstrap type margin and padding utils. E.g. mt-1 adds a small top margin.
    /// </summary>
    /// <param name="stringBuilder">The string builder.</param>
    private void AppendUtils(StringBuilder stringBuilder)
    {
      if (string.IsNullOrEmpty(this.utilsClasses))
      {
        var utilsBuilder = new StringBuilder();

        AppendDirectionUtils(utilsBuilder, ["0.5rem", "1rem", "1.5rem", "2rem", "3rem"], "m", "margin");
        AppendDirectionUtils(utilsBuilder, ["0.25rem", "0.5rem", "1rem", "1.5rem", "2rem"], "p", "padding");

        this.utilsClasses = utilsBuilder.ToString();
      }

      stringBuilder.AppendLine(this.utilsClasses);
    }

    private static void AppendDirectionUtils(StringBuilder stringBuilder, string[] units, string prefix, string name)
    {
      var directionPrefixes = new string[] { "t", "r", "b", "l" };
      var directionNames = new string[] { "top", "right", "bottom", "left" };

      for (var d = 0; d < directionPrefixes.Length; d++)
      {
        stringBuilder.AppendLine(CultureInfo.InvariantCulture, $".{prefix}{directionPrefixes[d]}-0 {{ {name}-{directionNames[d]}: 0 !important; }}");

        for (var u = 0; u < units.Length; u++)
        {
          stringBuilder.AppendLine(CultureInfo.InvariantCulture, $".{prefix}{directionPrefixes[d]}-{u + 1} {{ {name}-{directionNames[d]}: {units[u]}; }}");
        }
      }

      for (var u = 0; u < units.Length; u++)
      {
        stringBuilder.AppendLine(CultureInfo.InvariantCulture, $".{prefix}x-{u + 1} {{ {name}-left: {units[u]}; {name}-right: {units[u]}; }}");
        stringBuilder.AppendLine(CultureInfo.InvariantCulture, $".{prefix}y-{u + 1} {{ {name}-top: {units[u]}; {name}-bottom: {units[u]}; }}");
      }
    }
  }
}