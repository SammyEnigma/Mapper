using System.Data;
using Microsoft.SqlServer.Server;
using NUnit.Framework;

namespace BusterWood.Mapper.UnitTests
{
    [TestFixture]
    public class TypeAndMetaDataTests
    {
        [Test]
        public void same_types_and_metadata_are_equal()
        {
            TypeAndMetaData left = new TypeAndMetaData(typeof(string), new []{ new SqlMetaData("fist", SqlDbType.BigInt) });
            TypeAndMetaData right = new TypeAndMetaData(typeof(string), new []{ new SqlMetaData("fist", SqlDbType.BigInt) });
            Assert.AreEqual(left, right);
            Assert.AreEqual(left.GetHashCode(), right.GetHashCode());
        }

        [Test]
        public void different_types_and_same_metadata_are_not_equal()
        {
            TypeAndMetaData left = new TypeAndMetaData(typeof(string), new []{ new SqlMetaData("fist", SqlDbType.BigInt) });
            TypeAndMetaData right = new TypeAndMetaData(typeof(int), new []{ new SqlMetaData("fist", SqlDbType.BigInt) });
            Assert.AreNotEqual(left, right);
        } 

        [Test]
        public void same_types_and_same_column_names_but_different_column_types_are_not_equal()
        {
            TypeAndMetaData left = new TypeAndMetaData(typeof(string), new []{ new SqlMetaData("fist", SqlDbType.BigInt) });
            TypeAndMetaData right = new TypeAndMetaData(typeof(string), new []{ new SqlMetaData("fist", SqlDbType.Int) });
            Assert.AreNotEqual(left, right);
        }

        [Test]
        public void same_types_and_same_column_names_but_different_column_lengths_are_not_equal()
        {
            TypeAndMetaData left = new TypeAndMetaData(typeof(string), new[] { new SqlMetaData("fist", SqlDbType.VarChar, 10) });
            TypeAndMetaData right = new TypeAndMetaData(typeof(string), new[] { new SqlMetaData("fist", SqlDbType.VarChar, 11) });
            Assert.AreNotEqual(left, right);
        }
    }
}