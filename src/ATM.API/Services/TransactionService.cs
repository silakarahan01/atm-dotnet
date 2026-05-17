using ATM.Core.DTOs.Transaction;
using ATM.Core.Entities;
using ATM.Core.Enums;
using ATM.Core.Interfaces;

namespace ATM.API.Services;

public class TransactionService
{
    private readonly IAccountRepository _accountRepo;
    private readonly ITransactionRepository _transactionRepo;

    public TransactionService(IAccountRepository accountRepo, ITransactionRepository transactionRepo)
    {
        _accountRepo = accountRepo;
        _transactionRepo = transactionRepo;
    }

    public async Task DepositAsync(int accountId, decimal amount)
    {
        if (amount <= 0)
            throw new InvalidOperationException("Yatırılacak tutar sıfırdan büyük olmalıdır.");

        var account = await _accountRepo.GetByIdAsync(accountId)
            ?? throw new KeyNotFoundException("Hesap bulunamadı.");

        account.Balance += amount;
        await _accountRepo.UpdateAsync(account);

        await _transactionRepo.AddAsync(new Transaction
        {
            Type = TransactionType.Deposit,
            Amount = amount,
            BalanceAfter = account.Balance,
            AccountId = accountId,
            Description = "Para yatırma"
        });
    }

    public async Task WithdrawAsync(int accountId, decimal amount)
    {
        if (amount <= 0)
            throw new InvalidOperationException("Çekilecek tutar sıfırdan büyük olmalıdır.");

        var account = await _accountRepo.GetByIdAsync(accountId)
            ?? throw new KeyNotFoundException("Hesap bulunamadı.");

        if (account.Balance < amount)
            throw new InvalidOperationException("Yetersiz bakiye.");

        account.Balance -= amount;
        await _accountRepo.UpdateAsync(account);

        await _transactionRepo.AddAsync(new Transaction
        {
            Type = TransactionType.Withdrawal,
            Amount = amount,
            BalanceAfter = account.Balance,
            AccountId = accountId,
            Description = "Para çekme"
        });
    }

    public async Task TransferAsync(int sourceAccountId, string targetAccountNumber, decimal amount)
    {
        if (amount <= 0)
            throw new InvalidOperationException("Transfer tutarı sıfırdan büyük olmalıdır.");

        var source = await _accountRepo.GetByIdAsync(sourceAccountId)
            ?? throw new KeyNotFoundException("Kaynak hesap bulunamadı.");

        var target = await _accountRepo.GetByAccountNumberAsync(targetAccountNumber)
            ?? throw new KeyNotFoundException("Hedef hesap bulunamadı.");

        if (source.Id == target.Id)
            throw new InvalidOperationException("Aynı hesaba transfer yapamazsınız.");

        if (source.Balance < amount)
            throw new InvalidOperationException("Yetersiz bakiye.");

        source.Balance -= amount;
        target.Balance += amount;

        await _accountRepo.UpdateAsync(source);
        await _accountRepo.UpdateAsync(target);

        // Kaynak hesap için çıkış kaydı
        await _transactionRepo.AddAsync(new Transaction
        {
            Type = TransactionType.Transfer,
            Amount = amount,
            BalanceAfter = source.Balance,
            AccountId = sourceAccountId,
            TargetAccountId = target.Id,
            Description = $"Transfer → {targetAccountNumber}"
        });

        // Hedef hesap için giriş kaydı (kendi bakiyesini göster)
        await _transactionRepo.AddAsync(new Transaction
        {
            Type = TransactionType.Transfer,
            Amount = amount,
            BalanceAfter = target.Balance,
            AccountId = target.Id,
            Description = $"Transfer ← {source.AccountNumber}"
        });
    }

    public async Task<List<TransactionDto>> GetHistoryAsync(int accountId, int count = 10)
    {
        var transactions = await _transactionRepo.GetByAccountIdAsync(accountId, count);

        return transactions.Select(t => new TransactionDto
        {
            Id = t.Id,
            Type = t.Type.ToString(),
            Amount = t.Amount,
            BalanceAfter = t.BalanceAfter,
            Description = t.Description,
            CreatedAt = t.CreatedAt
        }).ToList();
    }
}
