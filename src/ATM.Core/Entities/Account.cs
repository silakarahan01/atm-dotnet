using ATM.Core.Enums;

namespace ATM.Core.Entities;

public class Account
{
    public int Id { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public decimal Balance { get; set; } = 0;
    public AccountType AccountType { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public ICollection<Card> Cards { get; set; } = new List<Card>();
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
