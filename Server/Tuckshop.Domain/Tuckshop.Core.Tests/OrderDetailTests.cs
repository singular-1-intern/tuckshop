namespace Tuckshop.Tests
{
  using Tuckshop.Core.Models;
  using Xunit;

  public class OrderDetailTests
  {
    [Theory]
    [InlineData(1, 10, 10, 1.30)]
    [InlineData(1, 9, 9, 1.17)]
    [InlineData(100, 5, 500, 65.22)]
    public void ValueAndVATCorrect(int quantity, decimal price, decimal correctValue, decimal correctVAT)
    {
      var order = new Order(string.Empty);
      var orderDetail = order.AddDetail(1, quantity, price);
      Assert.Equal(correctValue, orderDetail.Value);
      Assert.Equal(correctVAT, orderDetail.VAT);
    }
  }
}

