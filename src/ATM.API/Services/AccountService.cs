using ATM.Core.DTOs.Account;
using ATM.Core.Interfaces;

namespace ATM.API.Services;

public class AccountService
{
    private readonly IAccountRepository _accountRepo;

    public AccountService(IAccountRepository accountRepo) => _accountRepo = accountRepo;

    public async Task<BalanceDto> GetBalanceAsync(int accountId)
    {
        var account = await _accountRepo.GetByIdAsync(accountId)
            ?? throw new KeyNotFoundException("Hesap bulunamadı.");

        return new BalanceDto
        {
            Balance = account.Balance,
            AccountNumber = account.AccountNumber,
            AccountType = account.AccountType.ToString()
        };
    }

    public async Task<AccountInfoDto> GetAccountInfoAsync(int accountId)
    {
        var account = await _accountRepo.GetByIdAsync(accountId)
            ?? throw new KeyNotFoundException("Hesap bulunamadı.");

        return new AccountInfoDto
        {
            AccountNumber = account.AccountNumber,
            AccountType = account.AccountType.ToString(),
            Balance = account.Balance,
            OwnerName = $"{account.User.FirstName} {account.User.LastName}",
            CreatedAt = account.CreatedAt
        };
    }
}
