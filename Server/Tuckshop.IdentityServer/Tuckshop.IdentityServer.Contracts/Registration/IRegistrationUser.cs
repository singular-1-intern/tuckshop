namespace Tuckshop.IdentityServer.Contracts.Registration
{
  /// <summary>
  /// Interface for a registration user.
  /// </summary>
  public interface IRegistrationUser
  {
    /// <summary>
    /// Gets the First Name of the User.
    /// </summary>
    string FirstName { get; }

    /// <summary>
    /// Gets the Last Name of the User.
    /// </summary>
    string LastName { get; }

    /// <summary>
    /// Gets the email.
    /// </summary>
    string Email { get; }

    /// <summary>
    /// Gets or sets a value indicating whether the users email is confirmed.
    /// </summary>
    bool EmailConfirmed { get; set; }

    /// <summary>
    /// Gets the registering user's password.
    /// </summary>
    string Password { get; }
  }
}
