using Dapper;
using Npgsql;
using SchemaCompare.Core.Consts;
using SchemaCompare.Core.Interfaces;
using SchemaCompare.Core.Models;
using SchemaCompare.Core.SchemaReader;

namespace SchemaCompare.Providers.PostgreSQL.SchemaReader;

public class PostgresSchemaReader : ISchemaReader
{
    public string ProviderName => "PostgreSQL";

    public async Task<DatabaseSchema> ReadSchemaAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        await using NpgsqlConnection connection = new(connectionString);
        await connection.OpenAsync(cancellationToken);

        IEnumerable<RawColumnDto> rawColumns = await connection.QueryAsync<RawColumnDto>(GetTablesQuery());

        List<TableSchema> tables = BuildTables(rawColumns);

        string dbName = GetDatabaseName(connectionString);

        DatabaseSchema output = new(dbName, tables);

        return output;
    }

    /// <summary>
    /// Returns the query used to retrieve tables and columns from PostgreSQL.
    /// </summary>
    private static string GetTablesQuery()
    {
        return @"
            SELECT 
                t.table_schema AS Schema, 
                t.table_name AS TableName,
                c.column_name AS ColumnName, 
                c.data_type AS DataType,
                c.character_maximum_length AS MaxLength,
                CASE WHEN c.is_nullable = 'YES' THEN true ELSE false END AS IsNullable
            FROM information_schema.tables t
            JOIN information_schema.columns c 
              ON t.table_name = c.table_name AND t.table_schema = c.table_schema
            WHERE t.table_schema NOT IN ('pg_catalog', 'information_schema') 
              AND t.table_type = 'BASE TABLE'
            ORDER BY t.table_schema, t.table_name, c.ordinal_position;";
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
    /// Retrieves the database name from the PostgreSQL connection string.
    /// </summary>
    private static string GetDatabaseName(string connectionString)
    {
        return new NpgsqlConnectionStringBuilder(connectionString).Database
            ?? throw new InvalidOperationException(Warnings.DatabaseNameNotSpecified);
    }
}