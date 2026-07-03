namespace Tuckshop.Core.Api
{
  /// <summary>
  /// The Tuckshop Setup action names class.
  /// </summary>
  public static class StartupActions
  {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public const string DataServices = "TuckshopDataServices";

    public const string UserDataServices = "TuckshopUserDataServices";

    public const string ModelServices = "TuckshopModelServices";

    public const string ProcessingServices = "TuckshopProcessingServices";

    public const string IntegrationServices = "TuckshopIntegrationServices";

    public const string IntegrityChecking = "TuckshopIntegrityChecking";

    public const string Jobs = "TuckshopJobs";

    public const string EntityChangePublishers = "TuckshopEntityChangePublishers";

    public const string FileStorageServices = "TuckshopFileStorageServices";

    public const string Caches = "TuckshopCaches";

    public const string SignalR = "TuckshopSignalR";

    public const string Modules = "TuckshopModules";

    public const string MultiTenancy = "TuckshopMultiTenancy";

    public const string Logging = "TuckshopLogging";

    public const string SecretVaults = "TuckshopSecretVault";

    public const string AlwaysEncrypted = "TuckshopAlwaysEncrypted";
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
  }
}