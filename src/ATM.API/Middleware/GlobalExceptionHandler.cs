using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ATM.API.Middleware;

/// <summary>
/// Beklenmeyen (yakalanmamış) hataları RFC 7807 ProblemDetails yanıtına çevirir.
/// İş kuralı hataları Result deseni ile döndüğü için buraya yalnızca gerçek
/// hatalar (ör. altyapı/veritabanı) düşer.
/// </summary>
public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, "İşlenmeyen bir hata oluştu: {Message}", exception.Message);

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Sunucu hatası",
            Detail = "Beklenmeyen bir hata oluştu. Lütfen daha sonra tekrar deneyin."
        };

        httpContext.Response.StatusCode = problemDetails.Status.Value;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
