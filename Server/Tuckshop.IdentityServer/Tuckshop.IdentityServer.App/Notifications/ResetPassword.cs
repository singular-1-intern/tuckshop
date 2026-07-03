namespace Tuckshop.IdentityServer.App.Notifications
{
  using Neo.NotificationServer.Models;

  /// <summary>
  /// Reset password notification model.
  /// </summary>
  public class ResetPassword : NotificationModel
  {
    /// <summary>
    /// Gets the reset link.
    /// </summary>
    public string ResetLink { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the number of hours before the link expires.
    /// </summary>
    public double LinkExpiryHours { get; private set; } = 24;

    /// <summary>
    /// Creates a reset password model for an email address with a reset link.
    /// </summary>
    /// <param name="emailAddress">Email address of the recipient.</param>
    /// <param name="resetLink">Reset link to click on.</param>
    /// <param name="linkExpiryHours">The number of hours before the link will expire.</param>
    /// <returns>Reset password model.</returns>
    public static ResetPassword Create(string emailAddress, string resetLink, double linkExpiryHours)
    {
      return new ResetPassword()
      {
        Recipient = new Recipient() { EmailAddress = emailAddress },
        ResetLink = resetLink,
        LinkExpiryHours = linkExpiryHours,
      };
    }

    /// <summary>
    /// Creates a reset password model with example data.
    /// </summary>
    /// <returns>Example data.</returns>
    public static ResetPassword GetExampleData()
    {
      return PopulateExampleData(new ResetPassword()
      {
        ResetLink = "http://example.com/reset/1234",
      });
    }
  }
}
