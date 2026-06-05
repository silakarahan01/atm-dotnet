using ATM.Domain.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ATM.Application.Behaviors;

/// <summary>
/// Her komut/sorgu için başlangıç, başarı ve başarısızlık durumlarını loglar.
/// Cross-cutting loglama tek yerde toplanır.
/// </summary>
public sealed class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        logger.LogInformation("İşleniyor: {RequestName}", requestName);

        var response = await next();

        if (response.IsSuccess)
            logger.LogInformation("Tamamlandı: {RequestName}", requestName);
        else
            logger.LogWarning("Başarısız: {RequestName} — {ErrorCode}: {ErrorMessage}",
                requestName, response.Error.Code, response.Error.Message);

        return response;
    }
}
