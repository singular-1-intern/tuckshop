namespace Tuckshop.IdentityServer.Models.Options
{
  using Neo.Options;

  /// <summary>
  /// The Identity Server Data Protection options class.
  /// </summary>
  [ConfigSectionName("DataProtection")]
  public class CustomDataProtectionOptions : ValidateableOptions<CustomDataProtectionOptions>
  {
    /// <summary>
    /// Gets or sets the ID of a master key in Key Vault to use to encrypt the data protection keys.
    /// </summary>
    public string KeyVaultKeyId { get; set; } = string.Empty;
  }
}
