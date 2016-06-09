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
        [TestCase("OrderId")]
        [TestCase("ORDERID")]
        [TestCase("ORDER_ID")]
        [TestCase("Order_Id")]
        public void maps_property_to_column(string colName)
        {
            var col = new Column(0, colName, typeof(int));
            var map = DataReaderMapper.CreateMemberToColumnMap(new[] { col }, new TestPropertyId { OrderId = 1 }.GetType());
            Assert.AreEqual(1, map.Count, "count");
            var pair = map.Single();
            Assert.IsNotNull(pair.Key);
            Assert.AreEqual(col, pair.Value);
        }

        [TestCase("OrderId")]
        [TestCase("ORDERID")]
        [TestCase("ORDER_ID")]
        [TestCase("Order_Id")]
        public void maps_field_to_column(string colName)
        {
            var col = new Column(0, colName, typeof(int));
            var map = DataReaderMapper.CreateMemberToColumnMap(new[] { col }, new TestFieldId{ OrderId = 1 }.GetType());
            Assert.AreEqual(1, map.Count, "count");
            var pair = map.Single();
            Assert.IsNotNull(pair.Key);
            Assert.AreEqual(col, pair.Value);
        }

        [TestCase("OrderId")]
        [TestCase("ORDERID")]
        [TestCase("ORDER_ID")]
        [TestCase("Order_Id")]
        [TestCase("Order")]
        [TestCase("ORDER")]
        public void maps_property_to_column_without_id(string colName)
        {
            var col = new Column(0, colName, typeof(int));
            var map = DataReaderMapper.CreateMemberToColumnMap(new[] { col }, new TestProperty { Order = 1 }.GetType());
            Assert.AreEqual(1, map.Count, "count");
            var pair = map.Single();
            Assert.IsNotNull(pair.Key);
            Assert.AreEqual(col, pair.Value);
        }

        [TestCase("OrderId")]
        [TestCase("ORDERID")]
        [TestCase("ORDER_ID")]
        [TestCase("Order_Id")]
        [TestCase("Order")]
        [TestCase("ORDER")]
        public void maps_field_to_column_without_id(string colName)
        {
            var col = new Column(0, colName, typeof(int));
            var map = DataReaderMapper.CreateMemberToColumnMap(new[] { col }, new TestField { Order = 1 }.GetType());
            Assert.AreEqual(1, map.Count, "count");
            var pair = map.Single();
            Assert.IsNotNull(pair.Key);
            Assert.AreEqual(col, pair.Value);
        }

        [Test]
        public void can_read_int_into_int()
        {
            var stubDataReader = new StubDataReader
            {
                Names = new []{ "ORDER_ID"},
                Types = new []{ typeof(int)},
                Values = new object[] {1},
            };
            var func = DataReaderExtensions.GetMappingFunc<TestPropertyId>(stubDataReader);
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
            var func = DataReaderExtensions.GetMappingFunc<NullableLong>(stubDataReader);
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
            var val = stubDataReader.ReadSingle<long>();
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
            var val = stubDataReader.ReadSingle<long?>();
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
            var val = stubDataReader.ReadSingle<TestEnum>();
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
            var val = stubDataReader.ReadSingle<TestEnum?>();
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
            await stubDataReader.ReadSingleAsync<int>();
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
            var read = stubDataReader.ReadSingle<Guid>();
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
            var read = stubDataReader.ReadSingle<DateTime>();
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
            var read = stubDataReader.ReadSingle<TestSingle<Guid>>();
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
            var val = stubDataReader.ReadSingle<TestStruct<long>>();
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
