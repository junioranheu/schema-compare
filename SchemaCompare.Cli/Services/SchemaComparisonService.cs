using SchemaCompare.Cli.Factories;
using SchemaCompare.Cli.UI;
using SchemaCompare.Core.DiffEngine;
using SchemaCompare.Core.Diffs;
using SchemaCompare.Core.Enums;
using SchemaCompare.Core.Interfaces;
using SchemaCompare.Core.Models;
using Spectre.Console;

namespace SchemaCompare.Cli.Services;

/// <summary>
/// Handles database schema comparison operations.
/// </summary>
public class SchemaComparisonService
{
    public static async Task<SchemaDiff?> CompareSchemas(
        string sourceName,
        string sourceConnectionString,
        string targetName,
        string targetConnectionString,
        ProviderTypeEnum providerType)
    {
        AnsiConsole.MarkupLine($"[grey]Comparing schemas using provider {providerType}...[/]\n");

        ISchemaReader reader = SchemaReaderFactory.Create(providerType);
        DiffEngine engine = new();

        try
        {
            DatabaseSchema sourceSchema = await AnsiConsole.Status().
                StartAsync(
                    $"[yellow]Reading database [bold]{sourceName}[/]...[/]",
                    _ => reader.ReadSchemaAsync(sourceConnectionString));

            DatabaseSchema targetSchema = await AnsiConsole.Status().
                StartAsync(
                    $"[yellow]Reading database [bold]{targetName}[/]...[/]",
                    _ => reader.ReadSchemaAsync(targetConnectionString));

            SchemaDiff diff = engine.Compare(sourceSchema, targetSchema);

            AnsiConsole.MarkupLine("");
            DiffPrinter.Print(diff);

            return diff;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]\nFatal error:[/] {ex.Message}");
            return null;
        }
    }
}