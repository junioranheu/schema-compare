using SchemaCompare.Providers.MySQL.SchemaReader;

namespace SchemaCompare.Providers.MariaDB.SchemaReader;

// Inherits all MySQL logic, changing only the Provider name displayed in the interface.
public class MariaDbSchemaReader : MySqlSchemaReader
{
    public override string ProviderName => "MariaDB";
}