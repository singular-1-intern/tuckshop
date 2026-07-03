namespace Tuckshop.Core.App.Services
{
  using Neo.Model.Services;
  using Tuckshop.Core.Models;

  /// <summary>
  /// Service for reading, and saving models using tracking state from the client.
  /// This should only be used for the most basic entities. E.g. catalogue entities.
  /// </summary>
  /// <param name="dbContext">Db context on which to load / save data.</param>
  public class CatalogueModelService(ModelDbContext dbContext) : GenericModelService<ModelDbContext>(dbContext)
  {
  }
}