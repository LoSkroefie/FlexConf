# Getting Started with FlexConf

## Installation

Install FlexConf via NuGet:

```bash
dotnet add package FlexConf
```

## Basic Usage

### 1. Create a Configuration File

Create a file named `config.flexconf`:

```flexconf
# Application settings
app {
    name: "MyApp"
    version: "1.0.0"
    
    # Server configuration
    server {
        host: "localhost"
        port: 8080
        ssl: true
    }
    
    # Database settings
    database {
        connectionString: "Server=localhost;Database=myapp;User Id=admin;Password=${DB_PASSWORD}"
        poolSize: 10
        timeout: 30
    }
}

# Environment-specific overrides
@production {
    app.server.host: "prod.example.com"
    app.server.port: 443
}

@development {
    app.server.host: "dev.local"
    app.database.poolSize: 5
}
```

### 2. Parse the Configuration

```csharp
using FlexConf.Core;

// Create a parser instance
var parser = new Parser();

// Parse the configuration file
var config = parser.Parse("config.flexconf");

// Access configuration values
string serverHost = config.Get<string>("app.server.host");
int serverPort = config.Get<int>("app.server.port");
bool sslEnabled = config.Get<bool>("app.server.ssl");

// Access nested objects
var dbConfig = config.GetSection("app.database");
string connectionString = dbConfig.Get<string>("connectionString");
int poolSize = dbConfig.Get<int>("poolSize");
```

### 3. Environment-Specific Configuration

FlexConf automatically applies environment-specific overrides based on the current environment:

```csharp
// Set the environment
Environment.SetEnvironmentVariable("FLEXCONF_ENV", "production");

// Parse configuration (will include production overrides)
var config = parser.Parse("config.flexconf");

// Now app.server.host will be "prod.example.com"
string host = config.Get<string>("app.server.host");
```

## Next Steps

- Learn more about the [Configuration Format](Configuration-Format)
- Explore the [API Reference](API-Reference)
- Check out [Advanced Features](Advanced-Features)
- Review [Best Practices](Best-Practices)

## Common Issues

1. **File Not Found**: Ensure the configuration file path is correct relative to your application's working directory.
2. **Parse Errors**: Check your configuration file syntax for missing brackets, quotes, or incorrect indentation.
3. **Type Conversion**: Make sure the requested type matches the configuration value type.

## Need Help?

- Check our [Troubleshooting](Troubleshooting) guide
- Visit our [FAQ](FAQ)
- Contact support: jvrsoftware@gmail.com
