namespace SchemaCompare.Core.Models;

public record DatabaseSchema(
    string Name, 
    IReadOnlyCollection<TableSchema> Tables);