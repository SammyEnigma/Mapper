using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;

namespace Mapper
{
    /// <summary>
    /// Easy to use extension methods that build on the command and data reader extensions 
    /// </summary>
    public static class ConnectionExtension
    {
        public static int ExecuteNonQuery(this IDbConnection cnn, string sql, object parameters = null)
        {
            Contract.Requires(cnn != null);
            Contract.Requires(cnn.State == ConnectionState.Open);
            Contract.Requires(!string.IsNullOrWhiteSpace(sql));
            using (var cmd = cnn.CreateCommand())
            {
                SetupCommand(cmd, cnn, sql, parameters);
                return cmd.ExecuteNonQuery();
            }
        }

        public static Task<int> ExecuteNonQueryAsync(this SqlConnection cnn, string sql, object parameters = null)
        {
            Contract.Requires(cnn != null);
            Contract.Requires(cnn.State == ConnectionState.Open);
            Contract.Requires(!string.IsNullOrWhiteSpace(sql));
            using (var cmd = cnn.CreateCommand())
            {
                SetupCommand(cmd, cnn, sql, parameters);
                return cmd.ExecuteNonQueryAsync();
            }
        }

        public static T QuerySingle<T>(this IDbConnection cnn, string sql, object parameters = null)
        {
            Contract.Requires(cnn != null);
            Contract.Requires(cnn.State == ConnectionState.Open);
            Contract.Requires(!string.IsNullOrWhiteSpace(sql));
            Contract.Ensures(Contract.Result<T>() != null);
            using (var cmd = cnn.CreateCommand())
            {
                SetupCommand(cmd, cnn, sql, parameters);
                return cmd.ReadSingle<T>();
            }
        }

        public static Task<T> QuerySingleAsync<T>(this SqlConnection cnn, string sql, object parameters = null)
        {
            Contract.Requires(cnn != null);
            Contract.Requires(cnn.State == ConnectionState.Open);
            Contract.Requires(!string.IsNullOrWhiteSpace(sql));
            Contract.Ensures(Contract.Result<T>() != null);
            using (var cmd = cnn.CreateCommand())
            {
                SetupCommand(cmd, cnn, sql, parameters);
                return cmd.ReadSingleAsync<T>();
            }
        }

        public static T QuerySingleOrDefault<T>(this IDbConnection cnn, string sql, object parameters = null)
        {
            Contract.Requires(cnn != null);
            Contract.Requires(cnn.State == ConnectionState.Open);
            Contract.Requires(!string.IsNullOrWhiteSpace(sql));
            using (var cmd = cnn.CreateCommand())
            {
                SetupCommand(cmd, cnn, sql, parameters);
                return cmd.ReadSingleOrDefault<T>();
            }
        }

        public static Task<T> QuerySingleOrDefaultAsync<T>(this SqlConnection cnn, string sql, object parameters = null)
        {
            Contract.Requires(cnn != null);
            Contract.Requires(cnn.State == ConnectionState.Open);
            Contract.Requires(!string.IsNullOrWhiteSpace(sql));
            using (var cmd = cnn.CreateCommand())
            {
                SetupCommand(cmd, cnn, sql, parameters);
                return cmd.ReadSingleOrDefaultAsync<T>();
            }
        }

        /// <summary>Executes a command using the <paramref name="sql"/> and <paramref name="parameters"/> and reads all the records into a list</summary>
        public static List<T> QueryList<T>(this IDbConnection cnn, string sql, object parameters = null)
        {
            Contract.Requires(cnn != null);
            Contract.Requires(cnn.State == ConnectionState.Open);
            Contract.Requires(!string.IsNullOrWhiteSpace(sql));
            Contract.Ensures(Contract.Result<List<T>>() != null);
            using (var cmd = cnn.CreateCommand())
            {
                SetupCommand(cmd, cnn, sql, parameters);
                return cmd.ReadList<T>();
            }
        }

        /// <summary>Executes a command using the <paramref name="sql"/> and <paramref name="parameters"/> and reads all the records into a list</summary>
        public static Task<List<T>> QueryListAsync<T>(this SqlConnection cnn, string sql, object parameters = null)
        {
            Contract.Requires(cnn != null);
            Contract.Requires(cnn.State == ConnectionState.Open);
            Contract.Requires(!string.IsNullOrWhiteSpace(sql));
            Contract.Ensures(Contract.Result<List<T>>() != null);
            using (var cmd = cnn.CreateCommand())
            {
                SetupCommand(cmd, cnn, sql, parameters);
                return cmd.ReadListAsync<T>();
            }
        }

        /// <summary>Executes a command using the <paramref name="sql"/> and reads all the records into a dictionary</summary>
        public static Dictionary<TKey, TValue> QueryDictionary<TKey, TValue>(this IDbConnection cnn, string sql, Func<TValue, TKey> keyFunc)
        {
            Contract.Requires(cnn != null);
            Contract.Requires(cnn.State == ConnectionState.Open);
            Contract.Requires(!string.IsNullOrWhiteSpace(sql));
            Contract.Requires(keyFunc != null);
            Contract.Ensures(Contract.Result<Dictionary<TKey, TValue>>() != null);
            using (var cmd = cnn.CreateCommand())
            {
                SetupCommand(cmd, cnn, sql, null);
                return cmd.ReadDictionary(keyFunc);
            }
        }

        /// <summary>Executes a command using the <paramref name="sql"/> and reads all the records into a dictionary</summary>
        public static Task<Dictionary<TKey, TValue>> QueryDictionaryAsync<TKey, TValue>(this SqlConnection cnn, string sql, Func<TValue, TKey> keyFunc)
        {
            Contract.Requires(cnn != null);
            Contract.Requires(cnn.State == ConnectionState.Open);
            Contract.Requires(!string.IsNullOrWhiteSpace(sql));
            Contract.Requires(keyFunc != null);
            Contract.Ensures(Contract.Result<Dictionary<TKey, TValue>>() != null);
            using (var cmd = cnn.CreateCommand())
            {
                SetupCommand(cmd, cnn, sql, null);
                return cmd.ReadDictionaryAsync(keyFunc);
            }
        }

        /// <summary>Executes a command using the <paramref name="sql"/> and <paramref name="parameters"/> and reads all the records into a dictionary</summary>
        public static Dictionary<TKey, TValue> QueryDictionary<TKey, TValue>(this IDbConnection cnn, string sql, object parameters, Func<TValue, TKey> keyFunc)
        {
            Contract.Requires(cnn != null);
            Contract.Requires(cnn.State == ConnectionState.Open);
            Contract.Requires(!string.IsNullOrWhiteSpace(sql));
            Contract.Requires(parameters != null);
            Contract.Requires(keyFunc != null);
            Contract.Ensures(Contract.Result<Dictionary<TKey, TValue>>() != null);
            using (var cmd = cnn.CreateCommand())
            {
                SetupCommand(cmd, cnn, sql, parameters);
                return cmd.ReadDictionary(keyFunc);
            }
        }

        /// <summary>Executes a command using the <paramref name="sql"/> and <paramref name="parameters"/> and reads all the records into a dictionary</summary>
        public static Task<Dictionary<TKey, TValue>> QueryDictionaryAsync<TKey, TValue>(this SqlConnection cnn, string sql, object parameters, Func<TValue, TKey> keyFunc)
        {
            Contract.Requires(cnn != null);
            Contract.Requires(cnn.State == ConnectionState.Open);
            Contract.Requires(!string.IsNullOrWhiteSpace(sql));
            Contract.Requires(parameters != null);
            Contract.Requires(keyFunc != null);
            Contract.Ensures(Contract.Result<Dictionary<TKey, TValue>>() != null);
            using (var cmd = cnn.CreateCommand())
            {
                SetupCommand(cmd, cnn, sql, parameters);
                return cmd.ReadDictionaryAsync(keyFunc);
            }
        }

        /// <summary>Executes a command using the <paramref name="sql"/> and reads all the records into a lookup</summary>
        public static ILookup<TKey, TValue> QueryLookup<TKey, TValue>(this IDbConnection cnn, string sql, Func<TValue, TKey> keyFunc)
        {
            Contract.Requires(cnn != null);
            Contract.Requires(cnn.State == ConnectionState.Open);
            Contract.Requires(!string.IsNullOrWhiteSpace(sql));
            Contract.Requires(keyFunc != null);
            Contract.Ensures(Contract.Result<ILookup<TKey, TValue>>() != null);
            using (var cmd = cnn.CreateCommand())
            {
                SetupCommand(cmd, cnn, sql, null);
                return cmd.ReadLookup(keyFunc);
            }
        }

        /// <summary>Executes a command using the <paramref name="sql"/> and <paramref name="parameters"/> and reads all the records into a lookup</summary>
        public static ILookup<TKey, TValue> QueryLookup<TKey, TValue>(this IDbConnection cnn, string sql, object parameters, Func<TValue, TKey> keyFunc)
        {
            Contract.Requires(cnn != null);
            Contract.Requires(cnn.State == ConnectionState.Open);
            Contract.Requires(!string.IsNullOrWhiteSpace(sql));
            Contract.Requires(parameters != null);
            Contract.Requires(keyFunc != null);
            Contract.Ensures(Contract.Result<ILookup<TKey, TValue>>() != null);
            using (var cmd = cnn.CreateCommand())
            {
                SetupCommand(cmd, cnn, sql, parameters);
                return cmd.ReadLookup(keyFunc);
            }
        }

        /// <summary>Executes a command using the <paramref name="sql"/> and reads all the records into a lookup</summary>
        public static async Task<HashLookup<TKey, TValue>> QueryLookupAsync<TKey, TValue>(this SqlConnection cnn, string sql, Func<TValue, TKey> keyFunc)
        {
            Contract.Requires(cnn != null);
            Contract.Requires(cnn.State == ConnectionState.Open);
            Contract.Requires(!string.IsNullOrWhiteSpace(sql));
            Contract.Requires(keyFunc != null);
            Contract.Ensures(Contract.Result<HashLookup<TKey, TValue>>() != null);
            using (var cmd = cnn.CreateCommand())
            {
                SetupCommand(cmd, cnn, sql, null);
                return await cmd.ReadLookupAsync(keyFunc);
            }
        }

        /// <summary>Executes a command using the <paramref name="sql"/> and <paramref name="parameters"/> and reads all the records into a lookup</summary>
        public static async Task<HashLookup<TKey, TValue>> QueryLookupAsync<TKey, TValue>(this SqlConnection cnn, string sql, object parameters, Func<TValue, TKey> keyFunc)
        {
            Contract.Requires(cnn != null);
            Contract.Requires(cnn.State == ConnectionState.Open);
            Contract.Requires(!string.IsNullOrWhiteSpace(sql));
            Contract.Requires(parameters != null);
            Contract.Requires(keyFunc != null);
            Contract.Ensures(Contract.Result<HashLookup<TKey, TValue>>() != null);
            using (var cmd = cnn.CreateCommand())
            {
                SetupCommand(cmd, cnn, sql, parameters);
                return await cmd.ReadLookupAsync(keyFunc);
            }
        }

        private static void SetupCommand(IDbCommand cmd, IDbConnection cnn, string sql, object parameters)
        {
            cmd.Connection = cnn;
            cmd.CommandText = sql;
            if (parameters != null)
                cmd.AddParameters(parameters);
        }

    }
}