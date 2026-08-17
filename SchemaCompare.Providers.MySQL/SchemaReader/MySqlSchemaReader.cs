using System.Data;
using Dapper;
using MySqlConnector;
using SchemaCompare.Core.Consts;
using SchemaCompare.Core.Interfaces;
using SchemaCompare.Core.Models;
using SchemaCompare.Core.SchemaReader;

namespace SchemaCompare.Providers.MySQL.SchemaReader;

public class MySqlSchemaReader : ISchemaReader
{
    public virtual string ProviderName => "MySQL";

    public async Task<DatabaseSchema> ReadSchemaAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = new(connectionString);
        await connection.OpenAsync(cancellationToken);

        IEnumerable<RawColumnDto> rawColumns = await connection.QueryAsync<RawColumnDto>(GetTablesQuery());

        List<TableSchema> tables = BuildTables(rawColumns);

        string dbName = GetDatabaseName(connectionString);

        DatabaseSchema output = new(dbName, tables);

        return output;
    }

    /// <summary>
    /// Returns the query used to retrieve tables and columns from MySQL.
    /// </summary>
    private static string GetTablesQuery()
    {
        return @"
            SELECT 
                t.TABLE_SCHEMA AS `Schema`, 
                t.TABLE_NAME AS `TableName`,
                c.COLUMN_NAME AS `ColumnName`, 
                c.DATA_TYPE AS `DataType`,
                c.CHARACTER_MAXIMUM_LENGTH AS `MaxLength`,
                CASE WHEN c.IS_NULLABLE = 'YES' THEN 1 ELSE 0 END AS `IsNullable`
            FROM information_schema.tables t
            JOIN information_schema.columns c 
              ON t.TABLE_NAME = c.TABLE_NAME AND t.TABLE_SCHEMA = c.TABLE_SCHEMA
            WHERE t.TABLE_TYPE = 'BASE TABLE'
              AND t.TABLE_SCHEMA = DATABASE() 
            ORDER BY t.TABLE_SCHEMA, t.TABLE_NAME, c.ORDINAL_POSITION;";
    }

    /// <summary>
    /// Converts the raw query results into the canonical table schema model.
    /// </summary>
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

    /// <summary>
    /// Retrieves the database name from the MySQL connection string.
    /// </summary>
    private static string GetDatabaseName(string connectionString)
    {
        return new MySqlConnectionStringBuilder(connectionString).Database
            ?? throw new InvalidOperationException(Warnings.DatabaseNameNotSpecified);
    }
}