using System;
using System.Data;
using System.Data.SqlClient;
using Microsoft.SqlServer.Server;
using NUnit.Framework;

namespace BusterWood.Mapper.UnitTests
{
    [TestFixture]
    public class CommandExtensionTests
    {
        [Test]
        public void can_add_string_with_value()
        {
            SqlCommand cmd = new SqlCommand();
            cmd.AddParameters(new {Code = "fred"});
            Assert.AreEqual(1, cmd.Parameters.Count);
            Assert.AreEqual("@Code", cmd.Parameters[0].ParameterName);
            Assert.AreEqual(DbType.String, cmd.Parameters[0].DbType);
            Assert.AreEqual("fred", cmd.Parameters[0].Value);
        }

        [Test]
        public void can_add_string_null()
        {
            SqlCommand cmd = new SqlCommand();
            cmd.AddParameters(new {Code = (string)null});
            Assert.AreEqual(1, cmd.Parameters.Count);
            Assert.AreEqual("@Code", cmd.Parameters[0].ParameterName);
            Assert.AreEqual(DbType.String, cmd.Parameters[0].DbType);
            Assert.AreEqual(DBNull.Value, cmd.Parameters[0].Value);
        }

        [Test]
        public void can_add_byte_with_value()
        {
            SqlCommand cmd = new SqlCommand();
            cmd.AddParameters(new {Id = (byte)1});
            Assert.AreEqual(1, cmd.Parameters.Count);
            Assert.AreEqual("@Id", cmd.Parameters[0].ParameterName);
            Assert.AreEqual(DbType.Byte, cmd.Parameters[0].DbType);
            Assert.AreEqual(1, cmd.Parameters[0].Value);
        }

        [Test]
        public void can_add_short_with_value()
        {
            SqlCommand cmd = new SqlCommand();
            cmd.AddParameters(new {Id = (short)1});
            Assert.AreEqual(1, cmd.Parameters.Count);
            Assert.AreEqual("@Id", cmd.Parameters[0].ParameterName);
            Assert.AreEqual(DbType.Int16, cmd.Parameters[0].DbType);
            Assert.AreEqual(1, cmd.Parameters[0].Value);
        }

        [Test]
        public void can_add_int_with_value()
        {
            SqlCommand cmd = new SqlCommand();
            cmd.AddParameters(new {Id = 1});
            Assert.AreEqual(1, cmd.Parameters.Count);
            Assert.AreEqual("@Id", cmd.Parameters[0].ParameterName);
            Assert.AreEqual(DbType.Int32, cmd.Parameters[0].DbType);
            Assert.AreEqual(1, cmd.Parameters[0].Value);
        }

        [Test]
        public void can_add_long_with_value()
        {
            SqlCommand cmd = new SqlCommand();
            cmd.AddParameters(new {Id = (long)1});
            Assert.AreEqual(1, cmd.Parameters.Count);
            Assert.AreEqual("@Id", cmd.Parameters[0].ParameterName);
            Assert.AreEqual(DbType.Int64, cmd.Parameters[0].DbType);
            Assert.AreEqual(1, cmd.Parameters[0].Value);
        }

        [Test]
        public void can_add_nullable_long_with_value()
        {
            SqlCommand cmd = new SqlCommand();
            cmd.AddParameters(new {Id = (long?)1});
            Assert.AreEqual(1, cmd.Parameters.Count);
            Assert.AreEqual("@Id", cmd.Parameters[0].ParameterName);
            Assert.AreEqual(DbType.Int64, cmd.Parameters[0].DbType);
            Assert.AreEqual(1L, cmd.Parameters[0].Value);
            Assert.AreEqual(typeof(long), cmd.Parameters[0].Value.GetType());
        }

        [Test]
        public void can_add_nullable_long_without_value()
        {
            SqlCommand cmd = new SqlCommand();
            cmd.AddParameters(new {Id = (long?)null});
            Assert.AreEqual(1, cmd.Parameters.Count);
            Assert.AreEqual("@Id", cmd.Parameters[0].ParameterName);
            Assert.AreEqual(DbType.Int64, cmd.Parameters[0].DbType);
            Assert.AreEqual(DBNull.Value, cmd.Parameters[0].Value);
        }

        [Test]
        public void can_add_float_with_value()
        {
            SqlCommand cmd = new SqlCommand();
            cmd.AddParameters(new {Single = 1f});
            Assert.AreEqual(1, cmd.Parameters.Count);
            Assert.AreEqual("@Single", cmd.Parameters[0].ParameterName);
            Assert.AreEqual(DbType.Single, cmd.Parameters[0].DbType);
            Assert.AreEqual(1f, cmd.Parameters[0].Value);
        }

        [Test]
        public void can_add_double_with_value()
        {
            SqlCommand cmd = new SqlCommand();
            cmd.AddParameters(new {Double = 1d});
            Assert.AreEqual(1, cmd.Parameters.Count);
            Assert.AreEqual("@Double", cmd.Parameters[0].ParameterName);
            Assert.AreEqual(DbType.Double, cmd.Parameters[0].DbType);
            Assert.AreEqual(1d, cmd.Parameters[0].Value);
        }

        [Test]
        public void can_add_decimal_with_value()
        {
            SqlCommand cmd = new SqlCommand();
            cmd.AddParameters(new {Val = 1.2m});
            Assert.AreEqual(1, cmd.Parameters.Count);
            Assert.AreEqual("@Val", cmd.Parameters[0].ParameterName);
            Assert.AreEqual(DbType.Decimal, cmd.Parameters[0].DbType);
            Assert.AreEqual(1.2m, cmd.Parameters[0].Value);
        }

        [Test]
        public void can_add_nullable_decimal_with_value()
        {
            SqlCommand cmd = new SqlCommand();
            cmd.AddParameters(new {Val = (decimal?)1.2m});
            Assert.AreEqual(1, cmd.Parameters.Count);
            Assert.AreEqual("@Val", cmd.Parameters[0].ParameterName);
            Assert.AreEqual(DbType.Decimal, cmd.Parameters[0].DbType);
            Assert.AreEqual(1.2m, cmd.Parameters[0].Value);
        }

        [Test]
        public void can_add_nullable_decimal_with_null_value()
        {
            SqlCommand cmd = new SqlCommand();
            cmd.AddParameters(new {Val = (decimal?)null});
            Assert.AreEqual(1, cmd.Parameters.Count);
            Assert.AreEqual("@Val", cmd.Parameters[0].ParameterName);
            Assert.AreEqual(DbType.Decimal, cmd.Parameters[0].DbType);
            Assert.AreEqual(DBNull.Value, cmd.Parameters[0].Value);
        }

        [Test]
        public void can_add_DateTime()
        {
            SqlCommand cmd = new SqlCommand();
            var now = DateTime.Now;
            cmd.AddParameters(new { When = now });
            Assert.AreEqual(1, cmd.Parameters.Count);
            Assert.AreEqual("@When", cmd.Parameters[0].ParameterName);
            Assert.AreEqual(DbType.DateTime2, cmd.Parameters[0].DbType);
            Assert.AreEqual(now, cmd.Parameters[0].Value);
        }

        [Test]
        public void can_add_null_DateTime_with_value()
        {
            SqlCommand cmd = new SqlCommand();
            var now = DateTime.Now;
            cmd.AddParameters(new { When = (DateTime?)now });
            Assert.AreEqual(1, cmd.Parameters.Count);
            Assert.AreEqual("@When", cmd.Parameters[0].ParameterName);
            Assert.AreEqual(DbType.DateTime2, cmd.Parameters[0].DbType);
            Assert.AreEqual(now, cmd.Parameters[0].Value);
        }

        [Test]
        public void can_add_null_DateTime()
        {
            SqlCommand cmd = new SqlCommand();
            cmd.AddParameters(new { When = (DateTime?)null });
            Assert.AreEqual(1, cmd.Parameters.Count);
            Assert.AreEqual("@When", cmd.Parameters[0].ParameterName);
            Assert.AreEqual(DbType.DateTime2, cmd.Parameters[0].DbType);
            Assert.AreEqual(DBNull.Value, cmd.Parameters[0].Value);
        }

        [Test]
        public void can_add_enum_as_int()
        {
            SqlCommand cmd = new SqlCommand();
            cmd.AddParameters(new { SomeId = TestEnum.Something });
            Assert.AreEqual(1, cmd.Parameters.Count);
            Assert.AreEqual("@SomeId", cmd.Parameters[0].ParameterName);
            Assert.AreEqual(DbType.Int32, cmd.Parameters[0].DbType);
            Assert.AreEqual(1, cmd.Parameters[0].Value);
        }

        [Test]
        public void can_add_nulllable_enum_as_int()
        {
            SqlCommand cmd = new SqlCommand();
            cmd.AddParameters(new { SomeId = (TestEnum?)TestEnum.Something });
            Assert.AreEqual(1, cmd.Parameters.Count);
            Assert.AreEqual("@SomeId", cmd.Parameters[0].ParameterName);
            Assert.AreEqual(DbType.Int32, cmd.Parameters[0].DbType);
            Assert.AreEqual(1, cmd.Parameters[0].Value);
        }

        [Test]
        public void can_add_null_TableType()
        {
            var input = new[] {new {First = 1m},};
            var tt = new SqlTableType("SOME_TYPE", new SqlMetaData("first", SqlDbType.Decimal));
            SqlCommand cmd = new SqlCommand();
            cmd.AddParameters(new { Res = input.ToSqlTable(tt) });
            Assert.AreEqual(1, cmd.Parameters.Count);
            Assert.AreEqual("@Res", cmd.Parameters[0].ParameterName);
            Assert.AreEqual(SqlDbType.Structured, cmd.Parameters[0].SqlDbType);
            Assert.AreEqual("SOME_TYPE", cmd.Parameters[0].TypeName);
        }

        [Test]
        public void can_add_int_from_public_field()
        {
            SqlCommand cmd = new SqlCommand();
            cmd.AddParameters(new TestField { Id = 1 });
            Assert.AreEqual(1, cmd.Parameters.Count);
            Assert.AreEqual("@Id", cmd.Parameters[0].ParameterName);
            Assert.AreEqual(DbType.Int32, cmd.Parameters[0].DbType);
            Assert.AreEqual(1, cmd.Parameters[0].Value);
        }

        class TestField
        {
            public int Id;
        }
    }
}