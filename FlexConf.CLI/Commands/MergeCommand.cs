using FlexConf.Core;

namespace FlexConf.CLI.Commands
{
    public static class MergeCommand
    {
        public static int Execute(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: flexconf merge <baseConfig> <overlayConfig> [outputFile]");
                Console.WriteLine("Example: flexconf merge base.flexonconf dev.flexonconf merged.flexonconf");
                return 1;
            }

            string baseConfigFile = args[0];
            string overlayConfigFile = args[1];
            string outputFile = args.Length > 2 ? args[2] : "merged.flexonconf";

            if (!File.Exists(baseConfigFile))
            {
                Console.Error.WriteLine($"Base config file not found: {baseConfigFile}");
                return 1;
            }

            if (!File.Exists(overlayConfigFile))
            {
                Console.Error.WriteLine($"Overlay config file not found: {overlayConfigFile}");
                return 1;
            }

            try
            {
                // Parse base config
                string baseText = File.ReadAllText(baseConfigFile);
                var baseTokens = new Tokenizer(baseText).Tokenize();
                var baseConfig = new Parser(baseTokens).Parse();

                // Parse overlay config
                string overlayText = File.ReadAllText(overlayConfigFile);
                var overlayTokens = new Tokenizer(overlayText).Tokenize();
                var overlayConfig = new Parser(overlayTokens).Parse();

                // Merge configurations
                ApplyEnvironmentOverlay(baseConfig, overlayConfig);

                // Write merged config
                File.WriteAllText(outputFile, baseConfig.ToString());

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Merged configuration saved to: {outputFile}");
                Console.ResetColor();
                return 0;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine($"Merge failed: {ex.Message}");
                Console.ResetColor();
                return 1;
            }
        }

        private static void ApplyEnvironmentOverlay(FlexConfObject baseConfig, FlexConfObject overlay)
        {
            foreach (var pair in overlay.GetAll())
            {
                if (pair.Value is FlexConfObject overlaySection && 
                    baseConfig.Get(pair.Key) is FlexConfObject baseSection)
                {
                    ApplyEnvironmentOverlay(baseSection, overlaySection);
                }
                else
                {
                    baseConfig.Add(pair.Key, pair.Value);
                }
            }
        }
    }
}
