using NUnit.Framework;

namespace Mapper.UnitTests
{
    [TestFixture]
    public class DynamicTests
    {
        [Test]
        public void can_read_zero_rows()
        {
            var stubDataReader = new StubDataReader
            {
                Names = new[] { "ID" },
                Types = new[] { typeof(int) },
                Values = new object[] { 1 },
                read = true // zero rows
            };
            var seq = new DynamicDataSequence(stubDataReader);
            var enm = seq.GetEnumerator();
            Assert.IsFalse(enm.MoveNext());
        }

        [Test]
        public void can_read_one_rows()
        {
            var stubDataReader = new StubDataReader
            {
                Names = new[] { "ID" },
                Types = new[] { typeof(int) },
                Values = new object[] { 1 },
            };
            var seq = new DynamicDataSequence(stubDataReader);
            var enm = seq.GetEnumerator();
            Assert.IsTrue(enm.MoveNext());
            Assert.IsFalse(enm.MoveNext());
        }

        [Test]
        public void can_get_values_of_row()
        {
            var stubDataReader = new StubDataReader
            {
                Names = new[] { "ID" },
                Types = new[] { typeof(int) },
                Values = new object[] { 1 },
            };
            var seq = new DynamicDataSequence(stubDataReader);
            var enm = seq.GetEnumerator();
            Assert.IsTrue(enm.MoveNext());
            DynamicRow row = enm.Current;
            Assert.AreEqual(1, row.Count, "row.Count");
            Assert.AreEqual(true, row.ContainsKey("ID"), "row.ContainsKey ID");
            Assert.AreEqual(true, row.ContainsKey("id"), "row.ContainsKey id");
            Assert.AreEqual(1, row["id"], "row[id]");
            Assert.AreEqual(1, row[0], "row[0]");
        }

        [Test]
        public void can_get_values_of_row_via_dynamic()
        {
            var stubDataReader = new StubDataReader
            {
                Names = new[] { "ID" },
                Types = new[] { typeof(int) },
                Values = new object[] { 1 },
            };
            var seq = new DynamicDataSequence(stubDataReader);
            var enm = seq.GetEnumerator();
            Assert.IsTrue(enm.MoveNext());
            dynamic row = enm.Current;
            Assert.AreEqual(1, row.ID, "row.ID");
            Assert.AreEqual(1, row.Id, "row.Id");
        }
    }
}
