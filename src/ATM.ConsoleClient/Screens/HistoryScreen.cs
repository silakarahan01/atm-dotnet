using ATM.ConsoleClient.Services;
using Spectre.Console;

namespace ATM.ConsoleClient.Screens;

public class HistoryScreen
{
    private readonly ApiClient _api;

    public HistoryScreen(ApiClient api) => _api = api;

    public async Task ShowAsync()
    {
        AnsiConsole.Clear();
        AnsiConsole.MarkupLine("[bold yellow]İŞLEM GEÇMİŞİ[/]\n");

        var (success, transactions, error) = await _api.GetHistoryAsync(10);

        if (!success)
        {
            AnsiConsole.MarkupLine($"[red]Hata: {error}[/]");
        }
        else if (transactions.Count == 0)
        {
            AnsiConsole.MarkupLine("[grey]Henüz işlem bulunmuyor.[/]");
        }
        else
        {
            var table = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Yellow)
                .AddColumn("Tarih")
                .AddColumn("İşlem")
                .AddColumn("Tutar")
                .AddColumn("Sonraki Bakiye");

            foreach (var t in transactions)
            {
                var color = t.Type switch
                {
                    "Deposit" => "green",
                    "Withdrawal" => "red",
                    _ => "blue"
                };
                var typeLabel = t.Type switch
                {
                    "Deposit" => "Para Yatırma",
                    "Withdrawal" => "Para Çekme",
                    "Transfer" => "Transfer",
                    _ => t.Type
                };

                table.AddRow(
                    t.CreatedAt.ToLocalTime().ToString("dd.MM.yyyy HH:mm"),
                    $"[{color}]{typeLabel}[/]",
                    $"[{color}]{t.Amount:N2} TL[/]",
                    $"{t.BalanceAfter:N2} TL"
                );
            }

            AnsiConsole.Write(table);
        }

        AnsiConsole.MarkupLine("\n[grey]Ana menüye dönmek için Enter'a basın...[/]");
        Console.ReadLine();
    }
}
