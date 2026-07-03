namespace Tuckshop.App.Services
{
  using System.Collections.Generic;
  using System.Threading.Tasks;

  /// <summary>
  /// An interface for retrieving a products price
  /// </summary>
  public interface IProductPricesService
  {
    /// <summary>
    /// Will get the price for the provided list of product Ids
    /// </summary>
    /// <param name="productIds">The product Ids</param>
    /// <returns>A dictionary of product prices
    /// </returns>
    Task<Dictionary<int, decimal>> GetProductPricesAsync(ICollection<int> productIds);
  }
}