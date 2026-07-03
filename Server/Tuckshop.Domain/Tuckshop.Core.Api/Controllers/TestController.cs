namespace Tuckshop.Core.Api.Controllers
{
  using Microsoft.AspNetCore.Mvc;
  using Neo.Model.DataAnnotations;

  /// <summary>
  /// Controller for testing things.
  /// </summary>
  [ApiController]
  [Route("api/test")]
  [RequireRole(Security.Roles.TestApi.Execute)]
  public class TestController : ControllerBase
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="TestController"/> class.
    /// </summary>
    public TestController()
    {
    }

    /// <summary>
    /// Gets the app name.
    /// </summary>
    /// <returns>The app name.</returns>
    [HttpGet("app-name")]
    public string GetAppName()
    {
      return "Tuckshop";
    }
  }
}