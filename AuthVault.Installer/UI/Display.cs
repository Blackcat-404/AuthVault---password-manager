using Spectre.Console;

namespace AuthVault.Installer.UI;

public static class Display
{
    public static void Banner()
    {
        AnsiConsole.Write(new FigletText("AuthVault").Color(Color.SteelBlue1));
        AnsiConsole.MarkupLine("[steelblue1]Self-hosted Password Manager — Installer[/]");
        AnsiConsole.WriteLine();
    }

    public static void Section(string title)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule($"[bold steelblue1]{title}[/]").RuleStyle("grey").LeftJustified());
        AnsiConsole.WriteLine();
    }

    public static void Step(string message)
    {
        AnsiConsole.MarkupLine($"  [grey]>[/] {message}");
    }

    public static void Success(string message)
    {
        AnsiConsole.MarkupLine($"  [green]✓[/] {message}");
    }

    public static void Error(string message)
    {
        AnsiConsole.MarkupLine($"  [red]✗[/] {message}");
    }

    public static void Warning(string message)
    {
        AnsiConsole.MarkupLine($"  [yellow]![/] {message}");
    }

    public static void Info(string message)
    {
        AnsiConsole.MarkupLine($"  [grey]{message}[/]");
    }

    public static bool Confirm(string question)
    {
        return AnsiConsole.Confirm($"  [yellow]?[/] {question}", defaultValue: false);
    }

    public static string Ask(string question, string defaultValue = "")
    {
        return AnsiConsole.Ask<string>($"  [steelblue1]?[/] {question}", defaultValue);
    }

    public static string AskSecret(string question)
    {
        return AnsiConsole.Prompt(
            new TextPrompt<string>($"  [steelblue1]?[/] {question}")
                .Secret());
    }

    public static int AskInt(string question, int defaultValue)
    {
        return AnsiConsole.Ask($"  [steelblue1]?[/] {question}", defaultValue);
    }

    public static void Link(string url)
    {
        AnsiConsole.MarkupLine($"  [steelblue1][link={url}]{url}[/][/]");
    }

    public static string Select(string question, IEnumerable<string> choices)
    {
        return AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title($"  [steelblue1]?[/] {question}")
                .HighlightStyle(Style.Parse("steelblue1 bold"))
                .AddChoices(choices));
    }

    public static async Task SpinnerAsync(string message, Func<Task> action)
    {
        await AnsiConsole.Status()
            .SpinnerStyle(Style.Parse("steelblue1"))
            .StartAsync(message, async _ => await action());
    }

    public static async Task<T> SpinnerAsync<T>(string message, Func<Task<T>> action)
    {
        return await AnsiConsole.Status()
            .SpinnerStyle(Style.Parse("steelblue1"))
            .StartAsync(message, async _ => await action());
    }

    public static void Help()
    {
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Panel(
            "[bold]authvault install[/]   — Full installation (first-time setup)\n" +
            "[bold]authvault start[/]     — Start AuthVault\n" +
            "[bold]authvault stop[/]      — Stop AuthVault\n" +
            "[bold]authvault status[/]    — Show running status\n" +
            "[bold]authvault update[/]    — Apply updates and restart\n" +
            "[bold]authvault uninstall[/] — Remove AuthVault and certificates")
            .Header("[steelblue1]Commands[/]")
            .BorderColor(Color.Grey));
    }
}
