# FlexConf API Reference

## Core Classes

### Parser

The main class for parsing FlexConf files.

```csharp
public class Parser
{
    // Parse a configuration file
    public FlexConfObject Parse(string filePath);
    
    // Parse configuration from a string
    public FlexConfObject ParseString(string content);
    
    // Parse with a specific environment
    public FlexConfObject Parse(string filePath, string environment);
}
```

### FlexConfObject

Represents a parsed configuration object.

```csharp
public class FlexConfObject
{
    // Get a typed value
    public T Get<T>(string key);
    
    // Get a value with a default
    public T Get<T>(string key, T defaultValue);
    
    // Get a section as a new FlexConfObject
    public FlexConfObject GetSection(string key);
    
    // Check if a key exists
    public bool HasKey(string key);
    
    // Get all keys at the current level
    public IEnumerable<string> Keys { get; }
    
    // Add or update a value
    public void Add(string key, object value);
    
    // Add a new section
    public FlexConfObject AddSection(string key);
}
```

### Tokenizer

Handles lexical analysis of FlexConf files.

```csharp
public class Tokenizer
{
    public Tokenizer(string input);
    public IEnumerable<Token> Tokenize();
}
```

### SchemaValidator

Validates configuration against a schema.

```csharp
public class SchemaValidator
{
    public SchemaValidator(FlexConfObject schema);
    public ValidationResult Validate(FlexConfObject config);
}
```

## Extension Methods

```csharp
public static class FlexConfExtensions
{
    // Convert section to a strongly-typed object
    public static T ToObject<T>(this FlexConfObject config);
    
    // Merge two configurations
    public static FlexConfObject Merge(this FlexConfObject baseConfig, FlexConfObject overlay);
    
    // Export to different formats
    public static string ToJson(this FlexConfObject config);
    public static string ToYaml(this FlexConfObject config);
}
```

## Examples

### Basic Usage

```csharp
// Parse a configuration file
var parser = new Parser();
var config = parser.Parse("config.flexconf");

// Get values
string host = config.Get<string>("server.host");
int port = config.Get<int>("server.port");
bool ssl = config.Get<bool>("server.ssl");

// Get a section
var dbConfig = config.GetSection("database");
string connectionString = dbConfig.Get<string>("connectionString");
```

### Working with Objects

```csharp
// Define a configuration class
public class ServerConfig
{
    public string Host { get; set; }
    public int Port { get; set; }
    public bool Ssl { get; set; }
}

// Parse configuration to object
var serverConfig = config.GetSection("server").ToObject<ServerConfig>();
```

### Environment-Specific Configuration

```csharp
// Parse with environment
var prodConfig = parser.Parse("config.flexconf", "production");
var devConfig = parser.Parse("config.flexconf", "development");

// Or set environment variable
Environment.SetEnvironmentVariable("FLEXCONF_ENV", "production");
var config = parser.Parse("config.flexconf");
```

### Schema Validation

```csharp
// Create a schema
var schema = new FlexConfObject();
schema.Add("server.host", new SchemaType { Type = typeof(string), Required = true });
schema.Add("server.port", new SchemaType { Type = typeof(int), Default = 8080 });

// Validate configuration
var validator = new SchemaValidator(schema);
var result = validator.Validate(config);

if (!result.IsValid)
{
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"Error: {error}");
    }
}
```

### Configuration Merging

```csharp
// Load configurations
var baseConfig = parser.Parse("base.flexconf");
var overrides = parser.Parse("overrides.flexconf");

// Merge them
var finalConfig = baseConfig.Merge(overrides);
```

### Error Handling

```csharp
try
{
    var config = parser.Parse("config.flexconf");
}
catch (FlexConfParseException ex)
{
    Console.WriteLine($"Parse error at line {ex.Line}, column {ex.Column}: {ex.Message}");
}
catch (FlexConfValidationException ex)
{
    Console.WriteLine($"Validation error: {ex.Message}");
}
```

## Best Practices

1. **Exception Handling**: Always handle FlexConf exceptions appropriately
2. **Type Safety**: Use strongly-typed `Get<T>` methods
3. **Validation**: Implement schema validation for critical configurations
4. **Environment Awareness**: Consider using environment-specific configurations
5. **Default Values**: Provide defaults for optional configuration values

## Performance Tips

1. **Caching**: Cache parsed configurations when possible
2. **Reuse Parser**: Parser instances are thread-safe and can be reused
3. **Bulk Operations**: Use `GetSection()` for accessing multiple values in the same section
4. **Validation**: Validate configurations during startup, not at runtime

## Need Help?

Contact JVR Software:
- Email: jvrsoftware@gmail.com
- Website: [jvrsoftware.co.za](https://jvrsoftware.co.za)
