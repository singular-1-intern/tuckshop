using Tuckshop.Core.Models.Identity;
using Neo.Identity;
using System;
using System.Linq;
using System.Threading.Tasks;
using Tuckshop.App.Services;
using Tuckshop.Core.App.Services;
using Tuckshop.Core.Models;
using Tuckshop.Models.Orders.Commands;

public class OrdersCommandService
{
  private readonly OrdersModelService modelService;
  private readonly CustomersModelService customersModelService;
  private readonly IProductPricesService priceService;
  private readonly IUserResolver<User> userResolver;

  /// <summary>
  /// Initializes a new instance of the <see cref="OrdersCommandService"/> class.
  /// </summary>
  /// <param name="modelService">The orders model service</param>
  /// <param name="customersModelService"></param>
  /// <param name="priceService">The price service</param>
  /// <param name="userResolver">The user resolver</param>
  public OrdersCommandService(
    // Hanldes db opr for Orders
    OrdersModelService modelService,
     // jdf
     CustomersModelService customersModelService,
    // Calcs/Retrieves product prices
    IProductPricesService priceService,
    // Gets info about the currently logged-in user
    IUserResolver<User> userResolver)
  {
    this.modelService = modelService;
    this.customersModelService = customersModelService;
    this.priceService = priceService;
    this.userResolver = userResolver;
  }

  /// <summary>
  /// Will create a new order with the provided command.
  /// </summary>
  /// <param name="command">The create order command.</param>
  /// <returns>A task awaiting the order creation.</returns>
  public async Task<Order> CreateOrderAsync(CreateOrder command)
  {
    Order order = await this.CreateOrderEntityAsync(command).ConfigureAwait(false);
    this.modelService.AddEntity(order);
    await this.modelService.SaveAsync(order).ConfigureAwait(false);
    return order;
  }

  private async Task<Order> CreateOrderEntityAsync(CreateOrder command)
  {
    // Lookup the customer ID, in order to return the matching name
    var customer = await this.customersModelService.GetByIdAsync(command.CustomerId).ConfigureAwait(false);

    // Use the CustomerName from the CustomerId we just fetched
    var order = new Order(customer.CustomerName);

    // Extract all unique productIds from the order details into a HashSet
    var productIds = command.OrderDetails.Select(od => od.ProductId).ToHashSet();

    // Use our GetProductPricesAsync class to get all product prices at once
    var prices = await this.priceService.GetProductPricesAsync(productIds).ConfigureAwait(false);

    foreach (var od in command.OrderDetails)
    {
      order.AddDetail(od.ProductId, od.Quantity, prices[od.ProductId]);
    }

    return order;
  }
  /// <summary>
  /// Will complete the order from the provided command
  /// </summary>
  /// <param name="command">The complete order command</param>
  /// <returns>A task awaiting the order completion</returns>
  public async Task CompleteOrderAsync(CompleteOrder command)
  {
    await this.ProcessUserEvent(
              command.OrderId,
              (order, user) => order.Complete(user.UserId))
            .ConfigureAwait(false);
  }

  /// <summary>
  /// Will cancel the order from the provided command.
  /// </summary>
  /// <param name="command">The cancel order command.</param>
  /// <returns>A task awaiting the order cancellation.</returns>
  public async Task CancelOrderAsync(CancelOrder command)
  {
    await this.ProcessUserEvent(
              command.OrderId,
              (order, user) => order.Cancel(user.UserId, command.Reason))
            .ConfigureAwait(false);
  }

  private async Task ProcessUserEvent(int orderId, Action<Order, User> handler)
  {
    var order = await this.modelService.GetByIdAsync(orderId).ConfigureAwait(false);
    var user = await this.userResolver.GetUserAsync().ConfigureAwait(false);
    handler(order, user);
    await this.modelService.SaveChangesAsync().ConfigureAwait(false);
  }
}
