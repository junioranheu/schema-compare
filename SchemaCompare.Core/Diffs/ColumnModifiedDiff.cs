using SchemaCompare.Core.Models;

namespace SchemaCompare.Core.Diffs;

public record ColumnModifiedDiff(
    ColumnSchema SourceColumn,
    ColumnSchema TargetColumn,
    IReadOnlyCollection<string> Differences);