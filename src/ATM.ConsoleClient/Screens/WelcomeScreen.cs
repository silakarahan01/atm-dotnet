using ATM.ConsoleClient.Services;
using Spectre.Console;

namespace ATM.ConsoleClient.Screens;

public class WelcomeScreen
{
    private readonly ApiClient _api;
    private readonly SessionManager _session;

    public WelcomeScreen(ApiClient api, SessionManager session)
    {
        _api = api;
        _session = session;
    }

    public async Task ShowAsync()
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new FigletText("ATM").Color(Color.Yellow));
        AnsiConsole.MarkupLine("[grey]Hoş Geldiniz[/]\n");

        var cardNumber = AnsiConsole.Prompt(
            new TextPrompt<string>("[cyan]Kart Numarası:[/]")
                .ValidationErrorMessage("[red]16 haneli kart numarası girin[/]")
                .Validate(n => n.Length == 16 && n.All(char.IsDigit)
                    ? ValidationResult.Success()
                    : ValidationResult.Error()));

        var pin = AnsiConsole.Prompt(
            new TextPrompt<string>("[cyan]PIN:[/]")
                .Secret()
                .ValidationErrorMessage("[red]4 haneli PIN girin[/]")
                .Validate(p => p.Length == 4 && p.All(char.IsDigit)
                    ? ValidationResult.Success()
                    : ValidationResult.Error()));

        await AnsiConsole.Status().StartAsync("Giriş yapılıyor...", async ctx =>
        {
            var (success, name, token, error) = await _api.LoginAsync(cardNumber, pin);
            if (success)
            {
                _session.SetSession(token, name);
            }
            else
            {
                AnsiConsole.MarkupLine($"\n[red]Hata: {error}[/]");
                AnsiConsole.MarkupLine("[grey]Devam etmek için Enter'a basın...[/]");
                Console.ReadLine();
            }
        });
    }
}
