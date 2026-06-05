using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.PostgreSql;

namespace ATM.API.IntegrationTests;

/// <summary>
/// API'yi bellek içi test sunucusunda, Testcontainers ile ayağa kaldırılan gerçek bir
/// PostgreSQL örneğine bağlı olarak çalıştırır. Uygulama açılışta migrate + seed eder.
/// </summary>
public sealed class AtmApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16")
        .WithDatabase("atm")
        .WithUsername("atm")
        .WithPassword("atm")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:DefaultConnection", _postgres.GetConnectionString());
    }

    async Task IAsyncLifetime.InitializeAsync() => await _postgres.StartAsync();

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _postgres.DisposeAsync();
        await base.DisposeAsync();
    }
}
