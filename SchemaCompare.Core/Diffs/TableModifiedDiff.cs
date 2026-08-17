using SchemaCompare.Core.Models;

namespace SchemaCompare.Core.Diffs;

public record TableModifiedDiff(
    TableSchema SourceTable,
    TableSchema TargetTable,
    IReadOnlyCollection<ColumnDiff> ColumnsAdded,
    IReadOnlyCollection<ColumnDiff> ColumnsRemoved,
    IReadOnlyCollection<ColumnModifiedDiff> ColumnsModified);