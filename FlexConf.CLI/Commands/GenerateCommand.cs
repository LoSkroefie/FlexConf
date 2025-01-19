using FlexConf.Core;
using System;
using System.IO;

namespace FlexConf.CLI.Commands
{
    public static class GenerateCommand
    {
        public static int Execute(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: flexconf generate <schemaFile> <outputFile>");
                Console.WriteLine("Example: flexconf generate schema.flexonconf config.flexonconf");
                return 1;
            }

            string schemaFile = Path.GetFullPath(args[0]);
            string outputFile = Path.GetFullPath(args[1]);

            if (!File.Exists(schemaFile))
            {
                Console.Error.WriteLine($"Schema file not found: {schemaFile}");
                return 1;
            }

            try
            {
                // Parse schema file
                string schemaText = File.ReadAllText(schemaFile);
                var schemaTokens = new Tokenizer(schemaText).Tokenize();
                var schema = new Parser(schemaTokens).Parse();

                // Generate default config
                var config = GenerateDefaultConfig(schema);

                // Write to file
                File.WriteAllText(outputFile, config.ToString());
                Console.WriteLine($"Default configuration generated and saved to: {outputFile}");
                return 0;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine($"Error generating default config: {ex.Message}");
                Console.ResetColor();
                return 1;
            }
        }

        private static FlexConfObject GenerateDefaultConfig(FlexConfObject schema)
        {
            var config = new FlexConfObject();

            if (schema.TryGet("type", out var type) && type?.ToString() == "object" && 
                schema.TryGet("properties", out var props) && props is FlexConfObject propsObj)
            {
                foreach (var pair in propsObj.GetAll())
                {
                    if (pair.Value is FlexConfObject propSchema)
                    {
                        config.Add(pair.Key, GenerateDefaultConfig(propSchema));
                    }
                }
            }
            else
            {
                foreach (var pair in schema.GetAll())
                {
                    if (pair.Value is FlexConfObject propSchema)
                    {
                        if (propSchema.TryGet("type", out var propType))
                        {
                            var defaultValue = propSchema.TryGet("default", out var def) ? def : null;
                            config.Add(pair.Key, GetDefaultValue(propType.ToString(), defaultValue?.ToString()));
                        }
                    }
                }
            }

            return config;
        }

        private static object GetDefaultValue(string type, string? defaultValue)
        {
            if (defaultValue != null)
            {
                return type.ToLower() switch
                {
                    "string" => defaultValue,
                    "integer" => int.Parse(defaultValue),
                    "number" => double.Parse(defaultValue),
                    "boolean" => bool.Parse(defaultValue),
                    _ => throw new Exception($"Cannot convert default value for type: {type}")
                };
            }

            return type.ToLower() switch
            {
                "string" => "",
                "integer" => 0,
                "number" => 0.0,
                "boolean" => false,
                "array" => new List<object>(),
                "object" => new FlexConfObject(),
                _ => throw new Exception($"Unknown type: {type}")
            };
        }
    }
}
