#pragma warning disable
namespace Tuckshop.Models.Orders.Queries
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using Tuckshop.Core.Models;

  public class OrderLookup
  {
    public int OrderId { get; set; }
    public string CustomerName { get; set; }
    public DateTime OrderedOn { get; set; }
    public DateTime? CompletedOn { get; set; }
    public DateTime? CancelledOn { get; set; }
    public string CancelledReason { get; set; }
    public string CompletedBy { get; set; }
    public string CancelledBy { get; set; }
    public decimal OrderTotalExcl { get; set; }
    public decimal OrderTotal { get; set; }
    public List<OrderDetailLookup> Items { get; set; }

    /// <summary>
    /// Sets the <see cref="Items"/> property, and returns the order.
    /// </summary>
    /// <param name="orderDetails">Order details.</param>
    /// <returns>This instance.</returns>
    public OrderLookup WithDetails(IEnumerable<OrderDetailLookup> orderDetails)
    {
      this.OrderTotal = orderDetails.Sum(c => c.Value);
      this.OrderTotalExcl = orderDetails.Sum(c => c.Value - c.VAT);
      this.Items = orderDetails.ToList();
      return this;
    }
  }
}