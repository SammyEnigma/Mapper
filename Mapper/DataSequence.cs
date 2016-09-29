using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

namespace Mapper
{
    public struct DataSequence<T> : IEnumerable<T>, IDisposable
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

        /// <remarks>The underlying <see cref="DbDataReader"/> is disposed the <see cref="DataSequenceEnumerator"/> has been enumerated, i.e. after a foreach() loop on it</remarks>
        public DataSequenceEnumerator GetEnumerator() => new DataSequenceEnumerator(map, reader);

        /// <remarks>The underlying <see cref="DbDataReader"/> is disposed the <see cref="DataSequenceEnumerator"/> has been enumerated, i.e. after a foreach() loop on it</remarks>
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

        /// <remarks>The underlying <see cref="DbDataReader"/> is disposed the <see cref="DataSequenceEnumerator"/> has been enumerated, i.e. after a foreach() loop on it</remarks>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>Reads the next value from the underlying data reader</summary>
        /// <returns>Default for T if there is no next record</returns>
        public async Task<T> NextOrDefaultAsync() => await reader.ReadAsync() ? map(reader) : default(T);

        /// <summary>Reads exactly one item from the reader</summary>
        /// <exception cref="InvalidOperationException"> when zero values read or more than one value can be read</exception>
        /// <remarks>The underlying <see cref="DbDataReader"/> is disposed after this method has been called</remarks>
        public T Single()
        {
            using (reader)
            {
                if (!reader.Read()) throw new InvalidOperationException("Expected one value to be read but reader is empty");
                var item = map(reader);
                if (reader.Read()) throw new InvalidOperationException("Expected one value to be read but more than one value can be read");
                return item;
            }
        }

        /// <summary>Reads exactly one item from the reader</summary>
        /// <exception cref="InvalidOperationException"> when zero values read or more than one value can be read</exception>
        /// <remarks>The underlying <see cref="DbDataReader"/> is disposed after this method has been called</remarks>
        public async Task<T> SingleAsync()
        {
            using (reader)
            {
                if (!await reader.ReadAsync()) throw new InvalidOperationException("Expected one value to be read but reader is empty");
                var single = map(reader);
                if (await reader.ReadAsync()) throw new InvalidOperationException("Expected one value to be read but more than one value can be read");
                return single;
            }
        }

        /// <summary>Reads zero or one items from the reader</summary>
        /// <remarks>Returns the default vaue of T if no values be read, i.e may return null</remarks>
        /// <remarks>The underlying <see cref="DbDataReader"/> is disposed after this method has been called</remarks>
        public T SingleOrDefault()
        {
            using (reader)
            {
                if (!reader.Read()) return default(T);
                return map(reader);
            }
        }

        /// <summary>Reads zero or one items from the reader</summary>
        /// <remarks>Returns the default vaue of T if no values be read, i.e may return null</remarks>
        /// <remarks>The underlying <see cref="DbDataReader"/> is disposed after this method has been called</remarks>
        public async Task<T> SingleOrDefaultAsync()
        {
            using (reader)
            {
                if (!await reader.ReadAsync()) return default(T);
                return map(reader);
            }
        }

        /// <summary>Reads all the records in the reader into a list</summary>
        /// <remarks>The underlying <see cref="DbDataReader"/> is disposed after this method has been called</remarks>
        public List<T> ToList()
        {
            Contract.Ensures(Contract.Result<List<T>>() != null);
            using (reader)
            {
                var list = new List<T>();
                while (reader.Read())
                {
                    list.Add(map(reader));
                }
                return list;
            }
        }

        /// <summary>Reads all the records in the reader into a list</summary>
        /// <remarks>The underlying <see cref="DbDataReader"/> is disposed after this method has been called</remarks>
        public async Task<List<T>> ToListAsync()
        {
            Contract.Ensures(Contract.Result<Task<List<T>>>() != null);
            Contract.Ensures(Contract.Result<Task<List<T>>>().Result != null);
            using (reader)
            {
                var list = new List<T>();
                while (await reader.ReadAsync())
                {
                    list.Add(map(reader));
                }
                return list;
            }
        }

        /// <summary>Reads all the records in the reader into a <see cref="HashSet{T}"/></summary>
        /// <remarks>The underlying <see cref="DbDataReader"/> is disposed after this method has been called</remarks>
        public HashSet<T> ToHashSet()
        {
            Contract.Ensures(Contract.Result<HashSet<T>>() != null);
            using (reader)
            {
                var set = new HashSet<T>();
                while (reader.Read())
                {
                    set.Add(map(reader));
                }
                return set;
            }
        }

        /// <summary>Reads all the records in the reader into a <see cref="HashSet{T}"/></summary>
        /// <remarks>The underlying <see cref="DbDataReader"/> is disposed after this method has been called</remarks>
        public async Task<HashSet<T>> ToHashSetAsync()
        {
            Contract.Ensures(Contract.Result<Task<HashSet<T>>>() != null);
            Contract.Ensures(Contract.Result<Task<HashSet<T>>>().Result != null);
            using (reader)
            {
                var set = new HashSet<T>();
                while (await reader.ReadAsync())
                {
                    set.Add(map(reader));
                }
                return set;
            }
        }

        /// <summary>Reads all the records in the reader into a dictionary, using the supplied <paramref name="keyFunc"/> to generate the key</summary>
        /// <remarks>The underlying <see cref="DbDataReader"/> is disposed after this method has been called</remarks>
        public Dictionary<TKey, T> ToDictionary<TKey>(Func<T, TKey> keyFunc)
        {
            Contract.Ensures(Contract.Result<Dictionary<TKey, T>>() != null);
            using (reader)
            {
                var dict = new Dictionary<TKey, T>();
                while (reader.Read())
                {
                    T value = map(reader);
                    TKey key = keyFunc(value);
                    dict.Add(key, value);
                }
                return dict;
            }
        }

        /// <summary>Reads all the records in the reader into a dictionary, using the supplied <paramref name="keyFunc"/> to generate the key</summary>
        /// <remarks>The underlying <see cref="DbDataReader"/> is disposed after this method has been called</remarks>
        public Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(Func<T, TKey> keyFunc, Func<T, TValue> valueFunc)
        {
            Contract.Requires(valueFunc != null);
            Contract.Requires(keyFunc != null);
            Contract.Ensures(Contract.Result<Dictionary<TKey, TValue>>() != null);
            using (reader)
            {
                var dict = new Dictionary<TKey, TValue>();
                while (reader.Read())
                {
                    T item = map(reader);
                    TKey key = keyFunc(item);
                    TValue value = valueFunc(item);
                    dict.Add(key, value);
                }
                return dict;
            }
        }

        /// <summary>Reads all the records in the reader into a dictionary, using the supplied <paramref name="keyFunc"/> to generate the key</summary>
        /// <remarks>The underlying <see cref="DbDataReader"/> is disposed after this method has been called</remarks>
        public async Task<Dictionary<TKey, T>> ToDictionaryAsync<TKey>(Func<T, TKey> keyFunc)
        {
            Contract.Requires(keyFunc != null);
            Contract.Ensures(Contract.Result<Task<Dictionary<TKey, T>>>() != null);
            Contract.Ensures(Contract.Result<Task<Dictionary<TKey, T>>>().Result != null);
            using (reader)
            {
                var dict = new Dictionary<TKey, T>();
                while (await reader.ReadAsync())
                {
                    T item = map(reader);
                    TKey key = keyFunc(item);
                    dict.Add(key, item);
                }
                return dict;
            }
        }


        /// <summary>Reads all the records in the reader into a dictionary, using the supplied <paramref name="keyFunc"/> to generate the key</summary>
        /// <remarks>The underlying <see cref="DbDataReader"/> is disposed after this method has been called</remarks>
        public async Task<Dictionary<TKey, TValue>> ToDictionaryAsync<TKey, TValue>(Func<T, TKey> keyFunc, Func<T, TValue> valueFunc)
        {
            Contract.Requires(valueFunc != null);
            Contract.Requires(keyFunc != null);
            Contract.Ensures(Contract.Result<Task<Dictionary<TKey, TValue>>>() != null);
            Contract.Ensures(Contract.Result<Task<Dictionary<TKey, TValue>>>().Result != null);
            using (reader)
            {
                var dict = new Dictionary<TKey, TValue>();
                while (await reader.ReadAsync())
                {
                    T item = map(reader);
                    TKey key = keyFunc(item);
                    TValue value = valueFunc(item);
                    dict.Add(key, value);
                }
                return dict;
            }
        }

        /// <summary>Reads all the records in the lookup, group by key, using the supplied <paramref name="keyFunc"/> to generate the key</summary>
        /// <remarks>The underlying <see cref="DbDataReader"/> is disposed after this method has been called</remarks>
        public HashLookup<TKey, T> ToLookup<TKey>(Func<T, TKey> keyFunc)
        {
            Contract.Ensures(Contract.Result<HashLookup<TKey, T>>() != null);
            using (reader)
            {
                var lookup = new HashLookup<TKey, T>();
                while (reader.Read())
                {
                    T value = map(reader);
                    TKey key = keyFunc(value);
                    lookup.Add(key, value);
                }
                return lookup;
            }
        }

        /// <summary>Reads all the records in the lookup, group by key, using the supplied <paramref name="keyFunc"/> to generate the key</summary>
        /// <remarks>The underlying <see cref="DbDataReader"/> is disposed after this method has been called</remarks>
        public async Task<HashLookup<TKey, T>> ToLookupAsync<TKey>(Func<T, TKey> keyFunc)
        {
            Contract.Ensures(Contract.Result<Task<HashLookup<TKey, T>>>() != null);
            Contract.Ensures(Contract.Result<Task<HashLookup<TKey, T>>>().Result != null);
            using (reader)
            {
                var lookup = new HashLookup<TKey, T>();
                while (await reader.ReadAsync())
                {
                    T value = map(reader);
                    TKey key = keyFunc(value);
                    lookup.Add(key, value);
                }
                return lookup;
            }
        }

        /// <summary>
        /// Not call is normally need for this, enumerating this struct will close the reader
        /// </summary>
        void IDisposable.Dispose()
        {
            reader.Dispose();
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

            /// <summary>Gets the element in the collection at the current position of the enumerator.</summary>
            /// <returns>The element in the collection at the current position of the enumerator.</returns>
            public T Current { get; private set; }

            object IEnumerator.Current => Current;

            /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
            public void Dispose()
            {
                _reader.Dispose();
                Current = default(T);
            }

            /// <summary>Advances the enumerator to the next element of the collection.</summary>
            /// <returns>true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.</returns>
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

            /// <summary>Sets the enumerator to its initial position, which is before the first element in the collection.</summary>
            /// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception>
            public void Reset()
            {
                throw new InvalidOperationException();
            }
        }
    }

    public static partial class Extensions
    {
        public static async Task<T> SingleOrDefaultAsync<T>(this Task<DataSequence<T>> task)
        {
            Contract.Requires(task != null);
            Contract.Ensures(Contract.Result<Task<T>>() != null);
            var seq = await task;
            return await seq.SingleOrDefaultAsync();
        }

        public static async Task<T> SingleAsync<T>(this Task<DataSequence<T>> task)
        {
            Contract.Requires(task != null);
            Contract.Ensures(Contract.Result<Task<T>>() != null);
            var seq = await task;
            return await seq.SingleAsync();
        }

        public static async Task<List<T>> ToListAsync<T>(this Task<DataSequence<T>> task)
        {
            Contract.Requires(task != null);
            Contract.Ensures(Contract.Result<Task<List<T>>>() != null);
            Contract.Ensures(Contract.Result<Task<List<T>>>().Result != null);
            var seq = await task;
            return await seq.ToListAsync();
        }

        public static async Task<HashSet<T>> ToHashSetAsync<T>(this Task<DataSequence<T>> task)
        {
            Contract.Requires(task != null);
            Contract.Ensures(Contract.Result<Task<HashSet<T>>>() != null);
            Contract.Ensures(Contract.Result<Task<HashSet<T>>>().Result != null);
            var seq = await task;
            return await seq.ToHashSetAsync();
        }

        public static async Task<Dictionary<TKey, TValue>> ToDictionaryAsync<TKey, TValue>(this Task<DataSequence<TValue>> task, Func<TValue, TKey> keyFunc)
        {
            Contract.Requires(keyFunc != null);
            Contract.Requires(task != null);
            Contract.Ensures(Contract.Result<Task<Dictionary<TKey, TValue>>>() != null);
            Contract.Ensures(Contract.Result<Task<Dictionary<TKey, TValue>>>().Result != null);
            var seq = await task;
            return await seq.ToDictionaryAsync(keyFunc);
        }

        public static async Task<Dictionary<TKey, TValue>> ToDictionaryAsync<T, TKey, TValue>(this Task<DataSequence<T>> task, Func<T, TKey> keyFunc, Func<T, TValue> valueFunc)
        {
            Contract.Requires(valueFunc != null);
            Contract.Requires(keyFunc != null);
            Contract.Requires(task != null);
            Contract.Ensures(Contract.Result<Task<Dictionary<TKey, TValue>>>() != null);
            Contract.Ensures(Contract.Result<Task<Dictionary<TKey, TValue>>>().Result != null);
            var seq = await task;
            return await seq.ToDictionaryAsync(keyFunc, valueFunc);
        }

        public static async Task<HashLookup<TKey, TValue>> ToLookupAsync<TKey, TValue>(this Task<DataSequence<TValue>> task, Func<TValue, TKey> keyFunc)
        {
            Contract.Requires(keyFunc != null);
            Contract.Requires(task != null);
            Contract.Ensures(Contract.Result<Task<HashLookup<TKey, TValue>>>() != null);
            Contract.Ensures(Contract.Result<Task<HashLookup<TKey, TValue>>>().Result != null);
            var seq = await task;
            return await seq.ToLookupAsync(keyFunc);
        }

    }
}
