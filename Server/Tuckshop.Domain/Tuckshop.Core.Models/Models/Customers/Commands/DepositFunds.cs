namespace Tuckshop.Core.Models.Customers.Commands
{
  using Neo.Model;
  using Neo.Model.Validation;
  using System.ComponentModel.DataAnnotations;
  using System.ComponentModel.DataAnnotations.Schema;

  /// <summary>
  /// Command to deposit funds into a customer's wallet
  /// </summary>
  public class DepositFunds : ModelBase<DepositFunds>
  {
    /// <summary>
    /// Gets or sets the Customer Id.
    /// </summary>
    [Required]
    public int CustomerId { get; set; }

    /// <summary>
    /// Gets or sets the amount to deposit
    /// </summary>
    [Required]
    [Column(TypeName = "money")]
    public decimal Amount { get; set; }

    /// <inheritdoc/>
    protected override void AddBusinessRules(ValidationRules<DepositFunds> rules)
    {
      base.AddBusinessRules(rules);
      rules.FailWhen(c => c.Amount <= 0, "Amount must be greater than zero");
    }
  }
}
