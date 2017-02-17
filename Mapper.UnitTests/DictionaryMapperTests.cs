using NUnit.Framework;
using System.Collections.Generic;

namespace BusterWood.Mapper.UnitTests
{
    [TestFixture]
    public class DictionaryMapperTests
    {
        [Test]
        public void can_map_int_property()
        {
            var input = new Dictionary<string, object> { { "Fred", 1 } };
            var output = DictionaryMapper.Read<PropClass<int>>(input);
            Assert.IsNotNull(input);
            Assert.AreEqual(1, output.Fred);
        }

        [Test]
        public void can_map_null_to_default_int_property()
        {
            var input = new Dictionary<string, object> { { "Fred", null } };
            var output = DictionaryMapper.Read<PropClass<int>>(input);
            Assert.IsNotNull(input);
            Assert.AreEqual(0, output.Fred);
        }

        [Test]
        public void can_map_nullable_int_property()
        {
            var input = new Dictionary<string, object> { { "Fred", 1 } };
            var output = DictionaryMapper.Read<PropClass<int?>>(input);
            Assert.IsNotNull(input);
            Assert.AreEqual(1, output.Fred);
        }

        [Test]
        public void can_map_null_to_nullable_int_property()
        {
            var input = new Dictionary<string, object> { { "Fred", null } };
            var output = DictionaryMapper.Read<PropClass<int?>>(input);
            Assert.IsNotNull(input);
            Assert.AreEqual(default(int?), output.Fred);
        }

        [Test]
        public void can_map_string_property()
        {
            var input = new Dictionary<string, object> { { "Fred", "one" } };
            var output = DictionaryMapper.Read<PropClass<string>>(input);
            Assert.IsNotNull(input);
            Assert.AreEqual("one", output.Fred);
        }

        [Test]
        public void can_map_null_string_property()
        {
            var input = new Dictionary<string, object> { { "Fred", null } };
            var output = DictionaryMapper.Read<PropClass<string>>(input);
            Assert.IsNotNull(input);
            Assert.AreEqual(null, output.Fred);
        }

        class PropClass<T>
        {
            public T Fred { get; set; }
        }
    }
}
