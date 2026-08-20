using SchemaCompare.Cli.Services;
using SchemaCompare.Core.Diffs;
using SchemaCompare.Core.Enums;

// Display header.
ConsoleInteractionService.DisplayHeader();

// Select provider.
ProviderTypeEnum providerSelection = ConsoleInteractionService.SelectDatabaseProvider();

// Get database info
DatabaseInfo sourceInfo = ConsoleInteractionService.GetDatabaseInfo("Source");
DatabaseInfo targetInfo = ConsoleInteractionService.GetDatabaseInfo("Target");

// Confirm and run comparison.
if (!ConsoleInteractionService.ConfirmComparison())
{
    return;
}

SchemaDiff? diff = await SchemaComparisonService.CompareSchemas(
     sourceName: sourceInfo.Name,
     sourceConnectionString: sourceInfo.ConnectionString,
     targetName: targetInfo.Name,
     targetConnectionString: targetInfo.ConnectionString,
     providerType: providerSelection);

// If comparison succeeded and there are differences, offer to generate scripts.
if (diff is not null && diff.HasDifferences)
{
    if (ConsoleInteractionService.ConfirmScriptGeneration())
    {
        SqlScriptExportService.GenerateAndDisplayScripts(diff);
    }
}