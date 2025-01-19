using FlexConf.Core;
using Xunit;

namespace FlexConf.Tests
{
    public class ParserTests
    {
        [Fact]
        public void CanParseBasicKeyValuePairs()
        {
            string input = @"
                server {
                    host: ""localhost""
                    port: 8080
                    secure: true
                }";

            var tokens = new Tokenizer(input).Tokenize();
            var parser = new Parser(tokens);
            var config = parser.Parse();

            Assert.Equal("localhost", config.Get("server.host"));
            Assert.Equal(8080, config.Get("server.port"));
            Assert.Equal(true, config.Get("server.secure"));
        }

        [Fact]
        public void CanParseNestedSections()
        {
            string input = @"
                server {
                    host: ""localhost""
                    settings {
                        retries: 3
                        timeout: 5000
                    }
                }";

            var tokens = new Tokenizer(input).Tokenize();
            var parser = new Parser(tokens);
            var config = parser.Parse();

            var serverSettings = config.GetSection("server.settings");
            Assert.NotNull(serverSettings);
            Assert.Equal(3, serverSettings.Get("retries"));
            Assert.Equal(5000, serverSettings.Get("timeout"));
        }

        [Fact]
        public void CanParseLists()
        {
            string input = @"
                allowed_ips: [""127.0.0.1"", ""192.168.1.1""]
            ";

            var tokens = new Tokenizer(input).Tokenize();
            var parser = new Parser(tokens);
            var config = parser.Parse();

            var allowedIps = config.Get("allowed_ips") as List<object>;
            Assert.NotNull(allowedIps);
            Assert.Contains("127.0.0.1", allowedIps);
            Assert.Contains("192.168.1.1", allowedIps);
        }

        [Fact]
        public void CanHandleEnvironmentOverrides()
        {
            string input = @"
                server {
                    host: ""localhost""
                    port: 8080
                }

                environment dev {
                    server.port: 3000
                }
            ";

            var tokens = new Tokenizer(input).Tokenize();
            var parser = new Parser(tokens);
            var config = parser.Parse();

            Assert.Equal("localhost", config.Get("server.host"));
            Assert.Equal(3000, config.Get("server.port"));
        }

        [Fact]
        public void CanHandleComments()
        {
            string input = @"
                # Server configuration
                server {
                    host: ""localhost"" # Default host
                    /* Multi-line
                       comment */
                    port: 8080
                }";

            var tokens = new Tokenizer(input).Tokenize();
            var parser = new Parser(tokens);
            var config = parser.Parse();

            Assert.Equal("localhost", config.Get("server.host"));
            Assert.Equal(8080, config.Get("server.port"));
        }

        [Fact]
        public void ThrowsErrorOnInvalidSyntax()
        {
            string input = @"
                server {
                    host: ""localhost""
                    port: 8080
                # Missing closing brace
            ";

            var tokens = new Tokenizer(input).Tokenize();
            var parser = new Parser(tokens);

            Assert.Throws<Exception>(() => parser.Parse());
        }

        [Fact]
        public void CanTokenizeBasicConfig()
        {
            string input = @"
                server {
                    host: ""localhost""
                    port: 8080
                    secure: true
                }";

            var tokens = new Tokenizer(input).Tokenize();

            // Print tokens for debugging
            foreach (var token in tokens)
            {
                Console.WriteLine($"Token: {token.Type} = {token.Value}");
            }

            Assert.Contains(tokens, t => t.Type == "WORD" && t.Value == "server");
            Assert.Contains(tokens, t => t.Type == "BRACE_OPEN");
            Assert.Contains(tokens, t => t.Type == "WORD" && t.Value == "host");
            Assert.Contains(tokens, t => t.Type == "COLON");
            Assert.Contains(tokens, t => t.Type == "STRING" && t.Value == "localhost");
            Assert.Contains(tokens, t => t.Type == "WORD" && t.Value == "port");
            Assert.Contains(tokens, t => t.Type == "COLON");
            Assert.Contains(tokens, t => t.Type == "NUMBER" && t.Value == "8080");
            Assert.Contains(tokens, t => t.Type == "WORD" && t.Value == "secure");
            Assert.Contains(tokens, t => t.Type == "COLON");
            Assert.Contains(tokens, t => t.Type == "BOOLEAN" && t.Value == "true");
            Assert.Contains(tokens, t => t.Type == "BRACE_CLOSE");
        }
    }
}
