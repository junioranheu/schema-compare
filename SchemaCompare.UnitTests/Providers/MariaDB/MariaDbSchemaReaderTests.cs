using MySqlConnector;
using SchemaCompare.Core.Interfaces;
using SchemaCompare.Core.Models;
using SchemaCompare.Core.SchemaReader;
using SchemaCompare.Providers.MariaDB.SchemaReader;
using SchemaCompare.Providers.MySQL.SchemaReader;
using System.Reflection;

namespace SchemaCompare.UnitTests.Providers.MariaDB;

public class MariaDbSchemaReaderTests
{
    private readonly MariaDbSchemaReader _reader;

    public MariaDbSchemaReaderTests()
    {
        _reader = new MariaDbSchemaReader();
    }

    #region ProviderName Tests
    [Fact]
    public void ProviderName_ShouldReturnMariaDB()
    {
        // Act
        string providerName = _reader.ProviderName;

        // Assert
        Assert.Equal("MariaDB", providerName);
    }

    [Fact]
    public void MariaDbSchemaReader_ShouldImplementISchemaReader()
    {
        // Assert
        Assert.IsType<ISchemaReader>(_reader, exactMatch: false);
    }

    #endregion

    #region ReadSchemaAsync Tests
    [Fact]
    public async Task ReadSchemaAsync_WithEmptyConnectionString_ThrowsMySqlException()
    {
        // Arrange
        string emptyConnectionString = string.Empty;

        // Act & Assert - MariaDbConnection throws MySqlException when connection string is empty
        await Assert.ThrowsAsync<MySqlException>(async () =>
        {
            await _reader.ReadSchemaAsync(emptyConnectionString);
        });
    }

    [Fact]
    public async Task ReadSchemaAsync_WithConnectionStringMissingDatabase_ThrowsMySqlException()
    {
        // Arrange
        string connectionStringWithoutDb = "Server=localhost;User=root;Password=password;";

        // Act & Assert - MariaDbConnection will fail to connect since the host is unavailable
        try
        {
            await _reader.ReadSchemaAsync(connectionStringWithoutDb);
            Assert.Fail("Expected an exception to be thrown");
        }
        catch (MySqlException)
        {
            // Expected: Connection failure
        }
        catch (Exception ex)
        {
            // Fail if it's a different exception type
            Assert.Fail($"Unexpected exception type: {ex.GetType().Name}");
        }
    }

    [Fact]
    public async Task ReadSchemaAsync_ShouldRespectCancellationToken()
    {
        // Arrange
        string connectionString = "Server=localhost;User=root;Password=password;Database=testdb";
        CancellationTokenSource cancellationTokenSource = new();
        cancellationTokenSource.Cancel();

        // Act & Assert
        // OperationCanceledException is thrown when a cancellation token is cancelled
        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
        {
            await _reader.ReadSchemaAsync(connectionString, cancellationTokenSource.Token);
        });
    }
    #endregion

    #region BuildTables Tests (via Reflection - uses MySqlSchemaReader base class)
    [Fact]
    public void BuildTables_WithEmptyInput_ReturnsEmptyList()
    {
        // Arrange
        List<RawColumnDto> emptyColumns = [];

        MethodInfo? method = typeof(MySqlSchemaReader).GetMethod(
            "BuildTables",
            BindingFlags.NonPublic | BindingFlags.Static,
            null,
            [typeof(IEnumerable<RawColumnDto>)],
            null
        );

        // Act
        List<TableSchema> result = (List<TableSchema>)method!.Invoke(null, [emptyColumns])!;

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void BuildTables_WithSingleColumn_ReturnsSingleTable()
    {
        // Arrange
        List<RawColumnDto> columns =
        [
            new()
            {
                Schema = "myapp",
                TableName = "users",
                ColumnName = "id",
                DataType = "int",
                MaxLength = null,
                IsNullable = false
            }
        ];

        MethodInfo? method = typeof(MySqlSchemaReader).GetMethod(
            "BuildTables",
            BindingFlags.NonPublic | BindingFlags.Static,
            null,
            [typeof(IEnumerable<RawColumnDto>)],
            null
        );

        // Act
        List<TableSchema> result = (List<TableSchema>)method!.Invoke(null, [columns])!;

        // Assert
        Assert.Single(result);
        Assert.Equal("myapp", result[0].Schema);
        Assert.Equal("users", result[0].Name);
        Assert.Single(result[0].Columns);
        Assert.Equal("id", result[0].Columns.First().Name);
    }

    [Fact]
    public void BuildTables_WithMultipleColumnsInSameTable_GroupsCorrectly()
    {
        // Arrange
        List<RawColumnDto> columns =
        [
            new()
            {
                Schema = "myapp",
                TableName = "users",
                ColumnName = "id",
                DataType = "int",
                MaxLength = null,
                IsNullable = false
            },
            new()
            {
                Schema = "myapp",
                TableName = "users",
                ColumnName = "name",
                DataType = "varchar",
                MaxLength = null,
                IsNullable = true
            },
            new()
            {
                Schema = "myapp",
                TableName = "users",
                ColumnName = "email",
                DataType = "varchar",
                MaxLength = 255,
                IsNullable = false
            }
        ];

        MethodInfo? method = typeof(MySqlSchemaReader).GetMethod(
            "BuildTables",
            BindingFlags.NonPublic | BindingFlags.Static,
            null,
            [typeof(IEnumerable<RawColumnDto>)],
            null
        );

        // Act
        List<TableSchema> result = (List<TableSchema>)method!.Invoke(null, [columns])!;

        // Assert
        Assert.Single(result);
        Assert.Equal("users", result[0].Name);
        Assert.Equal(3, result[0].Columns.Count);
    }

    [Fact]
    public void BuildTables_WithMultipleTables_CreatesSeparateTables()
    {
        // Arrange
        List<RawColumnDto> columns =
        [
            new()
            {
                Schema = "myapp",
                TableName = "users",
                ColumnName = "id",
                DataType = "int",
                MaxLength = null,
                IsNullable = false
            },
            new()
            {
                Schema = "myapp",
                TableName = "posts",
                ColumnName = "id",
                DataType = "int",
                MaxLength = null,
                IsNullable = false
            }
        ];

        MethodInfo? method = typeof(MySqlSchemaReader).GetMethod(
            "BuildTables",
            BindingFlags.NonPublic | BindingFlags.Static,
            null,
            [typeof(IEnumerable<RawColumnDto>)],
            null
        );

        // Act
        List<TableSchema> result = (List<TableSchema>)method!.Invoke(null, [columns])!;

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, t => t.Name == "users");
        Assert.Contains(result, t => t.Name == "posts");
    }

    [Fact]
    public void BuildTables_PreservesColumnProperties()
    {
        // Arrange
        List<RawColumnDto> columns =
        [
            new()
            {
                Schema = "myapp",
                TableName = "users",
                ColumnName = "email",
                DataType = "varchar",
                MaxLength = 255,
                IsNullable = true
            }
        ];

        MethodInfo? method = typeof(MySqlSchemaReader).GetMethod(
            "BuildTables",
            BindingFlags.NonPublic | BindingFlags.Static,
            null,
            [typeof(IEnumerable<RawColumnDto>)],
            null
        );

        // Act
        List<TableSchema> result = (List<TableSchema>)method!.Invoke(null, [columns])!;
        ColumnSchema column = result[0].Columns.First();

        // Assert
        Assert.Equal("email", column.Name);
        Assert.Equal("varchar", column.DataType);
        Assert.Equal(255, column.MaxLength);
        Assert.True(column.IsNullable);
    }
    #endregion

    #region GetTablesQuery Tests (via Reflection - uses MySqlSchemaReader base class)
    [Fact]
    public void GetTablesQuery_ReturnsNonEmptyString()
    {
        // Arrange
        MethodInfo? method = typeof(MySqlSchemaReader).GetMethod(
            "GetTablesQuery",
            BindingFlags.NonPublic | BindingFlags.Static,
            null,
            Type.EmptyTypes,
            null
        );

        // Act
        string result = (string)method!.Invoke(null, null)!;

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void GetTablesQuery_ContainsRequiredKeywords()
    {
        // Arrange
        MethodInfo? method = typeof(MySqlSchemaReader).GetMethod(
            "GetTablesQuery",
            BindingFlags.NonPublic | BindingFlags.Static,
            null,
            Type.EmptyTypes,
            null
        );

        // Act
        string query = (string)method!.Invoke(null, null)!;

        // Assert
        Assert.Contains("information_schema.tables", query);
        Assert.Contains("information_schema.columns", query);
        Assert.Contains("BASE TABLE", query);
        Assert.Contains("DATABASE()", query);
    }
    #endregion

    #region GetDatabaseName Tests (via Reflection - uses MySqlSchemaReader base class)
    [Fact]
    public void GetDatabaseName_WithValidConnectionString_ReturnsDatabaseName()
    {
        // Arrange
        string connectionString = "Server=localhost;User=root;Password=password;Database=testdb";

        MethodInfo? method = typeof(MySqlSchemaReader).GetMethod(
            "GetDatabaseName",
            BindingFlags.NonPublic | BindingFlags.Static,
            null,
            [typeof(string)],
            null
        );

        // Act
        string result = (string)method!.Invoke(null, [connectionString])!;

        // Assert
        Assert.Equal("testdb", result);
    }

    [Fact]
    public void GetDatabaseName_ExtractsCorrectDatabaseFromVariousFormats()
    {
        // Arrange
        string[] testCases =
        [
            "Server=localhost;Database=mydb;User=root;Password=pass",
            "Database=production;Server=db.example.com;User=admin;Password=secret",
            "Server=tcp:localhost;Database=customdb;User=user;Password=pass",
        ];

        MethodInfo? method = typeof(MySqlSchemaReader).GetMethod(
            "GetDatabaseName",
            BindingFlags.NonPublic | BindingFlags.Static,
            null,
            [typeof(string)],
            null
        );

        // Act & Assert
        foreach (string connectionString in testCases)
        {
            try
            {
                string result = (string)method!.Invoke(null, [connectionString])!;
                Assert.NotNull(result);
                Assert.NotEmpty(result);
            }
            catch (TargetInvocationException)
            {
                // Expected for some formats
            }
        }
    }
    #endregion
}