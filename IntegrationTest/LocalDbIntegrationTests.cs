﻿using BusterWood.Mapper;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace IntegrationTest
{
    [TestFixture]
    public class LocalDbIntegrationTests
    {
        readonly string mapperTest;

        public LocalDbIntegrationTests()
        {
            var master = CreateDb.GetMasterConnectionString();
            mapperTest = new SqlConnectionStringBuilder(master) { InitialCatalog = "MapperTest" }.ToString();
        }

        [Test]
        public void can_call_stored_proc_with_param()
        {
            using (var cnn = new SqlConnection(mapperTest))
            {
                cnn.Open();
                var curr = cnn.QueryProc("dbo.GetCurrencyById", new { Id = 1 }).Single<Currency>();
                Assert.AreEqual("EUR", curr.IsoCode);
            }
        }

        [Test]
        public async Task can_call_stored_proc_with_param_async()
        {
            using (var cnn = new SqlConnection(mapperTest))
            {
                await cnn.OpenAsync();
                var curr = await cnn.QueryProcAsync("dbo.GetCurrencyById", new { Id = 1 }).SingleAsync<Currency>();
                Assert.AreEqual("EUR", curr.IsoCode);
            }
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

        [Test]
        public void can_pass_empty_table_type()
        {
            const string sql = @"select c.* from dbo.Currency c join @ids i on i.id = c.id";
            var tt = new SqlTableType("dbo.IntType", new Microsoft.SqlServer.Server.SqlMetaData("ID", System.Data.SqlDbType.Int) );
            using (var cnn = new SqlConnection(mapperTest))
            {
                var ids = new int[0].ToSqlTable(tt);
                var result = cnn.Query(sql, new { ids }).ToList<Currency>();
                Assert.IsNotNull(result);
                Assert.AreEqual(0, result.Count);
            }
        }

        [Test]
        public void can_pass_table_type()
        {
            const string sql = @"select c.* from dbo.Currency c join @ids i on i.id = c.id";
            var tt = new SqlTableType("dbo.IntType", new Microsoft.SqlServer.Server.SqlMetaData("ID", System.Data.SqlDbType.Int));
            using (var cnn = new SqlConnection(mapperTest))
            {
                var ids = new int[] { 1 }.ToSqlTable(tt);
                var result = cnn.Query(sql, new { ids }).ToList<Currency>();
                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Count);
            }
        }

        [Test]
        public void can_read_mutliple_results()
        {
            using (var cnn = new SqlConnection(mapperTest))
            {
                var rs = cnn.Query("select * from dbo.Currency where id <= 4; select * from dbo.Currency where id > 4;");
                var small = rs.ToList<Currency>();
                var big = rs.ToList<Currency>();
                Assert.IsTrue(rs.IsClosed);
                Assert.AreEqual(4, small.Count);
                Assert.AreEqual(6, big.Count);
            }
        }

        [Test]
        public async Task can_read_mutliple_results_async()
        {
            using (var cnn = new SqlConnection(mapperTest))
            {
                var rs = await cnn.QueryAsync("select * from dbo.Currency where id <= 4; select * from dbo.Currency where id > 4;");
                var small = await rs.ToListAsync<Currency>();
                var big = await rs.ToListAsync<Currency>();
                Assert.IsTrue(rs.IsClosed);
                Assert.AreEqual(4, small.Count);
                Assert.AreEqual(6, big.Count);
            }
        }

        [Test]
        public void can_read_timestamp()
        {
            using (var cnn = new SqlConnection(mapperTest))
            {
                var input = new byte[] { 1, 2, 3 };
                cnn.Execute("delete from [binary_test]");
                cnn.Execute("insert into [binary_test] (ID, data) values (@id, @data)", new { id=1, data=input });
                var bt = cnn.Query("select * from [binary_test] where ID = @id", new { id = 1 }).Single<BinaryTest>();
                Assert.IsNotNull(bt.Version);
                Assert.AreEqual(8, bt.Version.Length);
                Assert.IsFalse(Enumerable.SequenceEqual(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 }, bt.Version), "version is empty");
            }
        }

        [Test]
        public void can_read_varbinary()
        {
            using (var cnn = new SqlConnection(mapperTest))
            {
                var input = new byte[] { 1, 2, 3 };
                cnn.Execute("delete from [binary_test]");
                cnn.Execute("insert into [binary_test] (ID, data) values (@id, @data)", new { id = 1, data = input });
                var bt = cnn.Query("select * from [binary_test] where ID = @id", new { id = 1 }).Single<BinaryTest>();
                Assert.IsNotNull(bt.Data);
                Assert.AreEqual(3, bt.Data.Length);
                Assert.IsTrue(Enumerable.SequenceEqual(input, bt.Data), "wrong data read");
            }
        }

        [Test]
        public void can_list_of_strings()
        {
            using (var cnn = new SqlConnection(mapperTest))
            {
                var input = new byte[] { 1, 2, 3 };
                var currencyNames = cnn.Query("select CURRENCY_NAME from dbo.Currency").ToList<string>();
                Assert.IsNotNull(currencyNames.Count);
                Assert.AreEqual(10, currencyNames.Count);
            }
        }
    }

    internal class BinaryTest
    {
        public int Id { get; set; }
        public byte[] Version { get; set; }
        public byte[] Data { get; set; }
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
