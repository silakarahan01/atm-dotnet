using ATM.Domain.Common;

namespace ATM.Domain.Errors;

public static class AccountErrors
{
    public static readonly Error NotFound =
        Error.NotFound("Account.NotFound", "Hesap bulunamadı.");

    public static readonly Error TargetNotFound =
        Error.NotFound("Account.TargetNotFound", "Hedef hesap bulunamadı.");

    public static readonly Error InvalidAmount =
        Error.Validation("Account.InvalidAmount", "Tutar sıfırdan büyük olmalıdır.");

    public static readonly Error InsufficientFunds =
        Error.Validation("Account.InsufficientFunds", "Yetersiz bakiye.");

    public static readonly Error SameAccountTransfer =
        Error.Validation("Account.SameAccountTransfer", "Aynı hesaba transfer yapamazsınız.");
}
