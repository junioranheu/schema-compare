using SchemaCompare.Core.Diffs;
using SchemaCompare.Core.Models;

namespace SchemaCompare.Core.ScriptGeneration;

/// <summary>
/// Generic SQL script generator that works with most SQL dialects.
/// </summary>
public class SqlScriptGenerator : IScriptGenerator
{
    public virtual IEnumerable<string> GenerateScripts(SchemaDiff diff)
    {
        List<string> scripts =
        [
            // 1. Drop tables that were removed (if any)
            .. GenerateDropTableScripts(diff.TablesRemoved),

            // 2. Create new tables that were added
            .. GenerateCreateTableScripts(diff.TablesAdded),

            // 3. Alter tables that were modified
            .. GenerateAlterTableScripts(diff.TablesModified)
        ];

        return scripts;
    }

    protected virtual IEnumerable<string> GenerateDropTableScripts(IReadOnlyCollection<TableDiff> tables)
    {
        List<string> scripts = [];

        foreach (TableDiff table in tables)
        {
            // Note: DROP TABLE is commented or prefixed with warning to prevent accidental execution.
            scripts.Add($"-- WARNING: This table exists in source but not in target");
            scripts.Add($"-- DROP TABLE IF EXISTS {table.Table.FullName};");
            scripts.Add("");
        }

        return scripts;
    }

    protected virtual IEnumerable<string> GenerateCreateTableScripts(IReadOnlyCollection<TableDiff> tables)
    {
        List<string> scripts = [];

        foreach (TableDiff table in tables)
        {
            scripts.Add(GenerateCreateTableStatement(table.Table));
            scripts.Add("");
        }

        return scripts;
    }

    protected virtual IEnumerable<string> GenerateAlterTableScripts(IReadOnlyCollection<TableModifiedDiff> tableModifications)
    {
        List<string> scripts = [];

        foreach (TableModifiedDiff modification in tableModifications)
        {
            TableSchema table = modification.TargetTable;

            // Add columns.
            foreach (ColumnDiff column in modification.ColumnsAdded)
            {
                scripts.Add(GenerateAddColumnStatement(table, column.Column));
            }

            // Remove columns.
            foreach (ColumnDiff column in modification.ColumnsRemoved)
            {
                scripts.Add(GenerateDropColumnStatement(table, column.Column));
            }

            // Modify columns.
            foreach (ColumnModifiedDiff column in modification.ColumnsModified)
            {
                scripts.Add(GenerateModifyColumnStatement(table, column.TargetColumn, column.Differences));
            }

            if (modification.ColumnsAdded.Count > 0 || modification.ColumnsRemoved.Count > 0 || modification.ColumnsModified.Count > 0)
            {
                scripts.Add("");
            }
        }

        return scripts;
    }

    protected virtual string GenerateCreateTableStatement(TableSchema table)
    {
        string columnDefinitions = string.Join(",\n    ", table.Columns.Select(GenerateColumnDefinition));
        string output = $@"CREATE TABLE {table.FullName} (
    {columnDefinitions}
);";

        return output;
    }

    protected virtual string GenerateColumnDefinition(ColumnSchema column)
    {
        string definition = $"{column.Name} {column.DataType}";

        if (!column.IsNullable)
        {
            definition += " NOT NULL";
        }

        return definition;
    }

    protected virtual string GenerateAddColumnStatement(TableSchema table, ColumnSchema column)
    {
        string definition = GenerateColumnDefinition(column);
        string output = $"ALTER TABLE {table.FullName} ADD COLUMN {definition};";

        return output;
    }

    protected virtual string GenerateDropColumnStatement(TableSchema table, ColumnSchema column)
    {
        string output = $"ALTER TABLE {table.FullName} DROP COLUMN {column.Name};";

        return output;
    }

    protected virtual string GenerateModifyColumnStatement(TableSchema table, ColumnSchema column, IEnumerable<string> differences)
    {
        string definition = GenerateColumnDefinition(column);
        string diffComment = string.Join(", ", differences);

        return $"-- Modify: {diffComment}\n" +
               $"ALTER TABLE {table.FullName} MODIFY COLUMN {definition};";
    }
}