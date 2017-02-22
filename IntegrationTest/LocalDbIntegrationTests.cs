using BusterWood.Mapper;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace IntegrationTest
{
    [SetUpFixture]
    public class CreateDb
    {
        readonly string master;

        public CreateDb()
        {
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("APPVEYOR")))
                master = ConfigurationManager.ConnectionStrings["local"].ConnectionString;
            else
                master = ConfigurationManager.ConnectionStrings["AppVeyor"].ConnectionString;
        }

        [SetUp]
        public void Setup()
        {
            RecreateDatabase();
            PopulateDatabase();
        }

        private void RecreateDatabase()
        {
            const string sql = @"USE master;

IF EXISTS (SELECT name FROM sys.databases WHERE name = N'MapperTest')
    ALTER DATABASE [MapperTest] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;

IF EXISTS (SELECT name FROM sys.databases WHERE name = N'MapperTest')
    DROP DATABASE [MapperTest]; 

CREATE DATABASE [MapperTest];
";

            using (var cnn = new SqlConnection(master))
            {
                cnn.Execute(sql);
            }
        }

        private void PopulateDatabase()
        {
            const string sql = @"USE [MapperTest];

CREATE TABLE [dbo].[CURRENCY] (
	[ID] [int] NOT NULL,
	[CURRENCY_NAME] [varchar](200) NULL,
	[ISO_CODE] [varchar](200) NULL,
	[SOMETHING_ELSE] [varchar](30) NULL,
 CONSTRAINT [PK_CURRENCY] PRIMARY KEY CLUSTERED ( [ID] ASC )
);

INSERT INTO [dbo].[Currency] ([ID],[CURRENCY_NAME],[ISO_CODE])
VALUES
  (1,'Euro','EUR')
 ,(2,'British Pound','GBP')
 ,(3,'Swiss Franc','CHF')
 ,(4,'Swedish Krona','SEK')
 ,(5,'US Dollar','USD')
 ,(6,'Euro','EUR')
 ,(7,'Danish Krone','DKK')
 ,(8,'Norwegian Krone','NOK')
 ,(9,'Australian Dollar','AUD')
 ,(10,'Hong Kong Dollar','HKD')
;

CREATE TABLE [dbo].[time_test] (
	[ID] [int] NOT NULL,
	[when] [datetime],
	[when2] [datetime2],
	[whenOffset] [datetimeoffset]
 CONSTRAINT [PK_time_test] PRIMARY KEY CLUSTERED ( [ID] ASC )
);
";
            using (var cnn = new SqlConnection(master))
            {
                cnn.Execute(sql);
            }
        }
    }

    public class LocalDbIntegrationTests
    {
        string mapperTest;

        public LocalDbIntegrationTests()
        {
            var master = ConfigurationManager.ConnectionStrings["local"].ConnectionString;
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
