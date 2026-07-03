namespace Tuckshop.Extensions
{
  using System;
  using System.IO;
  using Microsoft.AspNetCore.Hosting;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.Extensions.Configuration;
  using Microsoft.Extensions.DependencyInjection;
  using Microsoft.Extensions.FileProviders;
  using Microsoft.Extensions.Hosting;
  using Neo.Extensions.DependencyInjection;
  using Neo.Identity;
  using Neo.Reporting.Pdf;
  using Neo.Reporting.PdfSharp.PostProcessing;
  using Tuckshop.RazorReports;
  using Tuckshop.Reporting;
  using Tuckshop.Reporting.App.Services;
  using Tuckshop.Reporting.Migrations.Initializers;

  /// <summary>
  /// Reporting server setup.
  /// </summary>
  public static class ReportingServerSetup
  {
    /// <summary>
    /// Add the reporting services to the main apps service collection.
    /// </summary>
    /// <typeparam name="TUser">User type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="env">The web host environment.</param>
    /// <param name="configuration">The configuration.</param>
    public static void AddTuckshopReportingServices<TUser>(this IServiceCollection services, IWebHostEnvironment env, IConfiguration configuration)
      where TUser : class, IClientUser, new()
    {
      services.AddNeoDbContext<ReportingDbContext>(
        options => options.UseSqlServer(
          configuration.GetConnectionString(ReportingDbContext.ConnectionStringKey),
          builder => builder.MigrationsAssembly(typeof(ReportingDbAsyncInitializer).Assembly.GetName().Name)));

      services.AddAsyncInitializer<ReportingDbAsyncInitializer>();

      services.AddHostedReportingWithNotifications<ReportingDbContext, TUser>(
        env,
        configuration,
        typeof(ReportingDbContext).Assembly,
        new Neo.Reporting.ReportingStartupOptions()
          .WithDbCache<ReportingDbContext>()
          .WithUserLayouts<ReportingDbContext>());

      services.AddIronPdfCreator(new StartupOptions()
        .WithPdfReportOptionsService<PdfReportOptionsService>()
        .WithPdfPostProcessing<PdfSharpPostProcessor>());

      services.AddSingleton<ReportStyleService>();

      IronPdf.License.LicenseKey = configuration["IronPdf.LicenseKey"];
      IronPdf.Installation.LinuxAndDockerDependenciesAutoConfig = false;
      IronPdf.Installation.AutomaticallyDownloadNativeBinaries = false;
    }

    /// <summary>
    /// Adds the reporting controllers.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="env">The web host environment.</param>
    /// <param name="configuration">The configuration.</param>
    /// <param name="mvcBuilder">The MVC builder.</param>
    public static void AddTuckshopReportingMvc(this IServiceCollection services, IWebHostEnvironment env, IConfiguration configuration, IMvcBuilder mvcBuilder)
    {
      services.AddReportingMvc(env, configuration, mvcBuilder);

      if (env.IsDevelopment() && (Environment.GetEnvironmentVariable("DEBUG_ENVIRONMENT") != "Docker"))
      {
        // Allow editing of cshtml files while the app is running.
        mvcBuilder.AddRazorRuntimeCompilation(options =>
        {
          var libraryPath = Path.GetFullPath(
              Path.Combine(env.ContentRootPath, "..\\Tuckshop.Reporting\\Tuckshop.RazorReports"));
          options.FileProviders.Add(new PhysicalFileProvider(libraryPath));
        });
      }
    }
  }
}