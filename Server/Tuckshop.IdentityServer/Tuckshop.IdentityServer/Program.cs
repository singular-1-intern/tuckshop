namespace Tuckshop.IdentityServer
{
  using System;
  using System.Collections.Generic;
  using System.Reflection;
  using System.Threading.Tasks;
  using Microsoft.AspNetCore.Hosting;
  using Microsoft.Extensions.Configuration;
  using Microsoft.Extensions.DependencyInjection;
  using Microsoft.Extensions.Hosting;
  using Microsoft.Extensions.Logging;
  using Neo.Extensions;
  using Neo.SecretVault;
  using Serilog;

  /// <summary>
  /// Represents the main Program.
  /// </summary>
  public sealed class Program
  {
    /// <summary>
    /// Application entry point.
    /// </summary>
    /// <param name="args">Startup args.</param>
    /// <returns>Nothing, the host should not stop.</returns>
    public static async Task Main(string[] args)
    {
      ClearArcManagedIdentityVariables();

      var host = CreateHostBuilder(args).Build();

      try
      {
        await host.InitAsync();
      }
      catch (Exception ex)
      {
        var logger = (ILogger<Program>)host.Services.GetRequiredService(typeof(ILogger<Program>));
        logger.LogError(ex, "Async Initialization Failure");
        throw;
      }

      host.Run();
    }

    /// <summary>
    /// Create the Host builder.
    /// </summary>
    /// <param name="args">The arguments.</param>
    /// <returns>The host builder.</returns>
    public static IHostBuilder CreateHostBuilder(string[] args) =>
      Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration((context, config) =>
        {
          IHostEnvironment environment = context.HostingEnvironment;
          var appSettings = new List<string> { environment.EnvironmentName };

          if (environment.IsDevStaging())
          {
            // DevStaging config layers on top of Development
            appSettings.Clear();
            appSettings.Add(Environments.Development);
            appSettings.Add("DevStaging");
          }

          if ((environment.IsDevelopment() || environment.IsDevStaging()) &&
              (Environment.GetEnvironmentVariable("DEBUG_ENVIRONMENT") == "Docker"))
          {
            // Docker config layers on top of Development or DevStaging
            appSettings.Add("Docker");
          }

          // Load the base appsettings.json, then layer the additional appsettings files on top of it
          config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
          foreach (string appSettingsFile in appSettings)
          {
            config.AddJsonFile($"appsettings.{appSettingsFile}.json", optional: true, reloadOnChange: true);
          }

          if (environment.IsDevelopment() || environment.IsDevStaging())
          {
            var appAssembly = Assembly.Load(new AssemblyName(environment.ApplicationName));
            if (appAssembly != null)
            {
              config.AddUserSecrets(appAssembly, optional: true);
            }
          }

          config.AddEnvironmentVariables();

          // We need to load the Key Vault secrets after environment variables to allow the Key Vault name to be loaded in scenarios where the app is deployed,
          // and the vault name is coming in via an environment variable. This should not be a problem because vault secrets shouldn't need to be overridden by
          // the config coming from environment variables, as that configuration should not contain any sensitive information.
          bool clearAzureAuthenticationVars = environment.IsDevelopment() || environment.IsDevStaging();
          config.AddNeoAzureKeyVault<DefaultSecretVaultConfigManager>(clearAzureAuthenticationVars);

          if (args != null)
          {
            config.AddCommandLine(args);
          }
        })
        .ConfigureWebHostDefaults(webBuilder =>
        {
          webBuilder
            .UseStartup<Startup>();
        })
        .UseSerilog();

    /// <summary>
    /// Clears the IDENTITY_ENDPOINT and IMDS_ENDPOINT environment variables when running in a local
    /// development environment. The Azure Arc Proxy (himds) sets these as system-level variables,
    /// causing DefaultAzureCredential to use ManagedIdentityCredential (which resolves to the wrong
    /// identity) instead of VisualStudioCredential. This must run before any Azure SDK code executes.
    /// </summary>
    private static void ClearArcManagedIdentityVariables()
    {
      var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                     ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");

      if (string.Equals(environment, Environments.Development, StringComparison.OrdinalIgnoreCase) ||
          string.Equals(environment, "DevStaging", StringComparison.OrdinalIgnoreCase))
      {
        Environment.SetEnvironmentVariable("IDENTITY_ENDPOINT", null);
        Environment.SetEnvironmentVariable("IMDS_ENDPOINT", null);
      }
    }
  }
}
