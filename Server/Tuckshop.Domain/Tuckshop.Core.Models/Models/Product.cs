namespace Tuckshop.Core.Models
{
  using Microsoft.EntityFrameworkCore;
  using Neo.Model;
  using Neo.Model.Validation;
  using System.ComponentModel.DataAnnotations;
  using System.ComponentModel.DataAnnotations.Schema;

  /// <summary>
  /// A class representing a Product
  /// </summary>
  public class Product : ModelBase<Product>
  {
    /// <summary>
    /// Gets or sets the Product Id
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Gets or sets the Product Name
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Price  
    /// </summary>
    [Column(TypeName = "money")]
    public decimal Price { get; set; }

    /// <inheritdoc />
    protected override void AddBusinessRules(ValidationRules<Product> rules)
    {
      base.AddBusinessRules(rules);

      rules.FailWhen(c => c.Price <= 0, "Price must be above zero.");
    }
  }
}