using SchemaCompare.Core.Models;

namespace SchemaCompare.Core.Interfaces;

public interface ISchemaReader
{
    string ProviderName { get; }
    Task<DatabaseSchema> ReadSchemaAsync(string connectionString, CancellationToken cancellationToken = default);
}