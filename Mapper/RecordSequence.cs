using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

namespace Mapper
{
    public struct DataSequence<T> : IEnumerable<T>
    {
        readonly Func<DbDataReader, T> map;
        readonly DbDataReader reader;

        public DataSequence(DbDataReader reader)
        {
            Contract.Requires(reader != null);
            Contract.Requires(reader.IsClosed == false);
            this.reader = reader;
            map = Extensions.GetMappingFunc<T>(reader);
        }

        public DataSequenceEnumerator GetEnumerator() => new DataSequenceEnumerator(map, reader);
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>Reads exactly one item from the reader</summary>
        /// <exception cref="InvalidOperationException"> when zero values read or more than one value can be read</exception>
        public T Single()
        {
            if (!reader.Read()) throw new InvalidOperationException("Expected one value to be read but reader is empty");
            var item = map(reader);
            if (reader.Read()) throw new InvalidOperationException("Expected one value to be read but more than one value can be read");
            return item;
        }

        /// <summary>Reads exactly one item from the reader</summary>
        /// <exception cref="InvalidOperationException"> when zero values read or more than one value can be read</exception>
        public async Task<T> SingleAsync()
        {
            if (!await reader.ReadAsync()) throw new InvalidOperationException("Expected one value to be read but reader is empty");
            var single = map(reader);
            if (await reader.ReadAsync()) throw new InvalidOperationException("Expected one value to be read but more than one value can be read");
            return single;
        }

        /// <summary>Reads zero or one items from the reader</summary>
        /// <remarks>Returns the default vaue of T if no values be read, i.e may return null</remarks>
        public T SingleOrDefault()
        {
            if (!reader.Read()) return default(T);
            return map(reader);
        }

        /// <summary>Reads zero or one items from the reader</summary>
        /// <remarks>Returns the default vaue of T if no values be read, i.e may return null</remarks>
        public async Task<T> SingleOrDefaultAsync()
        {
            if (!await reader.ReadAsync()) return default(T);
            return map(reader);
        }

        /// <summary>Reads all the records in the reader into a list</summary>
        public List<T> ToList()
        {
            Contract.Ensures(Contract.Result<List<T>>() != null);
            var list = new List<T>();
            while (reader.Read())
            {
                list.Add(map(reader));
            }
            return list;
        }

        /// <summary>Reads all the records in the reader into a list</summary>
        public async Task<List<T>> ToListAsync()
        {
            Contract.Ensures(Contract.Result<Task<List<T>>>() != null);
            Contract.Ensures(Contract.Result<Task<List<T>>>().Result != null);
            var list = new List<T>();
            while (await reader.ReadAsync())
            {
                list.Add(map(reader));
            }
            return list;
        }

        /// <summary>Reads all the records in the reader into a dictionary, using the supplied <paramref name="keyFunc"/> to generate the key</summary>
        public Dictionary<TKey, T> ToDictionary<TKey>(Func<T, TKey> keyFunc)
        {
            Contract.Ensures(Contract.Result<Dictionary<TKey, T>>() != null);
            var dict = new Dictionary<TKey, T>();
            while (reader.Read())
            {
                T value = map(reader);
                TKey key = keyFunc(value);
                dict.Add(key, value);
            }
            return dict;
        }

        /// <summary>Reads all the records in the reader into a dictionary, using the supplied <paramref name="keyFunc"/> to generate the key</summary>
        public async Task<Dictionary<TKey, T>> ToDictionaryAsync<TKey>(Func<T, TKey> keyFunc)
        {
            Contract.Ensures(Contract.Result<Task<Dictionary<TKey, T>>>() != null);
            Contract.Ensures(Contract.Result<Task<Dictionary<TKey, T>>>().Result != null);
            var dict = new Dictionary<TKey, T>();
            while (await reader.ReadAsync())
            {
                T value = map(reader);
                TKey key = keyFunc(value);
                dict.Add(key, value);
            }
            return dict;
        }

        /// <summary>Reads all the records in the lookup, group by key, using the supplied <paramref name="keyFunc"/> to generate the key</summary>
        public HashLookup<TKey, T> ToLookup<TKey>(Func<T, TKey> keyFunc)
        {
            Contract.Ensures(Contract.Result<HashLookup<TKey, T>>() != null);
            var lookup = new HashLookup<TKey, T>();
            while (reader.Read())
            {
                T value = map(reader);
                TKey key = keyFunc(value);
                lookup.Add(key, value);
            }
            return lookup;
        }

        /// <summary>Reads all the records in the lookup, group by key, using the supplied <paramref name="keyFunc"/> to generate the key</summary>
        public async Task<HashLookup<TKey, T>> ToLookupAsync<TKey>(Func<T, TKey> keyFunc)
        {
            Contract.Ensures(Contract.Result<Task<HashLookup<TKey, T>>>() != null);
            Contract.Ensures(Contract.Result<Task<HashLookup<TKey, T>>>().Result != null);
            var lookup = new HashLookup<TKey, T>();
            while (await reader.ReadAsync())
            {
                T value = map(reader);
                TKey key = keyFunc(value);
                lookup.Add(key, value);
            }
            return lookup;
        }

        public struct DataSequenceEnumerator : IEnumerator<T>
        {
            readonly Func<DbDataReader, T> _map;
            readonly DbDataReader _reader;

            internal DataSequenceEnumerator(Func<DbDataReader, T> map, DbDataReader reader)
            {
                _map = map;
                _reader = reader;
                Current = default(T);
            }

            public T Current { get; private set; }

            object IEnumerator.Current => Current;

            public void Dispose()
            {
                _reader.Dispose();
                Current = default(T);
            }

            public bool MoveNext()
            {
                if (_reader.Read())
                {
                    Current = _map(_reader);
                    return true;
                }
                else
                {
                    Current = default(T);
                    return false;
                }
            }

            public void Reset()
            {
                throw new NotImplementedException();
            }
        }
    }
}
