using System;

public class OrdersCommandServiceTests
{
	public OrdersCommandServiceTests()
	{
    [Fact]
    public async void CreateOrderAsync()
    {
      var unitTestHelper = new UnitTestHelper();
      var context = unitTestHelper.InitContext();
      var modelService = new OrdersModelService(context);
      var priceService = new ProductPricesService(context);
      var userResolver = new TestUserResolver<User>(1);
      var service = new OrdersCommandService(modelService, priceService, userResolver);

      var cmd = new CreateOrder()
      {
        CustomerName = "Create Cmd",
        OrderDetails = new List<CreateOrder.NewOrderDetail>()
        {
          new CreateOrder.NewOrderDetail() { ProductId = 1, Quantity = 5 },
          new CreateOrder.NewOrderDetail() { ProductId = 2, Quantity = 3 },
        }
      };

      var order = await service.CreateOrderAsync(cmd).ConfigureAwait(false);
      Assert.Equal("Create Cmd", order.CustomerName);
      Assert.Equal(2, order.OrderDetails.Count);
      Assert.Null(order.Completed.On);
      Assert.Null(order.Completed.By);
      Assert.Null(order.Cancelled.On);
      Assert.Null(order.Cancelled.By);
    }
    }
  }
}
