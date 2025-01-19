using FlexConf.Core;
using Xunit;

namespace FlexConf.Tests
{
    public class SchemaValidatorTests
    {
        [Fact]
        public void ValidatesCorrectTypes()
        {
            var schema = new FlexConfObject();
            schema.Add("string_field", "string");
            schema.Add("int_field", "int");
            schema.Add("bool_field", "bool");

            var config = new FlexConfObject();
            config.Add("string_field", "test");
            config.Add("int_field", 42);
            config.Add("bool_field", true);

            var validator = new SchemaValidator(schema);
            var exception = Record.Exception(() => validator.Validate(config));
            Assert.Null(exception);
        }

        [Fact]
        public void ThrowsOnMissingRequiredField()
        {
            var schema = new FlexConfObject();
            schema.Add("required_field", "string");

            var config = new FlexConfObject();

            var validator = new SchemaValidator(schema);
            Assert.Throws<ValidationException>(() => validator.Validate(config));
        }

        [Fact]
        public void ThrowsOnTypeMismatch()
        {
            var schema = new FlexConfObject();
            schema.Add("number_field", "int");

            var config = new FlexConfObject();
            config.Add("number_field", "not a number");

            var validator = new SchemaValidator(schema);
            Assert.Throws<ValidationException>(() => validator.Validate(config));
        }

        [Fact]
        public void ValidatesNestedStructures()
        {
            var nestedSchema = new FlexConfObject();
            nestedSchema.Add("port", "int");
            nestedSchema.Add("host", "string");

            var schema = new FlexConfObject();
            schema.AddSection("server", nestedSchema);

            var nestedConfig = new FlexConfObject();
            nestedConfig.Add("port", 8080);
            nestedConfig.Add("host", "localhost");

            var config = new FlexConfObject();
            config.AddSection("server", nestedConfig);

            var validator = new SchemaValidator(schema);
            var exception = Record.Exception(() => validator.Validate(config));
            Assert.Null(exception);
        }

        [Fact]
        public void ValidatesListType()
        {
            var schema = new FlexConfObject();
            schema.Add("items", "list");

            var config = new FlexConfObject();
            config.Add("items", new List<object> { 1, 2, 3 });

            var validator = new SchemaValidator(schema);
            var exception = Record.Exception(() => validator.Validate(config));
            Assert.Null(exception);
        }

        [Fact]
        public void ThrowsOnInvalidListType()
        {
            var schema = new FlexConfObject();
            schema.Add("items", "list");

            var config = new FlexConfObject();
            config.Add("items", "not a list");

            var validator = new SchemaValidator(schema);
            Assert.Throws<ValidationException>(() => validator.Validate(config));
        }

        [Fact]
        public void ThrowsOnUnknownType()
        {
            var schema = new FlexConfObject();
            schema.Add("field", "unknown_type");

            var config = new FlexConfObject();
            config.Add("field", "value");

            var validator = new SchemaValidator(schema);
            Assert.Throws<ValidationException>(() => validator.Validate(config));
        }

        [Fact]
        public void ValidatesNestedListTypes()
        {
            var schema = new FlexConfObject();
            var config = new FlexConfObject();

            schema.Add("nested_list", "list");
            var list = new List<object> { new List<object> { 1, 2, 3 }, new List<object> { 4, 5, 6 } };
            config.Add("nested_list", list);

            var validator = new SchemaValidator(schema);
            validator.Validate(config); // Should not throw
        }

        [Fact]
        public void ValidatesOptionalFields()
        {
            var schema = new FlexConfObject();
            var config = new FlexConfObject();

            var optionalSection = new FlexConfObject();
            optionalSection.Add("optional_field", "string?");
            schema.AddSection("optional", optionalSection);

            var configSection = new FlexConfObject();
            config.AddSection("optional", configSection);

            var validator = new SchemaValidator(schema);
            validator.Validate(config); // Should not throw - optional field can be missing
        }

        [Fact]
        public void ValidatesDefaultValues()
        {
            var schema = new FlexConfObject();
            var config = new FlexConfObject();

            schema.Add("port", "int=8080");
            
            var validator = new SchemaValidator(schema);
            validator.Validate(config); // Should not throw - default value is used
            Assert.Equal(8080, config.Get("port"));
        }
    }
}
