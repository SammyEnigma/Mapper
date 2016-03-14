using System;
using System.Data;
using System.Linq;
using Microsoft.SqlServer.Server;
using NUnit.Framework;

namespace Mapper.UnitTests
{
    [TestFixture]
    public class SqlDataRecordExtensionTests
    {
        [Test]
        public void can_map_string_to_varchar()
        { 
            var input = new SingleProp<string> {Value = "fred"};
            var meta = new[] {new SqlMetaData("VALUE", SqlDbType.VarChar, 10)};
            var recs = new[] {input}.ToDataRecords(meta).ToList();
            Assert.AreEqual(1, recs.Count, "Count");
            var rec = recs[0];
            Assert.NotNull(rec);
            Assert.AreEqual(1, rec.FieldCount);
            Assert.AreEqual("fred", rec.GetValue(0));
        }

        [Test]
        public void can_map_string_to_nvarchar()
        {
            var input = new SingleProp<string> { Value = "fred" };
            var meta = new[] { new SqlMetaData("VALUE", SqlDbType.NVarChar, 10) };
            var recs = new[] { input }.ToDataRecords(meta).ToList();
            Assert.AreEqual(1, recs.Count, "Count");
            var rec = recs[0];
            Assert.NotNull(rec);
            Assert.AreEqual(1, rec.FieldCount);
            Assert.AreEqual("fred", rec.GetValue(0));
        }

        [Test]
        public void can_map_string_to_fix_length_char()
        {
            var input = new SingleProp<string> { Value = "fred" };
            var meta = new[] { new SqlMetaData("VALUE", SqlDbType.Char, 10) };
            var recs = new[] { input }.ToDataRecords(meta).ToList();
            Assert.AreEqual(1, recs.Count, "Count");
            var rec = recs[0];
            Assert.NotNull(rec);
            Assert.AreEqual(1, rec.FieldCount);
            Assert.AreEqual("fred", rec.GetValue(0));
        }


        [Test]
        public void can_map_string_to_fix_length_nchar()
        {
            var input = new SingleProp<string> { Value = "fred" };
            var meta = new[] { new SqlMetaData("VALUE", SqlDbType.NChar, 10) };
            var recs = new[] { input }.ToDataRecords(meta).ToList();
            Assert.AreEqual(1, recs.Count, "Count");
            var rec = recs[0];
            Assert.NotNull(rec);
            Assert.AreEqual(1, rec.FieldCount);
            Assert.AreEqual("fred", rec.GetValue(0));
        }

        [Test]
        public void can_map_null_string()
        {
            var input = new SingleProp<string> { Value = null };
            var meta = new[] { new SqlMetaData("VALUE", SqlDbType.VarChar, 10) };
            var recs = new[] { input }.ToDataRecords(meta).ToList();
            Assert.AreEqual(1, recs.Count, "Count");
            Assert.AreEqual(true, recs[0].IsDBNull(0));
        }

        [Test]
        public void can_map_boolean()
        {
            var input = new SingleProp<bool> { Value = true };
            var meta = new[] { new SqlMetaData("VALUE", SqlDbType.Bit) };
            var recs = new[] { input }.ToDataRecords(meta).ToList();
            Assert.AreEqual(1, recs.Count, "Count");
            Assert.AreEqual(true, recs[0].GetValue(0));
            Assert.AreEqual(1, recs[0].FieldCount);
        }

        [Test]
        public void can_map_nullable_boolean()
        {
            var input = new SingleProp<bool?> { Value = true };
            var meta = new[] { new SqlMetaData("VALUE", SqlDbType.Bit) };
            var recs = new[] { input }.ToDataRecords(meta).ToList();
            Assert.AreEqual(1, recs.Count, "Count");
            Assert.AreEqual(true, recs[0].GetValue(0));
            Assert.AreEqual(1, recs[0].FieldCount);
        }

        [Test]
        public void can_map_null_boolean()
        {
            var input = new SingleProp<bool?> { Value = null };
            var meta = new[] { new SqlMetaData("VALUE", SqlDbType.Bit) };
            var recs = new[] { input }.ToDataRecords(meta).ToList();
            Assert.AreEqual(1, recs.Count, "Count");
            Assert.AreEqual(true, recs[0].IsDBNull(0));
        }

        [Test]
        public void can_map_byte()
        {
            var input = new SingleProp<byte> { Value = 1 };
            var meta = new[] { new SqlMetaData("VALUE", SqlDbType.TinyInt) };
            var recs = new[] { input }.ToDataRecords(meta).ToList();
            Assert.AreEqual(1, recs.Count, "Count");
            Assert.AreEqual((byte)1, recs[0].GetValue(0));
            Assert.AreEqual(1, recs[0].FieldCount);
        }

        [Test]
        public void can_map_nullable_byte()
        {
            var input = new SingleProp<byte?> { Value = 1 };
            var meta = new[] { new SqlMetaData("VALUE", SqlDbType.TinyInt) };
            var recs = new[] { input }.ToDataRecords(meta).ToList();
            Assert.AreEqual(1, recs.Count, "Count");
            Assert.AreEqual((byte)1, recs[0].GetValue(0));
            Assert.AreEqual(1, recs[0].FieldCount);
        }

        [Test]
        public void can_map_null_byte()
        {
            var input = new SingleProp<byte?> { Value = null };
            var meta = new[] { new SqlMetaData("VALUE", SqlDbType.TinyInt) };
            var recs = new[] { input }.ToDataRecords(meta).ToList();
            Assert.AreEqual(1, recs.Count, "Count");
            Assert.AreEqual(true, recs[0].IsDBNull(0));
            Assert.AreEqual(1, recs[0].FieldCount);
        }

        [Test]
        public void can_map_short()
        {
            var input = new SingleProp<short> { Value = 1 };
            var meta = new[] { new SqlMetaData("VALUE", SqlDbType.SmallInt) };
            var recs = new[] { input }.ToDataRecords(meta).ToList();
            Assert.AreEqual(1, recs.Count, "Count");
            Assert.AreEqual((short)1, recs[0].GetValue(0));
            Assert.AreEqual(1, recs[0].FieldCount);
        }

        [Test]
        public void can_map_int()
        {
            var input = new SingleProp<int> { Value = 1 };
            var meta = new[] { new SqlMetaData("VALUE", SqlDbType.Int) };
            var recs = new[] { input }.ToDataRecords(meta).ToList();
            Assert.AreEqual(1, recs.Count, "Count");
            Assert.AreEqual(1, recs[0].FieldCount);
            Assert.AreEqual(1, recs[0].GetValue(0));
        }

        [Test]
        public void can_map_to_long()
        {
            var input = new SingleProp<int> { Value = 1 };
            var meta = new[] { new SqlMetaData("VALUE", SqlDbType.BigInt) };
            var recs = new[] { input }.ToDataRecords(meta).ToList();
            Assert.AreEqual(1, recs.Count, "Count");
            Assert.AreEqual(1, recs[0].FieldCount);
            Assert.AreEqual(1L, recs[0].GetValue(0));
        }

        [Test]
        public void can_map_long()
        {
            var input = new SingleProp<long> { Value = 1 };
            var meta = new[] { new SqlMetaData("VALUE", SqlDbType.BigInt) };
            var recs = new[] { input }.ToDataRecords(meta).ToList();
            Assert.AreEqual(1L, recs[0].GetValue(0));
            Assert.AreEqual(1, recs[0].FieldCount);
        }

        [Test]
        public void can_map_float()
        {
            var input = new SingleProp<float> { Value = 1f };
            var meta = new[] { new SqlMetaData("VALUE", SqlDbType.Real) };
            var recs = new[] { input }.ToDataRecords(meta).ToList();
            Assert.AreEqual(1f, recs[0].GetValue(0));
            Assert.AreEqual(1, recs[0].FieldCount);
        }

        [Test]
        public void can_map_double()
        {
            var input = new SingleProp<double> { Value = 1.111d };
            var meta = new[] { new SqlMetaData("VALUE", SqlDbType.Float) };
            var recs = new[] { input }.ToDataRecords(meta).ToList();
            Assert.AreEqual(1.111d, recs[0].GetValue(0));
            Assert.AreEqual(1, recs[0].FieldCount);
        }

        [Test]
        public void can_map_decimal()
        {
            var input = new SingleProp<decimal> { Value = 1.111m };
            var meta = new[] { new SqlMetaData("VALUE", SqlDbType.Decimal) };
            var recs = new[] { input }.ToDataRecords(meta).ToList();
            Assert.AreEqual(1.111m, recs[0].GetValue(0));
            Assert.AreEqual(1, recs[0].FieldCount);
        }

        [Test]
        public void can_map_nullable_decimal()
        {
            var input = new SingleProp<decimal?> { Value = 1.111m };
            var meta = new[] { new SqlMetaData("VALUE", SqlDbType.Decimal) };
            var recs = new[] { input }.ToDataRecords(meta).ToList();
            Assert.AreEqual(1.111m, recs[0].GetValue(0));
            Assert.AreEqual(1, recs[0].FieldCount);
        }

        [Test]
        public void can_map_null_decimal()
        {
            var input = new SingleProp<decimal?> { Value = null };
            var meta = new[] { new SqlMetaData("VALUE", SqlDbType.Decimal) };
            var recs = new[] { input }.ToDataRecords(meta).ToList();
            Assert.AreEqual(true, recs[0].IsDBNull(0));
            Assert.AreEqual(1, recs[0].FieldCount);
        }


        [Test]
        public void can_map_datetime()
        {
            var input = new SingleProp<DateTime> { Value = DateTime.Now };
            var meta = new[] { new SqlMetaData("VALUE", SqlDbType.DateTime) };
            var recs = new[] { input }.ToDataRecords(meta).ToList();
            Assert.AreEqual(1, recs.Count, "Count");
            Assert.AreEqual(input.Value, recs[0].GetValue(0));
            Assert.AreEqual(1, recs[0].FieldCount);
        }

        [Test]
        public void can_map_nullable_datetime()
        {
            var input = new SingleProp<DateTime?> { Value = DateTime.Now };
            var meta = new[] { new SqlMetaData("VALUE", SqlDbType.DateTime) };
            var recs = new[] { input }.ToDataRecords(meta).ToList();
            Assert.AreEqual(1, recs.Count, "Count");
            Assert.AreEqual(input.Value.Value, recs[0].GetValue(0));
            Assert.AreEqual(1, recs[0].FieldCount);
        }

        [Test]
        public void can_map_null_datetime()
        {
            var input = new SingleProp<DateTime?> { Value = null };
            var meta = new[] { new SqlMetaData("VALUE", SqlDbType.DateTime) };
            var recs = new[] { input }.ToDataRecords(meta).ToList();
            Assert.AreEqual(1, recs.Count, "Count");
            Assert.AreEqual(true, recs[0].IsDBNull(0));
            Assert.AreEqual(1, recs[0].FieldCount);
        }

        [Test]
        public void can_map_enum_to_int()
        {
            var input = new SingleProp<TestEnum> { Value = TestEnum.Something };
            var meta = new[] { new SqlMetaData("VALUE", SqlDbType.Int) };
            var recs = new[] { input }.ToDataRecords(meta).ToList();
            Assert.AreEqual(1, recs.Count, "Count");
            Assert.AreEqual(1, recs[0].GetInt32(0));
            Assert.AreEqual(1, recs[0].FieldCount);
        }

        [Test]
        public void cannot_map_custom_type_so_null_value_instead()
        {
            var input = new SingleProp<Stuff> { Value = new Stuff() };
            var meta = new[] { new SqlMetaData("VALUE", SqlDbType.Int) };
            var recs = new[] { input }.ToDataRecords(meta).ToList();
            Assert.AreEqual(1, recs.Count, "Count");
            Assert.AreEqual(1, recs[0].FieldCount);
            Assert.IsTrue(recs[0].IsDBNull(0));
        }

        [Test]
        public void can_map_multiple_properties()
        {
            var input = new MultipleProperties { Int = 1, Long=2, Boolean=true};
            var meta = new[] {
                new SqlMetaData("int", SqlDbType.Int),
                new SqlMetaData("LOng", SqlDbType.BigInt),
                new SqlMetaData("BOOLEAN", SqlDbType.Bit),
            };
            var recs = new[] { input }.ToDataRecords(meta).ToList();
            Assert.AreEqual(1, recs.Count, "Count");
            Assert.AreEqual(1, recs[0].GetValue(0));
            Assert.AreEqual(2, recs[0].GetValue(1));
            Assert.AreEqual(true, recs[0].GetValue(2));
            Assert.AreEqual(3, recs[0].FieldCount);
        }

        [Test]
        public void can_map_multiple_recs()
        {
            var input = new[] {
                new MultipleProperties { Int = 1, Long = 10, Boolean = true },
                new MultipleProperties { Int = 2, Long = 20, Boolean = false },
            };
            var meta = new[] {
                new SqlMetaData("int", SqlDbType.Int),
                new SqlMetaData("LOng", SqlDbType.BigInt),
                new SqlMetaData("BOOLEAN", SqlDbType.Bit),
            };
            var recs = input.ToDataRecords(meta).ToList();
            Assert.AreEqual(2, recs.Count, "Count");

            Assert.AreEqual(1, recs[0].GetValue(0));
            Assert.AreEqual(10, recs[0].GetValue(1));
            Assert.AreEqual(true, recs[0].GetValue(2));
            Assert.AreEqual(3, recs[0].FieldCount);

            Assert.AreEqual(2, recs[1].GetValue(0));
            Assert.AreEqual(20, recs[1].GetValue(1));
            Assert.AreEqual(false, recs[1].GetValue(2));
            Assert.AreEqual(3, recs[1].FieldCount);
        }

        class MultipleProperties
        {
            public int Int { get; set; }
            public long Long { get; set; }
            public bool Boolean { get; set; }
        }

        class Stuff { }
    }
}