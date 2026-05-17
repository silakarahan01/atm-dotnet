using ATM.Core.Entities;

namespace ATM.Core.Interfaces;

public interface IAccountRepository
{
    Task<Account?> GetByIdAsync(int id);
    Task<Account?> GetByAccountNumberAsync(string accountNumber);
    Task UpdateAsync(Account account);
}
