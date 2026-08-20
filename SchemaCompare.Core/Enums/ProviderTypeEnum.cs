using System.ComponentModel;

namespace SchemaCompare.Core.Enums;

public enum ProviderTypeEnum
{
    [Description("PostgreSQL")]
    PostgreSql,

    [Description("SQL Server")]
    SqlServer,

    [Description("MySQL")]
    MySql,

    [Description("Firebird")]
    Firebird,

    [Description("MariaDb")]
    MariaDb
}