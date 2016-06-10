using System.Data;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

namespace Mapper
{
    /// <summary>
    /// Easy to use extension methods that build on the command and data reader extensions 
    /// </summary>
    public static partial class Extensions
    {
        public static DataSequence<T> Execute<T>(this DbConnection cnn, string sql, object parameters = null)
        {
            CheckConnectionAndSql(cnn, sql);
            using (var cmd = cnn.CreateCommand())
            {
                SetupCommand(cmd, cnn, sql, parameters);
                return cmd.Execute<T>();
            }
        }

        public static Task<DataSequence<T>> ExecuteAsync<T>(this DbConnection cnn, string sql, object parameters = null)
        {
            CheckConnectionAndSql(cnn, sql);
            using (var cmd = cnn.CreateCommand())
            {
                SetupCommand(cmd, cnn, sql, parameters);
                return cmd.ExecuteAsync<T>();
            }
        }

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