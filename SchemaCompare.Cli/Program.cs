using SchemaCompare.Cli.Factories;
using SchemaCompare.Cli.UI;
using SchemaCompare.Core.DiffEngine;
using SchemaCompare.Core.Diffs;
using SchemaCompare.Core.Enums;
using SchemaCompare.Core.Interfaces;
using SchemaCompare.Core.Models;
using Spectre.Console;
using System.CommandLine;
using System.CommandLine.Help;

// 1. Initial option configuration.
RootCommand rootCommand = new("SchemaCompare — Database Schema Comparison Tool");

Option<string> sourceOption = new(name: "--source")
{
    Required = true,
    Description = "Source database connection string (e.g. Dev)"
};

Option<string> targetOption = new(name: "--target")
{
    Required = true,
    Description = "Target database connection string (e.g. QA/Prod)"
};

Option<ProviderTypeEnum> providerOption = new(name: "--provider", aliases: ["-p"])
{
    Description = "Database provider (Default: PostgreSql)",
    DefaultValueFactory = _ => ProviderTypeEnum.PostgreSql
};

// 2. Command configuration.
Command compareCommand = new(name: "compare", description: "Compares two databases and displays the differences")
{
    sourceOption,
    targetOption,
    providerOption
};

compareCommand.SetAction(async (parseResult, cancellationToken) =>
{
    string source = parseResult.GetValue(sourceOption)!;
    string target = parseResult.GetValue(targetOption)!;
    ProviderTypeEnum provider = parseResult.GetValue(providerOption);

    await RunComparison(source, target, provider);
});

rootCommand.Subcommands.Add(compareCommand);

rootCommand.Options.Add(new HelpOption());
compareCommand.Options.Add(new HelpOption());

return await rootCommand.Parse(args).InvokeAsync();

static async Task RunComparison(string sourceConn, string targetConn, ProviderTypeEnum providerType)
{
    AnsiConsole.Write(new FigletText("SchemaCompare").LeftJustified().Color(Color.Blue));
    
    ISchemaReader reader = SchemaReaderFactory.Create(providerType);

    AnsiConsole.MarkupLine($"[grey]Analyzing schemas using provider {reader.ProviderName}...[/]\n");

    DiffEngine engine = new();

    try
    {
        DatabaseSchema sourceSchema = await AnsiConsole.Status().StartAsync("Reading source database...", _ => reader.ReadSchemaAsync(sourceConn));

        DatabaseSchema targetSchema = await AnsiConsole.Status().StartAsync("Reading target database...", _ => reader.ReadSchemaAsync(targetConn));

        SchemaDiff diff = engine.Compare(sourceSchema, targetSchema);

        DiffPrinter.Print(diff);
    }
    catch (Exception ex)
    {
        AnsiConsole.MarkupLine($"\n[red]Fatal error:[/] {ex.Message}");
    }
}