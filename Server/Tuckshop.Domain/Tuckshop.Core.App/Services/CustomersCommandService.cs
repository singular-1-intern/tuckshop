namespace Tuckshop.App.Services
{
  using System.Threading.Tasks;
  using Tuckshop.Core.App.Services;
  using Tuckshop.Core.Models.Customers;
  using Tuckshop.Core.Models.Customers.Commands;

  /// <summary>
  /// Service for customer command operations
  /// </summary>
  public class CustomersCommandService
  {
    private readonly CustomersModelService modelService;

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomersCommandService"/> class.
    /// </summary>
    /// <param name="modelService">The customers model service</param>
    public CustomersCommandService(CustomersModelService modelService)
    {
      this.modelService = modelService;
    }

    /// <summary>
    /// Deposits funds into the customer's wallet
    /// </summary>
    /// <param name="command">The deposit funds command</param>
    /// <returns>The updated customer</returns>
    public async Task<Customer> DepositFundsAsync(DepositFunds command)
    {
      var customer = await this.modelService.GetByIdAsync(command.CustomerId).ConfigureAwait(false);
      customer.IncreaseBalance(command.Amount);
      await this.modelService.SaveChangesAsync().ConfigureAwait(false);
      return customer;
    }

    /// <summary>
    /// Withdraws funds from the customer's wallet
    /// </summary>
    /// <param name="command">The withdraw funds command</param>
    /// <returns>The updated customer</returns>
    public async Task<Customer> WithdrawFundsAsync(WithdrawFunds command)
    {
      var customer = await this.modelService.GetByIdAsync(command.CustomerId).ConfigureAwait(false);
      customer.DecreaseBalance(command.Amount);
      await this.modelService.SaveChangesAsync().ConfigureAwait(false);
      return customer;
    }
  }
}
