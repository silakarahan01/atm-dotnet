using ATM.Domain.Common;
using ATM.Domain.Enums;
using ATM.Domain.Errors;

namespace ATM.Domain.Entities;

public class Account
{
    private Account() { } // EF Core

    public Account(string accountNumber, AccountType accountType, int userId, decimal initialBalance = 0m)
    {
        AccountNumber = accountNumber;
        AccountType = accountType;
        UserId = userId;
        Balance = initialBalance;
    }

    public int Id { get; private set; }
    public string AccountNumber { get; private set; } = string.Empty;
    public decimal Balance { get; private set; }
    public AccountType AccountType { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public int UserId { get; private set; }
    public User User { get; private set; } = null!;

    public ICollection<Card> Cards { get; private set; } = new List<Card>();
    public ICollection<Transaction> Transactions { get; private set; } = new List<Transaction>();

    /// <summary>Hesaba para yatırır. İş kurallarını entity'nin kendisi korur.</summary>
    public Result Deposit(decimal amount)
    {
        if (amount <= 0)
            return Result.Failure(AccountErrors.InvalidAmount);

        Balance += amount;
        return Result.Success();
    }

    /// <summary>Hesaptan para çeker. Yetersiz bakiye ve geçersiz tutarı engeller.</summary>
    public Result Withdraw(decimal amount)
    {
        if (amount <= 0)
            return Result.Failure(AccountErrors.InvalidAmount);

        if (Balance < amount)
            return Result.Failure(AccountErrors.InsufficientFunds);

        Balance -= amount;
        return Result.Success();
    }
}
