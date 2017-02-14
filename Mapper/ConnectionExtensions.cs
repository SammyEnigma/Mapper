using System.Data;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

namespace BusterWood.Mapper
{
    /// <summary>
    /// Easy to use extension methods that build on the command and data reader extensions 
    /// </summary>
    public static partial class Extensions
    {
        /// <summary>
        /// Executes some <paramref name="sql"/> using the optional <paramref name="parameters"/> and return a sequence of data
        /// </summary>
        public static DbDataReader Query(this DbConnection cnn, string sql, object parameters = null)
        {
            CheckConnectionAndSql(cnn, sql);
            var openConnection = cnn.State != ConnectionState.Open;
            if (openConnection)
            {
                cnn.Open();
            }
            try
            {
                using (var cmd = cnn.CreateCommand())
                {
                    SetupCommand(cmd, cnn, sql, parameters);
                    return cmd.ExecuteReader();
                }
            }
            finally
            {
                if (openConnection)
                    cnn.Close();
            }
        }

        /// <summary>
        /// Asynchronously executes some <paramref name="sql"/> using the optional <paramref name="parameters"/> and return a sequence of data
        /// </summary>
        public static async Task<DbDataReader> QueryAsync(this DbConnection cnn, string sql, object parameters = null)
        {
            CheckConnectionAndSql(cnn, sql);
            var openConnection = cnn.State != ConnectionState.Open;
            if (openConnection)
            {
                await cnn.OpenAsync();
            }
            try
            {
                using (var cmd = cnn.CreateCommand())
                {
                    SetupCommand(cmd, cnn, sql, parameters);
                    return await cmd.ExecuteReaderAsync();
                }
            }
            finally
            {
                if (openConnection)
                    cnn.Close();
            }
        }

        /// <summary>
        /// Executes some <paramref name="sql"/> using the optional <paramref name="parameters"/> and return the number of rows affected
        /// </summary>
        public static int Execute(this DbConnection cnn, string sql, object parameters = null)
        {
            CheckConnectionAndSql(cnn, sql);
            var openConnection = cnn.State != ConnectionState.Open;
            if (openConnection)
            {
                cnn.Open();
            }
            try
            {
                using (var cmd = cnn.CreateCommand())
                {
                    SetupCommand(cmd, cnn, sql, parameters);
                    return cmd.ExecuteNonQuery();
                }
            }
            finally
            {
                if (openConnection)
                    cnn.Close();
            }
        }

        /// <summary>
        /// Asynchronously executes some <paramref name="sql"/> using the optional <paramref name="parameters"/> and return the number of rows affected
        /// </summary>
        public static async Task<int> ExecuteAsync(this DbConnection cnn, string sql, object parameters = null)
        {
            CheckConnectionAndSql(cnn, sql);
            var openConnection = cnn.State != ConnectionState.Open;
            if (openConnection)
            {
                await cnn.OpenAsync();
            }
            try
            {
                using (var cmd = cnn.CreateCommand())
                {
                    SetupCommand(cmd, cnn, sql, parameters);
                    return await cmd.ExecuteNonQueryAsync();
                }
            }
            finally
            {
                if (openConnection)
                    cnn.Close();
            }
        }

        [ContractAbbreviator]
        static void CheckConnectionAndSql(DbConnection cnn, string sql)
        {
            Contract.Requires(cnn != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(sql));
        }

        static void SetupCommand(DbCommand cmd, DbConnection cnn, string sql, object parameters)
        {
            cmd.Connection = cnn;
            cmd.CommandText = sql;
            if (parameters != null)
                cmd.AddParameters(parameters);
        }
        
    }
}