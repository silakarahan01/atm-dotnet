using ATM.Domain.Common;
using ATM.Domain.Errors;

namespace ATM.Domain.Entities;

public class Card
{
    public const int MaxFailedAttempts = 3;

    private Card() { } // EF Core

    public Card(string cardNumber, string pinHash, DateTime expiryDate, int userId, int accountId)
    {
        CardNumber = cardNumber;
        PinHash = pinHash;
        ExpiryDate = expiryDate;
        UserId = userId;
        AccountId = accountId;
    }

    public int Id { get; private set; }
    public string CardNumber { get; private set; } = string.Empty;
    public string PinHash { get; private set; } = string.Empty;
    public DateTime ExpiryDate { get; private set; }
    public bool IsActive { get; private set; } = true;
    public bool IsBlocked { get; private set; }
    public int FailedAttempts { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public int UserId { get; private set; }
    public User User { get; private set; } = null!;

    public int AccountId { get; private set; }
    public Account Account { get; private set; } = null!;

    /// <summary>
    /// Hatalı PIN denemesini kaydeder; limit aşılırsa kartı bloke eder.
    /// Çağırana uygun hatayı (kalan deneme ya da bloke) döner.
    /// </summary>
    public Error RegisterFailedAttempt()
    {
        FailedAttempts++;

        if (FailedAttempts >= MaxFailedAttempts)
        {
            IsBlocked = true;
            return CardErrors.JustBlocked;
        }

        return CardErrors.InvalidPin(MaxFailedAttempts - FailedAttempts);
    }

    /// <summary>Başarılı girişte hatalı deneme sayacını sıfırlar.</summary>
    public void ResetFailedAttempts() => FailedAttempts = 0;

    /// <summary>PIN'i yeni bir hash ile günceller.</summary>
    public void ChangePin(string newPinHash) => PinHash = newPinHash;
}
