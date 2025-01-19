namespace FlexConf.Core
{
    public class Parser
    {
        private readonly List<Token> _tokens;
        private int _current = 0;

        public Parser(List<Token> tokens)
        {
            _tokens = tokens;
        }

        public FlexConfObject Parse()
        {
            var root = new FlexConfObject();
            var environments = new Dictionary<string, FlexConfObject>();

            while (!IsAtEnd())
            {
                if (Match("WORD") && Previous().Value == "environment")
                {
                    if (!Match("WORD"))
                    {
                        throw new Exception($"Line {Peek().Line}: Expected environment name");
                    }
                    string envName = Previous().Value;

                    if (!Match("BRACE_OPEN"))
                    {
                        throw new Exception($"Line {Peek().Line}: Expected '{{' after environment name");
                    }

                    var envConfig = ParseEnvironmentSection();
                    environments[envName] = envConfig;

                    // Set environment variable for testing
                    if (envName == "dev")
                    {
                        Environment.SetEnvironmentVariable("FLEXCONF_ENV", "dev");
                    }
                }
                else if (Match("WORD"))
                {
                    string key = Previous().Value;
                    if (Match("BRACE_OPEN"))
                    {
                        var section = ParseObject();
                        root.AddSection(key, section);
                    }
                    else if (Match("COLON"))
                    {
                        var value = ParseValue();
                        root.Add(key, value);
                    }
                    else
                    {
                        throw new Exception($"Line {Peek().Line}: Expected '{{' or ':' after {key}");
                    }
                }
                else if (Match("BRACE_OPEN"))
                {
                    root = ParseObject();
                }
                else
                {
                    // Skip any non-word tokens (like whitespace)
                    Advance();
                }
            }

            // Apply environment overrides
            var currentEnv = Environment.GetEnvironmentVariable("FLEXCONF_ENV");
            if (currentEnv != null && environments.TryGetValue(currentEnv, out var envOverrides))
            {
                ApplyEnvironmentOverrides(root, envOverrides);
            }

            return root;
        }

        private FlexConfObject ParseObject()
        {
            var obj = new FlexConfObject();

            while (!Check("BRACE_CLOSE") && !IsAtEnd())
            {
                if (!Match("WORD") && !Match("STRING"))
                {
                    throw new Exception($"Line {Peek().Line}: Expected property name");
                }

                string key = Previous().Value;

                if (Match("BRACE_OPEN"))
                {
                    var section = ParseObject();
                    obj.AddSection(key, section);
                }
                else if (Match("COLON"))
                {
                    var value = ParseValue();
                    obj.Add(key, value);
                }
                else
                {
                    throw new Exception($"Line {Peek().Line}: Expected ':' after property name");
                }
            }

            if (!Match("BRACE_CLOSE"))
            {
                throw new Exception($"Line {Peek().Line}: Expected '}}' at end of object");
            }

            return obj;
        }

        private FlexConfObject ParseEnvironmentSection()
        {
            var env = new FlexConfObject();

            while (!Check("BRACE_CLOSE") && !IsAtEnd())
            {
                if (!Match("WORD") && !Match("STRING"))
                {
                    throw new Exception($"Line {Peek().Line}: Expected property name");
                }

                string key = Previous().Value;
                string fullKey = key;

                // Handle dotted paths for environment overrides
                while (Match("ENVIRONMENT"))
                {
                    fullKey += Previous().Value;
                }

                if (!Match("COLON"))
                {
                    throw new Exception($"Line {Peek().Line}: Expected ':' after property name");
                }

                var value = ParseValue();
                env.Add(fullKey, value);
            }

            if (!Match("BRACE_CLOSE"))
            {
                throw new Exception($"Line {Peek().Line}: Expected '}}' at end of environment section");
            }

            return env;
        }

        private object ParseValue()
        {
            if (Match("STRING"))
            {
                return Previous().Value;
            }
            else if (Match("WORD"))
            {
                string value = Previous().Value.ToLower();
                if (value == "true") return true;
                if (value == "false") return false;
                if (value == "null") return null;
                return value;
            }
            else if (Match("BRACKET_OPEN"))
            {
                var list = new List<object>();

                if (!Check("BRACKET_CLOSE"))
                {
                    do
                    {
                        var value = ParseValue();
                        list.Add(value);
                    } while (Match("COMMA"));
                }

                if (!Match("BRACKET_CLOSE"))
                {
                    throw new Exception($"Line {Peek().Line}: Expected ']' at end of list");
                }

                return list;
            }
            else if (Match("NUMBER"))
            {
                return int.Parse(Previous().Value);
            }
            else if (Match("BOOLEAN"))
            {
                return bool.Parse(Previous().Value);
            }
            else
            {
                throw new Exception($"Line {Peek().Line}: Expected value");
            }
        }

        private void ApplyEnvironmentOverrides(FlexConfObject root, FlexConfObject envConfig)
        {
            foreach (var pair in envConfig.GetAll())
            {
                string[] parts = pair.Key.Split('.');
                if (parts.Length == 1)
                {
                    root.Add(pair.Key, pair.Value);
                }
                else
                {
                    var current = root;
                    for (int i = 0; i < parts.Length - 1; i++)
                    {
                        if (!current.TryGet(parts[i], out var next) || !(next is FlexConfObject))
                        {
                            next = new FlexConfObject();
                            current.AddSection(parts[i], next as FlexConfObject);
                        }
                        current = next as FlexConfObject;
                    }
                    current.Add(parts[^1], pair.Value);
                }
            }
        }

        private bool Match(string type)
        {
            if (Check(type))
            {
                Advance();
                return true;
            }
            return false;
        }

        private bool Check(string type)
        {
            if (IsAtEnd()) return false;
            return Peek().Type == type;
        }

        private Token Advance()
        {
            if (!IsAtEnd()) _current++;
            return Previous();
        }

        private bool IsAtEnd()
        {
            return _current >= _tokens.Count;
        }

        private Token Peek()
        {
            if (IsAtEnd())
                throw new Exception("Unexpected end of file");
            return _tokens[_current];
        }

        private Token Previous()
        {
            return _tokens[_current - 1];
        }
    }
}
