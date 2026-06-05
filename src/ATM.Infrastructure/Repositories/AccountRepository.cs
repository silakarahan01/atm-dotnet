using ATM.Application.Abstractions.Persistence;
using ATM.Domain.Entities;
using ATM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ATM.Infrastructure.Repositories;

public sealed class AccountRepository(AppDbContext context) : IAccountRepository
{
    public Task<Account?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => context.Accounts
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public Task<Account?> GetByAccountNumberAsync(string accountNumber, CancellationToken cancellationToken = default)
        => context.Accounts
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber, cancellationToken);
}
