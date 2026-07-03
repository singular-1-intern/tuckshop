namespace Tuckshop.IdentityServer.Tests
{
  using System;
  using System.Collections.Generic;
  using Microsoft.AspNetCore.Authentication;
  using Microsoft.AspNetCore.Authentication.OpenIdConnect;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.Extensions.DependencyInjection;
  using Moq;
  using Neo.Extensions.DependencyInjection;
  using Neo.IdentityServer.App.Services.IdentityProviders;
  using Neo.IdentityServer.Models.IdentityProviders;
  using Neo.Model.Mappers;
  using Neo.Model.Metadata;
  using Neo.Model.Processing;
  using Neo.Testing;
  using Tuckshop.IdentityServer;

  internal class UnitTestHelper
  {
    /// <summary>
    /// Initialise the identity database context.
    /// </summary>
    /// <returns>The identity database context.</returns>
    internal static IdentityDbContext InitIdentityDbContext()
    {
      DbContextOptionsBuilder<IdentityDbContext> builder =
        new DbContextOptionsBuilder<IdentityDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString());

      IdentityDbContext context = new IdentityDbContext(builder.Options, new List<IDbContextProcessor>());

      return context;
    }

    /// <summary>
    /// Create the identity provider services.
    /// </summary>
    /// <param name="addSchemeHandler">The add scheme handler.</param>
    /// <param name="removeSchemeHandler">The remove scheme handler.</param>
    /// <returns>The identity provider service creation result.</returns>
    internal static IdentityProviderServiceCreationResult CreateIdentityProviderServices(Action<AuthenticationScheme>? addSchemeHandler = null, Action<string>? removeSchemeHandler = null)
    {
      IServiceCollection serviceCollection = new ServiceCollection();
      serviceCollection.AddAuthentication();
      serviceCollection.RegisterAuthenticationSchemeDefinitions<IdentityProviderType>();
      serviceCollection.AddScoped<IAuthenticationSchemeDefinitionTypeProvider, AuthenticationSchemeDefinitionTypeProvider<IdentityProviderType>>();

      var serviceProvider = serviceCollection.BuildServiceProvider();
      var serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

      var identityDbContext = InitIdentityDbContext();

      var identityProviders = AddIdentityProviders(identityDbContext);

      var authenticationSchemeProviderMock = new Mock<IAuthenticationSchemeProvider>();
      authenticationSchemeProviderMock.Setup(authenticationSchemeProvider => authenticationSchemeProvider.AddScheme(It.IsAny<AuthenticationScheme>()))
        .Callback((AuthenticationScheme scheme) => addSchemeHandler?.Invoke(scheme));

      authenticationSchemeProviderMock.Setup(authenticationSchemeProvider => authenticationSchemeProvider.RemoveScheme(It.IsAny<string>()))
        .Callback((string name) => removeSchemeHandler?.Invoke(name));

      authenticationSchemeProviderMock.Setup(authenticationSchemeProvider => authenticationSchemeProvider.GetSchemeAsync(It.IsAny<string>()))
        .ReturnsAsync((string name) =>
        {
          AuthenticationScheme? scheme = null;
          if (name == "Google")
          {
            return new AuthenticationScheme(
              "Google",
              "Test Google",
              typeof(OpenIdConnectHandler));
          }

          return scheme;
        });

      var identityProviderCache = new IdentityProviderCache(DistributedCacheMock.CreateCacheMock());

      var result = new IdentityProviderServiceCreationResult(
        serviceScopeFactory,
        identityDbContext,
        identityProviderCache,
        new IdentityProviderService<IdentityDbContext>(serviceScopeFactory, authenticationSchemeProviderMock.Object, identityProviderCache, GetNeoMapper()));

      result.IdentityProviderService.LoadProvidersAsync(identityProviders).GetAwaiter().GetResult();

      return result;
    }

    private static INeoMapper? neoMapper;

    /// <summary>
    /// Gets the NeoMapper.
    /// </summary>
    /// <returns>The NeoMapper.</returns>
    public static INeoMapper GetNeoMapper()
    {
      neoMapper ??= new NeoMapper(new MetadataService());
      return neoMapper;
    }

    /// <summary>
    /// Add the identity providers to the identity database context.
    /// </summary>
    /// <param name="identityDbContext">The identity database context.</param>
    /// <returns>The identity providers.</returns>
    public static List<IdentityProvider> AddIdentityProviders(IdentityDbContext identityDbContext)
    {
      var identityProviders = new List<IdentityProvider>
      {
        IdentityProvider.LoginCredentials(setId: false)
      };

      var googleIdp = IdentityProvider.Google("google-client-id");
      googleIdp.OidcConfig?.Scopes = "test scopes";
      identityProviders.Add(googleIdp);

      identityProviders.Add(IdentityProvider.AzureAD("https://accounts.azure.com/{guid}", "azure-ad-client-id"));

      identityDbContext.IdentityProviders.AddRange(identityProviders);
      identityDbContext.SaveChanges();

      identityDbContext.ChangeTracker.Clear();

      return identityProviders;
    }

    internal class IdentityProviderServiceCreationResult
    {
      public IdentityProviderServiceCreationResult(
        IServiceScopeFactory serviceScopeFactory,
        IdentityDbContext identityDbContext,
        IdentityProviderCache identityProviderCache,
        IdentityProviderService<IdentityDbContext> identityProviderService)
      {
        this.ServiceScopeFactory = serviceScopeFactory;
        this.IdentityDbContext = identityDbContext;
        this.IdentityProviderCache = identityProviderCache;
        this.IdentityProviderService = identityProviderService;
      }

      /// <summary>
      /// Gets the service provider.
      /// </summary>
      public IServiceScopeFactory ServiceScopeFactory { get; }

      /// <summary>
      /// Gets the identity database context.
      /// </summary>
      public IdentityDbContext IdentityDbContext { get; }

      /// <summary>
      /// Gets the identity provider cache.
      /// </summary>
      public IdentityProviderCache IdentityProviderCache { get; }

      /// <summary>
      /// Gets the identity provider service.
      /// </summary>
      public IdentityProviderService<IdentityDbContext> IdentityProviderService { get; }
    }
  }
}