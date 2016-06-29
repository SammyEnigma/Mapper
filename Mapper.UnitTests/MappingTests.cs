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
            using (Mapping.Trace.Subscribe(new ConsoleObserver()))
            {
                var col = new Column(0, colName, typeof(int));
                var mapping = Mapping.CreateUsingSource(new Thing[] { col }, new TestPropertyId { OrderId = 1 }.GetType());
                Assert.AreEqual(1, mapping.Count, "count");
                var map = mapping[0];
                Assert.AreEqual(col, map.From);
                Assert.IsNotNull(map.To);
            }
        }

        [TestCase("OrderId")]
        [TestCase("ORDERID")]
        [TestCase("ORDER_ID")]
        [TestCase("Order_Id")]
        public void maps_column_to_field(string colName)
        {
            using (Mapping.Trace.Subscribe(new ConsoleObserver()))
            {
                var col = new Column(0, colName, typeof(int));
                var mapping = Mapping.CreateUsingSource(new Thing[] { col }, new TestFieldId { OrderId = 1 }.GetType());
                Assert.AreEqual(1, mapping.Count, "count");
                var map = mapping[0];
                Assert.AreEqual(col, map.From);
                Assert.IsNotNull(map.To);
            }
        }

        [TestCase("OrderId")]
        [TestCase("ORDERID")]
        [TestCase("ORDER_ID")]
        [TestCase("Order_Id")]
        [TestCase("Order")]
        [TestCase("ORDER")]
        public void maps_column_to_property_without_id_suffix(string colName)
        {
            using (Mapping.Trace.Subscribe(new ConsoleObserver()))
            {
                var col = new Column(0, colName, typeof(int));
                var mapping = Mapping.CreateUsingSource(new Thing[] { col }, new TestProperty { Order = 1 }.GetType());
                Assert.AreEqual(1, mapping.Count, "count");
                var map = mapping[0];
                Assert.AreEqual(col, map.From);
                Assert.IsNotNull(map.To);
            }
        }

        [TestCase("OrderId")]
        [TestCase("ORDERID")]
        [TestCase("ORDER_ID")]
        [TestCase("Order_Id")]
        [TestCase("Order")]
        [TestCase("ORDER")]
        public void maps_column_to_field_without_id_suffix(string colName)
        {
            using (Mapping.Trace.Subscribe(new ConsoleObserver()))
            {
                var col = new Column(0, colName, typeof(int));
                var mapping = Mapping.CreateUsingSource(new Thing[] { col }, new TestField { Order = 1 }.GetType());
                Assert.AreEqual(1, mapping.Count, "count");
                var map = mapping[0];
                Assert.AreEqual(col, map.From);
                Assert.IsNotNull(map.To);
            }
        }

        [Test]
        public void trace_logs_things_not_mapped_including_prefix()
        {
            var ob = new ConsoleObserver();
            using (Mapping.Trace.Subscribe(ob))
            {
                Mapping.CreateUsingSource(typeof(SingleProp<string>), typeof(SingleProp<int>), "Things");
                Assert.AreEqual(1, ob.Values.Count, "count");
                Assert.IsTrue(ob.Values[0].StartsWith("Things: "));
            }
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
            using (Mapping.Trace.Subscribe(new ConsoleObserver()))
            {
                var col = new Column(0, colName, typeof(int));
                var mapping = Mapping.CreateUsingDestination(new TestPropertyId { OrderId = 1 }.GetType(), new Thing[] { col });
                Assert.AreEqual(1, mapping.Count, "count");
                var map = mapping[0];
                Assert.AreEqual(col, map.To);
                Assert.IsNotNull(map.From);
            }
        }

        [TestCase("OrderId")]
        [TestCase("ORDERID")]
        [TestCase("ORDER_ID")]
        [TestCase("Order_Id")]
        public void maps_column_to_field(string colName)
        {
            using (Mapping.Trace.Subscribe(new ConsoleObserver()))
            {
                var col = new Column(0, colName, typeof(int));
                var mapping = Mapping.CreateUsingDestination(new TestFieldId { OrderId = 1 }.GetType(), new Thing[] { col });
                Assert.AreEqual(1, mapping.Count, "count");
                var map = mapping[0];
                Assert.AreEqual(col, map.To);
                Assert.IsNotNull(map.From);
            }
        }

        [TestCase("OrderId")]
        [TestCase("ORDERID")]
        [TestCase("ORDER_ID")]
        [TestCase("Order_Id")]
        [TestCase("Order")]
        [TestCase("ORDER")]
        public void maps_column_to_property_without_id_suffix(string colName)
        {
            using (Mapping.Trace.Subscribe(new ConsoleObserver()))
            {
                var col = new Column(0, colName, typeof(int));
                var mapping = Mapping.CreateUsingDestination(new TestProperty { Order = 1 }.GetType(), new Thing[] { col });
                Assert.AreEqual(1, mapping.Count, "count");
                var map = mapping[0];
                Assert.AreEqual(col, map.To);
                Assert.IsNotNull(map.From);
            }
        }

        [TestCase("OrderId")]
        [TestCase("ORDERID")]
        [TestCase("ORDER_ID")]
        [TestCase("Order_Id")]
        [TestCase("Order")]
        [TestCase("ORDER")]
        public void maps_column_to_field_without_id_suffix(string colName)
        {
            using (Mapping.Trace.Subscribe(new ConsoleObserver()))
            {
                var col = new Column(0, colName, typeof(int));
                var mapping = Mapping.CreateUsingDestination(new TestField { Order = 1 }.GetType(), new Thing[] { col });
                Assert.AreEqual(1, mapping.Count, "count");
                var map = mapping[0];
                Assert.AreEqual(col, map.To);
                Assert.IsNotNull(map.From);
            }
        }

        [Test]
        public void trace_logs_things_not_mapped_including_prefix()
        {
            var ob = new ConsoleObserver();
            using (Mapping.Trace.Subscribe(ob))
            {
                Mapping.CreateUsingDestination(typeof(SingleProp<string>), typeof(SingleProp<int>), "Things");
                Assert.AreEqual(1, ob.Values.Count, "count");
                Assert.IsTrue(ob.Values[0].StartsWith("Things: "));
            }
        }
    }
}
