using SchemaCompare.Core.Models;

namespace SchemaCompare.Core.Diffs;

public record ColumnDiff(ColumnSchema Column, DiffAction Action);