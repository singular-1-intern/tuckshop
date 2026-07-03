namespace Tuckshop.IdentityServer.Controllers.Custom
{
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using Microsoft.AspNetCore.Authorization;
  using Microsoft.AspNetCore.Mvc;
  using Neo.IdentityServer.App.Services;
  using Neo.IdentityServer.App.Services.IdentityProviders;
  using Neo.IdentityServer.Models.IdentityProviders;
  using Neo.Model.DataAnnotations;
  using Tuckshop.IdentityServer.Models.Security;
  using static Neo.IdentityServer.Core.OpenIddict.NeoOpenIddictConstants;

  /// <summary>
  /// Controller for managing identity providers.
  /// </summary>
  [Route("api/identity-providers")]
  [ApiController]
  [Authorize(LocalApi.PolicyName)]
  [RequireRole(Roles.IdentityProviders.Setup)]
  public class IdentityProviderController : ControllerBase
  {
    private readonly IIdentityProviderModelService<IdentityProvider> identityProviderModelService;
    private readonly IIdentityProviderCache identityProviderCache;
    private readonly IOpenIDVerificationClient openIDVerificationClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="IdentityProviderController"/> class.
    /// </summary>
    /// <param name="identityProviderModelService">The Identity Provider service.</param>
    /// <param name="identityProviderCache">The identity provider cache.</param>
    /// <param name="openIDVerificationClient">The openID Verification Client.</param>
    public IdentityProviderController(
      IIdentityProviderModelService<IdentityProvider> identityProviderModelService,
      IIdentityProviderCache identityProviderCache,
      IOpenIDVerificationClient openIDVerificationClient)
    {
      this.identityProviderModelService = identityProviderModelService;
      this.identityProviderCache = identityProviderCache;
      this.openIDVerificationClient = openIDVerificationClient;
    }

    /// <summary>
    /// Gets the Identity Providers.
    /// </summary>
    /// <returns>A list of IdentityProvider.</returns>
    [HttpGet]
    public Task<List<IdentityProviderLookup>?> GetLookups()
    {
      return this.identityProviderCache.GetAsync();
    }

    /// <summary>
    /// Gets the Identity Providers.
    /// </summary>
    /// <param name="identityProviderId">The Id of the identity provider.</param>
    /// <returns>A list of IdentityProvider.</returns>
    [HttpGet("{identityProviderId}")]
    public IdentityProvider? GetIdentityProvider([FromRoute] int identityProviderId)
    {
      return this.identityProviderModelService.GetIdentityProvider(identityProviderId);
    }

    /// <summary>
    /// Updates the Identity Provider.
    /// </summary>
    /// <param name="identityProvider">The identity provider.</param>
    /// <returns>A list of IdentityProvider.</returns>
    [HttpPost]
    public Task<int> UpdateIdentityProvider([FromBody] IdentityProvider identityProvider)
    {
      return this.identityProviderModelService.UpdateAsync(identityProvider);
    }

    /// <summary>
    /// Deletes the Identity Providers with the provided id.
    /// </summary>
    /// <param name="identityProviderId">The Id of the identity provider.</param>
    /// <returns>A list of IdentityProvider.</returns>
    [HttpDelete("{identityProviderId}")]
    public Task DeleteIdentityProvider([FromRoute] int identityProviderId)
    {
      return this.identityProviderModelService.DeleteIdentityProviderAsync(identityProviderId);
    }

    /// <summary>
    /// Gets Identity Providers for the current tenant.
    /// </summary>
    /// <returns>A list of IdentityProvider.</returns>
    [HttpGet("types")]
    public List<IdentityProviderTypeLookup> GetIdentityProviderTypes()
    {
      return this.identityProviderModelService.GetIdentityProviderTypes();
    }

    /// <summary>
    /// Tests a Tenant Identity Provider.
    /// </summary>
    /// <param name="provider">The Tenant Identity Provider to test.</param>
    /// <returns>A string with the error. Empty string indicates success.</returns>
    [HttpPost("test-provider")]
    public Task<string> TestProvider([FromBody] IdentityProvider provider)
    {
      return this.openIDVerificationClient.TestProviderAsync(provider);
    }
  }
}
