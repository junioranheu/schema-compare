using System.ComponentModel;

namespace SchemaCompare.Core.Enums;

/// <summary>
/// Enum que define o status de testes de um provider de banco de dados
/// </summary>
public enum TestingStatusEnum
{
    [Description("Not tested")]
    NotTested = 0,

    [Description("Tested with a simple schema")]
    TestedWithSimpleTable = 1,

    [Description("Tested with a real-world schema")]
    TestedWithRealTable = 2
}