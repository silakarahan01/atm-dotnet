using ATM.ConsoleClient.Services;
using Spectre.Console;

namespace ATM.ConsoleClient.Screens;

public class WithdrawScreen
{
    private readonly ApiClient _api;

    public WithdrawScreen(ApiClient api) => _api = api;

    public async Task ShowAsync()
    {
        AnsiConsole.Clear();
        AnsiConsole.MarkupLine("[bold yellow]PARA ÇEKME[/]\n");

        var quickAmounts = new SelectionPrompt<string>()
            .Title("[cyan]Hızlı seçim veya özel tutar:[/]")
            .AddChoices("100 TL", "200 TL", "500 TL", "1.000 TL", "Özel tutar gir");

        var seçim = AnsiConsole.Prompt(quickAmounts);

        decimal amount = seçim switch
        {
            "100 TL" => 100,
            "200 TL" => 200,
            "500 TL" => 500,
            "1.000 TL" => 1000,
            _ => AnsiConsole.Prompt(
                new TextPrompt<decimal>("[cyan]Çekilecek tutar (TL):[/]")
                    .Validate(a => a > 0
                        ? ValidationResult.Success()
                        : ValidationResult.Error("Tutar sıfırdan büyük olmalı")))
        };

        var (success, error) = await _api.WithdrawAsync(amount);

        if (success)
            AnsiConsole.MarkupLine($"\n[green]✓ {amount:N2} TL başarıyla çekildi. Paranızı alınız.[/]");
        else
            AnsiConsole.MarkupLine($"\n[red]Hata: {error}[/]");

        AnsiConsole.MarkupLine("\n[grey]Ana menüye dönmek için Enter'a basın...[/]");
        Console.ReadLine();
    }
}
