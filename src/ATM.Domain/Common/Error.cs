namespace ATM.Domain.Common;

/// <summary>
/// İş kuralı hatalarının türü. API katmanı bunu HTTP durum koduna eşler.
/// </summary>
public enum ErrorType
{
    Failure,
    Validation,
    NotFound,
    Conflict,
    Unauthorized
}

/// <summary>
/// Bir iş kuralı hatasını temsil eden değer nesnesi (kod + mesaj + tür).
/// Exception fırlatmak yerine <see cref="Result"/> içinde taşınır.
/// </summary>
public sealed record Error(string Code, string Message, ErrorType Type)
{
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);

    public static Error Failure(string code, string message) => new(code, message, ErrorType.Failure);
    public static Error Validation(string code, string message) => new(code, message, ErrorType.Validation);
    public static Error NotFound(string code, string message) => new(code, message, ErrorType.NotFound);
    public static Error Conflict(string code, string message) => new(code, message, ErrorType.Conflict);
    public static Error Unauthorized(string code, string message) => new(code, message, ErrorType.Unauthorized);
}
