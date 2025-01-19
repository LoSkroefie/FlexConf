using FlexConf.CLI.Commands;

namespace FlexConf.CLI
{
    public class Program
    {
        static int Main(string[] args)
        {
            if (args.Length == 0 || args[0] == "--help" || args[0] == "-h")
            {
                ShowHelp();
                return 0;
            }

            try
            {
                string command = args[0].ToLower();
                var remainingArgs = args.Skip(1).ToArray();

                return command switch
                {
                    "validate" => ValidateCommand.Execute(remainingArgs),
                    "serialize" => SerializeCommand.Execute(remainingArgs),
                    "deserialize" => DeserializeCommand.Execute(remainingArgs),
                    "merge" => MergeCommand.Execute(remainingArgs),
                    "schema-generate" => SchemaGenerateCommand.Execute(remainingArgs),
                    "generate" => GenerateCommand.Execute(remainingArgs),
                    _ => HandleUnknownCommand(command)
                };
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine($"Error: {ex.Message}");
                Console.ResetColor();
                return 1;
            }
        }

        private static void ShowHelp()
        {
            Console.WriteLine("FlexConf CLI - Version 1.0.0");
            Console.WriteLine("Usage: flexconf <command> [options]");
            Console.WriteLine();
            Console.WriteLine("Commands:");
            Console.WriteLine("  validate <configFile> <schemaFile>    Validate a configuration against a schema");
            Console.WriteLine("  serialize <configFile> [outputFile]   Convert to binary format");
            Console.WriteLine("  deserialize <binaryFile> [outputFile] Convert binary back to text");
            Console.WriteLine("  merge <baseConfig> <overlayConfig>    Combine multiple configurations");
            Console.WriteLine("  schema-generate <configFile>          Generate a schema from config");
            Console.WriteLine("  generate <schemaFile> <outputFile>    Generate default config from schema");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  flexconf validate config.flexon schema.flexon");
            Console.WriteLine("  flexconf serialize config.flexon config.bin");
            Console.WriteLine("  flexconf merge base.flexon overlay.flexon");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  --help, -h     Show this help message");
            Console.WriteLine();
        }

        private static int HandleUnknownCommand(string command)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine($"Unknown command: {command}");
            Console.ResetColor();
            Console.WriteLine();
            ShowHelp();
            return 1;
        }
    }
}
