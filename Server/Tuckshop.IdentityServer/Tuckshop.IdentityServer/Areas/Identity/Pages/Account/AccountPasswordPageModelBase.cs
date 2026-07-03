namespace Tuckshop.IdentityServer.Areas.Identity.Pages.Account
{
  using Tuckshop.IdentityServer.App.Services;
  using Tuckshop.IdentityServer.Models;

  /// <summary>
  /// A base class for account pages with passwords.
  /// </summary>
  /// <typeparam name="TPageModel">The page model type.</typeparam>
  public class AccountPasswordPageModelBase<TPageModel> : AccountPageModelBase<TPageModel>
  {
    private readonly IPasswordValidator<TuckshopApplicationUser> passwordValidator;
    private readonly UserManager<TuckshopApplicationUser> userManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="AccountPageModelBase{TPageModel}"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="urlProvider">The url provider.</param>
    /// <param name="configuration">The configuration.</param>
    /// <param name="passwordValidator">The password validator.</param>
    /// <param name="userManager">The user manager.</param>
    public AccountPasswordPageModelBase(
      ILogger<TPageModel> logger,
      IUrlProvider urlProvider,
      IConfiguration configuration,
      IPasswordValidator<TuckshopApplicationUser> passwordValidator,
      UserManager<TuckshopApplicationUser> userManager)
      : base(logger, urlProvider, configuration)
    {
      this.passwordValidator = passwordValidator;
      this.userManager = userManager;
    }

    /// <summary>
    /// Gets or sets the password.
    /// </summary>
    [BindProperty]
    [Required]
    [PageRemote(
      HttpMethod = "POST",
      PageHandler = "VerifyPassword",
      AdditionalFields = $"__RequestVerificationToken")] // NOTE: this can be changed to $"__RequestVerificationToken,Input.Email" to include the Input.Email property for further validation params.
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the confirm password.
    /// </summary>
    [BindProperty]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare(nameof(Password), ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    /// <summary>
    /// Validates the password.
    /// </summary>
    /// <returns>The validation result.</returns>
    public async Task<JsonResult> OnPostVerifyPassword()
    {
      var user = new TuckshopApplicationUser();
      var validationResult = await this.passwordValidator.ValidateAsync(this.userManager, user, this.Password);
      if (validationResult.Succeeded)
      {
        return new JsonResult(true);
      }
      else
      {
        return new JsonResult($"Invalid Password. {validationResult.Errors.First().Description}");
      }
    }
  }
}
