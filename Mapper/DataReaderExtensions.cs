using System;
using System.Data.Common;
using System.Diagnostics.Contracts;

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
        public static DataSequence<T> AsSeqOf<T>(this DbDataReader reader)
        {
            Contract.Requires(reader != null);
            Contract.Requires(reader.IsClosed == false);
            return new DataSequence<T>(reader);
        }
    }
}