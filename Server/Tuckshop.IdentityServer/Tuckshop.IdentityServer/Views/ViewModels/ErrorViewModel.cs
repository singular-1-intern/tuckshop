namespace Tuckshop.IdentityServer.Views.ViewModels
{
  /// <summary>
  /// Represents the Error ViewModel.
  /// </summary>
  public class ErrorViewModel
  {
    /// <summary>
    /// Gets or sets the Error Message.
    /// </summary>
    public ErrorMessage? Error { get; set; }
  }

  /// <summary>
  /// Represents an error message of the error view model.
  /// </summary>
  public class ErrorMessage
  {
    /// <summary>
    /// Gets or sets the Error.
    /// </summary>
    public string Error { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Error Description.
    /// </summary>
    public string ErrorDescription { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Request Id.
    /// </summary>
    public string RequestId { get; set; } = string.Empty;
  }
}
