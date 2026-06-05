using ATM.Domain.Common;

namespace ATM.Domain.Errors;

public static class CardErrors
{
    public static readonly Error NotFound =
        Error.NotFound("Card.NotFound", "Kart bulunamadı.");

    public static readonly Error Blocked =
        Error.Unauthorized("Card.Blocked", "Kart bloke edilmiştir. Lütfen bankanızla iletişime geçin.");

    public static readonly Error JustBlocked =
        Error.Unauthorized("Card.JustBlocked", "3 hatalı girişten sonra kart bloke edildi.");

    public static Error InvalidPin(int remainingAttempts) =>
        Error.Unauthorized("Card.InvalidPin", $"Hatalı PIN. {remainingAttempts} deneme hakkınız kaldı.");

    public static readonly Error WrongCurrentPin =
        Error.Unauthorized("Card.WrongCurrentPin", "Mevcut PIN hatalı.");

    public static readonly Error InvalidNewPin =
        Error.Validation("Card.InvalidNewPin", "Yeni PIN 4 haneli bir sayı olmalıdır.");
}
