# SchemaCompare

Open-source .NET 10 CLI tool for comparing database schemas across different database providers.

## Status

🚧 **In development**

SchemaCompare is currently under active development and is not production-ready yet.

## What does it do?

SchemaCompare compares the structure of two databases and identifies differences between them, including:

- Added and removed tables
- Added and removed columns
- Modified column properties
- Data type differences
- Nullable differences
- Maximum length differences

The goal is to make database schema comparison simple and provider-independent.

## Providers

The following database providers have been implemented:

- SQL Server
- PostgreSQL
- MySQL
- SQLite
- Oracle
- Firebird

> ⚠️ The providers have not been fully tested yet.

## Getting Started

// TO DO

## How It Works

SchemaCompare reads the schemas from the source and target databases, converts them into a common schema model, and compares the resulting structures.

The comparison currently identifies:

- Tables added or removed
- Columns added or removed
- Column data type changes
- Nullable changes
- Maximum length changes

## Architecture

SchemaCompare uses a provider-based architecture.

Each database engine has its own schema reader, while the comparison engine works with a common schema model. This makes it possible to add support for new database providers without changing the comparison logic.

## Testing

The project is currently in development and the implemented providers have not been fully tested yet.

Real-world testing and feedback are welcome.

## Contributing

SchemaCompare is open source and contributions are welcome.

Feel free to open an issue, report bugs, suggest improvements, or submit a pull request.

## License

This project is licensed under the MIT License.