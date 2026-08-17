using Dapper;
using FirebirdSql.Data.FirebirdClient;
using SchemaCompare.Core.Consts;
using SchemaCompare.Core.Interfaces;
using SchemaCompare.Core.Models;
using SchemaCompare.Core.SchemaReader;
using System.Data;

namespace SchemaCompare.Providers.Firebird.SchemaReader;

public class FirebirdSchemaReader : ISchemaReader
{
    public string ProviderName => "Firebird";

    public async Task<DatabaseSchema> ReadSchemaAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        await using FbConnection connection = new(connectionString);
        await connection.OpenAsync(cancellationToken);

        IEnumerable<RawColumnDto> rawColumns = await connection.QueryAsync<RawColumnDto>(GetTablesQuery());

        List<TableSchema> tables = BuildTables(rawColumns);

        string dbName = GetDatabaseName(connectionString);

        DatabaseSchema output = new(Path.GetFileName(dbName), tables);

        return output;
    }

    /// <summary>
    /// Returns the query used to retrieve user tables and columns from Firebird.
    /// </summary>
    private static string GetTablesQuery()
    {
        // Firebird uses RDB$ tables for system metadata. SYSTEM_FLAG = 0 ensures only user tables are returned.
        // TRIM() is required because Firebird returns names with trailing whitespace.
        return @"
            SELECT 
                TRIM(r.RDB$RELATION_NAME) AS TableName,
                TRIM(f.RDB$FIELD_NAME) AS ColumnName,
                TRIM(t.RDB$TYPE_NAME) AS DataType,
                f.RDB$FIELD_LENGTH AS MaxLength,
                CASE WHEN f.RDB$NULL_FLAG = 1 THEN 0 ELSE 1 END AS IsNullable
            FROM RDB$RELATIONS r
            JOIN RDB$RELATION_FIELDS f ON r.RDB$RELATION_NAME = f.RDB$RELATION_NAME
            JOIN RDB$FIELDS fld ON f.RDB$FIELD_SOURCE = fld.RDB$FIELD_NAME
            LEFT JOIN RDB$TYPES t ON fld.RDB$FIELD_TYPE = t.RDB$TYPE AND t.RDB$FIELD_NAME = 'RDB$FIELD_TYPE'
            WHERE COALESCE(r.RDB$SYSTEM_FLAG, 0) = 0
            ORDER BY r.RDB$RELATION_NAME, f.RDB$FIELD_POSITION";
    }

    /// <summary>
    /// Converts the raw query results into the canonical table schema model.
    /// </summary>
    private static List<TableSchema> BuildTables(IEnumerable<RawColumnDto> rawColumns)
    {
        return [.. rawColumns.
            GroupBy(x => x.TableName).
            Select(x => new TableSchema(
                Schema: string.Empty,
                Name: x.Key,
                Columns: [.. x.Select(c => new ColumnSchema(
                    Name: c.ColumnName,
                    DataType: c.DataType ?? "UNKNOWN",
                    MaxLength: c.MaxLength,
                    IsNullable: c.IsNullable))]
            ))];
    }

    /// <summary>
    /// Retrieves the database path from the Firebird connection string.
    /// </summary>
    private static string GetDatabaseName(string connectionString)
    {
        return new FbConnectionStringBuilder(connectionString).Database
            ?? throw new InvalidOperationException(Warnings.DatabaseNameNotSpecified);
    }
}