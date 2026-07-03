namespace Tuckshop.Core.Models
{
  using System;
  using System.Collections.Generic;
  using System.ComponentModel.DataAnnotations;
  using System.ComponentModel.DataAnnotations.Schema;
  using Neo.Model;
  using Neo.Model.Exceptions;
  using Neo.Model.ValueObjects;

  public class Order : ModelBase<Order>
  {

    /// <summary>
    /// Initializes a new instance of the <see cref="Order"/> class.
    /// </summary>
    // For EF Core to hydrate from DB
    private Order()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Order"/> class.
    /// </summary>
    /// <param name="customerName">The customer name</param>
    public Order(string customerName)
    {
      this.CustomerName = customerName;
      this.OrderedOn = DateTime.UtcNow;
      // Kind of hard-coded For EF Core
      this.TrackingState = TrackableEntities.Common.Core.TrackingState.Added;
    }

    public int OrderId { get; private set; }

    [Column(TypeName = "datetime")]
    public DateTime OrderedOn { get; private set; } = DateTime.UtcNow;

    [Required(AllowEmptyStrings = false)]
    [MaxLength(100)]
    public string CustomerName { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the Completed User Event.
    /// </summary>
    public UserEvent Completed { get; private set; } = UserEvent.None();

    /// <summary>
    /// Gets the Canceled User Event.
    /// </summary>
    public ReasonedUserEvent Cancelled { get; private set; } = ReasonedUserEvent.None();

    // Navigation Property - tells EF Core that an Order contains many OrderDetails
    public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    /// <summary>
    /// Will add a product to the order
    /// </summary>
    /// <param name="productId">The product Id</param>
    /// <param name="quantity">The quantity of the product</param>
    /// <param name="price">The price</param>
    /// <returns>The added order detail</returns>
    public OrderDetail AddDetail(int productId, int quantity, decimal price)
    {
      var orderDetail = new OrderDetail(productId, quantity, price);
      this.OrderDetails.Add(orderDetail);
      return orderDetail;
    }

    /// <summary>
    /// Will complete the order
    /// </summary>
    /// <param name="userId">The user who completed the event</param>
    public void Complete(int userId)
    {
      this.AssertNotCompletedOrCancelled();
      this.Completed = new UserEvent(userId);
    }

    /// <summary>
    /// Will cancel the order
    /// </summary>
    /// <param name="userId">The user who completed the event</param>
    /// <param name="reason">The reason the user cancelled the order</param>
    public void Cancel(int userId, string reason)
    {
      this.AssertNotCompletedOrCancelled();
      if (string.IsNullOrWhiteSpace(reason))
      {
        throw new InvalidDomainOperationException($"A reason is required when cancelling an order");
      }
      this.Cancelled = new ReasonedUserEvent(userId, reason);
    }

    private void AssertNotCompletedOrCancelled()
    {
      if (this.Completed.IsCompleted)
      {
        throw new InvalidDomainOperationException($"{this} has already been completed");
      }
      if (this.Cancelled.IsCompleted)
      {
        throw new InvalidDomainOperationException($"{this} has already been canceled");
      }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
      return $"Order No: {this.OrderId} for {this.CustomerName} on {this.OrderedOn:dd-MMM-yy HH:mm}";
    }
  }
}