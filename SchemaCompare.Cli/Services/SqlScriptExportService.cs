using SchemaCompare.Core.Diffs;
using SchemaCompare.Core.ScriptGeneration;
using Spectre.Console;

namespace SchemaCompare.Cli.Services;

/// <summary>
/// Handles generation and export of SQL scripts.
/// </summary>
public class SqlScriptExportService
{
    public static void GenerateAndDisplayScripts(SchemaDiff diff)
    {
        SqlScriptGenerator scriptGenerator = new();
        List<string> scripts = [.. scriptGenerator.GenerateScripts(diff)];

        if (scripts.Count == 0)
        {
            ConsoleInteractionService.DisplayMessage("[yellow]No scripts generated.[/]");
            return;
        }

        // Normalize scripts for display by escaping special characters.
        List<string> escapedScripts = [.. scripts.Select(Markup.Escape)];

        string scriptsDisplay = string.Join("\n\n", escapedScripts);
        ConsoleInteractionService.DisplayScriptsPanel(scriptsDisplay);

        if (ConsoleInteractionService.ConfirmScriptExport())
        {
            ExportScriptsToFile(string.Join("\n\n", scripts));
        }
    }

    private static void ExportScriptsToFile(string scripts)
    {
        try
        {
            string downloadsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");

            if (!Directory.Exists(downloadsFolder))
            {
                Directory.CreateDirectory(downloadsFolder);
            }

            string fileName = Path.Combine(downloadsFolder, $"{DateTime.Now:yyyyMMdd_HHmmss}.sql");

            File.WriteAllText(fileName, scripts);
            AnsiConsole.MarkupLine($"[green]✓ Scripts saved to: {fileName}[/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]✗ Error saving file: {ex.Message}[/]");
        }
    }
}