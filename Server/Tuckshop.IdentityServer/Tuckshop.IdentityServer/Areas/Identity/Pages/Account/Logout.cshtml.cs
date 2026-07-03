namespace Tuckshop.IdentityServer.Areas.Identity.Pages.Account
{
  using System.Threading.Tasks;
  using Microsoft.AspNetCore.Authorization;
  using Microsoft.AspNetCore.Identity;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.AspNetCore.Mvc.RazorPages;
  using Microsoft.Extensions.Logging;
  using Tuckshop.IdentityServer.Models;

  /// <summary>
  /// The logout model.
  /// </summary>
  [AllowAnonymous]
  public class LogoutModel : PageModel
  {
    private readonly SignInManager<TuckshopApplicationUser> signInManager;
    private readonly ILogger<LogoutModel> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="LogoutModel"/> class.
    /// </summary>
    /// <param name="signInManager">The sign in manager.</param>
    /// <param name="logger">The logger.</param>
    public LogoutModel(SignInManager<TuckshopApplicationUser> signInManager, ILogger<LogoutModel> logger)
    {
      this.signInManager = signInManager;
      this.logger = logger;
    }

    /// <summary>
    /// Override.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous get operation.</returns>
    public async Task OnGet()
    {
      await this.signInManager.SignOutAsync();
      this.logger.LogInformation("User logged out.");
    }

    /// <summary>
    /// Receives the post command.
    /// </summary>
    /// <param name="returnUrl">The return url.</param>
    /// <returns>A task awaiting the result.</returns>
    public async Task<IActionResult> OnPost(string? returnUrl = null)
    {
      await this.signInManager.SignOutAsync();
      this.logger.LogInformation("User logged out.");
      if (returnUrl != null)
      {
        return this.LocalRedirect(returnUrl);
      }
      else
      {
        return this.RedirectToPage();
      }
    }
  }
}
