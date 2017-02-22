using BusterWood.Mapper;
using NUnit.Framework;
using System;
using System.Configuration;
using System.Data.SqlClient;

namespace IntegrationTest
{
    [SetUpFixture]
    public class CreateDb
    {
        readonly string master;

        public CreateDb()
        {
            master = GetMasterConnectionString();
        }

        public static string GetMasterConnectionString()
        {
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("APPVEYOR")))
                return ConfigurationManager.ConnectionStrings["local"].ConnectionString;
            else
                return ConfigurationManager.ConnectionStrings["AppVeyor"].ConnectionString;
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
}