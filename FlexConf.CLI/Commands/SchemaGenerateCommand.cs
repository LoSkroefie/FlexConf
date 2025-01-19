using FlexConf.Core;

namespace FlexConf.CLI.Commands
{
    public static class SchemaGenerateCommand
    {
        public static int Execute(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: flexconf schema-generate <configFile> [outputFile]");
                Console.WriteLine("Example: flexconf schema-generate config.flexonconf schema.flexonconf");
                return 1;
            }

            string configFile = args[0];
            string outputFile = args.Length > 1 ? args[1] : Path.ChangeExtension(configFile, ".schema.flexonconf");

            if (!File.Exists(configFile))
            {
                Console.Error.WriteLine($"Config file not found: {configFile}");
                return 1;
            }

            try
            {
                // Parse config file
                string configText = File.ReadAllText(configFile);
                var tokens = new Tokenizer(configText).Tokenize();
                var config = new Parser(tokens).Parse();

                // Generate schema
                var schema = GenerateSchema(config);

                // Write schema to file
                File.WriteAllText(outputFile, schema.ToString());

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Schema generated and saved to: {outputFile}");
                Console.ResetColor();
                return 0;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine($"Schema generation failed: {ex.Message}");
                Console.ResetColor();
                return 1;
            }
        }

        private static FlexConfObject GenerateSchema(FlexConfObject config)
        {
            var schema = new FlexConfObject();
            foreach (var pair in config.GetAll())
            {
                if (pair.Value is FlexConfObject section)
                {
                    schema.AddSection(pair.Key, GenerateSchema(section));
                }
                else
                {
                    schema.Add(pair.Key, InferType(pair.Value));
                }
            }
            return schema;
        }

        private static string InferType(object value)
        {
            return value switch
            {
                int => "int",
                bool => "bool",
                string => "string",
                List<object> => "list",
                _ => "unknown"
            };
        }
    }
}
