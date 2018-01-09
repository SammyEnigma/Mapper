using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusterWood.Mapper.UnitTests
{
    [TestFixture, Category("Performance")]
    public class DataReaderDictionaryBenchmark
    {
        [Ignore]
        [TestCase(10)]
        [TestCase(100)]
        public void BenchmarkMostlyReadDictionary(int readsPerWrite)
        {
            Benchmark.Go("MostlyReadDictionary", iterations =>
            {
                var methods = new MostlyReadDictionary<DataReaderMapper.MetaData, Delegate>();

                while (iterations > 0)
                {
                    var reader = CreateDataReader("extra" + iterations);
                    var columns = DataReaderMapper.CreateColumnList(reader);

                    GC.KeepAlive(methods.GetOrAdd(new DataReaderMapper.MetaData(typeof(Target), columns), Target.Create));
                    for (int i = 0; i < readsPerWrite; i++)
                    {
                        if (--iterations <= 0) break;
                        GC.KeepAlive(methods.GetOrAdd(new DataReaderMapper.MetaData(typeof(Target), columns), Target.Create));
                    }
                }
            });
        }

        private static StubDataReader CreateDataReader(string additionalFieldName)
        {
            return new StubDataReader
            {
                Names = new[] { "ORDER_ID", "NAME", "WHEN", additionalFieldName },
                Types = new[] { typeof(int), typeof(string), typeof(DateTime), typeof(string) },
                Values = new object[] { 1, "Fred", DateTime.UtcNow, "test" },
            };
        }

        class Target
        {
            public int OrderId;
            public string Name;
            public DateTime When;

            public static Func<DbDataReader, Target> Create(DataReaderMapper.MetaData md)
            {
                Func<DbDataReader, Target> map = reader => new Target();
                return map;
            }


            public static Func<DbDataReader, Target> Create2(Type t, DbDataReader r)
            {
                Func<DbDataReader, Target> map = reader => new Target();
                return map;
            }
        }
    }
}
