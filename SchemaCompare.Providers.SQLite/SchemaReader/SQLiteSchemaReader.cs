using Dapper;
using Microsoft.Data.Sqlite;
using SchemaCompare.Core.Interfaces;
using SchemaCompare.Core.Models;
using SchemaCompare.Core.SchemaReader;
using SchemaCompare.Providers.SQLite.DTOs;
using System.Data;

namespace SchemaCompare.Providers.SQLite.SchemaReader;

public class SQLiteSchemaReader : ISchemaReader
{
    public string ProviderName => "SQLite";

    public async Task<DatabaseSchema> ReadSchemaAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        await using SqliteConnection connection = new(connectionString);
        await connection.OpenAsync(cancellationToken);

        List<string> tables = await GetTables(connection);
        List<RawColumnDto> allColumns = await GetColumns(connection, tables);

        List<TableSchema> tableSchemas = BuildTables(allColumns);

        string dbName = GetDatabaseName(connectionString);
        DatabaseSchema output = new(dbName, tableSchemas);

        return output;
    }

    /// <summary>
    /// Retrieves all user tables from the SQLite database.
    /// </summary>
    private static async Task<List<string>> GetTables(SqliteConnection connection)
    {
        string tablesQuery = @"
            SELECT name AS TableName 
            FROM sqlite_master 
            WHERE type = 'table' 
            AND name NOT LIKE 'sqlite_%';";

        return [.. (await connection.QueryAsync<string>(tablesQuery))];
    }

    /// <summary>
    /// Retrieves column information for all user tables in the SQLite database.
    /// </summary>
    private static async Task<List<RawColumnDto>> GetColumns(SqliteConnection connection, List<string> tables)
    {
        List<RawColumnDto> allColumns = [];

        foreach (string tableName in tables)
        {
            string columnsQuery = $@"
                SELECT 
                    cid AS ColumnId,
                    name AS ColumnName,
                    type AS DataType,
                    notnull AS IsNotNull,
                    dflt_value AS DefaultValue,
                    pk AS IsPrimaryKey
                FROM pragma_table_info('{tableName}');";

            IEnumerable<SqlitePragmaColumnDto> columns = await connection.QueryAsync<SqlitePragmaColumnDto>(columnsQuery);

            foreach (SqlitePragmaColumnDto col in columns)
            {
                allColumns.Add(new RawColumnDto
                {
                    TableName = tableName,
                    ColumnName = col.ColumnName,
                    DataType = col.DataType ?? "TEXT",
                    IsNullable = !col.IsNotNull // If it is not NOT NULL, then it is nullable.
                });
            }
        }

        return allColumns;
    }

    /// <summary>
    /// Converts the raw SQLite column data into the canonical schema model.
    /// </summary>
    private static List<TableSchema> BuildTables(List<RawColumnDto> allColumns)
    {
        return [.. allColumns.
            GroupBy(x => x.TableName).
            Select(x => new TableSchema(
                Schema: string.Empty, // SQLite does not use the traditional schema concept.
                Name: x.Key,
                Columns: [.. x.Select(c => new ColumnSchema(
                    Name: c.ColumnName,
                    DataType: c.DataType,
                    MaxLength: null, // SQLite uses dynamic typing/type affinity and rarely defines a fixed max length for simple columns.
                    IsNullable: c.IsNullable))]
            ))];
    }

    /// <summary>
    /// Retrieves the database file name from the SQLite connection string.
    /// </summary>
    private static string GetDatabaseName(string connectionString)
    {
        string dbName = new SqliteConnectionStringBuilder(connectionString).DataSource;
        string fileName = Path.GetFileName(dbName);

        return fileName;
    }
}