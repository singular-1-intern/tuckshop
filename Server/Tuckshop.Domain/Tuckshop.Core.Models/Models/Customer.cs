namespace Tuckshop.Core.Models
{
  using Neo.Model;
  using System.Collections.Generic;
  using System.ComponentModel.DataAnnotations;
  using System.ComponentModel.DataAnnotations.Schema;

  /// <summary>
  /// A class representing a Customer
  /// </summary>
  public class Customer : ModelBase<Customer>
  {

    private Customer()
    {
    }

    public Customer(int customerId, string customerName)
    {
      this.CustomerId = customerId;
      this.CustomerName = customerName;
    }

    /// <summary>
    /// Gets or sets the Customer Id
    /// </summary>
    public int CustomerId { get; private set; }

    /// <summary>
    /// Gets or sets the Customer Name
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string CustomerName { get; private set; } = string.Empty;

    // Navigation Property - tells EF Core that an Customer contains one or many Orders.
    //public ICollection<Order> Orders { get; set; } = new List<Order>();

  }
}