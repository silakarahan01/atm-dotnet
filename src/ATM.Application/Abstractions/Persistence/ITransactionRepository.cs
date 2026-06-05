using ATM.Domain.Entities;

namespace ATM.Application.Abstractions.Persistence;

public interface ITransactionRepository
{
    Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Transaction>> GetByAccountIdAsync(int accountId, int count, CancellationToken cancellationToken = default);
}
