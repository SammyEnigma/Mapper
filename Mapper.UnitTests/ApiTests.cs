using NUnit.Framework;
using System.Data.Common;
using System.Threading.Tasks;

namespace BusterWood.Mapper.UnitTests
{
    [TestFixture]
    public class ApiTests
    {
        [Test]
        public void can_read_single()
        {
            var reader = new StubDataReader
            {
                Names = new[] { "ID" },
                Types = new[] { typeof(long) },
                Values = new object[] { 1L },
            };

            reader.Read<long>().Single();
        }

        [Test]
        public async Task can_read_single_async()
        {
            var reader = new StubDataReader
            {
                Names = new[] { "ID" },
                Types = new[] { typeof(long) },
                Values = new object[] { 1L },
            };
            var task = Task.FromResult<DbDataReader>(reader);

            await task.Read<long>().SingleAsync();
        }

    }
}
