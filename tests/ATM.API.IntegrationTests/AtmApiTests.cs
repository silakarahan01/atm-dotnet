using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace ATM.API.IntegrationTests;

public class AtmApiTests(AtmApiFactory factory) : IClassFixture<AtmApiFactory>
{
    private const string Card1 = "1234567890123456";
    private const string Pin1 = "1234";
    private const string Card2 = "6543210987654321";

    private sealed record LoginResult(string Token, DateTime ExpiresAt, string CardholderName);
    private sealed record HistoryItem(int Id, string Type, decimal Amount, decimal BalanceAfter, string? Description, DateTime CreatedAt);

    private async Task<string> LoginAndGetTokenAsync(HttpClient client, string cardNumber, string pin)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new { cardNumber, pin });
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<LoginResult>();
        return body!.Token;
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOkWithToken()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new { cardNumber = Card1, pin = Pin1 });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<LoginResult>();
        body!.Token.Should().NotBeNullOrWhiteSpace();
        body.CardholderName.Should().Be("Ahmet Yılmaz");
    }

    [Fact]
    public async Task Login_WithWrongPin_ReturnsUnauthorized()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new { cardNumber = Card2, pin = "0000" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithoutToken_ReturnsUnauthorized()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/account/balance");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Withdraw_WithValidToken_SucceedsAndAppearsInHistory()
    {
        var client = factory.CreateClient();
        var token = await LoginAndGetTokenAsync(client, Card1, Pin1);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var withdraw = await client.PostAsJsonAsync("/api/transaction/withdraw", new { amount = 1000m });
        withdraw.StatusCode.Should().Be(HttpStatusCode.OK);

        var history = await client.GetFromJsonAsync<List<HistoryItem>>("/api/transaction/history?count=10");
        history.Should().NotBeNull();
        history!.Should().Contain(item => item.Type == "Withdrawal" && item.Amount == 1000m);
    }

    [Fact]
    public async Task Withdraw_WithInsufficientFunds_ReturnsBadRequestProblemDetails()
    {
        var client = factory.CreateClient();
        var token = await LoginAndGetTokenAsync(client, Card1, Pin1);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.PostAsJsonAsync("/api/transaction/withdraw", new { amount = 1_000_000m });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var payload = await response.Content.ReadAsStringAsync();
        payload.Should().Contain("Yetersiz bakiye");
    }
}
