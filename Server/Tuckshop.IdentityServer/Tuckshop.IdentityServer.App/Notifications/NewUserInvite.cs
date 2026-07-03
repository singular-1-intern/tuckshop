namespace Tuckshop.IdentityServer.App.Notifications
{
  using Neo.NotificationServer.Models;

  /// <summary>
  /// Class representing a new user invite.
  /// </summary>
  public class NewUserInvite : NotificationModel
  {
    /// <summary>
    /// Gets or sets the registration link.
    /// </summary>
    public string RegistrationLink { get; set; } = string.Empty;

    /// <summary>
    /// Creates a user invite for an email address with a registration link.
    /// </summary>
    /// <param name="emailAddress">Email address of the recipient.</param>
    /// <param name="registrationLink">Registration link to click on.</param>
    /// <returns>User invite model.</returns>
    public static NewUserInvite Create(string emailAddress, string registrationLink)
    {
      return new NewUserInvite()
      {
        Recipient = new Recipient() { EmailAddress = emailAddress },
        RegistrationLink = registrationLink,
      };
    }

    /// <summary>
    /// Creates a user invite with example data.
    /// </summary>
    /// <returns>Example data.</returns>
    public static NewUserInvite GetExampleData()
    {
      return PopulateExampleData(new NewUserInvite()
      {
        RegistrationLink = "http://example.com/register",
      });
    }
  }
}