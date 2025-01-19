using FlexConf.Core;

namespace FlexConf.CLI.Commands
{
    public static class SerializeCommand
    {
        public static int Execute(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: flexconf serialize <configFile> [outputFile]");
                Console.WriteLine("Example: flexconf serialize config.flexonconf config.bin");
                return 1;
            }

            string configFile = args[0];
            string outputFile = args.Length > 1 ? args[1] : Path.ChangeExtension(configFile, ".bin");

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

                // Serialize to binary
                byte[] binaryData = BinarySerializer.Serialize(config);
                File.WriteAllBytes(outputFile, binaryData);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Configuration serialized to: {outputFile}");
                Console.WriteLine($"Size: {binaryData.Length:N0} bytes");
                Console.ResetColor();
                return 0;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine($"Serialization failed: {ex.Message}");
                Console.ResetColor();
                return 1;
            }
        }
    }
}
