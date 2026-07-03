namespace Tuckshop.IdentityServer.Contracts.Registration
{
  using Tuckshop.IdentityServer.Contracts.UserManagement.Queries;

  /// <summary>
  /// Register result.
  /// </summary>
  public class RegisterResult
  {
    /// <summary>
    /// Prevents the default instance of the <see cref="RegisterResult"/> class from being created.
    /// </summary>
    private RegisterResult()
    {
    }

    /// <summary>
    /// Gets a value indicating whether the register was a success.
    /// </summary>
    public bool Success { get; private set; }

    /// <summary>
    /// Gets the user that was registered.
    /// </summary>
    public UserLookup? User { get; private set; }

    /// <summary>
    /// Gets the registration failure reason.
    /// </summary>
    public string FailureReason { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the registration type.
    /// </summary>
    public RegisterResultType Type { get; private set; }

    /// <summary>
    /// Creates a Failed RegisterResult.
    /// </summary>
    /// <param name="reason">The failure reason.</param>
    /// <returns>A Failed RegisterResult.</returns>
    public static RegisterResult FailResult(string reason) => new RegisterResult() { Type = RegisterResultType.Failure, Success = false, FailureReason = reason };

    /// <summary>
    /// Creates a Success Pending Email Verification RegisterResult.
    /// </summary>
    /// <param name="user">The registered user.</param>
    /// <returns>A Success Pending Email Verification RegisterResult.</returns>
    public static RegisterResult PendingEmailVerification(UserLookup user) => SuccessResult(RegisterResultType.PendingEmailVerification, user);

    /// <summary>
    /// Creates a Success Configure MFA RegisterResult.
    /// </summary>
    /// <param name="user">The registered user.</param>
    /// <returns>A Success Configure MFA RegisterResult.</returns>
    public static RegisterResult SuccessConfigureMFA(UserLookup user) => SuccessResult(RegisterResultType.ConfigureMFA, user);

    /// <summary>
    /// Creates a Success Requires MFA RegisterResult.
    /// </summary>
    /// <param name="user">The registered user.</param>
    /// <returns>A Success Requires MFA RegisterResult.</returns>
    public static RegisterResult SuccessRequiresMFA(UserLookup user) => SuccessResult(RegisterResultType.RequiresMFA, user);

    /// <summary>
    /// Creates a Success RegisterResult.
    /// </summary>
    /// <param name="user">The registered user.</param>
    /// <returns>A Success RegisterResult.</returns>
    public static RegisterResult SuccessNoMFA(UserLookup user) => SuccessResult(RegisterResultType.SignedIn, user);

    private static RegisterResult SuccessResult(RegisterResultType type, UserLookup user) => new RegisterResult() { Type = type, Success = true, User = user };
  }
}