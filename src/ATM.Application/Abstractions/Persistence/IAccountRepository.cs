using ATM.Domain.Entities;

namespace ATM.Application.Abstractions.Persistence;

public interface IAccountRepository
{
    Task<Account?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Account?> GetByAccountNumberAsync(string accountNumber, CancellationToken cancellationToken = default);
}
