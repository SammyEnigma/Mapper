using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

namespace Mapper
{
    public static class DataReaderExtensions
    {
        static readonly DataReaderMapper _mapper = new DataReaderMapper();

        internal static Func<DbDataReader, T> GetMappingFunc<T>(DbDataReader reader)
        {
            Contract.Requires(reader != null);
            Contract.Requires(!typeof(T).IsPrimitiveOrEnum() && !typeof(T).IsNullable(), "Please use DbCommandExtensions.ExecuteScalar<T>() method for reading single values");
            Delegate func = _mapper.GetOrCreateMappingFunc(typeof(T), reader);
            return (Func<DbDataReader, T>)func;
        }

        /// <summary>Reads exactly one item from the reader</summary>
        /// <exception cref="InvalidOperationException"> when zero values read or more than one value can be read</exception>
        public static T ReadSingle<T>(this DbDataReader reader)
        {
            Contract.Requires(reader != null);
            Contract.Requires(reader.IsClosed == false);
            Contract.Requires(!typeof(T).CanReadScalar(), "Please use DbCommandExtensions.ExecuteScalar<T>() method for reading single values");
            Contract.Ensures(Contract.Result<T>() != null);
            if (!reader.Read()) throw new InvalidOperationException("Expected one value to be read but reader is empty");
            var map = GetMappingFunc<T>(reader);
            var single = map(reader);
            if (reader.Read()) throw new InvalidOperationException("Expected one value to be read but more than one value can be read");
            return single;
        }

        /// <summary>Reads exactly one item from the reader</summary>
        /// <exception cref="InvalidOperationException"> when zero values read or more than one value can be read</exception>
        public static async Task<T> ReadSingleAsync<T>(this DbDataReader reader)
        {
            Contract.Requires(reader != null);
            Contract.Requires(reader.IsClosed == false);
            Contract.Requires(!typeof(T).CanReadScalar(), "Please use DbCommandExtensions.ExecuteScalarAsync<T>() method for reading single values");
            Contract.Ensures(Contract.Result<Task<T>>() != null);
            if (!await reader.ReadAsync()) throw new InvalidOperationException("Expected one value to be read but reader is empty");
            var map = GetMappingFunc<T>(reader);
            var single = map(reader);
            if (await reader.ReadAsync()) throw new InvalidOperationException("Expected one value to be read but more than one value can be read");
            return single;
        }

        /// <summary>Reads zero or one items from the reader</summary>
        /// <remarks>Returns the default vaue of T if no values be read, i.e may return null</remarks>
        public static T ReadSingleOrDefault<T>(this DbDataReader reader)
        {
            Contract.Requires(reader != null);
            Contract.Requires(reader.IsClosed == false);
            if (!reader.Read()) return default(T);
            var map = GetMappingFunc<T>(reader);
            return map(reader);
        }

        /// <summary>Reads zero or one items from the reader</summary>
        /// <remarks>Returns the default vaue of T if no values be read, i.e may return null</remarks>
        public static async Task<T> ReadSingleOrDefaultAsync<T>(this DbDataReader reader)
        {
            Contract.Requires(reader != null);
            Contract.Requires(reader.IsClosed == false);
            if (!await reader.ReadAsync()) return default(T);
            var map = GetMappingFunc<T>(reader);
            return map(reader);
        }

        /// <summary>Reads all the records in the reader into a list</summary>
        public static List<T> ReadList<T>(this DbDataReader reader)
        {
            Contract.Requires(reader != null);
            Contract.Requires(reader.IsClosed == false);
            Contract.Ensures(Contract.Result<List<T>>() != null);
            var map = GetMappingFunc<T>(reader);
            var list = new List<T>();
            while (reader.Read())
            {
                list.Add(map(reader));
            }
            return list;
        }

        /// <summary>Reads all the records in the reader into a list</summary>
        public static async Task<List<T>> ReadListAsync<T>(this DbDataReader reader)
        {
            Contract.Requires(reader != null);
            Contract.Requires(reader.IsClosed == false);
            Contract.Ensures(Contract.Result<Task<List<T>>>() != null);
            Contract.Ensures(Contract.Result<Task<List<T>>>().Result != null);
            var map = GetMappingFunc<T>(reader);
            var list = new List<T>();
            while (await reader.ReadAsync())
            {
                list.Add(map(reader));
            }
            return list;
        }

        /// <summary>Reads all the records in the reader into a dictionary, using the supplied <paramref name="keyFunc"/> to generate the key</summary>
        public static Dictionary<TKey, TValue> ReadDictionary<TKey, TValue>(this DbDataReader reader, Func<TValue, TKey> keyFunc)
        {
            Contract.Requires(reader != null);
            Contract.Requires(keyFunc != null);
            Contract.Requires(reader.IsClosed == false);
            Contract.Ensures(Contract.Result<Dictionary<TKey, TValue>>() != null);
            var map = GetMappingFunc<TValue>(reader);
            var dict = new Dictionary<TKey, TValue>();
            while (reader.Read())
            {
                TValue value = map(reader);
                TKey key = keyFunc(value);
                dict.Add(key, value);
            }
            return dict;
        }

        /// <summary>Reads all the records in the reader into a dictionary, using the supplied <paramref name="keyFunc"/> to generate the key</summary>
        public static async Task<Dictionary<TKey, TValue>> ReadDictionaryAsync<TKey, TValue>(this DbDataReader reader, Func<TValue, TKey> keyFunc)
        {
            Contract.Requires(reader != null);
            Contract.Requires(keyFunc != null);
            Contract.Requires(reader.IsClosed == false);
            Contract.Ensures(Contract.Result<Task<Dictionary<TKey, TValue>>>() != null);
            Contract.Ensures(Contract.Result<Task<Dictionary<TKey, TValue>>>().Result != null);
            var map = GetMappingFunc<TValue>(reader);
            var dict = new Dictionary<TKey, TValue>();
            while (await reader.ReadAsync())
            {
                TValue value = map(reader);
                TKey key = keyFunc(value);
                dict.Add(key, value);
            }
            return dict;
        }

        /// <summary>Reads all the records in the lookup, group by key, using the supplied <paramref name="keyFunc"/> to generate the key</summary>
        public static HashLookup<TKey, TValue> ReadLookup<TKey, TValue>(this DbDataReader reader, Func<TValue, TKey> keyFunc)
        {
            Contract.Requires(reader != null);
            Contract.Requires(keyFunc != null);
            Contract.Requires(reader.IsClosed == false);
            Contract.Ensures(Contract.Result<HashLookup<TKey, TValue>>() != null);
            var map = GetMappingFunc<TValue>(reader);
            var lookup = new HashLookup<TKey, TValue>();
            while (reader.Read())
            {
                TValue value = map(reader);
                TKey key = keyFunc(value);
                lookup.Add(key, value);
            }
            return lookup;
        }

        /// <summary>Reads all the records in the lookup, group by key, using the supplied <paramref name="keyFunc"/> to generate the key</summary>
        public static async Task<HashLookup<TKey, TValue>> ReadLookupAsync<TKey, TValue>(this DbDataReader reader, Func<TValue, TKey> keyFunc)
        {
            Contract.Requires(reader != null);
            Contract.Requires(keyFunc != null);
            Contract.Requires(reader.IsClosed == false);
            Contract.Ensures(Contract.Result<Task<HashLookup<TKey, TValue>>>() != null);
            Contract.Ensures(Contract.Result<Task<HashLookup<TKey, TValue>>>().Result != null);
            var map = GetMappingFunc<TValue>(reader);
            var lookup = new HashLookup<TKey, TValue>();
            while (await reader.ReadAsync())
            {
                TValue value = map(reader);
                TKey key = keyFunc(value);
                lookup.Add(key, value);
            }
            return lookup;
        }

    }
}