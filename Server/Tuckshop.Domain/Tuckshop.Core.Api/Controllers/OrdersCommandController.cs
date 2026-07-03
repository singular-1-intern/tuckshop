namespace Tuckshop.Api.Controllers
{
  using System.Threading.Tasks;
  using Microsoft.AspNetCore.Mvc;
  using Tuckshop.App.Services;
  using Tuckshop.Core.Models;
  using Tuckshop.Models.Orders;
  using Tuckshop.Models.Orders.Commands;

  /// <summary>
  /// An Api Controller for Orders commands
  /// </summary>
  [ApiController]
  [Route("api/orders/commands")]
  public class OrdersCommandController : ControllerBase
  {
    private readonly OrdersCommandService ordersCommandService;

    /// <summary>
    /// Initializes a new instance of the <see cref="OrdersCommandController"/> class.
    /// </summary>
    /// <param name="ordersCommandService">The orders command service</param>
    public OrdersCommandController(
      OrdersCommandService ordersCommandService)
    {
      this.ordersCommandService = ordersCommandService;
    }

    /// <summary>
    /// Will create a new Order
    /// </summary>
    /// <param name="command">The entity to add.</param>
    /// <returns>The result.</returns>
    [HttpPost("create")]
    public virtual Task<Order> CreateOrder([FromBody] CreateOrder command)
    {
      return this.ordersCommandService.CreateOrderAsync(command);
    }

    /// <summary>
    /// Will complete an Order
    /// </summary>
    /// <param name="command">The entity to add.</param>
    /// <returns>The result.</returns>
    [Route("complete")]
    [HttpPut]
    public virtual Task CompleteOrder([FromBody] CompleteOrder command)
    {
      return this.ordersCommandService.CompleteOrderAsync(command);
    }

    /// <summary>
    /// Will cancel an Order
    /// </summary>
    /// <param name="command">The entity to add.</param>
    /// <returns>The result.</returns>
    [Route("cancel")]
    [HttpPut]
    public virtual Task CancelOrder([FromBody] CancelOrder command)
    {
      return this.ordersCommandService.CancelOrderAsync(command);
    }
  }
}