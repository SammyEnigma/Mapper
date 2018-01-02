using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BusterWood.Mapper.UnitTests
{
    [TestFixture]
    public class DataReaderExtensionTests
    {

        [Test]
        public void can_read_int_into_int()
        {
            var stubDataReader = new StubDataReader
            {
                Names = new[] { "ORDER_ID" },
                Types = new[] { typeof(int) },
                Values = new object[] { 1 },
            };
            var func = Extensions.GetMappingFunc<TestPropertyId>(stubDataReader);
            var result = func(stubDataReader);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.OrderId);
        }

        [Test]
        public void can_read_single_or_default()
        {
            var stubDataReader = new StubDataReader
            {
                Names = new []{ "ORDER_ID"},
                Types = new []{ typeof(int)},
                Values = new object[] {1},
            };
            var val = stubDataReader.SingleOrDefault<int>();
            Assert.AreEqual(1, val);
        }

        [Test]
        public void can_read_default_value_for_single_or_default()
        {
            var stubDataReader = new StubDataReader
            {
                Names = new []{ "ORDER_ID"},
                Types = new []{ typeof(int)},
                Values = new object[] {1},
                RecordCount = 0,
            };
            var val = stubDataReader.SingleOrDefault<int>();
            Assert.AreEqual(default(int), val);
        }

        [Test]
        public void cannot_read_single_or_default_when_more_than_one_row()
        {
            var stubDataReader = new StubDataReader
            {
                Names = new []{ "ORDER_ID"},
                Types = new []{ typeof(int)},
                Values = new object[] {1},
                RecordCount = 2,
            };
            Assert.Throws<InvalidOperationException>(() => { stubDataReader.SingleOrDefault<int>(); });
        }

        [Test]
        public void cannot_read_default_value_for_single()
        {
            var stubDataReader = new StubDataReader
            {
                Names = new[] { "ORDER_ID" },
                Types = new[] { typeof(int) },
                Values = new object[] { 1 },
                RecordCount = 0,
            };
            Assert.Throws<InvalidOperationException>(() => { stubDataReader.Single<int>(); });
        }

        [Test]
        public void cannot_read_single_when_more_than_one_row()
        {
            var stubDataReader = new StubDataReader
            {
                Names = new []{ "ORDER_ID"},
                Types = new []{ typeof(int)},
                Values = new object[] {1},
                RecordCount = 2,
            };
            Assert.Throws<InvalidOperationException>(() => { stubDataReader.Single<int>(); });
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
            var val = stubDataReader.Single<long>();
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
            var val = stubDataReader.Single<long?>();
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
            var val = stubDataReader.Single<TestEnum>();
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
            var val = stubDataReader.Single<TestEnum?>();
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
            await stubDataReader.SingleAsync<long>();
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
            var read = stubDataReader.Single<Guid>();
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
            var read = stubDataReader.Single<DateTime>();
            Assert.AreEqual(dt, read);
        }

        [Test]
        public void can_read_single_DateTimeOffset()
        {
            var dt = DateTimeOffset.Now;
            var stubDataReader = new StubDataReader
            {
                Names = new[] { "ID" },
                Types = new[] { dt.GetType() },
                Values = new object[] { dt },
            };
            var read = stubDataReader.Single<DateTimeOffset>();
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
            var read = stubDataReader.Single<Guid?>();
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
            var val = stubDataReader.Single<TestStruct<long>>();
            Assert.AreEqual(1L, val.Value);
        }


        [Test]
        public void can_read_single_via_explicit_operator()
        {
            var stubDataReader = new StubDataReader
            {
                Names = new[] { "VALUE" },
                Types = new[] { typeof(int) },
                Values = new object[] { 1 },
            };
            var val = stubDataReader.Single<Id<Order>>();
            Assert.AreEqual((Id<Order>)1, val);
        }

        [Test]
        public void can_read_single_nullable_via_explicit_operator()
        {
            var stubDataReader = new StubDataReader
            {
                Names = new[] { "VALUE" },
                Types = new[] { typeof(int) },
                Values = new object[] { 1 },
            };
            var val = stubDataReader.Single<Id<Order>?>();
            Assert.AreEqual((Id<Order>)1, val.Value);
        }

        [Test]
        public void can_convert_to_dictionary()
        {
            var stubDataReader = new StubDataReader
            {
                Names = new[] { "ID", "Name" },
                Types = new[] { typeof(int), typeof(string) },
                Values = new object[] { 1, "hello" },
            };
            var val = stubDataReader.ToDictionary<int, Order>(ord => ord.Id);
            Assert.AreEqual(true, val.ContainsKey(1));
            Assert.AreEqual(1, val[1].Id);
            Assert.AreEqual("hello", val[1].Name);
            Assert.AreEqual(1, val.Count, "count");
        }

        [Test]
        public void can_convert_to_dictionary_and_change_type()
        {
            var stubDataReader = new StubDataReader
            {
                Names = new[] { "ID", "Name" },
                Types = new[] { typeof(int), typeof(string) },
                Values = new object[] { 1, "hello" },
            };
            var val = stubDataReader.ToDictionary<Order, int, string>(ord => ord.Id, ord => ord.Name);
            Assert.AreEqual(true, val.ContainsKey(1));
            Assert.AreEqual("hello", val[1]);
            Assert.AreEqual(1, val.Count, "count");
        }

        [Test]
        public void can_convert_to_lookup()
        {
            var stubDataReader = new StubDataReader
            {
                Names = new[] { "ID", "Name" },
                Types = new[] { typeof(int), typeof(string) },
                Values = new object[] { 1, "hello" },
            };
            var val = stubDataReader.ToLookup<int, Order>(ord => ord.Id);
            Assert.AreEqual(1, val[1].Count);
            Assert.AreEqual(1, val[1].First().Id);
            Assert.AreEqual("hello", val[1].First().Name);
        }

        [Test]
        public void can_convert_to_lookup_and_change_type()
        {
            var stubDataReader = new StubDataReader
            {
                Names = new[] { "ID", "Name" },
                Types = new[] { typeof(int), typeof(string) },
                Values = new object[] { 1, "hello" },
            };
            var val = stubDataReader.ToLookup<Order, int, string>(ord => ord.Id, ord => ord.Name);
            Assert.AreEqual(1, val[1].Count);
            Assert.AreEqual("hello", val[1].First());
            Assert.AreEqual(1, val.Count, "count");
        }

        public async Task can_convert_to_dictionary_async()
        {
            var stubDataReader = new StubDataReader
            {
                Names = new[] { "ID", "Name" },
                Types = new[] { typeof(int), typeof(string) },
                Values = new object[] { 1, "hello" },
            };
            var val = await stubDataReader.ToDictionaryAsync<int, Order>(ord => ord.Id);
            Assert.AreEqual(true, val.ContainsKey(1));
            Assert.AreEqual(1, val[1].Id);
            Assert.AreEqual("hello", val[1].Name);
            Assert.AreEqual(1, val.Count, "count");
        }

        [Test]
        public async Task can_convert_to_dictionary_and_change_type_async()
        {
            var stubDataReader = new StubDataReader
            {
                Names = new[] { "ID", "Name" },
                Types = new[] { typeof(int), typeof(string) },
                Values = new object[] { 1, "hello" },
            };
            var val = await stubDataReader.ToDictionaryAsync<Order, int, string>(ord => ord.Id, ord => ord.Name);
            Assert.AreEqual(true, val.ContainsKey(1));
            Assert.AreEqual("hello", val[1]);
            Assert.AreEqual(1, val.Count, "count");
        }

        [Test]
        public async Task can_convert_to_lookup_async()
        {
            var stubDataReader = new StubDataReader
            {
                Names = new[] { "ID", "Name" },
                Types = new[] { typeof(int), typeof(string) },
                Values = new object[] { 1, "hello" },
            };
            var val = await stubDataReader.ToLookupAsync<int, Order>(ord => ord.Id);
            Assert.AreEqual(1, val[1].Count);
            Assert.AreEqual(1, val[1].First().Id);
            Assert.AreEqual("hello", val[1].First().Name);
        }

        [Test]
        public async Task can_convert_to_lookup_and_change_type_async()
        {
            var stubDataReader = new StubDataReader
            {
                Names = new[] { "ID", "Name" },
                Types = new[] { typeof(int), typeof(string) },
                Values = new object[] { 1, "hello" },
            };
            var val = await stubDataReader.ToLookupAsync<Order, int, string>(ord => ord.Id, ord => ord.Name);
            Assert.AreEqual(1, val[1].Count);
            Assert.AreEqual("hello", val[1].First());
            Assert.AreEqual(1, val.Count, "count");
        }

        [Test]
        public void can_read_int_into_enum()
        {
            var stubDataReader = new StubDataReader
            {
                Names = new[] { "ID", "BOOKING_STATE" },
                Types = new[] { typeof(int), typeof(int) },
                Values = new object[] { 1, 2 },
            };
            var func = Extensions.GetMappingFunc<Booking>(stubDataReader);
            var result = func(stubDataReader);
            Assert.IsNotNull(result);
            Assert.AreEqual(State.Second, result.State);
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

    class Order
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    class Booking
    {
        public int Id { get; set; }
        public State State { get; set; }
    }

    enum State
    {
        First = 1,
        Second,
    }

    /// for testing explicit conversions
    struct Id<T>
    {
        public Id(int id)
        {
            Value = id;
        }
        public int Value { get; }
        public static explicit operator int(Id<T> id) => id.Value;
        public static explicit operator Id<T>(int id) => new Id<T>(id);
    }


}
