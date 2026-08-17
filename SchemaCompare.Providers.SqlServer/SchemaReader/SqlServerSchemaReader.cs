using Dapper;
using Microsoft.Data.SqlClient;
using SchemaCompare.Core.Consts;
using SchemaCompare.Core.Interfaces;
using SchemaCompare.Core.Models;
using SchemaCompare.Core.SchemaReader;
using System.Data;

namespace SchemaCompare.Providers.SqlServer.SchemaReader;

public class SqlServerSchemaReader : ISchemaReader
{
    public string ProviderName => "SqlServer";

    public async Task<DatabaseSchema> ReadSchemaAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        await using SqlConnection connection = new(connectionString);
        await connection.OpenAsync(cancellationToken);

        IEnumerable<RawColumnDto> rawColumns = await connection.QueryAsync<RawColumnDto>(GetTablesQuery());

        List<TableSchema> tables = BuildTables(rawColumns);

        string dbName = GetDatabaseName(connectionString);

        DatabaseSchema output = new(dbName, tables);

        return output;
    }

    private static string GetTablesQuery()
    {
        return @"
            SELECT 
                t.TABLE_SCHEMA AS [Schema], 
                t.TABLE_NAME AS [TableName],
                c.COLUMN_NAME AS [ColumnName], 
                c.DATA_TYPE AS [DataType],
                c.CHARACTER_MAXIMUM_LENGTH AS [MaxLength],
                CASE WHEN c.IS_NULLABLE = 'YES' THEN 1 ELSE 0 END AS [IsNullable]
            FROM INFORMATION_SCHEMA.TABLES t
            JOIN INFORMATION_SCHEMA.COLUMNS c 
              ON t.TABLE_NAME = c.TABLE_NAME AND t.TABLE_SCHEMA = c.TABLE_SCHEMA
            WHERE t.TABLE_TYPE = 'BASE TABLE'
            ORDER BY t.TABLE_SCHEMA, t.TABLE_NAME, c.ORDINAL_POSITION;";
    }

    private static List<TableSchema> BuildTables(IEnumerable<RawColumnDto> rawColumns)
    {
        return [.. rawColumns.
            GroupBy(x => new { x.Schema, x.TableName }).
            Select(x => new TableSchema(
                Schema: x.Key.Schema,
                Name: x.Key.TableName,
                Columns: [.. x.Select(c => new ColumnSchema(
                    Name: c.ColumnName,
                    DataType: c.DataType,
                    MaxLength: c.MaxLength,
                    IsNullable: c.IsNullable))]
            ))];
    }

    private static string GetDatabaseName(string connectionString)
    {
        return new SqlConnectionStringBuilder(connectionString).InitialCatalog
            ?? throw new InvalidOperationException(Warnings.DatabaseNameNotSpecified);
    }
}