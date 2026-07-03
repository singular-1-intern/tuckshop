namespace Tuckshop.Models.Orders.Enums
{
  /// <summary>
  /// Order status query enum
  /// </summary>
  public enum OrderStatus
  {
    /// <summary>
    /// A non-completed and non-cancelled order
    /// </summary>
    Pending,

    /// <summary>
    /// A completed order
    /// </summary>
    Completed,

    /// <summary>
    /// A cancelled order
    /// </summary>
    Cancelled,
  }
}