namespace FlexConf.Core
{
    public class SchemaValidator
    {
        private readonly FlexConfObject _schema;

        public SchemaValidator(FlexConfObject schema)
        {
            _schema = schema;
        }

        public void Validate(FlexConfObject config)
        {
            ValidateObject(config, _schema);
        }

        private void ValidateObject(FlexConfObject config, FlexConfObject schema)
        {
            var schemaProperties = schema.GetAll();
            var configProperties = config.GetAll();

            Console.WriteLine("Validating object:");
            Console.WriteLine("  Schema properties:");
            foreach (var prop in schemaProperties)
            {
                Console.WriteLine($"    {prop.Key}: {prop.Value}");
            }
            Console.WriteLine("  Config properties:");
            foreach (var prop in configProperties)
            {
                Console.WriteLine($"    {prop.Key}: {prop.Value}");
            }

            // First, check that all required fields in the schema are present in the config
            foreach (var pair in schemaProperties)
            {
                if (pair.Value is string type)
                {
                    var (baseType, isOptional, defaultValue) = ParseTypeInfo(type);
                    if (!configProperties.ContainsKey(pair.Key))
                    {
                        if (!isOptional && defaultValue == null)
                        {
                            throw new ValidationException($"Missing required key: {pair.Key}");
                        }
                        else if (defaultValue != null)
                        {
                            config.Add(pair.Key, ConvertDefaultValue(defaultValue, baseType));
                        }
                    }
                }
            }

            // Then validate each config value against its schema type
            foreach (var pair in configProperties)
            {
                var schemaValue = schema.Get(pair.Key);
                if (schemaValue == null)
                {
                    // Skip validation for extra fields in the config
                    continue;
                }

                if (pair.Value is FlexConfObject configSection)
                {
                    // If the config value is a section, validate it against the schema section
                    var schemaSection = schema.GetSection(pair.Key);
                    if (schemaSection == null)
                    {
                        // Skip validation for extra sections in the config
                        continue;
                    }
                    ValidateObject(configSection, schemaSection);
                }
                else if (schemaValue is string type)
                {
                    // If schema specifies a type, validate against that type
                    try
                    {
                        var (baseType, _, _) = ParseTypeInfo(type);
                        ValidateType(pair.Value, baseType.ToLower(), pair.Key);
                    }
                    catch (ValidationException ex)
                    {
                        Console.WriteLine($"Validation failed for {pair.Key}: {ex.Message}");
                        throw;
                    }
                }
                else if (schemaValue is FlexConfObject)
                {
                    throw new ValidationException($"Expected section for key '{pair.Key}', got a value.");
                }
                else
                {
                    throw new ValidationException($"Invalid schema type for key '{pair.Key}'. Expected 'string', 'int', 'bool', or 'list', got '{schemaValue}'");
                }
            }
        }

        private (string BaseType, bool IsOptional, string? DefaultValue) ParseTypeInfo(string type)
        {
            var baseType = type;
            var isOptional = false;
            string? defaultValue = null;

            if (type.EndsWith("?"))
            {
                isOptional = true;
                baseType = type[..^1];
            }

            if (type.Contains('='))
            {
                var parts = type.Split('=');
                baseType = parts[0];
                defaultValue = parts[1];
            }

            return (baseType, isOptional, defaultValue);
        }

        private object ConvertDefaultValue(string value, string type)
        {
            return type.ToLower() switch
            {
                "string" => value,
                "int" or "number" => int.Parse(value),
                "bool" or "boolean" => bool.Parse(value),
                _ => throw new ValidationException($"Cannot convert default value for type: {type}")
            };
        }

        private void ValidateType(object value, string expectedType, string key)
        {
            bool isValid = expectedType switch
            {
                "string" => value is string,
                "int" or "number" => value is int,
                "bool" or "boolean" => value is bool,
                "list" or "array" => value is List<object>,
                _ => throw new ValidationException($"Unknown type in schema: {expectedType}")
            };

            if (!isValid)
            {
                throw new ValidationException(
                    $"Type mismatch for key '{key}'. Expected {expectedType}, got {value?.GetType().Name ?? "null"}");
            }
        }
    }

    public class ValidationException : Exception
    {
        public ValidationException(string message) : base(message) { }
    }
}
