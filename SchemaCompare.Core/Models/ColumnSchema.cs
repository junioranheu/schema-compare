namespace SchemaCompare.Core.Models;

public record ColumnSchema(
    string Name,
    string DataType,
    int? MaxLength,
    bool IsNullable);