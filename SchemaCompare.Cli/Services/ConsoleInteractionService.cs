using SchemaCompare.Core.Consts;
using SchemaCompare.Core.Enums;
using SchemaCompare.Core.Extensions;
using SchemaCompare.Core.Models;
using Spectre.Console;

namespace SchemaCompare.Cli.Services;

/// <summary>
/// Handles all console interactions with the user.
/// </summary>
public static class ConsoleInteractionService
{
    public static void DisplayHeader()
    {
        const string appTitle = "Database Schema Comparison Tool";
        Console.Title = appTitle;

        AnsiConsole.Write(new FigletText("SchemaCompare").LeftJustified().Color(Color.Blue));
        AnsiConsole.MarkupLine($"[grey]{appTitle}[/]");
        AnsiConsole.MarkupLine("[dim]made by @junioranheu[/]\n");
    }

    public static ProviderTypeEnum SelectDatabaseProvider()
    {
        ProviderInfo selectedProvider = AnsiConsole.Prompt(
            new SelectionPrompt<ProviderInfo>().
                Title("[bold]Which database are you using?[/]").
                PageSize(10).
                AddChoices(ProviderCatalog.AllProviders).
                UseConverter(x =>
                    $"{x.DisplayName} ({x.TestingStatus.GetDescription().ToLowerInvariant()}" +
                    $"{(x.RealTestingDate.HasValue ? $" on {x.RealTestingDate:yyyy-MM-dd}" : "NOT TESTED IN A REAL ENVIRONMENT!")})"));

        return selectedProvider.ProviderType;
    }

    public static DatabaseInfo GetDatabaseInfo(string role)
    {
        AnsiConsole.MarkupLine("");

        string name = AnsiConsole.Ask<string>(
            prompt: $"[bold]{role} database name[/] (e.g. development):");

        string connectionString = AnsiConsole.Prompt(
            new TextPrompt<string>(prompt: $"[bold]Connection string for {name}[/]:").
                PromptStyle("green"));

        return new DatabaseInfo(name, connectionString);
    }

    public static bool ConfirmComparison()
    {
        AnsiConsole.MarkupLine("");

        return AnsiConsole.Confirm(
            prompt: "[yellow]Do you want to proceed with the comparison?[/]",
            defaultValue: true);
    }

    public static bool ConfirmScriptGeneration()
    {
        AnsiConsole.MarkupLine("");

        return AnsiConsole.Confirm(
            "[yellow]Do you want to generate SQL scripts to synchronize the databases?[/]",
            defaultValue: true);
    }

    public static bool ConfirmScriptExport()
    {
        AnsiConsole.MarkupLine("");

        return AnsiConsole.Confirm(
            "[yellow]Do you want to save these scripts to a file?[/]",
            defaultValue: true);
    }

    public static void DisplayMessage(string markup)
    {
        AnsiConsole.MarkupLine(markup);
    }

    public static void DisplayScriptsPanel(string scripts)
    {
        AnsiConsole.MarkupLine("[bold cyan]Generated SQL Scripts:[/]\n");

        Panel panel = new(scripts)
        {
            Border = BoxBorder.Rounded,
            Padding = new Padding(1, 1)
        };

        AnsiConsole.Write(panel);
    }
}

public record DatabaseInfo(string Name, string ConnectionString);