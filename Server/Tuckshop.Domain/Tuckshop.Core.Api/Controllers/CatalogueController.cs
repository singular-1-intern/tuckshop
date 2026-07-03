namespace Tuckshop.Core.Api.Controllers
{
  using Microsoft.AspNetCore.Mvc;
  using Tuckshop.Core.App.Services;

  /// <summary>
  /// Controller for catalogue data.
  /// </summary>
  /// <param name="catalogueModelService">Catalogue model service.</param>
  [ApiController]
  [Route("api/catalogue")]
  public class CatalogueController(CatalogueModelService catalogueModelService) : ControllerBase
  {
  }
}