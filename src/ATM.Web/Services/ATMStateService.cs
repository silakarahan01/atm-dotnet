using ATM.Core.DTOs.Transaction;
using ATM.Core.Entities;
using ATM.Core.Enums;
using ATM.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ATM.Web.Services;

public enum ATMScreen
{
    Welcome, Pin, Menu, Balance, Withdraw, Deposit, Transfer, History, CardBlocked
}

public class ATMStateService : IDisposable
{
    private readonly IServiceScopeFactory _scopeFactory;
    private System.Threading.Timer? _sessionTimer;
    private const int SessionDuration = 60;

    public ATMScreen CurrentScreen { get; private set; } = ATMScreen.Welcome;
    public string CardholderName { get; private set; } = "";
    public int AccountId { get; private set; }
    public int CardId { get; private set; }
    public decimal Balance { get; private set; }
    public string AccountNumber { get; private set; } = "";
    public int PinAttempts { get; private set; }
    public int SessionCountdown { get; private set; } = SessionDuration;
    public bool IsLoading { get; private set; }
    public bool HasError { get; private set; }
    public string? ErrorMessage { get; private set; }
    public string? SuccessMessage { get; private set; }
    public bool IsMoneyAnimating { get; private set; }
    public bool IsCardAnimating { get; private set; }
    public List<TransactionDto> Transactions { get; private set; } = new();

    public event Action? OnChange;

    public ATMStateService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    private void Notify() => OnChange?.Invoke();

    private void ClearMessages()
    {
        ErrorMessage = null;
        SuccessMessage = null;
        HasError = false;
    }

    public void InsertCard()
    {
        IsCardAnimating = true;
        Notify();
    }

    public async Task LoadCardAsync(string cardNumber)
    {
        IsLoading = true;
        ClearMessages();
        Notify();

        await Task.Delay(1200);

        using var scope = _scopeFactory.CreateScope();
        var cardRepo = scope.ServiceProvider.GetRequiredService<ICardRepository>();

        var card = await cardRepo.GetByCardNumberAsync(cardNumber);

        if (card == null)
        {
            HasError = true;
            ErrorMessage = "Kart tanınmadı.";
            IsCardAnimating = false;
            IsLoading = false;
            Notify();
            return;
        }

        if (card.IsBlocked)
        {
            IsCardAnimating = false;
            IsLoading = false;
            NavigateTo(ATMScreen.CardBlocked);
            return;
        }

        CardId = card.Id;
        AccountId = card.AccountId;
        AccountNumber = card.Account.AccountNumber;
        CardholderName = $"{card.User.FirstName} {card.User.LastName}";
        Balance = card.Account.Balance;
        PinAttempts = 0;

        IsLoading = false;
        IsCardAnimating = false;
        NavigateTo(ATMScreen.Pin);
    }

    public async Task VerifyPinAsync(string pin)
    {
        IsLoading = true;
        ClearMessages();
        Notify();

        await Task.Delay(600);

        using var scope = _scopeFactory.CreateScope();
        var cardRepo = scope.ServiceProvider.GetRequiredService<ICardRepository>();

        var card = await cardRepo.GetByIdAsync(CardId);
        if (card == null) { Logout(); return; }

        if (!BCrypt.Net.BCrypt.Verify(pin, card.PinHash))
        {
            card.FailedAttempts++;
            if (card.FailedAttempts >= 3)
            {
                card.IsBlocked = true;
                await cardRepo.UpdateAsync(card);
                IsLoading = false;
                Notify();
                await Task.Delay(1500);
                NavigateTo(ATMScreen.CardBlocked);
                return;
            }

            PinAttempts = card.FailedAttempts;
            await cardRepo.UpdateAsync(card);
            HasError = true;
            ErrorMessage = $"Hatalı PIN. {3 - PinAttempts} deneme hakkınız kaldı.";
            IsLoading = false;
            Notify();
            return;
        }

        card.FailedAttempts = 0;
        await cardRepo.UpdateAsync(card);
        PinAttempts = 0;
        IsLoading = false;
        StartSessionTimer();
        NavigateTo(ATMScreen.Menu);
    }

    public async Task RefreshBalanceAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IAccountRepository>();
        var acc = await repo.GetByIdAsync(AccountId);
        if (acc != null) Balance = acc.Balance;
        Notify();
    }

    public async Task WithdrawAsync(decimal amount)
    {
        IsLoading = true;
        ClearMessages();
        Notify();

        if (amount > Balance)
        {
            HasError = true;
            ErrorMessage = "Yetersiz bakiye.";
            IsLoading = false;
            Notify();
            return;
        }

        await Task.Delay(600);

        using var scope = _scopeFactory.CreateScope();
        var accountRepo = scope.ServiceProvider.GetRequiredService<IAccountRepository>();
        var txRepo = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();

        var account = await accountRepo.GetByIdAsync(AccountId);
        if (account == null) { IsLoading = false; return; }

        account.Balance -= amount;
        await accountRepo.UpdateAsync(account);
        await txRepo.AddAsync(new Transaction
        {
            Type = TransactionType.Withdrawal,
            Amount = amount,
            BalanceAfter = account.Balance,
            AccountId = AccountId,
            Description = "Para çekme"
        });

        Balance = account.Balance;
        IsLoading = false;
        IsMoneyAnimating = true;
        Notify();

        await Task.Delay(2800);
        IsMoneyAnimating = false;
        SuccessMessage = $"{amount:N2} ₺ başarıyla çekildi. Paranızı alınız.";
        Notify();

        await Task.Delay(2500);
        ClearMessages();
        NavigateTo(ATMScreen.Menu);
    }

    public async Task DepositAsync(decimal amount)
    {
        IsLoading = true;
        ClearMessages();
        Notify();
        await Task.Delay(1200);

        using var scope = _scopeFactory.CreateScope();
        var accountRepo = scope.ServiceProvider.GetRequiredService<IAccountRepository>();
        var txRepo = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();

        var account = await accountRepo.GetByIdAsync(AccountId);
        if (account == null) { IsLoading = false; return; }

        account.Balance += amount;
        await accountRepo.UpdateAsync(account);
        await txRepo.AddAsync(new Transaction
        {
            Type = TransactionType.Deposit,
            Amount = amount,
            BalanceAfter = account.Balance,
            AccountId = AccountId,
            Description = "Para yatırma"
        });

        Balance = account.Balance;
        IsLoading = false;
        SuccessMessage = $"{amount:N2} ₺ başarıyla yatırıldı.";
        Notify();

        await Task.Delay(2500);
        ClearMessages();
        NavigateTo(ATMScreen.Menu);
    }

    public async Task TransferAsync(string targetAccountNumber, decimal amount)
    {
        IsLoading = true;
        ClearMessages();
        Notify();

        if (amount > Balance)
        {
            HasError = true;
            ErrorMessage = "Yetersiz bakiye.";
            IsLoading = false;
            Notify();
            return;
        }

        await Task.Delay(800);

        using var scope = _scopeFactory.CreateScope();
        var accountRepo = scope.ServiceProvider.GetRequiredService<IAccountRepository>();
        var txRepo = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();

        var source = await accountRepo.GetByIdAsync(AccountId);
        var target = await accountRepo.GetByAccountNumberAsync(targetAccountNumber);

        if (target == null)
        {
            HasError = true;
            ErrorMessage = "Hedef hesap bulunamadı.";
            IsLoading = false;
            Notify();
            return;
        }

        if (source!.Id == target.Id)
        {
            HasError = true;
            ErrorMessage = "Aynı hesaba transfer yapamazsınız.";
            IsLoading = false;
            Notify();
            return;
        }

        source.Balance -= amount;
        target.Balance += amount;
        await accountRepo.UpdateAsync(source);
        await accountRepo.UpdateAsync(target);

        await txRepo.AddAsync(new Transaction
        {
            Type = TransactionType.Transfer, Amount = amount,
            BalanceAfter = source.Balance, AccountId = AccountId,
            TargetAccountId = target.Id, Description = $"Transfer → {targetAccountNumber}"
        });
        await txRepo.AddAsync(new Transaction
        {
            Type = TransactionType.Transfer, Amount = amount,
            BalanceAfter = target.Balance, AccountId = target.Id,
            Description = $"Transfer ← {source.AccountNumber}"
        });

        Balance = source.Balance;
        IsLoading = false;
        SuccessMessage = $"{amount:N2} ₺ {targetAccountNumber} hesabına transfer edildi.";
        Notify();

        await Task.Delay(2500);
        ClearMessages();
        NavigateTo(ATMScreen.Menu);
    }

    public async Task LoadHistoryAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var txRepo = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();
        var list = await txRepo.GetByAccountIdAsync(AccountId, 10);
        Transactions = list.Select(t => new TransactionDto
        {
            Id = t.Id, Type = t.Type.ToString(), Amount = t.Amount,
            BalanceAfter = t.BalanceAfter, Description = t.Description, CreatedAt = t.CreatedAt
        }).ToList();
        Notify();
    }

    public void NavigateTo(ATMScreen screen)
    {
        ClearMessages();
        CurrentScreen = screen;
        ResetSessionTimer();
        Notify();
    }

    public void Logout()
    {
        StopSessionTimer();
        CardholderName = ""; AccountId = 0; CardId = 0;
        Balance = 0; AccountNumber = ""; PinAttempts = 0;
        IsCardAnimating = false; IsMoneyAnimating = false;
        CurrentScreen = ATMScreen.Welcome;
        ClearMessages();
        Notify();
    }

    private void StartSessionTimer()
    {
        SessionCountdown = SessionDuration;
        _sessionTimer = new System.Threading.Timer(_ =>
        {
            SessionCountdown--;
            if (SessionCountdown <= 0) Logout();
            else Notify();
        }, null, 1000, 1000);
    }

    private void StopSessionTimer()
    {
        _sessionTimer?.Dispose();
        _sessionTimer = null;
        SessionCountdown = SessionDuration;
    }

    private void ResetSessionTimer()
    {
        if (_sessionTimer != null)
        {
            SessionCountdown = SessionDuration;
            _sessionTimer.Change(1000, 1000);
        }
    }

    public void Dispose() => StopSessionTimer();
}
