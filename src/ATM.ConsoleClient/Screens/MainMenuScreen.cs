using ATM.ConsoleClient.Services;
using Spectre.Console;

namespace ATM.ConsoleClient.Screens;

public class MainMenuScreen
{
    private readonly ApiClient _api;
    private readonly SessionManager _session;

    public MainMenuScreen(ApiClient api, SessionManager session)
    {
        _api = api;
        _session = session;
    }

    public async Task RunAsync()
    {
        while (_session.IsLoggedIn)
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new FigletText("ATM").Color(Color.Yellow));
            AnsiConsole.MarkupLine($"[bold green]Hoş geldiniz, {_session.CardholderName}[/]\n");

            var seçim = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[cyan]Ne yapmak istersiniz?[/]")
                    .AddChoices(
                        "💰 Bakiye Sorgula",
                        "⬆  Para Yatır",
                        "⬇  Para Çek",
                        "↔  Transfer",
                        "📋 İşlem Geçmişi",
                        "🔑 PIN Değiştir",
                        "🚪 Çıkış"
                    ));

            await (seçim switch
            {
                "💰 Bakiye Sorgula" => new BalanceScreen(_api).ShowAsync(),
                "⬆  Para Yatır" => new DepositScreen(_api).ShowAsync(),
                "⬇  Para Çek" => new WithdrawScreen(_api).ShowAsync(),
                "↔  Transfer" => new TransferScreen(_api).ShowAsync(),
                "📋 İşlem Geçmişi" => new HistoryScreen(_api).ShowAsync(),
                "🔑 PIN Değiştir" => new ChangePinScreen(_api).ShowAsync(),
                _ => Task.CompletedTask
            });

            if (seçim == "🚪 Çıkış")
            {
                _session.Clear();
                AnsiConsole.MarkupLine("[yellow]Güvenli çıkış yapıldı. Kartınızı almayı unutmayın.[/]");
                await Task.Delay(1500);
            }
        }
    }
}
