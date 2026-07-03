namespace Tuckshop.IdentityServer.Areas.Identity.Pages.Account
{
  using System;
  using System.ComponentModel.DataAnnotations;
  using System.Text.Encodings.Web;
  using System.Threading.Tasks;
  using Microsoft.AspNetCore.Authorization;
  using Microsoft.AspNetCore.Identity;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.AspNetCore.Mvc.RazorPages;
  using Microsoft.Extensions.Logging;
  using Neo.IdentityServer.Providers;
  using Neo.NotificationServer.Services;
  using Tuckshop.IdentityServer.App.Notifications;
  using Tuckshop.IdentityServer.App.Services;
  using Tuckshop.IdentityServer.Models;

  /// <summary>
  /// Forgot Password.
  /// </summary>
  [AllowAnonymous]
  public class ForgotPasswordModel : PageModel
  {
    private readonly UserManager<TuckshopApplicationUser> userManager;
    private readonly INotificationService notificationService;
    private readonly IDataProtectionOptionsProvider dataProtectionOptionsProvider;
    private readonly ILogger<ForgotPasswordModel> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ForgotPasswordModel"/> class.
    /// </summary>
    /// <param name="userManager">The user manager.</param>
    /// <param name="notificationService">The notification service.</param>
    /// <param name="dataProtectionOptionsProvider">The data protection options provider.</param>
    /// <param name="logger">The logger.</param>
    public ForgotPasswordModel(
      UserManager<TuckshopApplicationUser> userManager,
      INotificationService notificationService,
      IDataProtectionOptionsProvider dataProtectionOptionsProvider,
      ILogger<ForgotPasswordModel> logger)
    {
      this.userManager = userManager;
      this.notificationService = notificationService;
      this.dataProtectionOptionsProvider = dataProtectionOptionsProvider;
      this.logger = logger;
    }

    /// <summary>
    /// Gets or sets the input model.
    /// </summary>
    [BindProperty]
    public InputModel Input { get; set; } = new InputModel();

    /// <summary>
    /// Forgot Password Input Model.
    /// </summary>
    public class InputModel
    {
      [Required]
      [EmailAddress]
      public string? Email { get; set; }
    }

    public async Task<IActionResult> OnPostAsync()
    {
      if (this.ModelState.IsValid)
      {
        var user = await this.userManager.FindByEmailAsync(this.Input.Email!);
        if (user == null || !await this.userManager.IsEmailConfirmedAsync(user))
        {
          // Don't reveal that the user does not exist or is not confirmed
          return this.RedirectToPage("./ForgotPasswordConfirmation");
        }

        // For more information on how to enable account confirmation and password reset please 
        // visit https://go.microsoft.com/fwlink/?LinkID=532713

        var code = await this.userManager.GeneratePasswordResetTokenAsync(user);

        var callbackUrl = this.Url.Page(
            "/Account/ResetPassword",
            pageHandler: null,
            values: new { area = "Identity", code },
            protocol: this.Request.Scheme) ?? throw new ArgumentException("Reset Password page path not found.");

        try
        {
          var model = ResetPassword.Create(
            this.Input.Email!,
            HtmlEncoder.Default.Encode(callbackUrl),
            this.dataProtectionOptionsProvider.GetOptions(DataProtectionPurposes.ResetPassword).TokenLifespan.TotalHours);

          await this.notificationService.SendNotificationAsync(
            TemplateTypes.ResetPasswordEmail,
            model);

          return this.RedirectToPage("./ForgotPasswordConfirmation");
        }
        catch (Exception ex)
        {
          this.logger.LogError(ex, "Error sending ResetPassword Email");
          throw;
        }
      }

      return this.Page();
    }
  }
}
