using FlexConf.Core;
using System;
using System.Diagnostics;
using System.IO;

namespace FlexConf.CLI.Commands
{
    public static class ValidateCommand
    {
        public static int Execute(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: flexconf validate <configFile> <schemaFile>");
                Console.WriteLine("Example: flexconf validate config.flexonconf schema.flexonconf");
                return 1;
            }

            string configFile = Path.GetFullPath(args[0]);
            string schemaFile = Path.GetFullPath(args[1]);

            if (!File.Exists(configFile))
            {
                Console.Error.WriteLine($"Config file not found: {configFile}");
                return 1;
            }

            if (!File.Exists(schemaFile))
            {
                Console.Error.WriteLine($"Schema file not found: {schemaFile}");
                return 1;
            }

            try
            {
                // Parse config file first
                string configText = File.ReadAllText(configFile);
                Console.WriteLine($"Config content:\n{configText}");
                var configTokens = new Tokenizer(configText).Tokenize();
                var config = new Parser(configTokens).Parse();

                // Parse schema file
                string schemaText = File.ReadAllText(schemaFile);
                Console.WriteLine($"Schema content:\n{schemaText}");
                var schemaTokens = new Tokenizer(schemaText).Tokenize();
                var schema = new Parser(schemaTokens).Parse();

                try
                {
                    // Validate
                    var validator = new SchemaValidator(schema);
                    validator.Validate(config);

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Configuration is valid!");
                    Console.ResetColor();
                    return 0;
                }
                catch (ValidationException ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Error.WriteLine($"Validation failed: {ex.Message}");
                    Console.ResetColor();
                    return 1;
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine($"Error during validation: {ex.Message}");
                Console.ResetColor();
                return 1;
            }
        }
    }
}
