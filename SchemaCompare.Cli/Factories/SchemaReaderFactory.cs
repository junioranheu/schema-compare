using SchemaCompare.Core.Enums;
using SchemaCompare.Core.Interfaces;
using SchemaCompare.Providers.Firebird.SchemaReader;
using SchemaCompare.Providers.MariaDB.SchemaReader;
using SchemaCompare.Providers.MySQL.SchemaReader;
using SchemaCompare.Providers.PostgreSQL.SchemaReader;
using SchemaCompare.Providers.SqlServer.SchemaReader;

namespace SchemaCompare.Cli.Factories;

public static class SchemaReaderFactory
{
    public static ISchemaReader Create(ProviderTypeEnum providerType)
    {
        return providerType switch
        {
            ProviderTypeEnum.PostgreSql => new PostgresSchemaReader(),
            ProviderTypeEnum.SqlServer => new SqlServerSchemaReader(),
            ProviderTypeEnum.MySql => new MySqlSchemaReader(),     
            ProviderTypeEnum.Firebird => new FirebirdSchemaReader(), 
            ProviderTypeEnum.MariaDb => new MariaDbSchemaReader(),   
            _ => throw new ArgumentOutOfRangeException(nameof(providerType), $"The {providerType} provider is not implemented.")
        };
    }
}