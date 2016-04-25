using NUnit.Framework;
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
