# FlexConf

A powerful and flexible configuration format and parser for modern applications.

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Build Status](https://github.com/LoSkroefie/FlexConf/workflows/Build/badge.svg)](https://github.com/LoSkroefie/FlexConf/actions)

## Overview

FlexConf is a modern configuration format and parser that combines the best features of JSON, YAML, and INI formats while adding powerful features like environment-specific overrides, nested sections, and dotted key notation. Developed by [JVR Software](https://jvrsoftware.co.za), FlexConf aims to simplify configuration management for .NET applications.

## Features

- JSON-like syntax with enhanced readability
- Environment-specific configuration sections
- Nested objects and arrays support
- Dotted key notation for deep property access
- Comments support (single-line and multi-line)
- Environment variable references
- Strong type validation
- Extensible architecture

## Installation

Install FlexConf via NuGet:

```bash
dotnet add package FlexConf
```

## Quick Start

Here's a simple example of a FlexConf configuration file:

```flexconf
# Application configuration
app {
    name: "MyApp"
    version: "1.0.0"
    
    # Database settings
    database {
        host: "localhost"
        port: 5432
        credentials {
            username: "admin"
            password: ${DB_PASSWORD} # Environment variable reference
        }
    }
}

# Environment-specific overrides
@production {
    app.database.host: "prod-db.example.com"
}

@development {
    app.database.host: "dev-db.local"
}
```

Parse the configuration in your C# code:

```csharp
using FlexConf.Core;

var parser = new Parser();
var config = parser.Parse("config.flexconf");

// Access configuration values
string dbHost = config.Get<string>("app.database.host");
int dbPort = config.Get<int>("app.database.port");
```

## Documentation

For detailed documentation, please visit our [Wiki](https://github.com/LoSkroefie/FlexConf/wiki).

## Contributing

We welcome contributions! Please see our [Contributing Guidelines](CONTRIBUTING.md) for details.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Support

- Email: [jvrsoftware@gmail.com](mailto:jvrsoftware@gmail.com) or [jan@jvrsoftware.co.za](mailto:jan@jvrsoftware.co.za)
- Website: [jvrsoftware.co.za](https://jvrsoftware.co.za)
- GitHub: [github.com/LoSkroefie/FlexConf](https://github.com/LoSkroefie/FlexConf)

## Credits

Developed and maintained by JVR Software - Empowering developers with better tools.
