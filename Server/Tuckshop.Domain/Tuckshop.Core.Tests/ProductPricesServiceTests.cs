namespace Tuckshop.Core.Tests
{
  using Microsoft.EntityFrameworkCore;
  using System.Linq;
  using System.Threading.Tasks;
  using Tuckshop.App.Services;
  using Tuckshop.Core.Models;
  using Xunit;

  public class ProductPricesServiceTests
  {
    [Fact]
    public async Task GetProductPricesReturnAsync()
    {
      var unitTestHelper = await UnitTestHelper.InitWithContextAsync().ConfigureAwait(false);
      var context = unitTestHelper.DbContext;
      var priceService = new ProductPricesService(context);

      var products = await context.Products.ToListAsync().ConfigureAwait(false);
      var prices = await priceService.GetProductPricesAsync(products.Select(p => p.ProductId).ToHashSet()).ConfigureAwait(false);
      var expectedPrices = products.ToDictionary(p => p.ProductId, p => p.Price);

      Assert.Equal(expectedPrices, prices);
    }
  }
}