namespace Tuckshop.AuthorisationServer.Models
{
  using Microsoft.EntityFrameworkCore;
  using Neo.AuthorisationServer.Models;
  using Neo.Model.MultiTenancy;
  using Neo.Model.Processing;

  /// <summary>
  /// Initializes a new instance of the <see cref="AuthorisationDbContext"/> class.
  /// </summary>
  /// <param name="options">The options.</param>
  /// <param name="processingOptions">The processing options.</param>
  /// <param name="tenantService">The tenant service.</param>
  public class AuthorisationDbContext(
    DbContextOptions<AuthorisationDbContext> options,
    DbContextProcessingOptions<AuthorisationDbContext> processingOptions,
    ITenantService tenantService) : ModelDbContextBase<AuthorisationDbContext, TuckshopAuthorisationUser>(options, processingOptions, tenantService)
  {
    /// <summary>
    /// Key of the connection string for the authorisation database.
    /// </summary>
    public const string ConnectionStringKey = "Authorisation";
  }
}