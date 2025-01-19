using FlexConf.Core;

namespace FlexConf.CLI.Commands
{
    public static class DeserializeCommand
    {
        public static int Execute(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: flexconf deserialize <binaryFile> [outputFile]");
                Console.WriteLine("Example: flexconf deserialize config.bin config.flexonconf");
                return 1;
            }

            string binaryFile = args[0];
            string outputFile = args.Length > 1 ? args[1] : Path.ChangeExtension(binaryFile, ".flexonconf");

            if (!File.Exists(binaryFile))
            {
                Console.Error.WriteLine($"Binary file not found: {binaryFile}");
                return 1;
            }

            try
            {
                // Read binary file
                byte[] binaryData = File.ReadAllBytes(binaryFile);

                // Deserialize
                var config = BinarySerializer.Deserialize(binaryData);

                // Write to output file
                File.WriteAllText(outputFile, config.ToString());

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Configuration deserialized to: {outputFile}");
                Console.ResetColor();
                return 0;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine($"Deserialization failed: {ex.Message}");
                Console.ResetColor();
                return 1;
            }
        }
    }
}
