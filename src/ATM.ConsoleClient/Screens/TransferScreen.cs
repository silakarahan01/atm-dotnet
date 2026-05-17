using ATM.ConsoleClient.Services;
using Spectre.Console;

namespace ATM.ConsoleClient.Screens;

public class TransferScreen
{
    private readonly ApiClient _api;

    public TransferScreen(ApiClient api) => _api = api;

    public async Task ShowAsync()
    {
        AnsiConsole.Clear();
        AnsiConsole.MarkupLine("[bold yellow]PARA TRANSFERİ[/]\n");

        var targetAccount = AnsiConsole.Prompt(
            new TextPrompt<string>("[cyan]Hedef hesap numarası:[/]")
                .ValidationErrorMessage("[red]Hesap numarası boş olamaz[/]")
                .Validate(a => !string.IsNullOrWhiteSpace(a)
                    ? ValidationResult.Success()
                    : ValidationResult.Error()));

        var amount = AnsiConsole.Prompt(
            new TextPrompt<decimal>("[cyan]Transfer tutarı (TL):[/]")
                .Validate(a => a > 0
                    ? ValidationResult.Success()
                    : ValidationResult.Error("Tutar sıfırdan büyük olmalı")));

        AnsiConsole.MarkupLine($"\n[yellow]Onay: {targetAccount} hesabına {amount:N2} TL transfer edilecek.[/]");
        var onay = AnsiConsole.Confirm("Devam etmek istiyor musunuz?");

        if (!onay)
        {
            AnsiConsole.MarkupLine("[grey]Transfer iptal edildi.[/]");
        }
        else
        {
            var (success, error) = await _api.TransferAsync(targetAccount, amount);
            if (success)
                AnsiConsole.MarkupLine($"\n[green]✓ {amount:N2} TL başarıyla transfer edildi.[/]");
            else
                AnsiConsole.MarkupLine($"\n[red]Hata: {error}[/]");
        }

        AnsiConsole.MarkupLine("\n[grey]Ana menüye dönmek için Enter'a basın...[/]");
        Console.ReadLine();
    }
}
