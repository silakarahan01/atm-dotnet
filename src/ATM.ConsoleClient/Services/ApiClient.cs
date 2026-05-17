using System.Net.Http.Json;
using System.Net.Http.Headers;

namespace ATM.ConsoleClient.Services;

public class ApiClient
{
    private readonly HttpClient _http;
    private readonly SessionManager _session;

    public ApiClient(SessionManager session)
    {
        _session = session;
        _http = new HttpClient { BaseAddress = new Uri("http://localhost:5169") };
    }

    private void AttachToken()
    {
        _http.DefaultRequestHeaders.Authorization = _session.IsLoggedIn
            ? new AuthenticationHeaderValue("Bearer", _session.Token)
            : null;
    }

    public async Task<(bool success, string cardholderName, string token, string error)> LoginAsync(string cardNumber, string pin)
    {
        var response = await _http.PostAsJsonAsync("/api/auth/login", new { cardNumber, pin });
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
            return (true, result!.CardholderName, result.Token, string.Empty);
        }
        var err = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        return (false, string.Empty, string.Empty, err?.Message ?? "Bilinmeyen hata");
    }

    public async Task<(bool success, decimal balance, string accountNumber, string error)> GetBalanceAsync()
    {
        AttachToken();
        var response = await _http.GetAsync("/api/account/balance");
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<BalanceResponse>();
            return (true, result!.Balance, result.AccountNumber, string.Empty);
        }
        var err = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        return (false, 0, string.Empty, err?.Message ?? "Hata");
    }

    public async Task<(bool success, string error)> DepositAsync(decimal amount)
    {
        AttachToken();
        var response = await _http.PostAsJsonAsync("/api/transaction/deposit", new { amount });
        if (response.IsSuccessStatusCode) return (true, string.Empty);
        var err = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        return (false, err?.Message ?? "Hata");
    }

    public async Task<(bool success, string error)> WithdrawAsync(decimal amount)
    {
        AttachToken();
        var response = await _http.PostAsJsonAsync("/api/transaction/withdraw", new { amount });
        if (response.IsSuccessStatusCode) return (true, string.Empty);
        var err = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        return (false, err?.Message ?? "Hata");
    }

    public async Task<(bool success, string error)> TransferAsync(string targetAccountNumber, decimal amount)
    {
        AttachToken();
        var response = await _http.PostAsJsonAsync("/api/transaction/transfer", new { targetAccountNumber, amount });
        if (response.IsSuccessStatusCode) return (true, string.Empty);
        var err = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        return (false, err?.Message ?? "Hata");
    }

    public async Task<(bool success, List<TransactionItem> transactions, string error)> GetHistoryAsync(int count = 10)
    {
        AttachToken();
        var response = await _http.GetAsync($"/api/transaction/history?count={count}");
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<List<TransactionItem>>();
            return (true, result ?? [], string.Empty);
        }
        var err = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        return (false, [], err?.Message ?? "Hata");
    }

    public async Task<(bool success, string error)> ChangePinAsync(string currentPin, string newPin)
    {
        AttachToken();
        var response = await _http.PutAsJsonAsync("/api/auth/change-pin", new { currentPin, newPin });
        if (response.IsSuccessStatusCode) return (true, string.Empty);
        var err = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        return (false, err?.Message ?? "Hata");
    }

    private record LoginResponse(string Token, DateTime ExpiresAt, string CardholderName);
    private record BalanceResponse(decimal Balance, string AccountNumber, string AccountType);
    private record ErrorResponse(string Message);
    public record TransactionItem(int Id, string Type, decimal Amount, decimal BalanceAfter, string? Description, DateTime CreatedAt);
}
