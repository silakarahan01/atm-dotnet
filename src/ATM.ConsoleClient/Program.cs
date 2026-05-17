using ATM.ConsoleClient.Screens;
using ATM.ConsoleClient.Services;
using Spectre.Console;

var session = new SessionManager();
var api = new ApiClient(session);
var welcomeScreen = new WelcomeScreen(api, session);
var mainMenu = new MainMenuScreen(api, session);

while (true)
{
    try
    {
        await welcomeScreen.ShowAsync();

        if (session.IsLoggedIn)
            await mainMenu.RunAsync();
    }
    catch (HttpRequestException)
    {
        AnsiConsole.Clear();
        AnsiConsole.MarkupLine("[red]API sunucusuna bağlanılamıyor.[/]");
        AnsiConsole.MarkupLine("[grey]Lütfen ATM.API projesinin çalıştığından emin olun.[/]");
        AnsiConsole.MarkupLine("[grey]Devam etmek için Enter'a basın...[/]");
        Console.ReadLine();
    }
    catch (Exception ex)
    {
        AnsiConsole.MarkupLine($"[red]Beklenmeyen hata: {ex.Message}[/]");
        Console.ReadLine();
    }
}
