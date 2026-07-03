namespace Tuckshop.Core.Models.Initializers
{
  using System.Threading;
  using System.Threading.Tasks;
  using Extensions.Hosting.AsyncInitialization;
  using Microsoft.Extensions.Hosting;
  using Neo.Model.Identity.SystemUser;
  using Neo.NotificationServer.Services;
  using Tuckshop.Core.Models.Identity;
  // Added this for AnyAsync(), as its an extension method defined in EFCore Package
  using Microsoft.EntityFrameworkCore;
  using System.Collections.Generic;

  /// <summary>
  /// Seed data generation service.
  /// </summary>
  /// <remarks>
  /// Initializes a new instance of the <see cref="SeedDataAsyncInitializer"/> class.
  /// </remarks>
  /// <param name="context">The model database context.</param>
  /// <param name="systemUserService">The system user service.</param>
  /// <param name="environment">The host environment.</param>
  /// <param name="templateTypesService">The template types service.</param>
  public class SeedDataAsyncInitializer(
    ModelDbContext context,
    ISystemUserService<User> systemUserService,
    IHostEnvironment? environment,
    ITemplateTypesService? templateTypesService) : IAsyncInitializer
  {
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1823:Avoid unused private fields", Justification = "Future use")]
    private readonly ModelDbContext context = context;
    private readonly ISystemUserService<User> systemUserService = systemUserService;
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1823:Avoid unused private fields", Justification = "Future use")]
    private readonly IHostEnvironment? environment = environment;
    private readonly ITemplateTypesService? templateTypesService = templateTypesService;

    /// <inheritdoc/>
    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
      await this.systemUserService.RunWithSystemUserAsync(this.GenerateSeedDataAsync);

      await this.RegisterTemplateTypesAsync();
    }

    /// <summary> 
    /// Will generate the appropriate seed data for the given environment.
    /// </summary>
    /// <returns>A task awaiting the seed data generation.</returns>
    public async Task GenerateSeedDataAsync()
    {
      await this.GenerateProductSeedDataAsync().ConfigureAwait(false);
      await this.GenerateOrderSeedDataAsync().ConfigureAwait(false);
      //await this.GenerateCustomerSeedDataAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Registers template types used by this service.
    /// </summary>
    public Task RegisterTemplateTypesAsync()
    {
      if (this.templateTypesService != null)
      {
        // await this.templateTypesService.RegisterTemplateTypesAsync(typeof(TemplateTypes));
      }

      return Task.CompletedTask;
    }

    private async Task GenerateProductSeedDataAsync()
    {
      if ((this.environment == null || this.environment.IsDevelopment()) && !await this.context.Products.AnyAsync().ConfigureAwait(false))
      {
        var products = new List<Product>()
        {
          new Product() { ProductName = "Coke", Price = 10 },
          new Product() { ProductName = "Bar One", Price = 9 },
          new Product() { ProductName = "Smarties", Price = 8.5M },
          new Product() { ProductName = "Popcorn", Price = 2.5M },
          new Product() { ProductName = "Peanuts", Price = 5 },
          new Product() { ProductName = "Cappuccino", Price = 10 },
          new Product() { ProductName = "Tomato Chips", Price = 6 },
        };

        if (this.environment == null)
        {
          int i = 1;
          // this is test, give these products Ids
          foreach (var product in products)
          {
            product.ProductId = i++;
          }
        }

        this.context.Products.AddRange(products);

        // SaveChangesAsync(): Takes all inserts, updates or deletes (like the line above) & packages them into a db transaction.
        // "Async": Executes this db op asynchronously. Instead of blocking the entire application thread while waiting for the db to respond, the thread is freed up to handle other incoming request.
        await this.context.SaveChangesAsync().ConfigureAwait(false);
      }
    }

    private async Task GenerateOrderSeedDataAsync()
    {
      if (this.environment.IsDevelopment() && !await this.context.Orders.AnyAsync().ConfigureAwait(false))
      //if (this.environment.IsDevelopment())
      {
        var pendingOrder = new Order("Pending Order");
        pendingOrder.AddDetail(1, 1, 10);
        pendingOrder.AddDetail(2, 4, 9);

        var completedOrder = new Order("Completed Order");
        completedOrder.AddDetail(3, 2, 8.5m);
        completedOrder.AddDetail(4, 1, 2.5m);
        completedOrder.Complete(1);

        var cancelledOrder = new Order("Cancelled Order");
        cancelledOrder.AddDetail(5, 1, 5);
        cancelledOrder.Cancel(1, "Don't like peanuts");

        this.context.Orders.AddRange(pendingOrder, completedOrder, cancelledOrder);

        await this.context.SaveChangesAsync().ConfigureAwait(false);
      }
    }

    //private async Task GenerateCustomerSeedDataAsync()
    //{
    //  if ((this.environment == null || this.environment.IsDevelopment()) && !await this.context.Customers.AnyAsync().ConfigureAwait(false))
    //  {
    //    var customers = new List<Customer>()
    //    {
    //      new Customer() { CustomerName = "Bob Lee Swagger" },
    //      new Customer() { CustomerName = "Bob Shmob" },
    //      new Customer() { CustomerName = "Joe Shmoe" },
    //      new Customer() { CustomerName = "Bruce Spruce" },
    //      new Customer() { CustomerName = "Bo van Do" },
    //    };

    //    this.context.Customers.AddRange(customers);

    //    await this.context.SaveChangesAsync().ConfigureAwait(false);
    //  }
    //}
  }
}