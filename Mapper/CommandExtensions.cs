using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using System.Data.Common;

namespace Mapper
{
    public static class DbCommandExtensions
    {
        static readonly CommandParameterMapper _mapper = new CommandParameterMapper();
        
        /// <summary>Adds parameters to the <paramref name="cmd"/>, one parameter per property of <paramref name="parameters"/></summary>
        public static DbCommand AddParameters(this DbCommand cmd, object parameters)
        {
            Contract.Requires(cmd != null);
            Contract.Requires(parameters != null);
            return _mapper.AddParameters(cmd, parameters);
        }

        /// <summary>
        /// Executes the query, and returns the first column of the first row in the resultset returned by the query. Extra columns or rows are ignored
        /// </summary>
        /// <returns>Some T, or default(T) if the database returns null</returns>
        public static T ExecuteScalar<T>(this DbCommand cmd)
        {
            Contract.Requires(cmd != null);
            object obj = cmd.ExecuteScalar();
            return obj == null || obj is DBNull? default(T) : (T)obj;
        }

        /// <summary>
        /// Executes the query, and returns the first column of the first row in the resultset returned by the query. Extra columns or rows are ignored
        /// </summary>
        /// <returns>Some T, or default(T) if the database returns null</returns>
        public static async Task<T> ExecuteScalarAsync<T>(this DbCommand cmd)
        {
            Contract.Requires(cmd != null);
            Contract.Ensures(Contract.Result<Task<T>>() != null);
            object obj = await cmd.ExecuteScalarAsync();
            return obj == null || obj is DBNull? default(T) : (T)obj;
        }

        /// <summary>Executes the <paramref name="cmd"/> reading exactly one item</summary>
        /// <exception cref="InvalidOperationException"> when zero values read or more than one value can be read</exception>
        public static T ExecuteSingle<T>(this DbCommand cmd)
        {
            Contract.Requires(cmd != null);
            Contract.Requires(!typeof(T).CanReadScalar(), "Please use the ExecuteScalar<T>() extension method for reading single values");
            using (var reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
            {
                return reader.ReadSingle<T>();
            }
        }

        /// <summary>Executes the <paramref name="cmd"/> reading exactly one item</summary>
        /// <exception cref="InvalidOperationException"> when zero values read or more than one value can be read</exception>
        public static async Task<T> ExecuteSingleAsync<T>(this DbCommand cmd)
        {
            Contract.Requires(cmd != null);
            Contract.Requires(!typeof(T).CanReadScalar(), "Please use the ExecuteScalarAsync<T>() extension method for reading single values");
            Contract.Ensures(Contract.Result<Task<T>>() != null);
            using (var reader = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow))
            {
                return await reader.ReadSingleAsync<T>();
            }
        }

        /// <summary>Executes the <paramref name="cmd"/> reading one item</summary>
        /// <remarks>Returns the default vaue of T if no values be read, i.e may return null</remarks>
        public static T ExecuteSingleOrDefault<T>(this DbCommand cmd)
        {
            Contract.Requires(cmd != null);
            using (var reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
            {
                return reader.ReadSingleOrDefault<T>();
            }
        }

        /// <summary>Executes the <paramref name="cmd"/> reading one item</summary>
        /// <remarks>Returns the default vaue of T if no values be read, i.e may return null</remarks>
        public static async Task<T> ExecuteSingleOrDefaultAsync<T>(this DbCommand cmd)
        {
            Contract.Requires(cmd != null);
            using (var reader = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow))
            {
                return await reader.ReadSingleOrDefaultAsync<T>();
            }
        }

        /// <summary>Executes the <paramref name="cmd"/> and reads all the records into a list</summary>
        public static List<T> ExecuteList<T>(this DbCommand cmd)
        {
            Contract.Requires(cmd != null);
            Contract.Ensures(Contract.Result<List<T>>() != null);
            using (var reader = cmd.ExecuteReader())
            {
                return reader.ReadList<T>();
            }
        }

        /// <summary>Executes the <paramref name="cmd"/> and reads all the records into a list</summary>
        public static async Task<List<T>> ExecuteListAsync<T>(this DbCommand cmd)
        {
            Contract.Requires(cmd != null);
            Contract.Ensures(Contract.Result<Task<List<T>>>() != null);
            Contract.Ensures(Contract.Result<Task<List<T>>>().Result != null);
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                return await reader.ReadListAsync<T>();
            }
        }

        /// <summary>Executes the <paramref name="cmd"/> and reads all the records into a dictionary, using the supplied <paramref name="keyFunc"/> to generate the key</summary>
        public static Dictionary<TKey, TValue> ExecuteDictionary<TKey, TValue>(this DbCommand cmd, Func<TValue, TKey> keyFunc)
        {
            Contract.Requires(cmd != null);
            Contract.Requires(keyFunc != null);
            Contract.Ensures(Contract.Result<Dictionary<TKey, TValue>>() != null);
            using (var reader = cmd.ExecuteReader())
            {
                return reader.ReadDictionary(keyFunc);
            }
        }

        /// <summary>Executes the <paramref name="cmd"/> and reads all the records into a dictionary, using the supplied <paramref name="keyFunc"/> to generate the key</summary>
        public static async Task<Dictionary<TKey, TValue>> ExecuteDictionaryAsync<TKey, TValue>(this DbCommand cmd, Func<TValue, TKey> keyFunc)
        {
            Contract.Requires(cmd != null);
            Contract.Requires(keyFunc != null);
            Contract.Ensures(Contract.Result<Task<Dictionary<TKey, TValue>>>() != null);
            Contract.Ensures(Contract.Result<Task<Dictionary<TKey, TValue>>>().Result != null);
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                return await reader.ReadDictionaryAsync(keyFunc);
            }
        }

        /// <summary>Executes the <paramref name="cmd"/> and reads all the records in a lookup, grouped by key, using the supplied <paramref name="keyFunc"/> to generate the key</summary>
        public static HashLookup<TKey, TValue> ExecuteLookup<TKey, TValue>(this DbCommand cmd, Func<TValue, TKey> keyFunc)
        {
            Contract.Requires(cmd != null);
            Contract.Requires(keyFunc != null);
            Contract.Ensures(Contract.Result<HashLookup<TKey, TValue>>() != null);
            using (var reader = cmd.ExecuteReader())
            {
                return reader.ReadLookup(keyFunc);
            }
        }

        /// <summary>Executes the <paramref name="cmd"/> and reads all the records in a lookup, grouped by key, using the supplied <paramref name="keyFunc"/> to generate the key</summary>
        public static async Task<HashLookup<TKey, TValue>> ExecuteLookupAsync<TKey, TValue>(this DbCommand cmd, Func<TValue, TKey> keyFunc)
        {
            Contract.Requires(cmd != null);
            Contract.Requires(keyFunc != null);
            Contract.Ensures(Contract.Result<Task<HashLookup<TKey, TValue>>>() != null);
            Contract.Ensures(Contract.Result<Task<HashLookup<TKey, TValue>>>().Result != null);
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                return await reader.ReadLookupAsync(keyFunc);
            }
        }

    }

}