using ATM.Core.Entities;
using ATM.Core.Interfaces;
using ATM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ATM.Infrastructure.Repositories;

public class AccountRepository : IAccountRepository
{
    private readonly AppDbContext _context;

    public AccountRepository(AppDbContext context) => _context = context;

    public async Task<Account?> GetByIdAsync(int id)
        => await _context.Accounts
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.Id == id);

    public async Task<Account?> GetByAccountNumberAsync(string accountNumber)
        => await _context.Accounts
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber);

    public async Task UpdateAsync(Account account)
    {
        _context.Accounts.Update(account);
        await _context.SaveChangesAsync();
    }
}
