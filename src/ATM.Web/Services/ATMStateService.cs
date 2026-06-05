using ATM.Application.Features.Account.GetBalance;
using ATM.Application.Features.Auth.GetCardByNumber;
using ATM.Application.Features.Auth.Login;
using ATM.Application.Features.Transaction.Deposit;
using ATM.Application.Features.Transaction.GetHistory;
using ATM.Application.Features.Transaction.Transfer;
using ATM.Application.Features.Transaction.Withdraw;
using ATM.Domain.Common;
using ATM.Domain.Errors;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ATM.Web.Services;

public enum ATMScreen
{
    Welcome, Pin, Menu, Balance, Withdraw, Deposit, Transfer, History, CardBlocked
}

/// <summary>
/// Blazor ATM simülasyonunun ekran/animasyon/oturum durumunu yönetir.
/// İş mantığı içermez; tüm işlemleri API ile aynı MediatR komut/sorgularına devreder.
/// </summary>
public class ATMStateService(IServiceScopeFactory scopeFactory) : IDisposable
{
    private System.Threading.Timer? _sessionTimer;
    private const int SessionDuration = 60;

    private string _cardNumber = "";

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
    public IReadOnlyList<TransactionResponse> Transactions { get; private set; } = [];

    public event Action? OnChange;

    private void Notify() => OnChange?.Invoke();

    private void ClearMessages()
    {
        ErrorMessage = null;
        SuccessMessage = null;
        HasError = false;
    }

    private async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request)
    {
        using var scope = scopeFactory.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        return await sender.Send(request);
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

        var result = await SendAsync(new GetCardByNumberQuery(cardNumber));

        if (result.IsFailure)
        {
            IsCardAnimating = false;
            IsLoading = false;

            if (result.Error.Code == CardErrors.Blocked.Code)
            {
                NavigateTo(ATMScreen.CardBlocked);
                return;
            }

            HasError = true;
            ErrorMessage = "Kart tanınmadı.";
            Notify();
            return;
        }

        var card = result.Value;
        _cardNumber = cardNumber;
        CardId = card.CardId;
        AccountId = card.AccountId;
        AccountNumber = card.AccountNumber;
        CardholderName = card.CardholderName;
        Balance = card.Balance;
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

        var result = await SendAsync(new LoginCommand(_cardNumber, pin));

        if (result.IsSuccess)
        {
            PinAttempts = 0;
            IsLoading = false;
            StartSessionTimer();
            NavigateTo(ATMScreen.Menu);
            return;
        }

        // Kart bu denemeyle bloke olduysa bloke ekranına geç
        if (result.Error.Code == CardErrors.JustBlocked.Code || result.Error.Code == CardErrors.Blocked.Code)
        {
            IsLoading = false;
            Notify();
            await Task.Delay(1500);
            NavigateTo(ATMScreen.CardBlocked);
            return;
        }

        PinAttempts++;
        HasError = true;
        ErrorMessage = result.Error.Message;
        IsLoading = false;
        Notify();
    }

    public async Task RefreshBalanceAsync()
    {
        var result = await SendAsync(new GetBalanceQuery(AccountId));
        if (result.IsSuccess)
            Balance = result.Value.Balance;
        Notify();
    }

    public async Task WithdrawAsync(decimal amount)
    {
        IsLoading = true;
        ClearMessages();
        Notify();

        await Task.Delay(600);

        var result = await SendAsync(new WithdrawCommand(AccountId, amount));

        if (result.IsFailure)
        {
            HasError = true;
            ErrorMessage = result.Error.Message;
            IsLoading = false;
            Notify();
            return;
        }

        Balance -= amount;
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

        var result = await SendAsync(new DepositCommand(AccountId, amount));

        if (result.IsFailure)
        {
            HasError = true;
            ErrorMessage = result.Error.Message;
            IsLoading = false;
            Notify();
            return;
        }

        Balance += amount;
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

        await Task.Delay(800);

        var result = await SendAsync(new TransferCommand(AccountId, targetAccountNumber, amount));

        if (result.IsFailure)
        {
            HasError = true;
            ErrorMessage = result.Error.Message;
            IsLoading = false;
            Notify();
            return;
        }

        Balance -= amount;
        IsLoading = false;
        SuccessMessage = $"{amount:N2} ₺ {targetAccountNumber} hesabına transfer edildi.";
        Notify();

        await Task.Delay(2500);
        ClearMessages();
        NavigateTo(ATMScreen.Menu);
    }

    public async Task LoadHistoryAsync()
    {
        var result = await SendAsync(new GetHistoryQuery(AccountId, 10));
        Transactions = result.IsSuccess ? result.Value : [];
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
        _cardNumber = "";
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
