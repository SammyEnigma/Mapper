using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.SqlServer.Server;
using System.Data.Common;
using System.Data.SqlClient;

namespace Mapper
{
    public static class DbCommandExtensions
    {
        static readonly MostlyReadDictionary<Type, Delegate> Methods = new MostlyReadDictionary<Type, Delegate>();

        public static T ExecuteScalar<T>(this IDbCommand cmd)
        {
            Contract.Requires(cmd != null);
            return (T)cmd.ExecuteScalar();
        }

        public static async Task<T> ExecuteScalarAsync<T>(this DbCommand cmd)
        {
            Contract.Requires(cmd != null);
            return (T)await cmd.ExecuteScalarAsync();
        }

        /// <summary>Executes the <paramref name="cmd"/> reading exactly one item</summary>
        /// <exception cref="InvalidOperationException"> when zero values read or more than one value can be read</exception>
        public static T ExecuteSingle<T>(this IDbCommand cmd)
        {
            Contract.Requires(cmd != null);
            Contract.Ensures(Contract.Result<T>() != null);
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
            Contract.Ensures(Contract.Result<T>() != null);
            using (var reader = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow))
            {
                return await reader.ReadSingleAsync<T>();
            }
        }

        /// <summary>Executes the <paramref name="cmd"/> reading one item</summary>
        /// <remarks>Returns the default vaue of T if no values be read, i.e may return null</remarks>
        public static T ExecuteSingleOrDefault<T>(this IDbCommand cmd)
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
        public static List<T> ExecuteList<T>(this IDbCommand cmd)
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
            Contract.Ensures(Contract.Result<List<T>>() != null);
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                return await reader.ReadListAsync<T>();
            }
        }

        /// <summary>Executes the <paramref name="cmd"/> and reads all the records into a dictionary, using the supplied <paramref name="keyFunc"/> to generate the key</summary>
        public static Dictionary<TKey, TValue> ExecuteDictionary<TKey, TValue>(this IDbCommand cmd, Func<TValue, TKey> keyFunc)
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
            Contract.Ensures(Contract.Result<Dictionary<TKey, TValue>>() != null);
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                return await reader.ReadDictionaryAsync(keyFunc);
            }
        }

        /// <summary>Executes the <paramref name="cmd"/> and reads all the records in a lookup, grouped by key, using the supplied <paramref name="keyFunc"/> to generate the key</summary>
        public static HashLookup<TKey, TValue> ExecuteLookup<TKey, TValue>(this IDbCommand cmd, Func<TValue, TKey> keyFunc)
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
            Contract.Ensures(Contract.Result<HashLookup<TKey, TValue>>() != null);
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                return await reader.ReadLookupAsync(keyFunc);
            }
        }

        /// <summary>Adds parameters to the <paramref name="cmd"/>, one parameter per property of <paramref name="parameters"/></summary>
        public static IDbCommand AddParameters(this IDbCommand cmd, object parameters)
        {
            Contract.Requires(cmd != null);
            Contract.Requires(parameters != null);
            var action = (Action<IDbCommand, object>)Methods.GetOrAdd(parameters.GetType(), CreateAddParametersAction);
            action(cmd, parameters);
            return cmd;
        }

        static Action<IDbCommand, object> CreateAddParametersAction(Type type)
        {
            var obj = Expression.Parameter(typeof(object), "obj");
            var parameters = Expression.Parameter(type, "parameters");
            var cmd = Expression.Parameter(typeof(IDbCommand), "cmd");
            var dataParam = Expression.Parameter(typeof(IDataParameter), "dataParam");
            var lines = new List<Expression>
            {
                Expression.Assign(parameters, Expression.Convert(obj, type))
            };
            var createParameter = typeof(IDbCommand).GetMethod("CreateParameter", Type.EmptyTypes);
            foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var propertyType = prop.PropertyType;
                if (!IsSupportedType(propertyType)) continue;
                lines.Add(Expression.Assign(dataParam, Expression.Call(cmd, createParameter)));
                lines.Add(Expression.Assign(Expression.Property(dataParam, "ParameterName"), Expression.Constant("@" + prop.Name)));

                if (Types.IsStructured(propertyType))
                {
                    if (propertyType != typeof(TableType))
                        throw new NotSupportedException($"Parameter {dataParam.Name} implements {nameof(IEnumerable<SqlDataRecord>)} but type name is unknown.  Please wrap parameter by calling {nameof(SqlDataRecordExtensions.WithTypeName)}");

                    lines.Add(Expression.IfThen(
                        Expression.Not(Expression.TypeIs(cmd, typeof(DbCommand))),
                        Expression.Throw(Expression.New(typeof(NotSupportedException).GetConstructor(new[] { typeof(string) }), Expression.Constant("Structured parameters are supported only for SqlCommand")))
                        ));
                    lines.Add(Expression.Assign(Expression.Property(Expression.Convert(dataParam, typeof(SqlParameter)), "SqlDbType"), Expression.Constant(SqlDbType.Structured)));
                    lines.Add(Expression.Assign(Expression.Property(Expression.Convert(dataParam, typeof(SqlParameter)), "TypeName"), Expression.Property(Expression.Property(parameters, prop.Name), "TypeName")));
                }
                else if (propertyType.IsEnum)
                {
                    lines.Add(Expression.Assign(Expression.Property(dataParam, "DbType"), Expression.Constant(DbType.Int32)));
                }
                else
                {
                    lines.Add(Expression.Assign(Expression.Property(dataParam, "DbType"), Expression.Constant(Types.TypeToDbType[propertyType])));
                }

                if (Types.CanBeNull(propertyType))
                {
                    lines.Add(Expression.IfThenElse(
                        Expression.Equal(Expression.Property(parameters, prop.Name), Expression.Constant(null)),
                        Expression.Assign(Expression.Property(dataParam, "Value"), Expression.Convert(Expression.Field(null, typeof(DBNull), "Value"), typeof(object))),
                        Expression.Assign(Expression.Property(dataParam, "Value"), PropertyValue(parameters, prop))
                        ));
                }
                else
                {
                    lines.Add(Expression.Assign(Expression.Property(dataParam, "Value"), PropertyValue(parameters, prop)));
                }

                lines.Add(Expression.Call(Expression.Property(cmd, "Parameters"), typeof(IList).GetMethod("Add", new[] { typeof(object) }), dataParam));
            }
            var block = Expression.Block(new[] { dataParam, parameters }, lines);
            return Expression.Lambda<Action<IDbCommand, object>>(block, cmd, obj).Compile();
        }

        static bool IsSupportedType(Type propertyType)
        {
            return Types.IsStructured(propertyType) || Types.TypeToDbType.ContainsKey(propertyType) || propertyType.IsEnum;
        }

        static UnaryExpression PropertyValue(ParameterExpression obj, PropertyInfo prop)
        {
            Expression value = Expression.Property(obj, prop.Name);
            if (prop.PropertyType.IsEnum)
            {
                value = Expression.Convert(value, typeof(int));
            }
            return Expression.Convert(value, typeof(object));
        }

    }

}