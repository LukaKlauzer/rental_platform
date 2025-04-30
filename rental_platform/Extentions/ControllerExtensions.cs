using Core.Result;
using Microsoft.AspNetCore.Mvc;

namespace rental_platform.Extentions
{
  public static class ControllerExtensions
  {
    /// <summary>
    /// Converts a Result object to an appropriate ActionResult based on success/failure and error type
    /// </summary>
    public static IActionResult ToActionResult<T>(this ControllerBase controller, Result<T> result)
        where T : notnull
    {
      if (result.IsSuccess)
        return controller.Ok(result.Value);

      // Map error types to appropriate HTTP status codes
      return result.Error.ErrorType switch
      {
        ErrorType.ValidationError => controller.BadRequest(result.Error.Message),
        ErrorType.NullReference => controller.BadRequest(result.Error.Message),
        ErrorType.MappingError => controller.BadRequest(result.Error.Message),
        ErrorType.NotFound => controller.NotFound(result.Error.Message),
        ErrorType.DatabaseReadError => controller.StatusCode(503, result.Error.Message),
        ErrorType.DatabaseWriteError => controller.StatusCode(500, result.Error.Message),
        ErrorType.ProcessingCsv => controller.BadRequest(result.Error.Message),
        _ => controller.StatusCode(500, result.Error.Message)
      };
    }
  }
}
