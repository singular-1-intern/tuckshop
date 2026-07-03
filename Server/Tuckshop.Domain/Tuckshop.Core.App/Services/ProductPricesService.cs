namespace Tuckshop.App.Services
{
  using Microsoft.EntityFrameworkCore;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using Tuckshop.Core.Models;
  using Tuckshop.Models;

  /// <summary>
  /// Will retrieve product prices from the Model Db Context
  /// </summary>
  public class ProductPricesService : IProductPricesService
  {
    private ModelDbContext context;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProductPricesService"/> class.
    /// </summary>
    /// <param name="context">The db context</param>
    public ProductPricesService(ModelDbContext context)
    {
      this.context = context;
    }

    /// <inheritdoc/>
    public Task<Dictionary<int, decimal>> GetProductPricesAsync(ICollection<int> productIds)
    {
      return (from p in this.context.Products
              where productIds.Contains(p.ProductId)
              select new { p.ProductId, p.Price })
              .ToDictionaryAsync(pp => pp.ProductId, pp => pp.Price);
    }
  }
}