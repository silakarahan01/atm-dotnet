using ATM.Domain.Enums;

namespace ATM.Domain.Entities;

public class Transaction
{
    private Transaction() { } // EF Core

    public Transaction(
        TransactionType type,
        decimal amount,
        decimal balanceAfter,
        int accountId,
        string? description = null,
        int? targetAccountId = null)
    {
        Type = type;
        Amount = amount;
        BalanceAfter = balanceAfter;
        AccountId = accountId;
        Description = description;
        TargetAccountId = targetAccountId;
    }

    public int Id { get; private set; }
    public TransactionType Type { get; private set; }
    public decimal Amount { get; private set; }
    public decimal BalanceAfter { get; private set; }
    public string? Description { get; private set; }
    public bool IsSuccess { get; private set; } = true;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public int AccountId { get; private set; }
    public Account Account { get; private set; } = null!;

    public int? TargetAccountId { get; private set; }
    public Account? TargetAccount { get; private set; }
}
