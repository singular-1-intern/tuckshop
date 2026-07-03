namespace Tuckshop.IdentityServer.Contracts.UserManagement.Commands
{
  /// <summary>
  /// The user management action command.
  /// </summary>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "NEO2000:Property should specify the [StringLength] or [MaxLength] attribute", Justification = "Not a Neo Command.")]
  public class PerformUserActionCommand
  {
    /// <summary>
    /// Gets or sets the user id.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user management action to be performed.
    /// </summary>
    public UserManagementAction Action { get; set; }
  }
}
