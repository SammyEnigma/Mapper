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
        public void can_map_string()
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
    }
}