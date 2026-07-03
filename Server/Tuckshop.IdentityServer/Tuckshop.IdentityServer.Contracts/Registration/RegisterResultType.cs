namespace Tuckshop.IdentityServer.Contracts.Registration
{
  /// <summary>
  /// The results from registration.
  /// </summary>
  public enum RegisterResultType
  {
    /// <summary>
    /// Failed
    /// </summary>
    Failure = -1,

    /// <summary>
    /// Pending Email Verification
    /// </summary>
    PendingEmailVerification,

    /// <summary>
    /// Configure MFA
    /// </summary>
    ConfigureMFA,

    /// <summary>
    /// Requires MFA
    /// </summary>
    RequiresMFA,

    /// <summary>
    /// Signed In
    /// </summary>
    SignedIn,
  }
}