using SchemaCompare.Core.Diffs;
using Spectre.Console;

namespace SchemaCompare.Cli.UI;

public static class DiffPrinter
{
    /// <summary>
    /// Prints the differences between the source and target database schemas.
    /// </summary>
    public static void Print(SchemaDiff diff)
    {
        if (!diff.HasDifferences)
        {
            AnsiConsole.MarkupLine("[green]No differences found. The databases are synchronized![/]");
            return;
        }

        Tree rootTree = new("[yellow]Detected differences (Target vs Source)[/]");

        foreach (TableDiff t in diff.TablesRemoved)
        {
            rootTree.AddNode($"[red]- Table {t.Table.FullName} (Removed)[/]");
        }

        foreach (TableDiff t in diff.TablesAdded)
        {
            rootTree.AddNode($"[green]+ Table {t.Table.FullName} (Added)[/]");
        }

        foreach (TableModifiedDiff tm in diff.TablesModified)
        {
            TreeNode tableNode = rootTree.AddNode($"[yellow]~ Table {tm.SourceTable.FullName} (Modified)[/]");

            foreach (ColumnDiff c in tm.ColumnsRemoved)
            {
                tableNode.AddNode($"[red]- Column {c.Column.Name}[/]");
            }

            foreach (ColumnDiff c in tm.ColumnsAdded)
            {
                tableNode.AddNode($"[green]+ Column {c.Column.Name} ({c.Column.DataType})[/]");
            }

            foreach (ColumnModifiedDiff cm in tm.ColumnsModified)
            {
                TreeNode colNode = tableNode.AddNode($"[yellow]~ Column {cm.SourceColumn.Name}[/]");

                foreach (string detail in cm.Differences)
                {
                    colNode.AddNode($"[grey]Info:[/] {Markup.Escape(detail)}");
                }
            }
        }

        AnsiConsole.Write(rootTree);
        AnsiConsole.WriteLine();
    }
}