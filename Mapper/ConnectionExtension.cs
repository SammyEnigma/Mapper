﻿using System;
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
        public static T QuerySingle<T>(this IDbConnection cnn, string sql, object parameters = null)
        {
            Contract.Requires(cnn != null);
            Contract.Requires(cnn.State == ConnectionState.Open);
            Contract.Requires(!string.IsNullOrWhiteSpace(sql));
            Contract.Ensures(Contract.Result<T>() != null);
            using (var cmd = cnn.CreateCommand())
            {
                SetupCommand(cmd, cnn, sql, parameters);
                return cmd.Single<T>();
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
                return cmd.SingleAsync<T>();
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
                return cmd.SingleOrDefault<T>();
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
                return cmd.SingleOrDefaultAsync<T>();
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
                return cmd.ToList<T>();
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
                return cmd.ToListAsync<T>();
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
                return cmd.ToDictionary(keyFunc);
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
                return cmd.ToDictionaryAsync(keyFunc);
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
                return cmd.ToDictionary(keyFunc);
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
                return cmd.ToDictionaryAsync(keyFunc);
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
                return cmd.ToLookup(keyFunc);
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
                return cmd.ToLookup(keyFunc);
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
                return await cmd.ToLookupAsync(keyFunc);
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
                return await cmd.ToLookupAsync(keyFunc);
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