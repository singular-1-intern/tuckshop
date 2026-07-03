namespace Tuckshop.Models.Orders.Commands
{
  /// <summary>
  /// Represents a Complete Order command
  /// </summary>
  public class CompleteOrder
  {
    /// <summary>
    /// Gets or sets the Order Id to complete
    /// </summary>
    public int OrderId { get; set; }
  }
}