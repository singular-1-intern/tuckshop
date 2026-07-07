namespace Tuckshop.Core.Models.Customers.Commands
{
  using Neo.Model;
  using Neo.Model.Validation;
  using System.ComponentModel.DataAnnotations;
  using System.ComponentModel.DataAnnotations.Schema;

  /// <summary>
  /// Command to withdraw funds from a customer's wallet
  /// </summary>
  public class WithdrawFunds : ModelBase<WithdrawFunds>
  {
    /// <summary>
    /// Gets or sets the Customer Id.
    /// </summary>
    [Required]
    public int CustomerId { get; set; }

    /// <summary>
    /// Gets or sets the amount to withdraw
    /// </summary>
    [Required]
    [Column(TypeName = "money")]
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the reason for withdrawal (optional, for admin auditing)
    /// </summary>
    [MaxLength(255)]
    public string Reason { get; set; } = string.Empty;

    /// <inheritdoc/>
    protected override void AddBusinessRules(ValidationRules<WithdrawFunds> rules)
    {
      base.AddBusinessRules(rules);
      rules.FailWhen(c => c.Amount <= 0, "Amount must be greater than zero");
    }
  }
}
