using ATM.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace ATM.API.Extensions;

public static class ResultExtensions
{
    /// <summary>
    /// Bir <see cref="Error"/>'ı RFC 7807 uyumlu bir ProblemDetails yanıtına ve
    /// hata türüne uygun HTTP durum koduna dönüştürür.
    /// </summary>
    public static IActionResult ToProblem(this ControllerBase controller, Error error)
    {
        var statusCode = error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status500InternalServerError
        };

        return controller.Problem(detail: error.Message, statusCode: statusCode, title: error.Code);
    }
}
