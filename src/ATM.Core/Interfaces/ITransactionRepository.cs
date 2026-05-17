using ATM.Core.Entities;

namespace ATM.Core.Interfaces;

public interface ITransactionRepository
{
    Task AddAsync(Transaction transaction);
    Task<List<Transaction>> GetByAccountIdAsync(int accountId, int count = 10);
}
