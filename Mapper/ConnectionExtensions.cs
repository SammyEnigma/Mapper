using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

namespace Mapper
{
    /// <summary>
    /// Easy to use extension methods that build on the command and data reader extensions 
    /// </summary>
    public static class ConnectionExtensions
    {
        public static int ExecuteNonQuery(this DbConnection cnn, string sql, object parameters = null)
        {
            CheckConnectionAndSql(cnn, sql);
            using (var cmd = cnn.CreateCommand())
            {
                SetupCommand(cmd, cnn, sql, parameters);
                return cmd.ExecuteNonQuery();
            }
        }

        public static Task<int> ExecuteNonQueryAsync(this DbConnection cnn, string sql, object parameters = null)
        {
            CheckConnectionAndSql(cnn, sql);
            using (var cmd = cnn.CreateCommand())
            {
                SetupCommand(cmd, cnn, sql, parameters);
                return cmd.ExecuteNonQueryAsync();
            }
        }

        public static T QueryScalar<T>(this DbConnection cnn, string sql, object parameters = null)
        {
            CheckConnectionAndSql(cnn, sql);
            using (var cmd = cnn.CreateCommand())
            {
                SetupCommand(cmd, cnn, sql, parameters);
                return cmd.ExecuteScalar<T>();
            }
        }

        public static Task<T> QueryScalarAsync<T>(this DbConnection cnn, string sql, object parameters = null)
        {
            CheckConnectionAndSql(cnn, sql);
            Contract.Ensures(Contract.Result<Task<T>>() != null);
            using (var cmd = cnn.CreateCommand())
            {
                SetupCommand(cmd, cnn, sql, parameters);
                return cmd.ExecuteScalarAsync<T>();
            }
        }

        public static T QuerySingle<T>(this DbConnection cnn, string sql, object parameters = null)
        {
            CheckConnectionAndSql(cnn, sql);
            Contract.Ensures(Contract.Result<T>() != null);
            using (var cmd = cnn.CreateCommand())
            {
                SetupCommand(cmd, cnn, sql, parameters);
                return cmd.ExecuteSingle<T>();
            }
        }

        public static Task<T> QuerySingleAsync<T>(this DbConnection cnn, string sql, object parameters = null)
        {
            CheckConnectionAndSql(cnn, sql);
            Contract.Ensures(Contract.Result<Task<T>>() != null);
            using (var cmd = cnn.CreateCommand())
            {
                SetupCommand(cmd, cnn, sql, parameters);
                return cmd.ExecuteSingleAsync<T>();
            }
        }

        public static T QuerySingleOrDefault<T>(this DbConnection cnn, string sql, object parameters = null)
        {
            CheckConnectionAndSql(cnn, sql);
            using (var cmd = cnn.CreateCommand())
            {
                SetupCommand(cmd, cnn, sql, parameters);
                return cmd.ExecuteSingleOrDefault<T>();
            }
        }

        public static Task<T> QuerySingleOrDefaultAsync<T>(this DbConnection cnn, string sql, object parameters = null)
        {
            CheckConnectionAndSql(cnn, sql);
            using (var cmd = cnn.CreateCommand())
            {
                SetupCommand(cmd, cnn, sql, parameters);
                return cmd.ExecuteSingleOrDefaultAsync<T>();
            }
        }

        /// <summary>Executes a command using the <paramref name="sql"/> and <paramref name="parameters"/> and reads all the records into a list</summary>
        public static List<T> QueryList<T>(this DbConnection cnn, string sql, object parameters = null)
        {
            CheckConnectionAndSql(cnn, sql);
            Contract.Ensures(Contract.Result<List<T>>() != null);
            using (var cmd = cnn.CreateCommand())
            {
                SetupCommand(cmd, cnn, sql, parameters);
                return cmd.ExecuteList<T>();
            }
        }

        /// <summary>Executes a command using the <paramref name="sql"/> and <paramref name="parameters"/> and reads all the records into a list</summary>
        public static Task<List<T>> QueryListAsync<T>(this DbConnection cnn, string sql, object parameters = null)
        {
            CheckConnectionAndSql(cnn, sql);
            Contract.Ensures(Contract.Result<Task<List<T>>>() != null);
            Contract.Ensures(Contract.Result<Task<List<T>>>().Result != null);
            using (var cmd = cnn.CreateCommand())
            {
                SetupCommand(cmd, cnn, sql, parameters);
                return cmd.ExecuteListAsync<T>();
            }
        }

        /// <summary>Executes a command using the <paramref name="sql"/> and reads all the records into a dictionary</summary>
        public static Dictionary<TKey, TValue> QueryDictionary<TKey, TValue>(this DbConnection cnn, string sql, Func<TValue, TKey> keyFunc)
        {
            CheckConnectionAndSql(cnn, sql);
            Contract.Requires(keyFunc != null);
            Contract.Ensures(Contract.Result<Dictionary<TKey, TValue>>() != null);
            using (var cmd = cnn.CreateCommand())
            {
                SetupCommand(cmd, cnn, sql, null);
                return cmd.ExecuteDictionary(keyFunc);
            }
        }

        /// <summary>Executes a command using the <paramref name="sql"/> and reads all the records into a dictionary</summary>
        public static Task<Dictionary<TKey, TValue>> QueryDictionaryAsync<TKey, TValue>(this DbConnection cnn, string sql, Func<TValue, TKey> keyFunc)
        {
            CheckConnectionAndSql(cnn, sql);
            Contract.Requires(keyFunc != null);
            Contract.Ensures(Contract.Result<Task<Dictionary<TKey, TValue>>>() != null);
            Contract.Ensures(Contract.Result<Task<Dictionary<TKey, TValue>>>().Result != null);
            using (var cmd = cnn.CreateCommand())
            {
                SetupCommand(cmd, cnn, sql, null);
                return cmd.ExecuteDictionaryAsync(keyFunc);
            }
        }

        /// <summary>Executes a command using the <paramref name="sql"/> and <paramref name="parameters"/> and reads all the records into a dictionary</summary>
        public static Dictionary<TKey, TValue> QueryDictionary<TKey, TValue>(this DbConnection cnn, string sql, object parameters, Func<TValue, TKey> keyFunc)
        {
            CheckConnectionAndSql(cnn, sql);
            Contract.Requires(parameters != null);
            Contract.Requires(keyFunc != null);
            Contract.Ensures(Contract.Result<Dictionary<TKey, TValue>>() != null);
            using (var cmd = cnn.CreateCommand())
            {
                SetupCommand(cmd, cnn, sql, parameters);
                return cmd.ExecuteDictionary(keyFunc);
            }
        }

        /// <summary>Executes a command using the <paramref name="sql"/> and <paramref name="parameters"/> and reads all the records into a dictionary</summary>
        public static Task<Dictionary<TKey, TValue>> QueryDictionaryAsync<TKey, TValue>(this DbConnection cnn, string sql, object parameters, Func<TValue, TKey> keyFunc)
        {
            CheckConnectionAndSql(cnn, sql);
            Contract.Requires(parameters != null);
            Contract.Requires(keyFunc != null);
            Contract.Ensures(Contract.Result<Task<Dictionary<TKey, TValue>>>() != null);
            Contract.Ensures(Contract.Result<Task<Dictionary<TKey, TValue>>>().Result != null);
            using (var cmd = cnn.CreateCommand())
            {
                SetupCommand(cmd, cnn, sql, parameters);
                return cmd.ExecuteDictionaryAsync(keyFunc);
            }
        }

        /// <summary>Executes a command using the <paramref name="sql"/> and reads all the records into a lookup</summary>
        public static HashLookup<TKey, TValue> QueryLookup<TKey, TValue>(this DbConnection cnn, string sql, Func<TValue, TKey> keyFunc)
        {
            CheckConnectionAndSql(cnn, sql);
            Contract.Requires(keyFunc != null);
            Contract.Ensures(Contract.Result<HashLookup<TKey, TValue>>() != null);
            using (var cmd = cnn.CreateCommand())
            {
                SetupCommand(cmd, cnn, sql, null);
                return cmd.ExecuteLookup(keyFunc);
            }
        }

        /// <summary>Executes a command using the <paramref name="sql"/> and <paramref name="parameters"/> and reads all the records into a lookup</summary>
        public static HashLookup<TKey, TValue> QueryLookup<TKey, TValue>(this DbConnection cnn, string sql, object parameters, Func<TValue, TKey> keyFunc)
        {
            CheckConnectionAndSql(cnn, sql);
            Contract.Requires(parameters != null);
            Contract.Requires(keyFunc != null);
            Contract.Ensures(Contract.Result<HashLookup<TKey, TValue>>() != null);
            using (var cmd = cnn.CreateCommand())
            {
                SetupCommand(cmd, cnn, sql, parameters);
                return cmd.ExecuteLookup(keyFunc);
            }
        }

        /// <summary>Executes a command using the <paramref name="sql"/> and reads all the records into a lookup</summary>
        public static async Task<HashLookup<TKey, TValue>> QueryLookupAsync<TKey, TValue>(this DbConnection cnn, string sql, Func<TValue, TKey> keyFunc)
        {
            CheckConnectionAndSql(cnn, sql);
            Contract.Requires(keyFunc != null);
            Contract.Ensures(Contract.Result<Task<HashLookup<TKey, TValue>>>() != null);
            Contract.Ensures(Contract.Result<Task<HashLookup<TKey, TValue>>>().Result != null);
            using (var cmd = cnn.CreateCommand())
            {
                SetupCommand(cmd, cnn, sql, null);
                return await cmd.ExecuteLookupAsync(keyFunc);
            }
        }

        /// <summary>Executes a command using the <paramref name="sql"/> and <paramref name="parameters"/> and reads all the records into a lookup</summary>
        public static async Task<HashLookup<TKey, TValue>> QueryLookupAsync<TKey, TValue>(this DbConnection cnn, string sql, object parameters, Func<TValue, TKey> keyFunc)
        {
            CheckConnectionAndSql(cnn, sql);
            Contract.Requires(parameters != null);
            Contract.Requires(keyFunc != null);
            Contract.Ensures(Contract.Result<Task<HashLookup<TKey, TValue>>>() != null);
            Contract.Ensures(Contract.Result<Task<HashLookup<TKey, TValue>>>().Result != null);
            using (var cmd = cnn.CreateCommand())
            {
                SetupCommand(cmd, cnn, sql, parameters);
                return await cmd.ExecuteLookupAsync(keyFunc);
            }
        }

        [ContractAbbreviator]
        static void CheckConnectionAndSql(DbConnection cnn, string sql)
        {
            Contract.Requires(cnn != null);
            Contract.Requires(cnn.State == ConnectionState.Open);
            Contract.Requires(!string.IsNullOrWhiteSpace(sql));
        }

        static void SetupCommand(DbCommand cmd, DbConnection cnn, string sql, object parameters)
        {
            cmd.Connection = cnn;
            cmd.CommandText = sql;
            if (parameters != null)
                cmd.AddParameters(parameters);
        }

        public static string GenerateSqlProcCall(this DbConnection cnn, string procName, object parameters)
        {
            CheckConnectionAndSql(cnn, procName);
            return SqlStoredProcCallGenerator.Generate(cnn, procName, parameters);
        }
    }
}