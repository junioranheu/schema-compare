using SchemaCompare.Core.Diffs;
using SchemaCompare.Core.Interfaces;
using SchemaCompare.Core.Models;

namespace SchemaCompare.Core.DiffEngine;

/// <summary>
/// Compares database schemas and identifies differences between their tables and columns.
/// </summary>
public class DiffEngine : IDiffEngine
{
    /// <summary>
    /// Compares the source and target database schemas and returns the detected differences.
    /// </summary>
    /// <param name="source">The source database schema.</param>
    /// <param name="target">The target database schema.</param>
    /// <returns>A <see cref="SchemaDiff"/> containing the differences between the schemas.</returns>
    public SchemaDiff Compare(DatabaseSchema source, DatabaseSchema target)
    {
        // Stores tables that exist in the source schema but not in the target schema.
        List<TableDiff> tablesAdded = [];

        // Stores tables that exist in the target schema but not in the source schema.
        List<TableDiff> tablesRemoved = [];

        // Stores tables that exist in both schemas but have different columns.
        List<TableModifiedDiff> tablesModified = [];

        // Creates dictionaries to allow fast table lookup by their fully qualified name.
        Dictionary<string, TableSchema> sourceTables = source.Tables.ToDictionary(x => x.FullName);
        Dictionary<string, TableSchema> targetTables = target.Tables.ToDictionary(x => x.FullName);

        // Compares each source table against the target schema.
        foreach (TableSchema sourceTable in source.Tables)
        {
            // If the table does not exist in the target, it is considered added.
            if (!targetTables.TryGetValue(sourceTable.FullName, out TableSchema? targetTable))
            {
                tablesAdded.Add(new TableDiff(sourceTable, DiffAction.Added));
                continue;
            }

            // Compares columns when the table exists in both schemas.
            TableModifiedDiff modifiedDiff = CompareTables(sourceTable, targetTable);

            // Adds the table only when at least one column difference is detected.
            if (modifiedDiff.ColumnsAdded.Count != 0 || modifiedDiff.ColumnsRemoved.Count != 0 || modifiedDiff.ColumnsModified.Count != 0)
            {
                tablesModified.Add(modifiedDiff);
            }
        }

        // Identifies tables that exist in the target schema but not in the source schema.
        foreach (TableSchema targetTable in target.Tables)
        {
            if (!sourceTables.ContainsKey(targetTable.FullName))
            {
                tablesRemoved.Add(new TableDiff(targetTable, DiffAction.Removed));
            }
        }

        // Returns all detected table and column differences.
        return new SchemaDiff(tablesAdded, tablesRemoved, tablesModified);
    }

    /// <summary>
    /// Compares the columns of two tables and identifies added, removed, and modified columns.
    /// </summary>
    /// <param name="source">The source table schema.</param>
    /// <param name="target">The target table schema.</param>
    /// <returns>A <see cref="TableModifiedDiff"/> containing the detected column differences.</returns>
    private static TableModifiedDiff CompareTables(TableSchema source, TableSchema target)
    {
        // Stores columns that exist in the source table but not in the target table.
        List<ColumnDiff> colsAdded = [];

        // Stores columns that exist in the target table but not in the source table.
        List<ColumnDiff> colsRemoved = [];

        // Stores columns that exist in both tables but have different properties.
        List<ColumnModifiedDiff> colsModified = [];

        // Creates dictionaries to allow fast column lookup by name.
        Dictionary<string, ColumnSchema> sourceCols = source.Columns.ToDictionary(x => x.Name);
        Dictionary<string, ColumnSchema> targetCols = target.Columns.ToDictionary(x => x.Name);

        // Compares each source column against the target table.
        foreach (ColumnSchema sCol in source.Columns)
        {
            // If the column does not exist in the target, it is considered added.
            if (!targetCols.TryGetValue(sCol.Name, out ColumnSchema? tCol))
            {
                colsAdded.Add(new ColumnDiff(sCol, DiffAction.Added));
                continue;
            }

            // Compares the properties of columns that exist in both tables.
            List<string> diffs = CompareColumns(sCol, tCol);

            // Adds the column only when at least one property differs.
            if (diffs.Count != 0)
            {
                colsModified.Add(new ColumnModifiedDiff(sCol, tCol, diffs));
            }
        }

        // Identifies columns that exist in the target table but not in the source table.
        foreach (ColumnSchema tCol in target.Columns)
        {
            if (!sourceCols.ContainsKey(tCol.Name))
            {
                colsRemoved.Add(new ColumnDiff(tCol, DiffAction.Removed));
            }
        }

        // Returns all detected column differences for the table.
        return new TableModifiedDiff(source, target, colsAdded, colsRemoved, colsModified);
    }

    /// <summary>
    /// Compares the properties of two columns and returns the detected differences.
    /// </summary>
    /// <param name="source">The source column schema.</param>
    /// <param name="target">The target column schema.</param>
    /// <returns>A list containing the differences between the columns.</returns>
    private static List<string> CompareColumns(ColumnSchema source, ColumnSchema target)
    {
        List<string> diffs = [];

        // Compares the column data type.
        if (!string.Equals(source.DataType, target.DataType, StringComparison.OrdinalIgnoreCase))
        {
            diffs.Add($"Tipo: [{target.DataType}] -> [{source.DataType}]");
        }

        // Compares whether the column allows null values.
        if (source.IsNullable != target.IsNullable)
        {
            diffs.Add($"Nullable: [{target.IsNullable}] -> [{source.IsNullable}]");
        }

        // Compares the maximum length defined for the column.
        if (source.MaxLength != target.MaxLength)
        {
            diffs.Add($"MaxLength: [{target.MaxLength}] -> [{source.MaxLength}]");
        }

        return diffs;
    }
}