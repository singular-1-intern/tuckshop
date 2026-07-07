namespace Tuckshop.Api.Controllers
{
  using System.Threading.Tasks;
  using Microsoft.AspNetCore.Mvc;
  using Tuckshop.App.Services;
  using Tuckshop.Core.Models.Customers;
  using Tuckshop.Core.Models.Customers.Commands;

  /// <summary>
  /// An Api Controller for Customer wallet commands
  /// </summary>
  [ApiController]
  [Route("api/customers/commands")]
  public class CustomersCommandController : ControllerBase
  {
    private readonly CustomersCommandService customersCommandService;

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomersCommandController"/> class.
    /// </summary>
    /// <param name="customersCommandService">The customers command service</param>
    public CustomersCommandController(
      CustomersCommandService customersCommandService)
    {
      this.customersCommandService = customersCommandService;
    }

    /// <summary>
    /// Deposits funds into a customer's wallet
    /// </summary>
    /// <param name="command">The deposit funds command</param>
    /// <returns>The updated customer</returns>
    [HttpPost("wallet/deposit")]
    public virtual Task<Customer> DepositFunds([FromBody] DepositFunds command)
    {
      return this.customersCommandService.DepositFundsAsync(command);
    }

    /// <summary>
    /// Withdraws funds from a customer's wallet
    /// </summary>
    /// <param name="command">The withdraw funds command</param>
    /// <returns>The updated customer</returns>
    [HttpPost("wallet/withdraw")]
    public virtual Task<Customer> WithdrawFunds([FromBody] WithdrawFunds command)
    {
      return this.customersCommandService.WithdrawFundsAsync(command);
    }
  }
}
