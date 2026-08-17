using SchemaCompare.Core.Models;

namespace SchemaCompare.Core.Diffs;

public record TableDiff(TableSchema Table, DiffAction Action);