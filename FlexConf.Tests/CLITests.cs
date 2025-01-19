using FlexConf.CLI;
using FlexConf.CLI.Commands;
using Xunit;

namespace FlexConf.Tests
{
    public class CLITests
    {
        private const string TestConfigContent = @"
server {
    host: ""localhost""
    port: 8080
}";

        private const string TestSchemaContent = @"
server {
    host: string
    port: int
}";

        [Fact]
        public void SerializeCommand_CreatesValidBinaryFile()
        {
            string configFile = "test_config.flexon";
            string binaryFile = "test_config.bin";

            File.WriteAllText(configFile, TestConfigContent);

            try
            {
                int result = SerializeCommand.Execute(new[] { configFile, binaryFile });
                Assert.Equal(0, result);
                Assert.True(File.Exists(binaryFile));
                Assert.True(new FileInfo(binaryFile).Length > 0);
            }
            finally
            {
                File.Delete(configFile);
                if (File.Exists(binaryFile))
                    File.Delete(binaryFile);
            }
        }

        [Fact]
        public void DeserializeCommand_RecreatesOriginalConfig()
        {
            string configFile = "test_config.flexon";
            string binaryFile = "test_config.bin";
            string outputFile = "test_output.flexon";

            File.WriteAllText(configFile, TestConfigContent);

            try
            {
                SerializeCommand.Execute(new[] { configFile, binaryFile });
                int result = DeserializeCommand.Execute(new[] { binaryFile, outputFile });
                
                Assert.Equal(0, result);
                Assert.True(File.Exists(outputFile));

                string deserializedContent = File.ReadAllText(outputFile);
                Assert.Contains("localhost", deserializedContent);
                Assert.Contains("8080", deserializedContent);
            }
            finally
            {
                File.Delete(configFile);
                File.Delete(binaryFile);
                if (File.Exists(outputFile))
                    File.Delete(outputFile);
            }
        }

        [Fact]
        public void MergeCommand_CombinesConfigurations()
        {
            string baseConfig = "base_config.flexon";
            string overlayConfig = "overlay_config.flexon";
            string mergedConfig = "merged_config.flexon";

            File.WriteAllText(baseConfig, @"
                server {
                    host: ""localhost""
                    port: 8080
                }");

            File.WriteAllText(overlayConfig, @"
                server {
                    port: 3000
                }");

            try
            {
                int result = MergeCommand.Execute(new[] { baseConfig, overlayConfig, mergedConfig });
                
                Assert.Equal(0, result);
                Assert.True(File.Exists(mergedConfig));

                string mergedContent = File.ReadAllText(mergedConfig);
                Assert.Contains("localhost", mergedContent);
                Assert.Contains("3000", mergedContent);
                Assert.DoesNotContain("8080", mergedContent);
            }
            finally
            {
                File.Delete(baseConfig);
                File.Delete(overlayConfig);
                if (File.Exists(mergedConfig))
                    File.Delete(mergedConfig);
            }
        }

        [Fact]
        public void SchemaGenerateCommand_CreatesValidSchema()
        {
            string configFile = "test_config.flexon";
            string schemaFile = "generated_schema.flexon";

            File.WriteAllText(configFile, TestConfigContent);

            try
            {
                int result = SchemaGenerateCommand.Execute(new[] { configFile, schemaFile });
                
                Assert.Equal(0, result);
                Assert.True(File.Exists(schemaFile));

                string schemaContent = File.ReadAllText(schemaFile);
                Assert.Contains("string", schemaContent);
                Assert.Contains("int", schemaContent);
            }
            finally
            {
                File.Delete(configFile);
                if (File.Exists(schemaFile))
                    File.Delete(schemaFile);
            }
        }

        [Fact]
        public void GenerateCommand_CreatesDefaultConfig()
        {
            string schemaFile = "test_schema.flexon";
            string outputFile = "test_output.flexon";

            string schemaContent = @"{
    ""type"": ""object"",
    ""properties"": {
        ""server"": {
            ""type"": ""object"",
            ""properties"": {
                ""host"": { ""type"": ""string"", ""default"": ""localhost"" },
                ""port"": { ""type"": ""integer"", ""default"": 8080 },
                ""secure"": { ""type"": ""boolean"", ""default"": false },
                ""tags"": { ""type"": ""array"" }
            }
        },
        ""logging"": {
            ""type"": ""object"",
            ""properties"": {
                ""level"": { ""type"": ""string"", ""default"": ""info"" },
                ""file"": { ""type"": ""string"" }
            }
        }
    }
}";

            File.WriteAllText(schemaFile, schemaContent);

            try
            {
                int result = GenerateCommand.Execute(new[] { schemaFile, outputFile });
                Assert.Equal(0, result);
                Assert.True(File.Exists(outputFile));

                string generatedContent = File.ReadAllText(outputFile);
                Assert.Contains("\"host\": \"localhost\"", generatedContent);
                Assert.Contains("\"port\": 8080", generatedContent);
                Assert.Contains("\"secure\": false", generatedContent);
                Assert.Contains("\"level\": \"info\"", generatedContent);
            }
            finally
            {
                File.Delete(schemaFile);
                if (File.Exists(outputFile))
                {
                    File.Delete(outputFile);
                }
            }
        }
    }
}
