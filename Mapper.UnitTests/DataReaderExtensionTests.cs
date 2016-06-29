using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mapper.UnitTests
{
    [TestFixture]
    public class DataReaderExtensionTests
    {

        [Test]
        public void can_read_int_into_int()
        {
            var stubDataReader = new StubDataReader
            {
                Names = new []{ "ORDER_ID"},
                Types = new []{ typeof(int)},
                Values = new object[] {1},
            };
            var func = Extensions.GetMappingFunc<TestPropertyId>(stubDataReader);
            var result = func(stubDataReader);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.OrderId);
        }

        [Test]
        public void can_read_long_into_nullable_long()
        {
            var stubDataReader = new StubDataReader
            {
                Names = new []{ "ID"},
                Types = new []{ typeof(long)},
                Values = new object[] {1L},
            };
            var func = Extensions.GetMappingFunc<NullableLong>(stubDataReader);
            var result = func(stubDataReader);
            Assert.IsNotNull(result);
            Assert.AreEqual(1L, result.Id);
        }

        [Test]
        public void can_read_single_primative_type()
        {
            var stubDataReader = new StubDataReader
            {
                Names = new[] { "ID" },
                Types = new[] { typeof(long) },
                Values = new object[] { 1L },
            };
            var val = stubDataReader.AsSeqOf<long>().Single();
        }

        [Test]
        public void can_read_single_nullable_primative_type()
        {
            var stubDataReader = new StubDataReader
            {
                Names = new[] { "ID" },
                Types = new[] { typeof(long) },
                Values = new object[] { 1L },
            };
            var val = stubDataReader.AsSeqOf<long?>().Single();
        }

        [Test]
        public void can_read_single_enum()
        {
            var stubDataReader = new StubDataReader
            {
                Names = new[] { "ID" },
                Types = new[] { typeof(int) },
                Values = new object[] { 1 },
            };
            var val = stubDataReader.AsSeqOf<TestEnum>().Single();
        }

        [Test]
        public void can_read_single_nullable_enum()
        {
            var stubDataReader = new StubDataReader
            {
                Names = new[] { "ID" },
                Types = new[] { typeof(int) },
                Values = new object[] { 1 },
            };
            var val = stubDataReader.AsSeqOf<TestEnum?>().Single();
        }

        [Test]
        public async Task can_read_single_primative_type_async()
        {
            var stubDataReader = new StubDataReader
            {
                Names = new[] { "ID" },
                Types = new[] { typeof(long) },
                Values = new object[] { 1L },
            };
            await stubDataReader.AsSeqOf<long>().SingleAsync();
        }

        [Test]
        public void can_read_single_guid()
        {
            var g = Guid.NewGuid();
            var stubDataReader = new StubDataReader
            {
                Names = new[] { "ID" },
                Types = new[] { g.GetType() },
                Values = new object[] { g },
            };
            var read = stubDataReader.AsSeqOf<Guid>().Single();
            Assert.AreEqual(g, read);
        }

        [Test]
        public void can_read_single_datetime()
        {
            var dt = DateTime.Now;
            var stubDataReader = new StubDataReader
            {
                Names = new[] { "ID" },
                Types = new[] { dt.GetType() },
                Values = new object[] { dt },
            };
            var read = stubDataReader.AsSeqOf<DateTime>().Single();
            Assert.AreEqual(dt, read);
        }

        [Test]
        public void can_read_single_object_containing_guid()
        {
            var g = Guid.NewGuid();
            var stubDataReader = new StubDataReader
            {
                Names = new[] { "VALUE" },
                Types = new[] { g.GetType() },
                Values = new object[] { g },
            };
            var read = stubDataReader.AsSeqOf<Guid?>().Single();
            Assert.AreEqual(g, read.Value);
        }

        [Test]
        public void can_read_into_struct()
        {
            var stubDataReader = new StubDataReader
            {
                Names = new[] { "VALUE" },
                Types = new[] { typeof(long) },
                Values = new object[] { 1L },
            };
            var val = stubDataReader.AsSeqOf<TestStruct<long>>().Single();
            Assert.AreEqual(1L, val.Value);
        }

    }

    struct TestStruct<T>
    {
        public T Value { get; set; }
    }

    class TestSingle<T>
    {
        public T Value { get; set; }
    }

    class TestPropertyId
    {
        public int OrderId { get; set; }
    }

    class TestFieldId
    {
        public int OrderId;
    }

    class TestProperty
    {
        public int Order { get; set; }
    }

    class TestField
    {
        public int Order;
    }

    class NullableLong
    {
        public long? Id { get; set; }
    }
}
