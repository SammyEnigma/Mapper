using NUnit.Framework;

namespace Mapper.UnitTests.MappingTests
{
    [TestFixture]
    public class UsingSource
    {
        [TestCase("OrderId")]
        [TestCase("ORDERID")]
        [TestCase("ORDER_ID")]
        [TestCase("Order_Id")]
        public void maps_column_to_property(string colName)
        {
            var col = new Column(0, colName, typeof(int));
            var mapping = Mapping.CreateUsingSource(new[] { col }, new TestPropertyId { OrderId = 1 }.GetType());
            Assert.AreEqual(1, mapping.Count, "count");
            var map = mapping[0];
            Assert.AreEqual(col, map.From);
            Assert.IsNotNull(map.To);
        }

        [TestCase("OrderId")]
        [TestCase("ORDERID")]
        [TestCase("ORDER_ID")]
        [TestCase("Order_Id")]
        public void maps_column_to_field(string colName)
        {
            var col = new Column(0, colName, typeof(int));
            var mapping = Mapping.CreateUsingSource(new[] { col }, new TestFieldId { OrderId = 1 }.GetType());
            Assert.AreEqual(1, mapping.Count, "count");
            var map = mapping[0];
            Assert.AreEqual(col, map.From);
            Assert.IsNotNull(map.To);
        }

        [TestCase("OrderId")]
        [TestCase("ORDERID")]
        [TestCase("ORDER_ID")]
        [TestCase("Order_Id")]
        [TestCase("Order")]
        [TestCase("ORDER")]
        public void maps_column_to_property_without_id_suffix(string colName)
        {
            var col = new Column(0, colName, typeof(int));
            var mapping = Mapping.CreateUsingSource(new[] { col }, new TestProperty { Order = 1 }.GetType());
            Assert.AreEqual(1, mapping.Count, "count");
            var map = mapping[0];
            Assert.AreEqual(col, map.From);
            Assert.IsNotNull(map.To);
        }

        [TestCase("OrderId")]
        [TestCase("ORDERID")]
        [TestCase("ORDER_ID")]
        [TestCase("Order_Id")]
        [TestCase("Order")]
        [TestCase("ORDER")]
        public void maps_column_to_field_without_id_suffix(string colName)
        {
            var col = new Column(0, colName, typeof(int));
            var mapping = Mapping.CreateUsingSource(new[] { col }, new TestField { Order = 1 }.GetType());
            Assert.AreEqual(1, mapping.Count, "count");
            var map = mapping[0];
            Assert.AreEqual(col, map.From);
            Assert.IsNotNull(map.To);
        }
    }

    [TestFixture]
    public class UsingDestination
    {
        [TestCase("OrderId")]
        [TestCase("ORDERID")]
        [TestCase("ORDER_ID")]
        [TestCase("Order_Id")]
        public void maps_column_to_property(string colName)
        {
            var col = new Column(0, colName, typeof(int));
            var mapping = Mapping.CreateUsingDestination(new TestPropertyId { OrderId = 1 }.GetType(), new[] { col });
            Assert.AreEqual(1, mapping.Count, "count");
            var map = mapping[0];
            Assert.AreEqual(col, map.To);
            Assert.IsNotNull(map.From);
        }

        [TestCase("OrderId")]
        [TestCase("ORDERID")]
        [TestCase("ORDER_ID")]
        [TestCase("Order_Id")]
        public void maps_column_to_field(string colName)
        {
            var col = new Column(0, colName, typeof(int));
            var mapping = Mapping.CreateUsingDestination(new TestFieldId { OrderId = 1 }.GetType(), new[] { col });
            Assert.AreEqual(1, mapping.Count, "count");
            var map = mapping[0];
            Assert.AreEqual(col, map.To);
            Assert.IsNotNull(map.From);
        }

        [TestCase("OrderId")]
        [TestCase("ORDERID")]
        [TestCase("ORDER_ID")]
        [TestCase("Order_Id")]
        [TestCase("Order")]
        [TestCase("ORDER")]
        public void maps_column_to_property_without_id_suffix(string colName)
        {
            var col = new Column(0, colName, typeof(int));
            var mapping = Mapping.CreateUsingDestination(new TestProperty { Order = 1 }.GetType(), new[] { col });
            Assert.AreEqual(1, mapping.Count, "count");
            var map = mapping[0];
            Assert.AreEqual(col, map.To);
            Assert.IsNotNull(map.From);
        }

        [TestCase("OrderId")]
        [TestCase("ORDERID")]
        [TestCase("ORDER_ID")]
        [TestCase("Order_Id")]
        [TestCase("Order")]
        [TestCase("ORDER")]
        public void maps_column_to_field_without_id_suffix(string colName)
        {
            var col = new Column(0, colName, typeof(int));
            var mapping = Mapping.CreateUsingDestination(new TestField { Order = 1 }.GetType(), new[] { col });
            Assert.AreEqual(1, mapping.Count, "count");
            var map = mapping[0];
            Assert.AreEqual(col, map.To);
            Assert.IsNotNull(map.From);
        }
    }
}
