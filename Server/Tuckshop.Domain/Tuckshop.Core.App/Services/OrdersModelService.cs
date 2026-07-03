namespace Tuckshop.Core.App.Services
{
  using Neo.Model.Services;
  using Tuckshop.Core.Models;

  /// Service to working with the Orders aggregate. UpdateableModelService Provides basic CRUD functionality.
  public class OrdersModelService : UpdateableModelService<Order, ModelDbContext, int>
  {
    /* Initializes a new instance of the <see cref="OrdersModelService"/> class. */
    /* <param name="context">The db context</param> */
    public OrdersModelService(ModelDbContext context)
      // Read Tut 1, Step 5 for context on this line:
      : base(context, new ModelServiceOptions<Order>(o => o.OrderDetails))
    {
    }
  }
}