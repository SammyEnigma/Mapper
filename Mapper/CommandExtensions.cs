using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using System.Data.Common;

namespace Mapper
{
    public static partial class Extensions
    {
        static readonly CommandParameterMapper _cmdMapper = new CommandParameterMapper();
        
        /// <summary>Adds parameters to the <paramref name="cmd"/>, one parameter per property of <paramref name="parameters"/></summary>
        public static DbCommand AddParameters(this DbCommand cmd, object parameters)
        {
            Contract.Requires(cmd != null);
            Contract.Requires(parameters != null);
            return _cmdMapper.AddParameters(cmd, parameters);
        }

        /// <summary>
        /// Executes a <paramref name="cmd"/> and return a sequence of data
        /// </summary>
        public static DataSequence<T> Query<T>(this DbCommand cmd)
        {
            Contract.Requires(cmd != null);
            return cmd.ExecuteReader().AsSeqOf<T>();
        }

        /// <summary>
        /// Asychronously executes a <paramref name="cmd"/> and return a sequence of data
        /// </summary>
        public static async Task<DataSequence<T>> QueryAsync<T>(this DbCommand cmd)
        {
            Contract.Requires(cmd != null);
            return (await cmd.ExecuteReaderAsync()).AsSeqOf<T>();
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

        /// <summary>
        /// Executes a <paramref name="cmd"/> and return a sequence of dynamic data
        /// </summary>
        public static DynamicDataSequence Query(this DbCommand cmd)
        {
            Contract.Requires(cmd != null);
            return new DynamicDataSequence(cmd.ExecuteReader());
        }

        /// <summary>
        /// Executes a <paramref name="cmd"/> and return a sequence of dynamic data
        /// </summary>
        public static async Task<DynamicDataSequence> QueryAsync(this DbCommand cmd)
        {
            Contract.Requires(cmd != null);
            return new DynamicDataSequence(await cmd.ExecuteReaderAsync());
        }

    }

}