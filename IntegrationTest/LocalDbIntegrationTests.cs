using BusterWood.Mapper;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace IntegrationTest
{
    public class LocalDbIntegrationTests
    {
        readonly string mapperTest;

        public LocalDbIntegrationTests()
        {
            var master = CreateDb.GetMasterConnectionString();
            mapperTest = new SqlConnectionStringBuilder(master) { InitialCatalog = "MapperTest" }.ToString();
        }

        [Test]
        public void can_select_from_table_when_conenction_closed()
        {
            var currencies = GetCurrencies();
            CheckLoaded(currencies);
        }

        [Test]
        public void can_select_from_table_when_connection_open()
        {
            var currencies = GetCurrencies(openConnection: true);
            CheckLoaded(currencies);
        }

        [Test]
        public async Task can_async_select_from_table_when_conenction_closed()
        {
            var currencies = await GetCurrenciesAsync();
            CheckLoaded(currencies);
        }

        [Test]
        public async Task can_async_select_from_table_when_connection_open()
        {
            var currencies = await GetCurrenciesAsync(openConnection: true);
            CheckLoaded(currencies);
        }

        List<Currency> GetCurrencies(bool openConnection = false)
        {
            using (var cnn = new SqlConnection(mapperTest))
            {
                if (openConnection)
                    cnn.Open();
                return cnn.Query("select * from dbo.Currency").ToList<Currency>();
            }
        }

        async Task<List<Currency>> GetCurrenciesAsync(bool openConnection = false)
        {
            using (var cnn = new SqlConnection(mapperTest))
            {
                if (openConnection)
                    await cnn.OpenAsync();
                return await cnn.QueryAsync("select * from dbo.Currency").ToListAsync<Currency>();
            }
        }

        static void CheckLoaded(List<Currency> currencies)
        {
            Assert.IsNotNull(currencies);
            Assert.AreNotEqual(0, currencies.Count);
            foreach (var c in currencies)
            {
                Assert.AreNotEqual(0, c.Id);
                Assert.NotNull(c.Name, $"{nameof(c.Name)} of currency {c.Id} was null");
                Assert.NotNull(c.IsoCode, $"{nameof(c.IsoCode)} of currency {c.Id} was null");
            }
        }

        [Test]
        public void can_read_datetime()
        {
            const string sql = @"
INSERT INTO [dbo].[time_test] ([ID],[when])
OUTPUT INSERTED.*
VALUES (1, @when)";

            var input = new DateTime(2017, 2, 22, 13, 33, 44, 550);
            using (var cnn = new SqlConnection(mapperTest))
            {
                var result = cnn.Query(sql, new { when=input }).Single<TimeTest>();
                Assert.IsNotNull(result);
                Assert.AreEqual(input, result.When);
            }
        }

        [Test]
        public void can_read_datetime2()
        {
            const string sql = @"
INSERT INTO [dbo].[time_test] ([ID],[when2])
OUTPUT INSERTED.*
VALUES (2, @when)";

            var input = new DateTime(2017, 2, 22, 13, 33, 44, 551);
            using (var cnn = new SqlConnection(mapperTest))
            {
                var result = cnn.Query(sql, new { when = input }).Single<TimeTest>();
                Assert.IsNotNull(result);
                Assert.AreEqual(input, result.When2);
            }
        }

        [Test]
        public void can_read_datetimeoffset()
        {
            const string sql = @"
INSERT INTO [dbo].[time_test] ([ID],[whenoffset])
OUTPUT INSERTED.*
VALUES (3, @when)";

            var input = new DateTimeOffset(2017, 2, 22, 13, 33, 44, 551, TimeSpan.Zero);
            using (var cnn = new SqlConnection(mapperTest))
            {
                var result = cnn.Query(sql, new { when=input }).Single<TimeTest>();
                Assert.IsNotNull(result);
                Assert.AreEqual(input, result.WhenOffset);
            }
        }

    }

    class TimeTest
    {
        public DateTime When;
        public DateTime When2;
        public DateTimeOffset WhenOffset;
    }

    class Currency
    {
        public int Id;
        public string Name;
        public string IsoCode;
    }
}
