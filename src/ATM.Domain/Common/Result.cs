namespace ATM.Domain.Common;

/// <summary>
/// Bir işlemin başarı/başarısızlık sonucunu, akış kontrolü için exception
/// kullanmadan temsil eden hafif Result deseni.
/// </summary>
public class Result
{
    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None)
            throw new InvalidOperationException("Başarılı bir sonuç hata taşıyamaz.");
        if (!isSuccess && error == Error.None)
            throw new InvalidOperationException("Başarısız bir sonuç hata taşımak zorundadır.");

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    public static Result Success() => new(true, Error.None);
    public static Result Failure(Error error) => new(false, error);

    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, Error.None);
    public static Result<TValue> Failure<TValue>(Error error) => new(default, false, error);
}

/// <summary>
/// Bir değer taşıyan başarı/başarısızlık sonucu.
/// </summary>
public class Result<TValue> : Result
{
    private readonly TValue? _value;

    protected internal Result(TValue? value, bool isSuccess, Error error)
        : base(isSuccess, error) => _value = value;

    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Başarısız bir sonucun değeri okunamaz.");

    public static implicit operator Result<TValue>(TValue value) => Success(value);
}
