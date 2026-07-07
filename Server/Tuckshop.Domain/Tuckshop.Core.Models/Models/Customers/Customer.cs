namespace Tuckshop.Core.Models.Customers
{
  using Neo.Model;
  using Neo.Model.Exceptions;
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
      this.WalletBalance = 0m;
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

    [Column(TypeName = "money")]
    public decimal WalletBalance { get; private set; }

    public void IncreaseBalance(decimal amount)
    {
      if (amount <= 0)
      {
        throw new InvalidDomainOperationException("Please add a positive amount");
      }

      this.WalletBalance += amount;
    }

    public void DecreaseBalance(decimal amount)
    {
      if (amount <= 0)
      {
        throw new InvalidDomainOperationException("Amount must be a positive value");
      }

      if (amount > this.WalletBalance)
      {
        throw new InvalidDomainOperationException($"Insufficient wallet balance. Available: {this.WalletBalance:C}, Required: {amount:C}");
      }

      this.WalletBalance -= amount;
    }

    public bool HasSufficientBalance(decimal amount)
    {
      return this.WalletBalance >= amount;
    }

  }
}