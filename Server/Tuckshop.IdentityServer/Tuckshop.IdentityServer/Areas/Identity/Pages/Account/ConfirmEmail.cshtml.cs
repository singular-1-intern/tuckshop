namespace Tuckshop.IdentityServer.Areas.Identity.Pages.Account
{
  using System.Text;
  using System.Threading.Tasks;
  using Microsoft.AspNetCore.Authorization;
  using Microsoft.AspNetCore.Identity;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.AspNetCore.Mvc.RazorPages;
  using Microsoft.AspNetCore.WebUtilities;
  using Tuckshop.IdentityServer.Models;

  /// <summary>
  /// Confirm email.
  /// </summary>
  [AllowAnonymous]
  public class ConfirmEmailModel : PageModel
  {
    private readonly UserManager<TuckshopApplicationUser> userManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfirmEmailModel"/> class.
    /// </summary>
    /// <param name="userManager">The user manager.</param>
    public ConfirmEmailModel(UserManager<TuckshopApplicationUser> userManager)
    {
      this.userManager = userManager;
    }

    /// <summary>
    /// Gets or sets the Status Message.
    /// </summary>
    [TempData]
    public string StatusMessage { get; set; } = string.Empty;

    /// <summary>
    /// Executes when the Confirm EMail page is fetched.
    /// </summary>
    /// <param name="userId">The user id.</param>
    /// <param name="code">Confirm email code.</param>
    /// <returns>A task awaiting the get operation.</returns>
    public async Task<IActionResult> OnGetAsync([FromQuery] string userId, [FromQuery] string code)
    {
      if (userId == null || code == null)
      {
        return this.RedirectToPage("/Index");
      }

      var user = await this.userManager.FindByIdAsync(userId);
      if (user == null)
      {
        return this.NotFound($"Unable to load user with ID '{userId}'.");
      }

      // Decode the token
      code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
      var result = await this.userManager.ConfirmEmailAsync(user, code);
      this.StatusMessage = result.Succeeded ? "Thank you for confirming your email." : "Error confirming your email.";

      return this.Page();
    }
  }
}
