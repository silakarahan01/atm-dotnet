using ATM.Core.Enums;

namespace ATM.Core.Entities;

public class Transaction
{
    public int Id { get; set; }
    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public decimal BalanceAfter { get; set; }
    public string? Description { get; set; }
    public bool IsSuccess { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int AccountId { get; set; }
    public Account Account { get; set; } = null!;

    public int? TargetAccountId { get; set; }
    public Account? TargetAccount { get; set; }
}
