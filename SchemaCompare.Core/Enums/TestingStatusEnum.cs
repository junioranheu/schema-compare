using System.ComponentModel;

namespace SchemaCompare.Core.Enums;

/// <summary>
/// Defines the testing status of a database provider.
/// </summary>
public enum TestingStatusEnum
{
    [Description("Not tested")] // Not tested in a real environment.
    NotTested = 0,

    [Description("Tested with a simple schema")]
    TestedWithSimpleTable = 1,

    [Description("Tested with a real-world schema")]
    TestedWithRealTable = 2
}