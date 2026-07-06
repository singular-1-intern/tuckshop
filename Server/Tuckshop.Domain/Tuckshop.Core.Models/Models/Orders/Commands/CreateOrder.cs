#pragma warning disable CA1034 // Nested types should not be visible
namespace Tuckshop.Models.Orders.Commands
{
  using Neo.Model;
  using Neo.Model.Validation;
  using System.Collections.Generic;
  using System.ComponentModel.DataAnnotations;

  /// <summary>
  /// Class representing a new order command
  /// </summary>
  public class CreateOrder : ModelBase<CreateOrder>
  {
    /// <summary>
    /// Gets or sets the Customer Id.
    /// </summary>
    [Required]
    public int CustomerId { get; set; }

    //[Required]
    //[StringLength(50)]
    ///// <summary>
    ///// Gets or sets the Customer Name.
    ///// </summary>
    //public string CustomerName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Order Details
    /// </summary>
    public ICollection<NewOrderDetail> OrderDetails { get; set; } = new List<NewOrderDetail>();

    /// <summary>
    /// Represents a new order detail
    /// </summary>
    public class NewOrderDetail : ModelBase<NewOrderDetail>
    {
      /// <summary>
      /// Gets or sets the Order Detail Id.
      /// </summary>
      public int ProductId { get; set; }

      /// <summary>
      /// Gets or sets the Quantity.
      /// </summary>
      public int Quantity { get; set; }
    }

    /// <inheritdoc/>
    protected override void AddBusinessRules(ValidationRules<CreateOrder> rules)
    {
      rules.FailWhen(
        o => o.OrderDetails == null || o.OrderDetails.Count == 0,
        "Order details are required");
    }
  }
}