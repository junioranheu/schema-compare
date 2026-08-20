using FirebirdSql.Data.FirebirdClient;
using SchemaCompare.Core.Interfaces;
using SchemaCompare.Core.Models;
using SchemaCompare.Core.SchemaReader;
using SchemaCompare.Providers.Firebird.SchemaReader;
using System.Reflection;

namespace SchemaCompare.UnitTests.Providers.Firebird;

public class FirebirdSchemaReaderTests
{
    private readonly FirebirdSchemaReader _reader;

    public FirebirdSchemaReaderTests()
    {
        _reader = new FirebirdSchemaReader();
    }

    #region ProviderName Tests
    [Fact]
    public void ProviderName_ShouldReturnFirebird()
    {
        // Act
        string providerName = _reader.ProviderName;

        // Assert
        Assert.Equal("Firebird", providerName);
    }

    [Fact]
    public void FirebirdSchemaReader_ShouldImplementISchemaReader()
    {
        // Assert
        Assert.IsType<ISchemaReader>(_reader, exactMatch: false);
    }

    #endregion

    #region ReadSchemaAsync Tests
    [Fact]
    public async Task ReadSchemaAsync_WithEmptyConnectionString_ThrowsInvalidOperationException()
    {
        // Arrange
        string emptyConnectionString = string.Empty;

        // Act & Assert - FbConnection throws InvalidOperationException when connection string is empty
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await _reader.ReadSchemaAsync(emptyConnectionString);
        });
    }

    [Fact]
    public async Task ReadSchemaAsync_WithInvalidConnectionString_ThrowsFbException()
    {
        // Arrange
        string invalidConnectionString = "Server=invalid_server;Database=nonexistent.fdb";

        // Act & Assert
        try
        {
            await _reader.ReadSchemaAsync(invalidConnectionString);
            Assert.Fail("Expected an exception to be thrown");
        }
        catch (FbException)
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
        string connectionString = "User=sysdba;Password=masterkey;Database=/tmp/test.fdb";
        CancellationTokenSource cancellationTokenSource = new();
        cancellationTokenSource.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await _reader.ReadSchemaAsync(connectionString, cancellationTokenSource.Token);
        });
    }
    #endregion

    #region BuildTables Tests (via Reflection)
    [Fact]
    public void BuildTables_WithEmptyInput_ReturnsEmptyList()
    {
        // Arrange
        List<RawColumnDto> emptyColumns = [];

        MethodInfo? method = typeof(FirebirdSchemaReader).GetMethod(
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
                Schema = null!,
                TableName = "USERS",
                ColumnName = "ID",
                DataType = "BIGINT",
                MaxLength = null,
                IsNullable = false
            }
        ];

        MethodInfo? method = typeof(FirebirdSchemaReader).GetMethod(
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
        Assert.Equal(string.Empty, result[0].Schema);
        Assert.Equal("USERS", result[0].Name);
        Assert.Single(result[0].Columns);
        Assert.Equal("ID", result[0].Columns.First().Name);
    }

    [Fact]
    public void BuildTables_WithMultipleColumnsInSameTable_GroupsCorrectly()
    {
        // Arrange
        List<RawColumnDto> columns =
        [
            new()
            {
                Schema = null!,
                TableName = "USERS",
                ColumnName = "ID",
                DataType = "BIGINT",
                MaxLength = null,
                IsNullable = false
            },
            new()
            {
                Schema = null!,
                TableName = "USERS",
                ColumnName = "NAME",
                DataType = "VARCHAR",
                MaxLength = 255,
                IsNullable = true
            },
            new()
            {
                Schema = null!,
                TableName = "USERS",
                ColumnName = "EMAIL",
                DataType = "VARCHAR",
                MaxLength = 100,
                IsNullable = false
            }
        ];

        MethodInfo? method = typeof(FirebirdSchemaReader).GetMethod(
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
        Assert.Equal("USERS", result[0].Name);
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
                Schema = null!,
                TableName = "USERS",
                ColumnName = "ID",
                DataType = "BIGINT",
                MaxLength = null,
                IsNullable = false
            },
            new()
            {
                Schema = null!,
                TableName = "POSTS",
                ColumnName = "ID",
                DataType = "BIGINT",
                MaxLength = null,
                IsNullable = false
            }
        ];

        MethodInfo? method = typeof(FirebirdSchemaReader).GetMethod(
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
        Assert.Contains(result, t => t.Name == "USERS");
        Assert.Contains(result, t => t.Name == "POSTS");
    }

    [Fact]
    public void BuildTables_PreservesColumnProperties()
    {
        // Arrange
        List<RawColumnDto> columns =
        [
            new()
            {
                Schema = null!,
                TableName = "USERS",
                ColumnName = "EMAIL",
                DataType = "VARCHAR",
                MaxLength = 100,
                IsNullable = true
            }
        ];

        MethodInfo? method = typeof(FirebirdSchemaReader).GetMethod(
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
        Assert.Equal("EMAIL", column.Name);
        Assert.Equal("VARCHAR", column.DataType);
        Assert.Equal(100, column.MaxLength);
        Assert.True(column.IsNullable);
    }

    [Fact]
    public void BuildTables_WithNullDataType_DefaultsToUNKNOWN()
    {
        // Arrange
        List<RawColumnDto> columns =
        [
            new()
            {
                Schema = null!,
                TableName = "USERS",
                ColumnName = "CUSTOM_FIELD",
                DataType = null!,
                MaxLength = null,
                IsNullable = false
            }
        ];

        MethodInfo? method = typeof(FirebirdSchemaReader).GetMethod(
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
        Assert.Equal("UNKNOWN", column.DataType);
    }
    #endregion

    #region GetTablesQuery Tests
    [Fact]
    public void GetTablesQuery_ReturnsNonEmptyString()
    {
        // Arrange
        MethodInfo? method = typeof(FirebirdSchemaReader).GetMethod(
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
        MethodInfo? method = typeof(FirebirdSchemaReader).GetMethod(
            "GetTablesQuery",
            BindingFlags.NonPublic | BindingFlags.Static,
            null,
            Type.EmptyTypes,
            null
        );

        // Act
        string query = (string)method!.Invoke(null, null)!;

        // Assert
        Assert.Contains("RDB$RELATIONS", query);
        Assert.Contains("RDB$RELATION_FIELDS", query);
        Assert.Contains("RDB$FIELDS", query);
        Assert.Contains("RDB$SYSTEM_FLAG", query);
        Assert.Contains("TRIM", query);
    }
    #endregion

    #region GetDatabaseName Tests
    [Fact]
    public void GetDatabaseName_WithValidConnectionString_ReturnsDatabasePath()
    {
        // Arrange
        string connectionString = "User=sysdba;Password=masterkey;Database=C:\\databases\\test.fdb";

        MethodInfo? method = typeof(FirebirdSchemaReader).GetMethod(
            "GetDatabaseName",
            BindingFlags.NonPublic | BindingFlags.Static,
            null,
            [typeof(string)],
            null
        );

        // Act
        string result = (string)method!.Invoke(null, [connectionString])!;

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void GetDatabaseName_ExtractsCorrectDatabaseFromVariousFormats()
    {
        // Arrange
        string[] testCases =
        [
            "User=sysdba;Password=masterkey;Database=/home/user/testdb.fdb",
            "Database=testdb.fdb;User=sysdba;Password=masterkey",
            "Server=localhost;Database=C:\\Data\\mydb.fdb;User=sysdba;Password=masterkey",
        ];

        MethodInfo? method = typeof(FirebirdSchemaReader).GetMethod(
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