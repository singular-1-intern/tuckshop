#pragma warning disable CA1724 // type name overlaps the namespace name
namespace Tuckshop.Reporting.Security
{
  using Neo.AuthorisationServer.Client;

  /// <summary>
  /// The reporting roles.
  /// </summary>
  public class Roles : IRoles
  {
    /// <summary>
    /// Roles for reports which don't have their own roles.
    /// </summary>
    public enum General
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
      View,
      Download,
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }

    /// <inheritdoc/>
    public string ResourceName => "Reporting";

    /// <inheritdoc/>
    public string DisplayName => "Reporting";
  }
}