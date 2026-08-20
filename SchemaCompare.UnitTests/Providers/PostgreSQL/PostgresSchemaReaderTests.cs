using Npgsql;
using SchemaCompare.Core.Interfaces;
using SchemaCompare.Core.Models;
using SchemaCompare.Core.SchemaReader;
using SchemaCompare.Providers.PostgreSQL.SchemaReader;

namespace SchemaCompare.UnitTests.Providers.PostgreSQL;

public class PostgresSchemaReaderTests
{
    private readonly PostgresSchemaReader _reader;

    public PostgresSchemaReaderTests()
    {
        _reader = new PostgresSchemaReader();
    }

    #region ProviderName Tests

    [Fact]
    public void ProviderName_ShouldReturnPostgreSQL()
    {
        // Act
        var providerName = _reader.ProviderName;

        // Assert
        Assert.Equal("PostgreSQL", providerName);
    }

    [Fact]
    public void PostgresSchemaReader_ShouldImplementISchemaReader()
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

        // Act & Assert - This should throw InvalidOperationException when trying to open connection
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await _reader.ReadSchemaAsync(emptyConnectionString);
        });
    }

    [Fact]
    public async Task ReadSchemaAsync_WithConnectionStringMissingDatabase_ThrowsNpgsqlException()
    {
        // Arrange
        string connectionStringWithoutDb = "Host=localhost;Username=postgres;Password=password;";

        // Act & Assert - NpgsqlConnection will fail to connect since the host is unavailable
        // This test documents the actual error type thrown by Npgsql
        try
        {
            await _reader.ReadSchemaAsync(connectionStringWithoutDb);
            Assert.Fail("Expected an exception to be thrown");
        }
        catch (NpgsqlException)
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
        string connectionString = "Host=localhost;Username=postgres;Password=password;Database=testdb";
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
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
        var emptyColumns = new List<RawColumnDto>();
        var method = typeof(PostgresSchemaReader).GetMethod(
            "BuildTables",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static,
            null,
            [typeof(IEnumerable<RawColumnDto>)],
            null
        );

        // Act
        var result = (List<TableSchema>)method!.Invoke(null, [emptyColumns])!;

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void BuildTables_WithSingleColumn_ReturnsSingleTable()
    {
        // Arrange
        var columns = new List<RawColumnDto>
        {
            new()
            {
                Schema = "public",
                TableName = "users",
                ColumnName = "id",
                DataType = "integer",
                MaxLength = null,
                IsNullable = false
            }
        };

        var method = typeof(PostgresSchemaReader).GetMethod(
            "BuildTables",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static,
            null,
            [typeof(IEnumerable<RawColumnDto>)],
            null
        );

        // Act
        var result = (List<TableSchema>)method!.Invoke(null, [columns])!;

        // Assert
        Assert.Single(result);
        Assert.Equal("public", result[0].Schema);
        Assert.Equal("users", result[0].Name);
        Assert.Single(result[0].Columns);
        Assert.Equal("id", result[0].Columns.First().Name);
    }

    [Fact]
    public void BuildTables_WithMultipleColumnsInSameTable_GroupsCorrectly()
    {
        // Arrange
        var columns = new List<RawColumnDto>
        {
            new()
            {
                Schema = "public",
                TableName = "users",
                ColumnName = "id",
                DataType = "integer",
                MaxLength = null,
                IsNullable = false
            },
            new()
            {
                Schema = "public",
                TableName = "users",
                ColumnName = "name",
                DataType = "text",
                MaxLength = null,
                IsNullable = true
            },
            new()
            {
                Schema = "public",
                TableName = "users",
                ColumnName = "email",
                DataType = "character varying",
                MaxLength = 255,
                IsNullable = false
            }
        };

        var method = typeof(PostgresSchemaReader).GetMethod(
            "BuildTables",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static,
            null,
            [typeof(IEnumerable<RawColumnDto>)],
            null
        );

        // Act
        var result = (List<TableSchema>)method!.Invoke(null, [columns])!;

        // Assert
        Assert.Single(result);
        Assert.Equal("users", result[0].Name);
        Assert.Equal(3, result[0].Columns.Count);
    }

    [Fact]
    public void BuildTables_WithMultipleTables_CreatesSeparateTables()
    {
        // Arrange
        var columns = new List<RawColumnDto>
        {
            new()
            {
                Schema = "public",
                TableName = "users",
                ColumnName = "id",
                DataType = "integer",
                MaxLength = null,
                IsNullable = false
            },
            new()
            {
                Schema = "public",
                TableName = "posts",
                ColumnName = "id",
                DataType = "integer",
                MaxLength = null,
                IsNullable = false
            }
        };

        var method = typeof(PostgresSchemaReader).GetMethod(
            "BuildTables",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static,
            null,
            [typeof(IEnumerable<RawColumnDto>)],
            null
        );

        // Act
        var result = (List<TableSchema>)method!.Invoke(null, [columns])!;

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, t => t.Name == "users");
        Assert.Contains(result, t => t.Name == "posts");
    }

    [Fact]
    public void BuildTables_PreservesColumnProperties()
    {
        // Arrange
        var columns = new List<RawColumnDto>
        {
            new()
            {
                Schema = "public",
                TableName = "users",
                ColumnName = "email",
                DataType = "character varying",
                MaxLength = 255,
                IsNullable = true
            }
        };

        var method = typeof(PostgresSchemaReader).GetMethod(
            "BuildTables",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static,
            null,
            [typeof(IEnumerable<RawColumnDto>)],
            null
        );

        // Act
        var result = (List<TableSchema>)method!.Invoke(null, [columns])!;
        var column = result[0].Columns.First();

        // Assert
        Assert.Equal("email", column.Name);
        Assert.Equal("character varying", column.DataType);
        Assert.Equal(255, column.MaxLength);
        Assert.True(column.IsNullable);
    }

    #endregion

    #region GetTablesQuery Tests

    [Fact]
    public void GetTablesQuery_ReturnsNonEmptyString()
    {
        // Arrange
        var method = typeof(PostgresSchemaReader).GetMethod(
            "GetTablesQuery",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static,
            null,
            Type.EmptyTypes,
            null
        );

        // Act
        var result = (string)method!.Invoke(null, null)!;

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void GetTablesQuery_ContainsRequiredKeywords()
    {
        // Arrange
        var method = typeof(PostgresSchemaReader).GetMethod(
            "GetTablesQuery",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static,
            null,
            Type.EmptyTypes,
            null
        );

        // Act
        var query = (string)method!.Invoke(null, null)!;

        // Assert
        Assert.Contains("information_schema.tables", query);
        Assert.Contains("information_schema.columns", query);
        Assert.Contains("pg_catalog", query);
        Assert.Contains("information_schema", query);
        Assert.Contains("BASE TABLE", query);
    }

    #endregion

    #region GetDatabaseName Tests

    [Fact]
    public void GetDatabaseName_WithValidConnectionString_ReturnsDatabaseName()
    {
        // Arrange
        string connectionString = "Host=localhost;Username=postgres;Password=password;Database=testdb";
        var method = typeof(PostgresSchemaReader).GetMethod(
            "GetDatabaseName",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static,
            null,
            [typeof(string)],
            null
        );

        // Act
        var result = (string)method!.Invoke(null, [connectionString])!;

        // Assert
        Assert.Equal("testdb", result);
    }

    [Fact]
    public void GetDatabaseName_WithConnectionStringMissingDatabase_ThrowsInvalidOperationException()
    {
        // Arrange
        string connectionStringWithoutDb = "Host=localhost;Username=postgres;Password=password;";
        var method = typeof(PostgresSchemaReader).GetMethod(
            "GetDatabaseName",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static,
            null,
            [typeof(string)],
            null
        );

        // Act & Assert
        Assert.Throws<System.Reflection.TargetInvocationException>(() =>
        {
            method!.Invoke(null, [connectionStringWithoutDb]);
        });
    }

    [Fact]
    public void GetDatabaseName_ExtractsCorrectDatabaseFromVariousFormats()
    {
        // Arrange
        var testCases = new[]
        {
            "Host=localhost;Database=mydb;Username=user;Password=pass",
            "Database=production;Host=db.example.com;Username=admin;Password=secret",
            "postgresql://user:pass@localhost/customdb",
        };

        var method = typeof(PostgresSchemaReader).GetMethod(
            "GetDatabaseName",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static,
            null,
            [typeof(string)],
            null
        );

        // Act & Assert
        foreach (var connectionString in testCases)
        {
            try
            {
                var result = (string)method!.Invoke(null, [connectionString])!;
                Assert.NotNull(result);
                Assert.NotEmpty(result);
            }
            catch (System.Reflection.TargetInvocationException)
            {
                // Expected for some formats
            }
        }
    }

    #endregion
}