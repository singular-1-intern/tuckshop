#pragma warning disable
namespace Tuckshop.Models.Orders.Queries
{
  public class OrderDetailLookup
  {
    public string Product { get; set; }
    public decimal Price { get; set; }
    public decimal Value { get; set; }
    public decimal VAT { get; set; }
    public int Quantity { get; set; }
  }
}