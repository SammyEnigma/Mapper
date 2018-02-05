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
        public static DbDataReader Query(this DbConnection cnn, string sql, object parameters = null, int? timeoutSecs = null)
        {
            Contract.Requires(cnn != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(sql));
            return QueryInternal(cnn, sql, parameters, CommandType.Text, timeoutSecs);
        }

        /// <summary>
        /// Executes a stored procedure using the optional <paramref name="parameters"/> and return a sequence of data
        /// </summary>
        public static DbDataReader QueryProc(this DbConnection cnn, string procName, object parameters = null, int? timeoutSecs = null)
        {
            Contract.Requires(cnn != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(procName));
            return QueryInternal(cnn, procName, parameters, CommandType.StoredProcedure, timeoutSecs);
        }

        private static DbDataReader QueryInternal(DbConnection cnn, string sql, object parameters, CommandType cmdType, int? timeoutSecs)
        {
            Contract.Requires(cnn != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(sql));
            var openConnection = cnn.State != ConnectionState.Open;
            var cb = CommandBehavior.Default;
            if (openConnection)
            {
                cnn.Open();
                cb |= CommandBehavior.CloseConnection;
            }
            try
            {
                using (var cmd = CreateCommand(cnn, cmdType, sql, timeoutSecs, parameters))
                {
                    return cmd.ExecuteReader(cb);
                }
            }
            catch
            {
                if (openConnection)
                    cnn.Close();
                throw;
            }
        }

        /// <summary>
        /// Asynchronously executes some <paramref name="sql"/> using the optional <paramref name="parameters"/> and return a sequence of data
        /// </summary>
        public static Task<DbDataReader> QueryAsync(this DbConnection cnn, string sql, object parameters = null, int? timeoutSecs = null)
        {
            Contract.Requires(cnn != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(sql));
            return QueryAsyncInternal(cnn, sql, parameters, CommandType.Text, timeoutSecs);
        }

        /// <summary>
        /// Asynchronously executes a stored procedure using the optional <paramref name="parameters"/> and return a sequence of data
        /// </summary>
        public static Task<DbDataReader> QueryProcAsync(this DbConnection cnn, string procName, object parameters = null, int? timeoutSecs = null)
        {
            Contract.Requires(cnn != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(procName));
            return QueryAsyncInternal(cnn, procName, parameters, CommandType.StoredProcedure, timeoutSecs);
        }

        private static async Task<DbDataReader> QueryAsyncInternal(DbConnection cnn, string sql, object parameters, CommandType cmdType, int? timeoutSecs)
        {
            Contract.Requires(cnn != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(sql));
            var openConnection = cnn.State != ConnectionState.Open;
            var cb = CommandBehavior.Default;
            if (openConnection)
            {
                await cnn.OpenAsync();
                cb |= CommandBehavior.CloseConnection;
            }
            try
            {
                using (var cmd = CreateCommand(cnn, cmdType, sql, timeoutSecs, parameters))
                {
                    return await cmd.ExecuteReaderAsync(cb);
                }
            }
            catch
            {
                if (openConnection)
                    cnn.Close();
                throw;
            }
        }

        /// <summary>
        /// Executes some <paramref name="sql"/> using the optional <paramref name="parameters"/> and return the number of rows affected
        /// </summary>
        public static int Execute(this DbConnection cnn, string sql, object parameters = null, int? timeoutSecs = null)
        {
            Contract.Requires(cnn != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(sql));
            return ExecuteCore(cnn, sql, parameters, CommandType.Text, timeoutSecs);
        }

        /// <summary>
        /// Executes the <paramref name="procName"/> using the optional <paramref name="parameters"/> and return the number of rows affected
        /// </summary>
        public static int ExecuteProc(this DbConnection cnn, string procName, object parameters = null, int? timeoutSecs = null)
        {
            Contract.Requires(cnn != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(procName));
            return ExecuteCore(cnn, procName, parameters, CommandType.StoredProcedure, timeoutSecs);
        }

        private static int ExecuteCore(DbConnection cnn, string sql, object parameters, CommandType cmdType, int? timeoutSecs)
        {
            Contract.Requires(cnn != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(sql));
            var openConnection = cnn.State != ConnectionState.Open;
            if (openConnection)
            {
                cnn.Open();
            }
            try
            {
                using (var cmd = CreateCommand(cnn, cmdType, sql, timeoutSecs, parameters))
                {
                    return cmd.ExecuteNonQuery();
                }
            }
            catch
            {
                if (openConnection)
                    cnn.Close();
                throw;
            }
        }

        /// <summary>
        /// Asynchronously executes some <paramref name="sql"/> using the optional <paramref name="parameters"/> and return the number of rows affected
        /// </summary>
        public static Task<int> ExecuteAsync(this DbConnection cnn, string sql, object parameters = null, int? timeoutSecs = null)
        {
            Contract.Requires(cnn != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(sql));
            return ExecuteAsyncInternal(cnn, sql, parameters, CommandType.Text, timeoutSecs);
        }

        /// <summary>
        /// Asynchronously executes the <paramref name="procName"/> using the optional <paramref name="parameters"/> and return the number of rows affected
        /// </summary>
        public static Task<int> ExecuteProcAsync(this DbConnection cnn, string procName, object parameters = null, int? timeoutSecs = null)
        {
            Contract.Requires(cnn != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(procName));
            return ExecuteAsyncInternal(cnn, procName, parameters, CommandType.StoredProcedure, timeoutSecs);
        }

        private static async Task<int> ExecuteAsyncInternal(DbConnection cnn, string sql, object parameters, CommandType cmdType, int? timeoutSecs)
        {
            Contract.Requires(cnn != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(sql));
            var openConnection = cnn.State != ConnectionState.Open;
            if (openConnection)
            {
                await cnn.OpenAsync();
            }
            try
            {
                using (var cmd = CreateCommand(cnn, cmdType, sql, timeoutSecs, parameters))
                {
                    return await cmd.ExecuteNonQueryAsync();
                }
            }
            catch
            {
                if (openConnection)
                    cnn.Close();
                throw;
            }
        }

        private static DbCommand CreateCommand(DbConnection cnn, CommandType cmdType, string sql, int? timeoutSecs, object parameters)
        {
            var cmd = cnn.CreateCommand();
            try
            {
                cmd.CommandType = cmdType;
                cmd.Connection = cnn;
                cmd.CommandText = sql;
                if (timeoutSecs.HasValue)
                    cmd.CommandTimeout = timeoutSecs.Value;
                if (parameters != null)
                    cmd.AddParameters(parameters);
                return cmd;
            }
            catch
            {
                cmd.Dispose();
                throw;
            }
        }
    }
}