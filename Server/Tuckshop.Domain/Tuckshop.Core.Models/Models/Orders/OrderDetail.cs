namespace Tuckshop.Core.Models
{
  using System;
  using System.ComponentModel.DataAnnotations;
  using System.ComponentModel.DataAnnotations.Schema;
  using Neo.Model;

  [Table("OrderDetails")]
  public class OrderDetail : ModelBase<OrderDetail>
  {

    // Initializes a new instance of the <see cref="OrderDetail"/> class.
    private OrderDetail()
    {
    }

    internal OrderDetail(int productId, int quantity, decimal price)
    {
      this.ProductId = productId;
      this.Quantity = quantity;
      this.Value = quantity * price;
      this.VAT = Math.Round(this.Value - (this.Value / 1.15m), 2, MidpointRounding.AwayFromZero);
      this.TrackingState = TrackableEntities.Common.Core.TrackingState.Added;
    }

    public int OrderDetailId { get; private set; }

    public int ProductId { get; private set; }

    // Navigation Property - tells EF Core that an OrderDetail contains a Product
    [ForeignKey(nameof(ProductId))]
    public Product? Product { get; private set; }

    public int Quantity { get; private set; }

    [Column(TypeName = "money")]
    public decimal Value { get; private set; }

    [Column(TypeName = "money")]
    public decimal VAT { get; private set; }
  }
}