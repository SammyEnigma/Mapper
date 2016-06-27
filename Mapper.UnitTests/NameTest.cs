using NUnit.Framework;

namespace Mapper.UnitTests
{
    [TestFixture]
    public class NameTest
    {
        [Test]
        public void candidates_include_original_name()
        {
            var list = Names.Candidates("ORDER_ID", typeof(long));
            Assert.AreEqual(true, list.Contains("ORDER_ID"));
        }

        [Test]
        public void candidates_include_id_remove_for_primative_type()
        {
            var list = Names.Candidates("ORDER_ID", typeof(long));
            Assert.AreEqual(true, list.Contains("ORDER"));
        }

        [Test]
        public void candidates_include_id_remove_for_nullable_primative_type()
        {
            var list = Names.Candidates("ORDER_ID", typeof(long?));
            Assert.AreEqual(true, list.Contains("ORDER"));
        }

        [Test]
        public void candidates_include_id_remove_for_enum()
        {
            var list = Names.Candidates("ORDER_ID", typeof(TestEnum));
            Assert.AreEqual(true, list.Contains("ORDER"));
        }

        [Test]
        public void candidates_include_id_remove_for_nullable_enum()
        {
            var list = Names.Candidates("ORDER_ID", typeof(TestEnum?));
            Assert.AreEqual(true, list.Contains("ORDER"));
        }

        [Test]
        public void candidates_does_not_remove_id_for_other_types()
        {
            var list = Names.Candidates("ORDER_ID", typeof(string));
            Assert.AreEqual(false, list.Contains("ORDER"));
        }

        [Test]
        public void candidates_include_underscore_removed()
        {
            var list = Names.Candidates("ORDER_ID", typeof(long));
            Assert.AreEqual(true, list.Contains("ORDERID"));
        }

        [Test]
        public void candidates_include_prefix_removed()
        {
            var list = Names.Candidates("ORDER_ID", typeof(long), "ORDER");
            Assert.AreEqual(true, list.Contains("ID"));
        }

        [Test]
        public void candidates_include_prefix_removed_when_case_does_not_match()
        {
            var list = Names.Candidates("ORDER_ID", typeof(long), "Order");
            Assert.AreEqual(true, list.Contains("ID"));
        }

        [Test]
        public void candidates_include_all_underscore_removed()
        {
            var list = Names.Candidates("BROKER_ORDER_STATUS", typeof(long), "ORDER");
            Assert.AreEqual(true, list.Contains("BROKERORDERSTATUS"));
        }

        [Test]
        public void candidates_include_all_underscore_removed_then_prefixed_removed()
        {
            var list = Names.Candidates("BROKER_ORDER_STATUS", typeof(long), "BROKERORDER");
            Assert.AreEqual(true, list.Contains("STATUS"));
        }

    }
}
