namespace Tuckshop.IdentityServer.Filters
{
  using System;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.AspNetCore.Mvc.Filters;

  /// <summary>
  /// A filter that will pick up an UnauthorizedAccessException and push the correct Forbidden result 
  /// into the response.
  /// </summary>
  public class UnauthorizedAccessExceptionFilter : IExceptionFilter
  {
    /// <inheritdoc/>
    public void OnException(ExceptionContext context)
    {
      if (context.Exception is UnauthorizedAccessException)
      {
        context.ExceptionHandled = true;
        context.Result = new StatusCodeResult(403);
      }
    }
  }
}
