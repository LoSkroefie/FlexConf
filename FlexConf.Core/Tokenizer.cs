using System.Text;
using System.Collections.Generic;

namespace FlexConf.Core
{
    public class Token
    {
        public string Type { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public int Line { get; set; }
        public int Position { get; set; }
    }

    public class Tokenizer
    {
        private readonly string _source;
        private int _current;
        private int _line = 1;
        private int _start;
        private List<Token> _tokens = new List<Token>();

        public Tokenizer(string source)
        {
            _source = source;
        }

        public List<Token> Tokenize()
        {
            while (_current < _source.Length)
            {
                ScanToken();
            }

            return _tokens;
        }

        private void ScanToken()
        {
            char c = Advance();
            switch (c)
            {
                case '{': AddToken("BRACE_OPEN"); break;
                case '}': AddToken("BRACE_CLOSE"); break;
                case '[': AddToken("BRACKET_OPEN"); break;
                case ']': AddToken("BRACKET_CLOSE"); break;
                case ':': AddToken("COLON"); break;
                case ',': AddToken("COMMA"); break;
                case '#': SkipSingleLineComment(); break;
                case '/':
                    if (_current < _source.Length && _source[_current] == '*')
                    {
                        SkipMultiLineComment();
                    }
                    else
                    {
                        throw new Exception($"Unexpected character at position {_current}: {c}");
                    }
                    break;
                case '.': 
                    if (IsDigit(PeekPrevious()))
                    {
                        _current--;
                        ScanNumber();
                    }
                    else
                    {
                        // Environment variable reference
                        var value = new StringBuilder(".");
                        while (_current < _source.Length && (IsAlphaNumeric(_source[_current]) || _source[_current] == '_' || _source[_current] == '.'))
                        {
                            value.Append(_source[_current]);
                            _current++;
                        }
                        AddToken("ENVIRONMENT", value.ToString());
                    }
                    break;
                case '"': ScanString(); break;
                case ' ':
                case '\r':
                case '\t':
                case '\n':
                    // Ignore whitespace
                    break;
                default:
                    if (IsDigit(c))
                    {
                        _current--;
                        ScanNumber();
                    }
                    else if (IsAlpha(c))
                    {
                        ScanWord();
                    }
                    else
                    {
                        throw new Exception($"Unexpected character at position {_current}: {c}");
                    }
                    break;
            }
        }

        private void ScanString()
        {
            var value = new StringBuilder();
            while (_current < _source.Length && _source[_current] != '"')
            {
                if (_source[_current] == '\\' && _current + 1 < _source.Length)
                {
                    _current++;
                    switch (_source[_current])
                    {
                        case 'n': value.Append('\n'); break;
                        case 'r': value.Append('\r'); break;
                        case 't': value.Append('\t'); break;
                        case '"': value.Append('"'); break;
                        case '\\': value.Append('\\'); break;
                        default: throw new Exception($"Invalid escape sequence: \\{_source[_current]}");
                    }
                }
                else
                {
                    value.Append(_source[_current]);
                }
                _current++;
            }

            if (_current >= _source.Length)
            {
                throw new Exception("Unterminated string");
            }

            _current++; // Skip closing quote
            AddToken("STRING", value.ToString());
        }

        private void ScanNumber()
        {
            while (_current < _source.Length && IsDigit(_source[_current]))
            {
                _current++;
            }

            // Look for a fractional part
            if (_current < _source.Length && _source[_current] == '.' && _current + 1 < _source.Length && IsDigit(_source[_current + 1]))
            {
                _current++; // Consume the "."

                while (_current < _source.Length && IsDigit(_source[_current]))
                {
                    _current++;
                }
            }

            AddToken("NUMBER", _source.Substring(_start, _current - _start));
        }

        private void ScanWord()
        {
            while (_current < _source.Length && (IsAlphaNumeric(_source[_current]) || _source[_current] == '_'))
            {
                _current++;
            }

            string text = _source.Substring(_start, _current - _start);
            string type = text.ToLower() switch
            {
                "true" or "false" => "BOOLEAN",
                "null" => "NULL",
                _ => "WORD"
            };

            AddToken(type, text);
        }

        private void SkipSingleLineComment()
        {
            while (_current < _source.Length && _source[_current] != '\n')
            {
                _current++;
            }
        }

        private void SkipMultiLineComment()
        {
            _current++; // Skip the *
            while (_current + 1 < _source.Length && !(_source[_current] == '*' && _source[_current + 1] == '/'))
            {
                if (_source[_current] == '\n') _line++;
                _current++;
            }

            if (_current + 1 >= _source.Length)
            {
                throw new Exception("Unterminated multi-line comment");
            }

            _current += 2; // Skip the */
        }

        private void AddToken(string type, string value = "")
        {
            if (string.IsNullOrEmpty(value))
            {
                value = _source.Substring(_start, _current - _start);
            }
            _tokens.Add(new Token { Type = type, Value = value, Line = _line, Position = _start });
        }

        private bool IsAlpha(char c)
        {
            return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
        }

        private bool IsDigit(char c)
        {
            return c >= '0' && c <= '9';
        }

        private bool IsAlphaNumeric(char c)
        {
            return IsAlpha(c) || IsDigit(c);
        }

        private char Advance()
        {
            if (_source[_current] == '\n') _line++;
            _start = _current;
            return _source[_current++];
        }

        private char PeekPrevious()
        {
            return _source[_current - 1];
        }
    }
}
