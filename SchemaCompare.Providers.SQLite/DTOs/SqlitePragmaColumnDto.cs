namespace SchemaCompare.Providers.SQLite.DTOs;

public sealed class SqlitePragmaColumnDto
{
    public int ColumnId { get; set; }
    public string ColumnName { get; set; } = string.Empty;
    public string? DataType { get; set; }
    public bool IsNotNull { get; set; }
    public string? DefaultValue { get; set; }
    public bool IsPrimaryKey { get; set; }
}