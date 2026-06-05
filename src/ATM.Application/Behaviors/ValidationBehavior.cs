using ATM.Domain.Common;
using FluentValidation;
using MediatR;

namespace ATM.Application.Behaviors;

/// <summary>
/// İlgili komut/sorgu için kayıtlı tüm FluentValidation doğrulayıcılarını çalıştırır.
/// Hata varsa exception fırlatmak yerine başarısız bir <see cref="Result"/> döner;
/// böylece tüm akış Result deseni üzerinde tutarlı kalır.
/// </summary>
public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);

        var failures = (await Task.WhenAll(
                validators.Select(v => v.ValidateAsync(context, cancellationToken))))
            .SelectMany(result => result.Errors)
            .Where(failure => failure is not null)
            .ToList();

        if (failures.Count == 0)
            return await next();

        var message = string.Join(" ", failures.Select(f => f.ErrorMessage).Distinct());
        var error = Error.Validation("Validation.Failed", message);

        return CreateValidationResult(error);
    }

    private static TResponse CreateValidationResult(Error error)
    {
        if (typeof(TResponse) == typeof(Result))
            return (TResponse)(object)Result.Failure(error);

        // TResponse == Result<TValue> => uygun jenerik Failure<TValue> metodunu çağır.
        var valueType = typeof(TResponse).GetGenericArguments()[0];
        var failure = typeof(Result)
            .GetMethods()
            .First(m => m is { Name: nameof(Result.Failure), IsGenericMethod: true })
            .MakeGenericMethod(valueType)
            .Invoke(null, [error])!;

        return (TResponse)failure;
    }
}
