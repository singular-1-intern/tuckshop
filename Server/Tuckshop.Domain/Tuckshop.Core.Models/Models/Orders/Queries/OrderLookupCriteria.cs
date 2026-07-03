namespace Tuckshop.Models.Orders.Queries
{
  using System;
  using Tuckshop.Models.Orders.Enums;

  /// <summary>
  /// The criteria for querying orders
  /// </summary>
  public class OrderLookupCriteria
  {
    /// <summary>
    /// Gets or sets the Order Status filter value
    /// </summary>
    public OrderStatus? OrderStatus { get; set; }

    /// <summary>
    /// Gets or sets the Start Date filter value
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// Gets or sets the End Date filter value
    /// </summary>
    public DateTime? EndDate { get; set; }
  }
}