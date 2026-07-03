namespace Tuckshop.Reporting.Reports.Example
{
  using System;

  /// <summary>
  /// The example report lookup.
  /// </summary>
  public class ExampleReportLookup
  {
    /// <summary>
    /// Gets or sets the UserId.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the FirstName.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the LastName.
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the join date.
    /// </summary>
    public DateTime JoinDate { get; set; }

    /// <summary>
    /// Gets or sets the salary.
    /// </summary>
    public decimal Salary { get; set; }
  }
}