namespace Tuckshop.IdentityServer.App.Notifications
{
  using Neo.NotificationServer.Models;

  /// <summary>
  /// Email verification notification model.
  /// </summary>
  public class EmailVerification : NotificationModel
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="EmailVerification"/> class.
    /// </summary>
    public EmailVerification()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailVerification"/> class.
    /// </summary>
    /// <param name="recipientDisplayName">The recipient display name.</param>
    /// <param name="emailAddress">The email address.</param>
    /// <param name="verificationUrl">The verification url.</param>
    public EmailVerification(string recipientDisplayName, string emailAddress, string verificationUrl)
    {
      this.Recipient = new Recipient()
      {
        EmailAddress = emailAddress,
        DisplayName = recipientDisplayName,
      };
      this.VerificationUrl = verificationUrl;
    }

    /// <summary>
    /// Gets or sets the verification link the user needs to visit in order to verify their email.
    /// </summary>
    public string VerificationUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the number of hours before the link expires.
    /// </summary>
    public double LinkExpiryHours { get; set; } = 24;

    /// <summary>
    /// Creates a user invite for an email address with a registration link.
    /// </summary>
    /// <param name="emailAddress">Email address of the recipient.</param>
    /// <param name="verificationUrl">Verification link to click on.</param>
    /// <param name="linkExpiryHours">The link expiry hours.</param>
    /// <returns>User invite model.</returns>
    public static EmailVerification Create(string emailAddress, string verificationUrl, double linkExpiryHours)
    {
      return new EmailVerification()
      {
        Recipient = new Recipient() { EmailAddress = emailAddress },
        VerificationUrl = verificationUrl,
        LinkExpiryHours = linkExpiryHours,
      };
    }

    /// <summary>
    /// Creates a user invite with example data.
    /// </summary>
    /// <returns>Example data.</returns>
    public static EmailVerification GetExampleData()
    {
      return PopulateExampleData(new EmailVerification()
      {
        VerificationUrl = "http://example.com/verifyEmail?code=abc123",
      });
    }
  }
}
