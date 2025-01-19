using System.Text;
using System.Collections.Generic;

namespace FlexConf.Core
{
    public class FlexConfObject
    {
        private readonly Dictionary<string, object> _data = new();

        public void Add(string key, object value)
        {
            if (key.Contains('.'))
            {
                var parts = key.Split('.');
                var current = this;
                for (int i = 0; i < parts.Length - 1; i++)
                {
                    if (!current._data.TryGetValue(parts[i], out var sectionValue))
                    {
                        sectionValue = new FlexConfObject();
                        current._data[parts[i]] = sectionValue;
                    }
                    else if (sectionValue is not FlexConfObject)
                    {
                        throw new Exception($"Cannot add nested property to non-object value at '{string.Join(".", parts.Take(i + 1))}'");
                    }
                    current = (FlexConfObject)sectionValue;
                }
                current._data[parts[^1]] = value;
            }
            else
            {
                _data[key] = value;
            }
        }

        public void AddSection(string key, FlexConfObject section)
        {
            if (key.Contains('.'))
            {
                var parts = key.Split('.');
                var current = this;
                for (int i = 0; i < parts.Length - 1; i++)
                {
                    if (!current._data.TryGetValue(parts[i], out var sectionValue))
                    {
                        sectionValue = new FlexConfObject();
                        current._data[parts[i]] = sectionValue;
                    }
                    else if (sectionValue is not FlexConfObject)
                    {
                        throw new Exception($"Cannot add nested section to non-object value at '{string.Join(".", parts.Take(i + 1))}'");
                    }
                    current = (FlexConfObject)sectionValue;
                }
                current._data[parts[^1]] = section;
            }
            else
            {
                _data[key] = section;
            }
        }

        public object? Get(string key)
        {
            if (key.Contains('.'))
            {
                var parts = key.Split('.');
                var current = this;
                for (int i = 0; i < parts.Length - 1; i++)
                {
                    if (!current._data.TryGetValue(parts[i], out var sectionValue) || sectionValue is not FlexConfObject section)
                        return null;
                    current = section;
                }
                return current.Get(parts[^1]);
            }
            return _data.TryGetValue(key, out var value) ? value : null;
        }

        public FlexConfObject? GetSection(string key)
        {
            var value = Get(key);
            if (value is FlexConfObject section)
            {
                return section;
            }
            return null;
        }

        public Dictionary<string, object> GetAll()
        {
            return new Dictionary<string, object>(_data);
        }

        public bool TryGet(string key, out object? value)
        {
            if (key.Contains('.'))
            {
                var parts = key.Split('.');
                var current = this;
                for (int i = 0; i < parts.Length - 1; i++)
                {
                    if (!current._data.TryGetValue(parts[i], out var sectionValue) || sectionValue is not FlexConfObject section)
                    {
                        value = null;
                        return false;
                    }
                    current = section;
                }
                return current.TryGet(parts[^1], out value);
            }
            return _data.TryGetValue(key, out value);
        }

        public override string ToString()
        {
            return ToString(0);
        }

        private string ToString(int indent)
        {
            var sb = new StringBuilder();
            string indentStr = new(' ', indent * 2);
            string nextIndentStr = new(' ', (indent + 1) * 2);

            sb.AppendLine("{");
            var pairs = _data.ToList();
            for (int i = 0; i < pairs.Count; i++)
            {
                var pair = pairs[i];
                sb.Append(nextIndentStr);
                sb.Append($"\"{pair.Key}\": ");

                if (pair.Value is FlexConfObject section)
                {
                    sb.Append(section.ToString(indent + 1).TrimEnd());
                }
                else if (pair.Value is List<object> list)
                {
                    sb.Append($"[{string.Join(", ", list.Select(FormatValue))}]");
                }
                else
                {
                    sb.Append(FormatValue(pair.Value));
                }

                if (i < pairs.Count - 1)
                {
                    sb.AppendLine(",");
                }
                else
                {
                    sb.AppendLine();
                }
            }
            sb.Append($"{indentStr}}}");
            return sb.ToString();
        }

        private static string FormatValue(object value)
        {
            return value switch
            {
                string str => $"\"{str}\"",
                bool b => b.ToString().ToLower(),
                int or double or float or decimal => value.ToString()?.ToLower() ?? "null",
                _ => value.ToString() ?? "null"
            };
        }
    }
}
