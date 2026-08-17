namespace SchemaCompare.Core.Models;

public record TableSchema(string Schema, string Name, IReadOnlyCollection<ColumnSchema> Columns)
{
    public string FullName => string.IsNullOrEmpty(Schema) ? Name : $"{Schema}.{Name}";
}