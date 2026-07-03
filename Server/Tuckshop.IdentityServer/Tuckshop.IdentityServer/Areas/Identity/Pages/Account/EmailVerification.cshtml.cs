#pragma warning disable SA1649 // File name should match first type name
#pragma warning disable CA1056 // URI-like properties should not be strings
#pragma warning disable CA1034 // Nested types should not be visible

namespace Tuckshop.IdentityServer.Areas.Identity.Pages.Account
{
  using System;
  using System.ComponentModel.DataAnnotations;
  using System.Threading.Tasks;
  using Microsoft.AspNetCore.Identity;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.AspNetCore.Mvc.RazorPages;
  using Tuckshop.IdentityServer.App.Services;
  using Tuckshop.IdentityServer.Models;

  /// <summary>
  /// The Email Verification Model class.
  /// </summary>
  public class EmailVerificationModel : PageModel
  {
    private readonly IRegistrationEmailService emailVerificationService;
    private readonly UserManager<TuckshopApplicationUser> userManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailVerificationModel"/> class.
    /// </summary>
    /// <param name="emailVerificationService">The Email Verification Service.</param>
    /// <param name="userManager">The User Manger.</param>
    public EmailVerificationModel(
        IRegistrationEmailService emailVerificationService,
        UserManager<TuckshopApplicationUser> userManager)
    {
      this.emailVerificationService = emailVerificationService;
      this.userManager = userManager;
    }

    /// <summary>
    /// Gets or Sets an Input.
    /// </summary>
    [BindProperty]
    public InputModel Input { get; set; } = new InputModel();

    /// <summary>
    /// Gets or Sets the Return Url.
    /// </summary>
    public string? ReturnUrl { get; set; }

    /// <summary>
    /// Gets the link expiry hours.
    /// </summary>
    public double LinkExpiryHours { get; private set; }

    /// <summary>
    /// The On Get method.
    /// </summary>
    /// <param name="returnUrl">The return url.</param>
    public void OnGet(string? returnUrl = null)
    {
      this.ReturnUrl = returnUrl;
    }

    /// <summary>
    /// The On Post method.
    /// </summary>
    /// <returns>A Task.</returns>
    public async Task<IActionResult> OnPostAsync()
    {
      if (this.ModelState.IsValid)
      {
        var user = await this.userManager.FindByEmailAsync(this.Input.Email!);
        if (user == null || (await this.userManager.IsEmailConfirmedAsync(user)))
        {
          this.ModelState.AddModelError(string.Empty, $"That email address was not found or is already verified.");
        }
        else
        {
          try
          {
            await this.emailVerificationService.SendVerificationEmailAsync(user);
            return this.RedirectToPage("./EmailVerificationConfirmation");
          }
          catch (Exception)
          {
            return this.RedirectToPage("./Error");
          }
        }
      }

      return this.Page();
    }

    /// <summary>
    /// An Input Model.
    /// </summary>
    public class InputModel
    {
      /// <summary>
      /// Gets or sets an Email.
      /// </summary>
      [Required]
      [EmailAddress]
      [Display(Name = "Email To Send Verification")]
      public string? Email { get; set; }
    }
  }
}
