namespace Tuckshop.Reporting.Reports.Example
{
  using System.ComponentModel.DataAnnotations;
  using Neo.Model;
  using Neo.Model.Validation;

  /// <summary>
  /// The example report.
  /// </summary>
  public class ExampleCriteria : CriteriaBase<ExampleCriteria>
  {
    /// <summary>
    /// Gets or sets the search string to filter by.
    /// </summary>
    [Required]
    public string? SearchString { get; set; }

    /// <inheritdoc/>
    protected override void AddBusinessRules(ValidationRules<ExampleCriteria> rules)
    {
      base.AddBusinessRules(rules);
    }
  }
}
