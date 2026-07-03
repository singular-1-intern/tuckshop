namespace Tuckshop.IdentityServer.App.Services
{
  using System;
  using Microsoft.AspNetCore.Identity;
  using Neo.IdentityServer.Providers;

  /// <summary>
  /// Data protection options provider.
  /// </summary>
  public class DataProtectionOptionsProvider : IDataProtectionOptionsProvider
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="DataProtectionOptionsProvider"/> class.
    /// </summary>
    public DataProtectionOptionsProvider()
    {
    }

    /// <inheritdoc/>
    public DataProtectionTokenProviderOptions GetOptions(string purpose)
    {
      switch (purpose)
      {
        case DataProtectionPurposes.EmailConfirmation:
          return new DataProtectionTokenProviderOptions() { Name = purpose, TokenLifespan = TimeSpan.FromDays(1) };

        case DataProtectionPurposes.ResetPassword:
          return new DataProtectionTokenProviderOptions() { Name = purpose, TokenLifespan = TimeSpan.FromMinutes(15) };

        default: return new DataProtectionTokenProviderOptions() { Name = purpose, TokenLifespan = TimeSpan.FromDays(1) };
      }
    }
  }
}
