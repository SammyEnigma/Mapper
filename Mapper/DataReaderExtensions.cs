using System;
using System.Collections.Generic;
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
        public static DynamicDataSequence ToDynamic(this DbDataReader reader)
        {
            Contract.Requires(reader != null);
            return new DynamicDataSequence(reader);
        }

        /// <summary>Executes a <paramref name="cmd"/> and return a sequence of dynamic data</summary>
        public static async Task<DynamicDataSequence> ToDynamicAsync(this Task<DbDataReader> reader)
        {
            Contract.Requires(reader != null);
            Contract.Ensures(Contract.Result<Task<DynamicDataSequence>>() != null);
            return new DynamicDataSequence(await reader);
        }

        /// <summary>Reads exactly one item from the reader</summary>
        /// <exception cref="InvalidOperationException"> when zero values read or more than one value can be read</exception>
        /// <remarks>The underlying <see cref="DbDataReader"/> is disposed after this method has been called</remarks>
        public static T Single<T>(this DbDataReader reader, Action<DbDataReader, T> extraAction = null)
        {
            Contract.Requires(reader != null);
            try
            {
                var map = GetMappingFunc<T>(reader);
                if (!reader.Read()) throw new InvalidOperationException("Expected one value to be read but reader is empty");
                var item = map(reader);
                extraAction?.Invoke(reader, item);
                if (reader.Read()) throw new InvalidOperationException("Expected one value to be read but more than one value can be read");
                return item;
            }
            finally
            {
                if (!reader.NextResult())
                    reader.Close();
            }
        }

        /// <summary>Reads exactly one item from the reader</summary>
        /// <exception cref="InvalidOperationException"> when zero values read or more than one value can be read</exception>
        /// <remarks>The underlying <see cref="DbDataReader"/> is disposed after this method has been called</remarks>
        public static async Task<T> SingleAsync<T>(this DbDataReader reader, Action<DbDataReader, T> extraAction = null)
        {
            Contract.Requires(reader != null);
            Contract.Ensures(Contract.Result<T>() != null);
            try
            {
                var map = GetMappingFunc<T>(reader);
                if (!await reader.ReadAsync()) throw new InvalidOperationException("Expected one value to be read but reader is empty");
                var single = map(reader);
                extraAction?.Invoke(reader, single);
                if (await reader.ReadAsync()) throw new InvalidOperationException("Expected one value to be read but more than one value can be read");
                return single;
            }
            finally
            {
                if (!await reader.NextResultAsync())
                    reader.Close();
            }

        }

        /// <summary>Reads zero or one items from the reader</summary>
        /// <remarks>Returns the default vaue of T if no values be read, i.e may return null</remarks>
        /// <remarks>The underlying <see cref="DbDataReader"/> is disposed after this method has been called</remarks>
        public static T SingleOrDefault<T>(this DbDataReader reader, Action<DbDataReader, T> extraAction = null)
        {
            Contract.Requires(reader != null);
            try
            {
                var map = GetMappingFunc<T>(reader);
                if (!reader.Read()) return default(T);
                var item = map(reader);
                extraAction?.Invoke(reader, item);
                return item;
            }
            finally
            {
                if (!reader.NextResult())
                    reader.Close();
            }
        }

        /// <summary>Reads zero or one items from the reader</summary>
        /// <remarks>Returns the default vaue of T if no values be read, i.e may return null</remarks>
        /// <remarks>The underlying <see cref="DbDataReader"/> is disposed after this method has been called</remarks>
        public static async Task<T> SingleOrDefaultAsync<T>(this DbDataReader reader, Action<DbDataReader, T> extraAction = null)
        {
            Contract.Requires(reader != null);
            try
            {
                var map = GetMappingFunc<T>(reader);
                if (!await reader.ReadAsync()) return default(T);
                var item = map(reader);
                extraAction?.Invoke(reader, item);
                return item;
            }
            finally
            {
                if (!await reader.NextResultAsync())
                    reader.Close();
            }
        }

        /// <summary>Reads all the records in the reader into a list</summary>
        /// <remarks>The underlying <see cref="DbDataReader"/> is disposed after this method has been called</remarks>
        public static List<T> ToList<T>(this DbDataReader reader, Action<DbDataReader, T> extraAction = null)
        {
            Contract.Requires(reader != null);
            Contract.Ensures(Contract.Result<List<T>>() != null);
            try
            {
                var map = GetMappingFunc<T>(reader);
                var list = new List<T>();
                while (reader.Read())
                {
                    var item = map(reader);
                    extraAction?.Invoke(reader, item);
                    list.Add(item);
                }
                return list;
            }
            finally
            {
                if (!reader.NextResult())
                    reader.Close();
            }
        }

        /// <summary>Reads all the records in the reader into a list</summary>
        /// <remarks>The underlying <see cref="DbDataReader"/> is disposed after this method has been called</remarks>
        public static async Task<List<T>> ToListAsync<T>(this DbDataReader reader, Action<DbDataReader, T> extraAction = null)
        {
            Contract.Requires(reader != null);
            Contract.Ensures(Contract.Result<Task<List<T>>>() != null);
            //Contract.Ensures(Contract.Result<Task<List<T>>>().Result != null);
            try
            {
                var map = GetMappingFunc<T>(reader);
                var list = new List<T>();
                while (await reader.ReadAsync())
                {
                    var item = map(reader);
                    extraAction?.Invoke(reader, item);
                    list.Add(item);
                }
                return list;
            }
            finally
            {
                if (!await reader.NextResultAsync())
                    reader.Close();
            }
        }

        /// <summary>Reads all the records in the reader into a <see cref="HashSet{T}"/></summary>
        /// <remarks>The underlying <see cref="DbDataReader"/> is disposed after this method has been called</remarks>
        public static HashSet<T> ToHashSet<T>(this DbDataReader reader, Action<DbDataReader, T> extraAction = null)
        {
            Contract.Requires(reader != null);
            Contract.Ensures(Contract.Result<HashSet<T>>() != null);
            try
            {
                var map = GetMappingFunc<T>(reader);
                var set = new HashSet<T>();
                while (reader.Read())
                {
                    var item = map(reader);
                    extraAction?.Invoke(reader, item);
                    set.Add(item);
                }
                return set;
            }
            finally
            {
                if (!reader.NextResult())
                    reader.Close();
            }
        }

        /// <summary>Reads all the records in the reader into a <see cref="HashSet{T}"/></summary>
        /// <remarks>The underlying <see cref="DbDataReader"/> is disposed after this method has been called</remarks>
        public static async Task<HashSet<T>> ToHashSetAsync<T>(this DbDataReader reader, Action<DbDataReader, T> extraAction = null)
        {
            Contract.Requires(reader != null);
            Contract.Ensures(Contract.Result<Task<HashSet<T>>>() != null);
            Contract.Ensures(Contract.Result<Task<HashSet<T>>>().Result != null);
            try
            {
                var map = GetMappingFunc<T>(reader);
                var set = new HashSet<T>();
                while (await reader.ReadAsync())
                {
                    var item = map(reader);
                    extraAction?.Invoke(reader, item);
                    set.Add(item);
                }
                return set;
            }
            finally
            {
                if (!await reader.NextResultAsync())
                    reader.Close();
            }
        }

        /// <summary>Reads all the records in the reader into a dictionary, using the supplied <paramref name="keyFunc"/> to generate the key</summary>
        /// <remarks>The underlying <see cref="DbDataReader"/> is disposed after this method has been called</remarks>
        public static Dictionary<TKey, T> ToDictionary<TKey, T>(this DbDataReader reader, Func<T, TKey> keyFunc, Action<DbDataReader, T> extraAction = null)
        {
            Contract.Requires(reader != null);
            Contract.Ensures(Contract.Result<Dictionary<TKey, T>>() != null);
            try
            {
                var map = GetMappingFunc<T>(reader);
                var dict = new Dictionary<TKey, T>();
                while (reader.Read())
                {
                    T value = map(reader);
                    extraAction?.Invoke(reader, value);
                    TKey key = keyFunc(value);
                    dict.Add(key, value);
                }
                return dict;
            }
            finally
            {
                if (!reader.NextResult())
                    reader.Close();
            }
        }        
        
        /// <summary>Reads all the records in the reader into a dictionary, using the supplied <paramref name="keyFunc"/> to generate the key</summary>
        /// <remarks>The underlying <see cref="DbDataReader"/> is disposed after this method has been called</remarks>
        public static Dictionary<TKey, TValue> ToDictionary<T, TKey, TValue>(this DbDataReader reader, Func<T, TKey> keyFunc, Func<T, TValue> valueFunc, Action<DbDataReader, T> extraAction = null)
        {
            Contract.Requires(reader != null);
            Contract.Requires(keyFunc != null);
            Contract.Requires(valueFunc != null);
            Contract.Ensures(Contract.Result<Dictionary<TKey, TValue>>() != null);
            try
            {
                var map = GetMappingFunc<T>(reader);
                var dict = new Dictionary<TKey, TValue>();
                while (reader.Read())
                {
                    T temp = map(reader);
                    extraAction?.Invoke(reader, temp);
                    TKey key = keyFunc(temp);
                    TValue value = valueFunc(temp);
                    dict.Add(key, value);
                }
                return dict;
            }
            finally
            {
                if (!reader.NextResult())
                    reader.Close();
            }
        }

        /// <summary>Reads all the records in the reader into a dictionary, using the supplied <paramref name="keyFunc"/> to generate the key</summary>
        /// <remarks>The underlying <see cref="DbDataReader"/> is disposed after this method has been called</remarks>
        public static async Task<Dictionary<TKey, T>> ToDictionaryAsync<TKey, T>(this DbDataReader reader, Func<T, TKey> keyFunc, Action<DbDataReader, T> extraAction = null)
        {
            Contract.Requires(reader != null);
            Contract.Requires(keyFunc != null);
            Contract.Ensures(Contract.Result<Task<Dictionary<TKey, T>>>() != null);
            Contract.Ensures(Contract.Result<Task<Dictionary<TKey, T>>>().Result != null);
            try
            {
                var map = GetMappingFunc<T>(reader);
                var dict = new Dictionary<TKey, T>();
                while (await reader.ReadAsync())
                {
                    T value = map(reader);
                    extraAction?.Invoke(reader, value);
                    TKey key = keyFunc(value);
                    dict.Add(key, value);
                }
                return dict;
            }
            finally
            {
                if (!await reader.NextResultAsync())
                    reader.Close();
            }
        }


        /// <summary>Reads all the records in the reader into a dictionary, using the supplied <paramref name="keyFunc"/> to generate the key</summary>
        /// <remarks>The underlying <see cref="DbDataReader"/> is disposed after this method has been called</remarks>
        public static async Task<Dictionary<TKey, TValue>> ToDictionaryAsync<T, TKey, TValue>(this DbDataReader reader, Func<T, TKey> keyFunc, Func<T, TValue> valueFunc, Action<DbDataReader, T> extraAction = null)
        {
            Contract.Requires(reader != null);
            Contract.Requires(keyFunc != null);
            Contract.Requires(valueFunc != null);
            Contract.Ensures(Contract.Result<Task<Dictionary<TKey, TValue>>>() != null);
            //Contract.Ensures(Contract.Result<Task<Dictionary<TKey, TValue>>>().Result != null);
            try
            {
                var map = GetMappingFunc<T>(reader);
                var dict = new Dictionary<TKey, TValue>();
                while (await reader.ReadAsync())
                {
                    T temp = map(reader);
                    extraAction?.Invoke(reader, temp);
                    TKey key = keyFunc(temp);
                    TValue value = valueFunc(temp);
                    dict.Add(key, value);
                }
                return dict;
            }
            finally
            {
                if (!await reader.NextResultAsync())
                    reader.Close();
            }
        }

        /// <summary>Reads all the records in the lookup, group by key, using the supplied <paramref name="keyFunc"/> to generate the key</summary>
        /// <remarks>The underlying <see cref="DbDataReader"/> is disposed after this method has been called</remarks>
        public static HashLookup<TKey, T> ToLookup<TKey, T>(this DbDataReader reader, Func<T, TKey> keyFunc, Action<DbDataReader, T> extraAction = null)
        {
            Contract.Requires(reader != null);
            Contract.Ensures(Contract.Result<HashLookup<TKey, T>>() != null);
            try
            {
                var map = GetMappingFunc<T>(reader);
                var lookup = new HashLookup<TKey, T>();
                while (reader.Read())
                {
                    T value = map(reader);
                    extraAction?.Invoke(reader, value);
                    TKey key = keyFunc(value);
                    lookup.Add(key, value);
                }
                return lookup;
            }
            finally
            {
                if (!reader.NextResult())
                    reader.Close();
            }
        }

        /// <summary>Reads all the records in the lookup, group by key, using the supplied <paramref name="keyFunc"/> to generate the key</summary>
        /// <remarks>The underlying <see cref="DbDataReader"/> is disposed after this method has been called</remarks>
        public static HashLookup<TKey, TValue> ToLookup<T, TKey, TValue>(this DbDataReader reader, Func<T, TKey> keyFunc, Func<T, TValue> valueFunc, Action<DbDataReader, T> extraAction = null)
        {
            Contract.Requires(reader != null);
            Contract.Requires(keyFunc != null);
            Contract.Requires(valueFunc != null);
            Contract.Ensures(Contract.Result<HashLookup<TKey, TValue>>() != null);
            try
            {
                var map = GetMappingFunc<T>(reader);
                var lookup = new HashLookup<TKey, TValue>();
                while (reader.Read())
                {
                    T temp = map(reader);
                    extraAction?.Invoke(reader, temp);
                    TKey key = keyFunc(temp);
                    TValue value = valueFunc(temp);
                    lookup.Add(key, value);
                }
                return lookup;
            }
            finally
            {
                if (!reader.NextResult())
                    reader.Close();
            }
        }

        /// <summary>Reads all the records in the lookup, group by key, using the supplied <paramref name="keyFunc"/> to generate the key</summary>
        /// <remarks>The underlying <see cref="DbDataReader"/> is disposed after this method has been called</remarks>
        public static async Task<HashLookup<TKey, T>> ToLookupAsync<TKey, T>(this DbDataReader reader, Func<T, TKey> keyFunc, Action<DbDataReader, T> extraAction = null)
        {
            Contract.Requires(reader != null);
            Contract.Ensures(Contract.Result<Task<HashLookup<TKey, T>>>() != null);
            //Contract.Ensures(Contract.Result<Task<HashLookup<TKey, T>>>().Result != null);
            try
            {
                var map = GetMappingFunc<T>(reader);
                var lookup = new HashLookup<TKey, T>();
                while (await reader.ReadAsync())
                {
                    T value = map(reader);
                    extraAction?.Invoke(reader, value);
                    TKey key = keyFunc(value);
                    lookup.Add(key, value);
                }
                return lookup;
            }
            finally
            {
                if (!await reader.NextResultAsync())
                    reader.Close();
            }
        }

        /// <summary>Reads all the records in the lookup, group by key, using the supplied <paramref name="keyFunc"/> to generate the key</summary>
        /// <remarks>The underlying <see cref="DbDataReader"/> is disposed after this method has been called</remarks>
        public static async Task<HashLookup<TKey, TValue>> ToLookupAsync<T, TKey, TValue>(this DbDataReader reader, Func<T, TKey> keyFunc, Func<T, TValue> valueFunc, Action<DbDataReader, T> extraAction = null)
        {
            Contract.Requires(reader != null);
            Contract.Requires(keyFunc != null);
            Contract.Requires(valueFunc != null);
            Contract.Ensures(Contract.Result<Task<HashLookup<TKey, TValue>>>() != null);
            //Contract.Ensures(Contract.Result<Task<HashLookup<TKey, TValue>>>().Result != null);
            try
            {
                var map = GetMappingFunc<T>(reader);
                var lookup = new HashLookup<TKey, TValue>();
                while (await reader.ReadAsync())
                {
                    T temp = map(reader);
                    extraAction?.Invoke(reader, temp);
                    TKey key = keyFunc(temp);
                    TValue value = valueFunc(temp);
                    lookup.Add(key, value);
                }
                return lookup;
            }
            finally
            {
                if (!await reader.NextResultAsync())
                    reader.Close();
            }
        }

        public static async Task<T> SingleOrDefaultAsync<T>(this Task<DbDataReader> task, Action<DbDataReader, T> extraAction = null)
        {
            Contract.Requires(task != null);
            Contract.Ensures(Contract.Result<Task<T>>() != null);
            var reader = await task;
            return await reader.SingleOrDefaultAsync(extraAction);
        }

        public static async Task<T> SingleAsync<T>(this Task<DbDataReader> task, Action<DbDataReader, T> extraAction = null)
        {
            Contract.Requires(task != null);
            Contract.Ensures(Contract.Result<Task<T>>() != null);
            var reader = await task;
            return await reader.SingleAsync(extraAction);
        }

        public static async Task<List<T>> ToListAsync<T>(this Task<DbDataReader> task, Action<DbDataReader, T> extraAction = null)
        {
            Contract.Requires(task != null);
            Contract.Ensures(Contract.Result<Task<List<T>>>() != null);
            //Contract.Ensures(Contract.Result<Task<List<T>>>().Result != null);
            var reader = await task;
            return await reader.ToListAsync(extraAction);
        }

        public static async Task<HashSet<T>> ToHashSetAsync<T>(this Task<DbDataReader> task, Action<DbDataReader, T> extraAction = null)
        {
            Contract.Requires(task != null);
            Contract.Ensures(Contract.Result<Task<HashSet<T>>>() != null);
            Contract.Ensures(Contract.Result<Task<HashSet<T>>>().Result != null);
            var reader = await task;
            return await reader.ToHashSetAsync(extraAction);
        }

        public static async Task<Dictionary<TKey, TValue>> ToDictionaryAsync<TKey, TValue>(this Task<DbDataReader> task, Func<TValue, TKey> keyFunc, Action<DbDataReader, TValue> extraAction = null)
        {
            Contract.Requires(keyFunc != null);
            Contract.Requires(task != null);
            Contract.Ensures(Contract.Result<Task<Dictionary<TKey, TValue>>>() != null);
            Contract.Ensures(Contract.Result<Task<Dictionary<TKey, TValue>>>().Result != null);
            var reader = await task;
            return await reader.ToDictionaryAsync(keyFunc, extraAction);
        }

        public static async Task<HashLookup<TKey, TValue>> ToLookupAsync<TKey, TValue>(this Task<DbDataReader> task, Func<TValue, TKey> keyFunc, Action<DbDataReader, TValue> extraAction = null)
        {
            Contract.Requires(keyFunc != null);
            Contract.Requires(task != null);
            Contract.Ensures(Contract.Result<Task<HashLookup<TKey, TValue>>>() != null);
            Contract.Ensures(Contract.Result<Task<HashLookup<TKey, TValue>>>().Result != null);
            var reader = await task;
            return await reader.ToLookupAsync(keyFunc, extraAction);
        }

        public static async Task<Dictionary<TKey, TValue>> ToDictionaryAsync<T, TKey, TValue>(this Task<DbDataReader> task, Func<T, TKey> keyFunc, Func<T, TValue> valueFunc, Action<DbDataReader, T> extraAction = null)
        {
            Contract.Requires(task != null);
            Contract.Requires(keyFunc != null);
            Contract.Requires(valueFunc != null);
            Contract.Ensures(Contract.Result<Task<Dictionary<TKey, TValue>>>() != null);
            Contract.Ensures(Contract.Result<Task<Dictionary<TKey, TValue>>>().Result != null);
            var reader = await task;
            return await reader.ToDictionaryAsync(keyFunc, valueFunc, extraAction);
        }

        public static async Task<HashLookup<TKey, TValue>> ToLookupAsync<T, TKey, TValue>(this Task<DbDataReader> task, Func<T, TKey> keyFunc, Func<T, TValue> valueFunc, Action<DbDataReader, T> extraAction = null)
        {
            Contract.Requires(task != null);
            Contract.Requires(keyFunc != null);
            Contract.Requires(valueFunc != null);
            Contract.Ensures(Contract.Result<Task<HashLookup<TKey, TValue>>>() != null);
            Contract.Ensures(Contract.Result<Task<HashLookup<TKey, TValue>>>().Result != null);
            var reader = await task;
            return await reader.ToLookupAsync(keyFunc, valueFunc, extraAction);
        }

    }

}