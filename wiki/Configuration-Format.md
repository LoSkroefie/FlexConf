# FlexConf Configuration Format

## Overview

FlexConf uses a human-readable format that combines the best features of JSON, YAML, and INI formats. The syntax is designed to be intuitive and flexible while maintaining strict parsing rules for reliability.

## Basic Syntax

### Comments

```flexconf
# Single-line comment
/* Multi-line
   comment */
```

### Key-Value Pairs

```flexconf
key: value
string_value: "Hello, World!"
number_value: 42
boolean_value: true
null_value: null
```

### Sections (Objects)

```flexconf
section {
    key1: "value1"
    key2: "value2"
    
    nested_section {
        key3: "value3"
    }
}
```

### Arrays

```flexconf
simple_array: [1, 2, 3, 4, 5]
string_array: ["one", "two", "three"]
mixed_array: [1, "two", true, null]

complex_array: [
    {
        name: "Item 1"
        value: 42
    },
    {
        name: "Item 2"
        value: 84
    }
]
```

### Environment Variables

```flexconf
database {
    password: ${DB_PASSWORD}  # References environment variable
    host: ${DB_HOST:-localhost}  # With default value
}
```

### Environment-Specific Overrides

```flexconf
# Base configuration
app {
    url: "http://localhost"
    debug: false
}

# Production overrides
@production {
    app.url: "https://example.com"
}

# Development overrides
@development {
    app.debug: true
}

# Testing overrides
@testing {
    app.url: "http://test.local"
}
```

### Dotted Key Notation

```flexconf
# These are equivalent:

# Nested syntax
database {
    credentials {
        username: "admin"
    }
}

# Dotted notation
database.credentials.username: "admin"
```

## Data Types

### Strings

```flexconf
# With quotes (allows special characters)
message1: "Hello, World!"

# Without quotes (simple strings only)
message2: Hello

# Multi-line strings
description: """
    This is a multi-line
    string value that preserves
    line breaks and indentation.
    """
```

### Numbers

```flexconf
# Integers
port: 8080
count: -42

# Floating point
pi: 3.14159
temperature: -0.5
```

### Booleans

```flexconf
debug: true
production: false
```

### Null

```flexconf
optional_value: null
```

### Arrays

```flexconf
# Single line
numbers: [1, 2, 3]

# Multi-line
colors: [
    "red",
    "green",
    "blue"
]
```

### Objects

```flexconf
# Single line
point: { x: 10, y: 20 }

# Multi-line
person {
    name: "John Doe"
    age: 30
    address {
        street: "123 Main St"
        city: "Example City"
    }
}
```

## Advanced Features

### References

```flexconf
# Define common values
common {
    timeout: 30
    retries: 3
}

# Reference them elsewhere
service1 {
    timeout: ${common.timeout}
    maxRetries: ${common.retries}
}

service2 {
    timeout: ${common.timeout}
    maxRetries: ${common.retries}
}
```

### Includes

```flexconf
# Include other configuration files
@include "database.flexconf"
@include "logging.flexconf"
```

### Templates

```flexconf
# Define a template
@template "service" {
    port: 8080
    timeout: 30
    enabled: true
}

# Use the template
service1 @extends "service" {
    port: 8081  # Override specific values
}

service2 @extends "service" {
    port: 8082
}
```

## Best Practices

1. **Use Sections**: Group related configuration items in sections
2. **Consistent Naming**: Use consistent naming conventions (e.g., camelCase or snake_case)
3. **Comments**: Add comments for non-obvious configuration values
4. **Environment Variables**: Use environment variables for sensitive data
5. **Validation**: Define and use schemas to validate configuration
6. **Defaults**: Provide sensible default values where appropriate

## Common Mistakes

1. **Missing Quotes**: String values with special characters must be quoted
2. **Incorrect Indentation**: While not required, consistent indentation improves readability
3. **Unclosed Sections**: Make sure all section braces are properly closed
4. **Invalid References**: Environment variable and reference syntax must be exact
5. **Type Mismatches**: Be careful with implicit type conversion

## Tools and Validation

Use the FlexConf CLI tools to validate your configuration:

```bash
flexconf validate config.flexconf
flexconf format config.flexconf  # Format and validate
flexconf check-refs config.flexconf  # Check references
```

## Need Help?

Contact JVR Software:
- Email: jvrsoftware@gmail.com
- Website: [jvrsoftware.co.za](https://jvrsoftware.co.za)
