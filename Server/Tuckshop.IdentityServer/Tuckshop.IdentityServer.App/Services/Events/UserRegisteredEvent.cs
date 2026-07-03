namespace Tuckshop.IdentityServer.App.Services.Events
{
  using Neo.Model.DomainEvents;
  using Tuckshop.IdentityServer.Contracts.Registration;

  /// <summary>
  /// Domain event that is fired when a new user is registered.
  /// </summary>
  public class UserRegisteredEvent : IDomainEvent
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="UserRegisteredEvent"/> class.
    /// </summary>
    /// <param name="id">The user's id/.</param>
    /// <param name="user">The user.</param>
    /// <param name="registerType">The type of registration.</param>
    public UserRegisteredEvent(string id, IRegistrationUser user, RegisterResultType registerType)
    {
      this.Id = id;
      this.User = user;
      this.RegisterType = registerType;
    }

    /// <summary>
    /// Gets the Id of the registered user.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets the registration user.
    /// </summary>
    public IRegistrationUser User { get; }

    /// <summary>
    /// Gets the Register Type.
    /// </summary>
    public RegisterResultType RegisterType { get; }
  }
}
