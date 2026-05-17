using ATM.ConsoleClient.Services;
using Spectre.Console;

namespace ATM.ConsoleClient.Screens;

public class ChangePinScreen
{
    private readonly ApiClient _api;

    public ChangePinScreen(ApiClient api) => _api = api;

    public async Task ShowAsync()
    {
        AnsiConsole.Clear();
        AnsiConsole.MarkupLine("[bold yellow]PIN DEĞİŞTİRME[/]\n");

        var currentPin = AnsiConsole.Prompt(
            new TextPrompt<string>("[cyan]Mevcut PIN:[/]")
                .Secret()
                .Validate(p => p.Length == 4 && p.All(char.IsDigit)
                    ? ValidationResult.Success()
                    : ValidationResult.Error("4 haneli PIN girin")));

        var newPin = AnsiConsole.Prompt(
            new TextPrompt<string>("[cyan]Yeni PIN:[/]")
                .Secret()
                .Validate(p => p.Length == 4 && p.All(char.IsDigit)
                    ? ValidationResult.Success()
                    : ValidationResult.Error("4 haneli PIN girin")));

        var newPinConfirm = AnsiConsole.Prompt(
            new TextPrompt<string>("[cyan]Yeni PIN (tekrar):[/]")
                .Secret()
                .Validate(p => p == newPin
                    ? ValidationResult.Success()
                    : ValidationResult.Error("PIN'ler eşleşmiyor")));

        var (success, error) = await _api.ChangePinAsync(currentPin, newPin);

        if (success)
            AnsiConsole.MarkupLine("\n[green]✓ PIN başarıyla güncellendi.[/]");
        else
            AnsiConsole.MarkupLine($"\n[red]Hata: {error}[/]");

        AnsiConsole.MarkupLine("\n[grey]Ana menüye dönmek için Enter'a basın...[/]");
        Console.ReadLine();
    }
}
