using SchemaCompare.Core.Diffs;
using SchemaCompare.Core.Models;

namespace SchemaCompare.Core.Interfaces;

public interface IDiffEngine
{
    SchemaDiff Compare(DatabaseSchema source, DatabaseSchema target);
}