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
            var cmd = cnn.CreateCommand();
            SetupCommand(cmd, cnn, sql, parameters);
            return cmd.ExecuteReader(CommandBehavior.CloseConnection);
        }

        /// <summary>
        /// Asynchronously executes some <paramref name="sql"/> using the optional <paramref name="parameters"/> and return a sequence of data
        /// </summary>
        public static Task<DbDataReader> QueryAsync(this DbConnection cnn, string sql, object parameters = null)
        {
            CheckConnectionAndSql(cnn, sql);
            var cmd = cnn.CreateCommand();
            SetupCommand(cmd, cnn, sql, parameters);
            return cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection);
        }

        /// <summary>
        /// Executes some <paramref name="sql"/> using the optional <paramref name="parameters"/> and return the number of rows affected
        /// </summary>
        public static int Execute(this DbConnection cnn, string sql, object parameters = null)
        {
            CheckConnectionAndSql(cnn, sql);
            using (var cmd = cnn.CreateCommand())
            {
                SetupCommand(cmd, cnn, sql, parameters);
                return cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Asynchronously executes some <paramref name="sql"/> using the optional <paramref name="parameters"/> and return the number of rows affected
        /// </summary>
        public static Task<int> ExecuteAsync(this DbConnection cnn, string sql, object parameters = null)
        {
            CheckConnectionAndSql(cnn, sql);
            using (var cmd = cnn.CreateCommand())
            {
                SetupCommand(cmd, cnn, sql, parameters);
                return cmd.ExecuteNonQueryAsync();
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
        
    }
}