namespace Tuckshop.Models.Orders.Commands
{
  using System.ComponentModel.DataAnnotations;

  /// <summary>
  /// Represents a Cancel Order command
  /// </summary>
  public class CancelOrder
  {
    /// <summary>
    /// Gets or sets the Order Id to cancel
    /// </summary>
    public int OrderId { get; set; }

    /// <summary>
    /// Gets or sets the reason the order was cancelled
    /// </summary>
    [Required]
    public string Reason { get; set; } = string.Empty;
  }
}