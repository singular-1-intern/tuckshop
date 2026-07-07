namespace Tuckshop.Api.Controllers
{
  using Neo.Model.Controllers;
  using Tuckshop.App.Services;
  using Tuckshop.Core.App.Services;
  using Tuckshop.Core.Models;
  using Tuckshop.Core.Models.Customers;
  using Tuckshop.Models;
  using Tuckshop.Models.Orders;

  /// <summary>
  /// The Orders Api Controller
  /// </summary>
  public class CustomersController : UpdateableControllerBase<Customer, ModelDbContext, int>
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="OrdersController"/> class.
    /// </summary>
    /// <param name="modelService">The </param>
    public CustomersController(CustomersModelService modelService)
      : base(modelService, o => o.CustomerId)
    {
    }
  }
}