# FlexConf

FlexConf is a modern configuration format and toolset designed for simplicity, readability, and extensibility. It provides a human-friendly way to write configuration files with support for schemas, validation, and environment-specific overlays.

## Features

- Simple, human-readable syntax
- Schema validation with type checking
- Support for nested sections
- Environment-specific configuration overlays
- Binary serialization for efficient loading
- Default values and optional fields
- Command-line interface (CLI)

## Installation

```bash
dotnet tool install --global FlexConf.CLI
```

## File Format

FlexConf files use a simple, indentation-optional syntax:

```
# This is a comment
server {
    host: "localhost"  # String value
    port: 8080        # Integer value
    secure: true      # Boolean value
    tags: ["web", "api"]  # List value
}

# Nested sections
logging {
    level: "info"
    file: "app.log"
}
```

## Schema Format

Schemas define the structure and types of configuration files:

```
server {
    host: string="localhost"  # String with default value
    port: int=8080           # Integer with default value
    secure: bool             # Required boolean
    tags: list              # List of any type
}

logging {
    level: string="info"     # String with default value
    file: string?           # Optional string (note the ?)
}
```

Supported types:
- `string`: Text values
- `int`: Integer numbers
- `bool`: Boolean values (true/false)
- `list`: Lists of any type
- Add `?` suffix for optional fields
- Add `=value` for default values

## CLI Commands

### Validate Configuration

Validates a configuration file against a schema:

```bash
flexconf validate config.flexon schema.flexon
```

### Generate Default Configuration

Generates a configuration file with default values from a schema:

```bash
flexconf generate schema.flexon config.flexon
```

### Merge Configurations

Merges multiple configuration files, with later files overriding earlier ones:

```bash
flexconf merge base.flexon dev.flexon output.flexon
```

### Binary Serialization

Convert configuration to/from binary format for efficient loading:

```bash
# Serialize
flexconf serialize config.flexon config.bin

# Deserialize
flexconf deserialize config.bin config.flexon
```

## Environment Overlays

FlexConf supports environment-specific configuration overlays:

```
# Base configuration
server {
    host: "localhost"
    port: 8080
}

# Production environment overlay
production {
    server {
        host: "prod.example.com"
        port: 443
    }
}
```

## API Usage

```csharp
using FlexConf.Core;

// Parse configuration
var configText = File.ReadAllText("config.flexon");
var tokens = new Tokenizer(configText).Tokenize();
var config = new Parser(tokens).Parse();

// Parse schema
var schemaText = File.ReadAllText("schema.flexon");
var schemaTokens = new Tokenizer(schemaText).Tokenize();
var schema = new Parser(schemaTokens).Parse();

// Validate configuration
var validator = new SchemaValidator(schema);
validator.Validate(config);

// Access configuration values
var host = config.Get("server.host") as string;
var port = config.Get("server.port") as int;
```

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the LICENSE file for details.
