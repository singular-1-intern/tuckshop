namespace Tuckshop.Core.App.Services
{
  using Neo.Model.Services;
  using Tuckshop.Core.Models;
  using Tuckshop.Core.Models.Customers;

  /// <summary>
  /// Service for working with Customers. Provides basic CRUD functionality.
  /// </summary>
  public class CustomersModelService : UpdateableModelService<Customer, ModelDbContext, int>
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="CustomersModelService"/> class.
    /// </summary>
    /// <param name="context">The db context</param>
    public CustomersModelService(ModelDbContext context)
      : base(context, new ModelServiceOptions<Customer>())
    {
    }
  }
}