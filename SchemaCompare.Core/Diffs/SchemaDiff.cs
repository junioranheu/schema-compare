namespace SchemaCompare.Core.Diffs;

public record SchemaDiff(
    IReadOnlyCollection<TableDiff> TablesAdded,
    IReadOnlyCollection<TableDiff> TablesRemoved,
    IReadOnlyCollection<TableModifiedDiff> TablesModified)
{
    public bool HasDifferences => TablesAdded.Count != 0 || TablesRemoved.Count != 0 || TablesModified.Count != 0;
}