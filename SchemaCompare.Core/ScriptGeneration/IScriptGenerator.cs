using SchemaCompare.Core.Diffs;

namespace SchemaCompare.Core.ScriptGeneration;

/// <summary>
/// Generates SQL scripts based on schema differences.
/// </summary>
public interface IScriptGenerator
{
    IEnumerable<string> GenerateScripts(SchemaDiff diff);
}