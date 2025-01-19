using System.Text;

namespace FlexConf.Core
{
    public class BinarySerializer
    {
        private const byte TYPE_NULL = 0;
        private const byte TYPE_INT = 1;
        private const byte TYPE_BOOL = 2;
        private const byte TYPE_STRING = 3;
        private const byte TYPE_OBJECT = 4;
        private const byte TYPE_LIST = 5;

        public static byte[] Serialize(FlexConfObject config)
        {
            using var ms = new MemoryStream();
            SerializeObject(config, ms);
            return ms.ToArray();
        }

        private static void SerializeObject(FlexConfObject obj, MemoryStream ms)
        {
            foreach (var pair in obj.GetAll())
            {
                WriteString(ms, pair.Key);
                WriteValue(ms, pair.Value);
            }
            // Write end marker
            ms.WriteByte(TYPE_NULL);
        }

        private static void WriteValue(MemoryStream ms, object? value)
        {
            switch (value)
            {
                case null:
                    ms.WriteByte(TYPE_NULL);
                    break;
                case int intValue:
                    ms.WriteByte(TYPE_INT);
                    WriteInt(ms, intValue);
                    break;
                case bool boolValue:
                    ms.WriteByte(TYPE_BOOL);
                    ms.WriteByte((byte)(boolValue ? 1 : 0));
                    break;
                case string strValue:
                    ms.WriteByte(TYPE_STRING);
                    WriteString(ms, strValue);
                    break;
                case FlexConfObject objValue:
                    ms.WriteByte(TYPE_OBJECT);
                    SerializeObject(objValue, ms);
                    break;
                case List<object> listValue:
                    ms.WriteByte(TYPE_LIST);
                    WriteInt(ms, listValue.Count);
                    foreach (var item in listValue)
                    {
                        WriteValue(ms, item);
                    }
                    break;
                default:
                    throw new Exception($"Unsupported type: {value.GetType()}");
            }
        }

        private static void WriteString(MemoryStream ms, string value)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            WriteInt(ms, bytes.Length);
            ms.Write(bytes, 0, bytes.Length);
        }

        private static void WriteInt(MemoryStream ms, int value)
        {
            ms.Write(BitConverter.GetBytes(value), 0, 4);
        }

        public static FlexConfObject Deserialize(byte[] data)
        {
            using var ms = new MemoryStream(data);
            return DeserializeObject(ms);
        }

        private static FlexConfObject DeserializeObject(MemoryStream ms)
        {
            var obj = new FlexConfObject();

            while (ms.Position < ms.Length)
            {
                byte type = (byte)ms.ReadByte();
                if (type == TYPE_NULL) break;

                ms.Position--;
                string key = ReadString(ms);
                object value = ReadValue(ms);
                obj.Add(key, value);
            }

            return obj;
        }

        private static object ReadValue(MemoryStream ms)
        {
            byte type = (byte)ms.ReadByte();
            return type switch
            {
                TYPE_NULL => null!,
                TYPE_INT => ReadInt(ms),
                TYPE_BOOL => ms.ReadByte() == 1,
                TYPE_STRING => ReadString(ms),
                TYPE_OBJECT => DeserializeObject(ms),
                TYPE_LIST => ReadList(ms),
                _ => throw new Exception($"Unknown type: {type}")
            };
        }

        private static string ReadString(MemoryStream ms)
        {
            int length = ReadInt(ms);
            byte[] bytes = new byte[length];
            ms.Read(bytes, 0, length);
            return Encoding.UTF8.GetString(bytes);
        }

        private static int ReadInt(MemoryStream ms)
        {
            byte[] bytes = new byte[4];
            ms.Read(bytes, 0, 4);
            return BitConverter.ToInt32(bytes, 0);
        }

        private static List<object> ReadList(MemoryStream ms)
        {
            int count = ReadInt(ms);
            var list = new List<object>(count);
            for (int i = 0; i < count; i++)
            {
                list.Add(ReadValue(ms));
            }
            return list;
        }
    }
}
