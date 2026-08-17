using Dapper;
using Oracle.ManagedDataAccess.Client;
using SchemaCompare.Core.Consts;
using SchemaCompare.Core.Interfaces;
using SchemaCompare.Core.Models;
using SchemaCompare.Core.SchemaReader;
using System.Data;

namespace SchemaCompare.Providers.Oracle.SchemaReader;

public class OracleSchemaReader : ISchemaReader
{
    public string ProviderName => "Oracle";

    public async Task<DatabaseSchema> ReadSchemaAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        await using OracleConnection connection = new(connectionString);
        await connection.OpenAsync(cancellationToken);

        IEnumerable<RawColumnDto> rawColumns = await connection.QueryAsync<RawColumnDto>(GetTablesQuery());

        List<TableSchema> tables = BuildTables(rawColumns);

        string dbName = GetDatabaseName(connectionString);

        DatabaseSchema output = new(dbName, tables);

        return output;
    }

    /// <summary>
    /// Returns the query used to retrieve tables and columns from Oracle.
    /// </summary>
    private static string GetTablesQuery()
    {
        // In Oracle, USER_TABLES contains tables owned by the current user, excluding system tables.
        return @"
            SELECT 
                t.TABLE_NAME AS TableName,
                c.COLUMN_NAME AS ColumnName,
                c.DATA_TYPE AS DataType,
                c.DATA_LENGTH AS MaxLength,
                CASE WHEN c.NULLABLE = 'Y' THEN 1 ELSE 0 END AS IsNullable
            FROM USER_TABLES t
            JOIN USER_TAB_COLUMNS c ON t.TABLE_NAME = c.TABLE_NAME
            ORDER BY t.TABLE_NAME, c.COLUMN_ID";
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
                    DataType: c.DataType,
                    MaxLength: c.MaxLength,
                    IsNullable: c.IsNullable))]
            ))];
    }

    /// <summary>
    /// Retrieves the database name from the Oracle connection string.
    /// </summary>
    private static string GetDatabaseName(string connectionString)
    {
        return new OracleConnectionStringBuilder(connectionString).UserID
            ?? throw new InvalidOperationException(Warnings.DatabaseNameNotSpecified);
    }
}