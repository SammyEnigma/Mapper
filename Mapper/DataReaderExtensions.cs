using System;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

namespace BusterWood.Mapper
{
    public static partial class Extensions
    {
        static readonly DataReaderMapper _readerMapper = new DataReaderMapper();

        internal static Func<DbDataReader, T> GetMappingFunc<T>(DbDataReader reader)
        {
            Contract.Requires(reader != null);
            Delegate func = _readerMapper.GetOrCreateMappingFunc(typeof(T), reader);
            return (Func<DbDataReader, T>)func;
        }

        /// <summary>Converts the <paramref name="reader"/> into a <see cref="DataSequence{T}"/></summary>
        public static DataSequence<T> Read<T>(this DbDataReader reader, Action<DbDataReader, T> extraAction = null)
        {
            Contract.Requires(reader != null);
            Contract.Requires(reader.IsClosed == false);
            return new DataSequence<T>(reader, extraAction);
        }

        /// <summary>Converts the <paramref name="reader"/> into a <see cref="DataSequence{T}"/></summary>
        public static async Task<DataSequence<T>> Read<T>(this Task<DbDataReader> reader, Action<DbDataReader, T> extraAction = null)
        {
            Contract.Requires(reader != null);
            return new DataSequence<T>(await reader, extraAction);
        }

        /// <summary>Executes a <paramref name="cmd"/> and return a sequence of dynamic data</summary>
        public static DynamicDataSequence ToDynamic(this DbCommand cmd)
        {
            Contract.Requires(cmd != null);
            return new DynamicDataSequence(cmd.ExecuteReader());
        }

        /// <summary>Executes a <paramref name="cmd"/> and return a sequence of dynamic data</summary>
        public static async Task<DynamicDataSequence> ToDynamicAsync(this DbCommand cmd)
        {
            Contract.Requires(cmd != null);
            Contract.Ensures(Contract.Result<Task<DynamicDataSequence>>() != null);
            return new DynamicDataSequence(await cmd.ExecuteReaderAsync());
        }

    }
}