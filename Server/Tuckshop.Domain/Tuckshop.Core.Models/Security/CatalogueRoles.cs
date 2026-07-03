namespace Tuckshop.Core.Security
{
  using System.ComponentModel;
  using Neo.AuthorisationServer.Client;

  /// <summary>
  /// Catalogue roles.
  /// </summary>
  public class CatalogueRoles : IRoles
  {
    /// <summary>
    /// Shows the catalogue page on the main menu.
    /// </summary>
    public enum CataloguePage
    {
      [Description("Shows the catalogue page on the main menu.")]
      View,
    }

    /// <inheritdoc/>
    public string ResourceName => "Catalogue";

    /// <inheritdoc/>
    public string DisplayName => "Catalogue";
  }
}