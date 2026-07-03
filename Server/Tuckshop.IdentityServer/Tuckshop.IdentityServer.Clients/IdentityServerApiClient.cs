namespace Tuckshop.IdentityServer.Clients
{
  using System;
  using System.Net.Http;
  using System.Threading.Tasks;
  using Microsoft.Extensions.Logging;
  using Neo.Identity;
  using Neo.Model.Services.Api;
  using Tuckshop.IdentityServer.Contracts.UserManagement.Queries;
  using Tuckshop.IdentityServer.Services;

  /// <summary>
  /// The Identity Server API client.
  /// </summary>
  public class IdentityServerApiClient : ApiClient<IdentityServerApiClient>, IIdentityServerService
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="IdentityServerApiClient"/> class.
    /// </summary>
    /// <param name="authenticationClientProvider">The authentication client provider.</param>
    /// <param name="options">The api client options.</param>
    /// <param name="httpClient">The http client.</param>
    /// <param name="logger">The logger.</param>
    public IdentityServerApiClient(
      IAuthenticationClientProvider authenticationClientProvider,
      ApiClientOptions<IdentityServerApiClient> options,
      HttpClient httpClient,
      ILogger<IdentityServerApiClient> logger)
      : base(authenticationClientProvider, options, httpClient, logger)
    {
    }

    /// <inheritdoc />
    public Task<UserInviteLookup?> FindInvitedUserAsync(IClientUser user)
    {
      var userGuid = user.IdentityGuid ?? throw new ArgumentException($"{nameof(user.IdentityGuid)} must have a value.");

      return this.GetAsync<UserInviteLookup?>($"user-management/find-invited-user/{userGuid}");
    }
  }
}