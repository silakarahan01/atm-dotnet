using ATM.ConsoleClient.Services;
using Spectre.Console;

namespace ATM.ConsoleClient.Screens;

public class DepositScreen
{
    private readonly ApiClient _api;

    public DepositScreen(ApiClient api) => _api = api;

    public async Task ShowAsync()
    {
        AnsiConsole.Clear();
        AnsiConsole.MarkupLine("[bold yellow]PARA YATIRMA[/]\n");

        var amount = AnsiConsole.Prompt(
            new TextPrompt<decimal>("[cyan]Yatırılacak tutar (TL):[/]")
                .ValidationErrorMessage("[red]Geçerli bir tutar girin[/]")
                .Validate(a => a > 0
                    ? ValidationResult.Success()
                    : ValidationResult.Error("Tutar sıfırdan büyük olmalı")));

        var (success, error) = await _api.DepositAsync(amount);

        if (success)
            AnsiConsole.MarkupLine($"\n[green]✓ {amount:N2} TL başarıyla yatırıldı.[/]");
        else
            AnsiConsole.MarkupLine($"\n[red]Hata: {error}[/]");

        AnsiConsole.MarkupLine("\n[grey]Ana menüye dönmek için Enter'a basın...[/]");
        Console.ReadLine();
    }
}
