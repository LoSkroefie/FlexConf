using FlexConf.Core;
using Xunit;

namespace FlexConf.Tests
{
    public class BinarySerializerTests
    {
        [Fact]
        public void CanSerializeAndDeserializeBasicTypes()
        {
            var config = new FlexConfObject();
            config.Add("string", "test");
            config.Add("int", 42);
            config.Add("bool", true);

            byte[] binary = BinarySerializer.Serialize(config);
            var deserialized = BinarySerializer.Deserialize(binary);

            Assert.Equal("test", deserialized.Get("string"));
            Assert.Equal(42, deserialized.Get("int"));
            Assert.Equal(true, deserialized.Get("bool"));
        }

        [Fact]
        public void CanSerializeAndDeserializeNestedObjects()
        {
            var nested = new FlexConfObject();
            nested.Add("key", "value");

            var config = new FlexConfObject();
            config.AddSection("nested", nested);

            byte[] binary = BinarySerializer.Serialize(config);
            var deserialized = BinarySerializer.Deserialize(binary);

            var deserializedNested = deserialized.GetSection("nested");
            Assert.NotNull(deserializedNested);
            Assert.Equal("value", deserializedNested.Get("key"));
        }

        [Fact]
        public void CanSerializeAndDeserializeLists()
        {
            var config = new FlexConfObject();
            config.Add("list", new List<object> { 1, "two", true });

            byte[] binary = BinarySerializer.Serialize(config);
            var deserialized = BinarySerializer.Deserialize(binary);

            var list = deserialized.Get("list") as List<object>;
            Assert.NotNull(list);
            Assert.Equal(3, list.Count);
            Assert.Equal(1, list[0]);
            Assert.Equal("two", list[1]);
            Assert.Equal(true, list[2]);
        }

        [Fact]
        public void CanSerializeAndDeserializeComplexStructure()
        {
            var config = new FlexConfObject();
            
            var server = new FlexConfObject();
            server.Add("host", "localhost");
            server.Add("port", 8080);
            
            var settings = new FlexConfObject();
            settings.Add("timeout", 5000);
            settings.Add("retries", 3);
            
            server.AddSection("settings", settings);
            config.AddSection("server", server);
            config.Add("allowed_ips", new List<object> { "127.0.0.1", "192.168.1.1" });

            byte[] binary = BinarySerializer.Serialize(config);
            var deserialized = BinarySerializer.Deserialize(binary);

            Assert.Equal("localhost", deserialized.Get("server.host"));
            Assert.Equal(8080, deserialized.Get("server.port"));
            Assert.Equal(5000, deserialized.Get("server.settings.timeout"));
            Assert.Equal(3, deserialized.Get("server.settings.retries"));

            var ips = deserialized.Get("allowed_ips") as List<object>;
            Assert.NotNull(ips);
            Assert.Contains("127.0.0.1", ips);
            Assert.Contains("192.168.1.1", ips);
        }
    }
}
