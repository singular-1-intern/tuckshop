namespace Tuckshop.Api.Controllers
{
  using Neo.Model.Controllers;
  using Tuckshop.Core.App.Services;
  using Tuckshop.Core.Models;

  /// <summary>
  ///   The Orders Api Controller. UpdateableCntroller base class provides a REST endpoint for CRUD.
  /// </summary>
  /* UpdateableControlleBase takes in 3 paramters: <Entity, Context, Key> */
  public class OrdersController : UpdateableControllerBase<Order, ModelDbContext, int>
  {
    /* Initializes a new instance of the <see cref="OrdersController"/> class. */
    /* <param name="modelService">The </param> */
    public OrdersController(OrdersModelService modelService)
      : base(modelService, o => o.OrderId)
    {
    }
  }
}