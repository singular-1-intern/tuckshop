namespace Tuckshop.IdentityServer.Initializers
{
  using System.ComponentModel.DataAnnotations;
  using Neo.Options;

  /// <summary>
  /// Seed user options.
  /// </summary>
  [ConfigSectionName("SeedUsers")]
  public class SeedUserOptions : ValidateableOptions<SeedUserOptions>
  {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    /// <summary>
    /// Gets or sets the Test User Password (used in non Prod environments).
    /// </summary>
    [Required]
    public string TestUserPassword { get; set; }

    /// <summary>
    /// Gets or sets the Super User Password (used in Prod environments).
    /// </summary>
    [Required]
    public string SuperUserPassword { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
  }
}
