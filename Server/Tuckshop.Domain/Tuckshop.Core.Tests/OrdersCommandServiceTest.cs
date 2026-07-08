namespace Tuckshop.Core.Tests
{
  using Tuckshop.Core.Models.Identity;
  using Neo.Testing;
  using System;
  using System.Collections.Generic;
  using System.Text;
  using System.Threading.Tasks;
  using Tuckshop.App.Services;
  using Tuckshop.Core.App.Services;
  using Tuckshop.Core.Models;
  using Tuckshop.Core.Models.Customers;
  using Tuckshop.Models.Orders.Commands;
  using Xunit;

  public class OrdersCommandServiceTest
  {
    private ModelDbContext context;

    [Fact]
    public async Task CreateOrderAsync()
    {
      var service = await this.CreateOrdersCommandServiceAsync();

      // Create a test customer
      var customer = new Customer(1, "Test Customer");
      this.context.Customers.Add(customer);
      await this.context.SaveChangesAsync();
      this.context.DetachAllEntities();

      Order order = await this.CreateOrderWithCommandAsync(service, customer.CustomerId);
      Assert.Equal("Test Customer", order.CustomerName);
      Assert.Equal(2, order.OrderDetails.Count);
      Assert.Null(order.Completed.On);
      Assert.Null(order.Completed.By);
      Assert.Null(order.Cancelled.On);
      Assert.Null(order.Cancelled.By);
    }

    private async Task<Order> CreateOrderWithCommandAsync(
      OrdersCommandService service,
      int customerId)
    {
      var createCommand = new CreateOrder()
      {
        CustomerId = customerId,
        OrderDetails = new List<CreateOrder.NewOrderDetail>()
        {
          new CreateOrder.NewOrderDetail() { ProductId = 1, Quantity = 5 },
          new CreateOrder.NewOrderDetail() { ProductId = 2, Quantity = 3 },
        }
      };

      var order = await service.CreateOrderAsync(createCommand).ConfigureAwait(false);
      this.context.DetachAllEntities();
      return order;
    }

    private async Task<OrdersCommandService> CreateOrdersCommandServiceAsync()
    {
      var unitTestHelper = await UnitTestHelper.InitWithContextAsync().ConfigureAwait(false);
      this.context = unitTestHelper.DbContext;
      var ordersModelService = new OrdersModelService(this.context);
      var customersModelService = new CustomersModelService(this.context);
      var priceService = new ProductPricesService(this.context);
      var userResolver = new TestUserResolver<User>(1);
      var service = new OrdersCommandService(ordersModelService, customersModelService, priceService, userResolver);
      return service;
    }

    [Fact]
    public async Task CompleteOrderAsync()
    {
      OrdersCommandService service = await CreateOrdersCommandServiceAsync().ConfigureAwait(false);

      // Create a test customer
      var customer = new Customer(2, "Complete Customer");
      this.context.Customers.Add(customer);
      await this.context.SaveChangesAsync();
      this.context.DetachAllEntities();

      Order order = await CreateOrderWithCommandAsync(service, customer.CustomerId).ConfigureAwait(false);

      var completeCommand = new CompleteOrder()
      {
        OrderId = order.OrderId,
      };
      await service.CompleteOrderAsync(completeCommand).ConfigureAwait(false);
      this.context.DetachAllEntities();

      // Assert that it is completed
      var completedOrder = await this.context.Orders.FindAsync(order.OrderId).ConfigureAwait(false);
      Assert.NotNull(completedOrder);
      Assert.NotNull(completedOrder.Completed.On);
      Assert.NotNull(completedOrder.Completed.By);
    }

    [Fact]
    public async Task CancelOrderAsync()
    {
      OrdersCommandService service = await CreateOrdersCommandServiceAsync().ConfigureAwait(false);

      // Create a test customer
      var customer = new Customer(3, "Cancel Customer");
      this.context.Customers.Add(customer);
      await this.context.SaveChangesAsync();
      this.context.DetachAllEntities();

      Order order = await CreateOrderWithCommandAsync(service, customer.CustomerId).ConfigureAwait(false);

      var cancelCommand = new CancelOrder()
      {
        OrderId = order.OrderId,
        Reason = "Insufficient stock",
      };
      await service.CancelOrderAsync(cancelCommand).ConfigureAwait(false);
      this.context.DetachAllEntities();

      // Assert that it is completed
      var cancelledOrder = await this.context.Orders.FindAsync(order.OrderId).ConfigureAwait(false);
      Assert.NotNull(cancelledOrder);
      Assert.NotNull(cancelledOrder.Cancelled.On);
      Assert.NotNull(cancelledOrder.Cancelled.By);
      Assert.Equal("Insufficient stock", cancelledOrder.Cancelled.Reason);
    }
  }
}
