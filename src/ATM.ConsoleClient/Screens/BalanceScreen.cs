using ATM.ConsoleClient.Services;
using Spectre.Console;

namespace ATM.ConsoleClient.Screens;

public class BalanceScreen
{
    private readonly ApiClient _api;

    public BalanceScreen(ApiClient api) => _api = api;

    public async Task ShowAsync()
    {
        AnsiConsole.Clear();
        AnsiConsole.MarkupLine("[bold yellow]BAKİYE SORGULAMA[/]\n");

        var (success, balance, accountNumber, error) = await _api.GetBalanceAsync();

        if (success)
        {
            var panel = new Panel(
                $"[bold]Hesap No:[/] {accountNumber}\n[bold]Bakiye:[/]   [green]{balance:N2} TL[/]"
            ).Header("Hesap Bilgileri").BorderColor(Color.Yellow);

            AnsiConsole.Write(panel);
        }
        else
        {
            AnsiConsole.MarkupLine($"[red]Hata: {error}[/]");
        }

        AnsiConsole.MarkupLine("\n[grey]Ana menüye dönmek için Enter'a basın...[/]");
        Console.ReadLine();
    }
}
