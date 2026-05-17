namespace ATM.Core.Entities;

public class Card
{
    public int Id { get; set; }
    public string CardNumber { get; set; } = string.Empty;
    public string PinHash { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsBlocked { get; set; } = false;
    public int FailedAttempts { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int AccountId { get; set; }
    public Account Account { get; set; } = null!;
}
