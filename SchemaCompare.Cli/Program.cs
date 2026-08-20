using SchemaCompare.Cli.Factories;
using SchemaCompare.Cli.UI;
using SchemaCompare.Core.DiffEngine;
using SchemaCompare.Core.Diffs;
using SchemaCompare.Core.Enums;
using SchemaCompare.Core.Interfaces;
using SchemaCompare.Core.Models;
using SchemaCompare.Core.ScriptGeneration;
using Spectre.Console;

const string appTitle = "Database Schema Comparison Tool";
Console.Title = appTitle;

AnsiConsole.Write(new FigletText("SchemaCompare").LeftJustified().Color(Color.Blue));
AnsiConsole.MarkupLine($"[grey]{appTitle}[/]");
AnsiConsole.MarkupLine("[dim]made by @junioranheu[/]\n");

// Select provider.
ProviderTypeEnum[] providers = Enum.GetValues<ProviderTypeEnum>();

ProviderTypeEnum providerSelection = AnsiConsole.
    Prompt(new SelectionPrompt<ProviderTypeEnum>().
        Title("[bold]Which database are you using?[/]").
        PageSize(10).
        AddChoices(providers));

AnsiConsole.MarkupLine("");

// Get source database info.
string sourceName = AnsiConsole.Ask<string>(prompt: "[bold]Source database name[/] (e.g. development):");

string sourceConn = AnsiConsole.
    Prompt(new TextPrompt<string>(prompt: $"[bold]Connection string for {sourceName}[/]:").
        PromptStyle("green"));

AnsiConsole.MarkupLine("");

// Get target database info.
string targetName = AnsiConsole.Ask<string>(prompt: "[bold]Target database name[/] (e.g. production):");

string targetConn = AnsiConsole.Prompt(new TextPrompt<string>($"[bold]Connection string for {targetName}[/]:").
    PromptStyle("green"));

AnsiConsole.MarkupLine("");

// Confirm and run.
if (AnsiConsole.Confirm(prompt: "[yellow]Do you want to proceed with the comparison??[/]", defaultValue: true))
{
    AnsiConsole.MarkupLine("");
    await RunComparison(sourceName, sourceConn, targetName, targetConn, providerSelection);
}

static async Task RunComparison(string sourceName, string sourceConn, string targetName, string targetConn, ProviderTypeEnum providerType)
{
    AnsiConsole.MarkupLine($"[grey]Comparing schemas using provider {providerType}...[/]\n");

    ISchemaReader reader = SchemaReaderFactory.Create(providerType);

    DiffEngine engine = new();

    try
    {
        DatabaseSchema sourceSchema = await AnsiConsole.Status().StartAsync($"[yellow]Reading database [bold]{sourceName}[/]...[/]", _ => reader.ReadSchemaAsync(sourceConn));

        DatabaseSchema targetSchema = await AnsiConsole.Status().StartAsync($"[yellow]Reading database [bold]{targetName}[/]...[/]", _ => reader.ReadSchemaAsync(targetConn));

        SchemaDiff diff = engine.Compare(sourceSchema, targetSchema);

        AnsiConsole.MarkupLine("");
        DiffPrinter.Print(diff);

        // If there are differences, offer to generate SQL scripts.
        if (diff.HasDifferences)
        {
            AnsiConsole.MarkupLine("");
            if (AnsiConsole.Confirm("[yellow]Do you want to generate SQL scripts to synchronize the databases?[/]", defaultValue: true))
            {
                AnsiConsole.MarkupLine("");
                GenerateAndDisplayScripts(diff);
            }
        }
    }
    catch (Exception ex)
    {
        AnsiConsole.MarkupLine($"[red]\nFatal error:[/] {ex.Message}");
    }
}

static void GenerateAndDisplayScripts(SchemaDiff diff)
{
    IScriptGenerator generator = new SqlScriptGenerator();
    List<string> scripts = [.. generator.GenerateScripts(diff)];

    if (scripts.Count == 0)
    {
        AnsiConsole.MarkupLine("[yellow]No scripts generated.[/]");
        return;
    }

    // Display scripts in console.
    AnsiConsole.MarkupLine("[bold cyan]Generated SQL Scripts:[/]\n");

    // Normalize scripts for display by escaping special characters.
    for (int i = 0; i < scripts.Count; i++)
    {
        scripts[i] = Markup.Escape(scripts[i]);
    }

    Panel panel = new(string.Join("\n\n", scripts))
    {
        Border = BoxBorder.Rounded,
        Padding = new Padding(1, 1)
    };

    AnsiConsole.Write(panel);

    // Offer to export to file.
    AnsiConsole.MarkupLine("");

    if (AnsiConsole.Confirm("[yellow]Do you want to save these scripts to a file?[/]", defaultValue: true))
    {
        try
        {
            string downloadsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");

            if (!Directory.Exists(downloadsFolder))
            {
                Directory.CreateDirectory(downloadsFolder);
            }

            string fileName = Path.Combine(downloadsFolder, $"{DateTime.Now:yyyyMMdd_HHmmss}.sql");
            File.WriteAllText(fileName, string.Join("\n\n", scripts));
            AnsiConsole.MarkupLine($"[green]✓ Scripts saved to: {fileName}[/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]✗ Error saving file: {ex.Message}[/]");
        }
    }
}