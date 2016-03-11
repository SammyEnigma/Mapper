using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.SqlServer.Server;

namespace Mapper
{
    public static class CommandExtension
    {
        private static readonly MostlyReadDictionary<Type, Delegate> Methods = new MostlyReadDictionary<Type, Delegate>();

        private static readonly Dictionary<Type, DbType> TypeMap;

        static CommandExtension()
        {
            TypeMap = new Dictionary<Type, DbType>
            {
                [typeof (byte)] = DbType.Byte,
                [typeof (sbyte)] = DbType.SByte,
                [typeof (short)] = DbType.Int16,
                [typeof (ushort)] = DbType.UInt16,
                [typeof (int)] = DbType.Int32,
                [typeof (uint)] = DbType.UInt32,
                [typeof (long)] = DbType.Int64,
                [typeof (ulong)] = DbType.UInt64,
                [typeof (float)] = DbType.Single,
                [typeof (double)] = DbType.Double,
                [typeof (decimal)] = DbType.Decimal,
                [typeof (bool)] = DbType.Boolean,
                [typeof (string)] = DbType.String,
                [typeof (char)] = DbType.StringFixedLength,
                [typeof (Guid)] = DbType.Guid,
                [typeof (DateTime)] = DbType.DateTime,
                [typeof (DateTimeOffset)] = DbType.DateTimeOffset,
                [typeof (byte[])] = DbType.Binary,
                [typeof (byte?)] = DbType.Byte,
                [typeof (sbyte?)] = DbType.SByte,
                [typeof (short?)] = DbType.Int16,
                [typeof (ushort?)] = DbType.UInt16,
                [typeof (int?)] = DbType.Int32,
                [typeof (uint?)] = DbType.UInt32,
                [typeof (long?)] = DbType.Int64,
                [typeof (ulong?)] = DbType.UInt64,
                [typeof (float?)] = DbType.Single,
                [typeof (double?)] = DbType.Double,
                [typeof (decimal?)] = DbType.Decimal,
                [typeof (bool?)] = DbType.Boolean,
                [typeof (char?)] = DbType.StringFixedLength,
                [typeof (Guid?)] = DbType.Guid,
                [typeof (DateTime?)] = DbType.DateTime,
                [typeof (DateTimeOffset?)] = DbType.DateTimeOffset
            };
        }

        /// <summary>Executes the <paramref name="cmd"/> reading exactly one item</summary>
        /// <exception cref="InvalidOperationException"> when zero values read or more than one value can be read</exception>
        public static T Single<T>(this IDbCommand cmd)
        {
            Contract.Requires(cmd != null);
            Contract.Ensures(Contract.Result<T>() != null);
            using (var reader = cmd.ExecuteReader())
            {
                return reader.Single<T>();
            }
        }

        /// <summary>Executes the <paramref name="cmd"/> reading exactly one item</summary>
        /// <exception cref="InvalidOperationException"> when zero values read or more than one value can be read</exception>
        public static async Task<T> SingleAsync<T>(this SqlCommand cmd)
        {
            Contract.Requires(cmd != null);
            Contract.Ensures(Contract.Result<T>() != null);
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                return await reader.SingleAsync<T>();
            }
        }

        /// <summary>Executes the <paramref name="cmd"/> reading one item</summary>
        /// <remarks>Returns the default vaue of T if no values be read, i.e may return null</remarks>
        public static T SingleOrDefault<T>(this IDbCommand cmd)
        {
            Contract.Requires(cmd != null);
            using (var reader = cmd.ExecuteReader())
            {
                return reader.SingleOrDefault<T>();
            }
        }

        /// <summary>Executes the <paramref name="cmd"/> reading one item</summary>
        /// <remarks>Returns the default vaue of T if no values be read, i.e may return null</remarks>
        public static async Task<T> SingleOrDefaultAsync<T>(this SqlCommand cmd)
        {
            Contract.Requires(cmd != null);
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                return await reader.SingleOrDefaultAsync<T>();
            }
        }

        /// <summary>Executes the <paramref name="cmd"/> and reads all the records into a list</summary>
        public static List<T> ToList<T>(this IDbCommand cmd)
        {
            Contract.Requires(cmd != null);
            Contract.Ensures(Contract.Result<List<T>>() != null);
            using (var reader = cmd.ExecuteReader())
            {
                return reader.ToList<T>();
            }
        }

        /// <summary>Executes the <paramref name="cmd"/> and reads all the records into a list</summary>
        public static async Task<List<T>> ToListAsync<T>(this SqlCommand cmd)
        {
            Contract.Requires(cmd != null);
            Contract.Ensures(Contract.Result<List<T>>() != null);
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                return await reader.ToListAsync<T>();
            }
        }

        /// <summary>Executes the <paramref name="cmd"/> and reads all the records into a dictionary, using the supplied <paramref name="keyFunc"/> to generate the key</summary>
        public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IDbCommand cmd, Func<TValue, TKey> keyFunc)
        {
            Contract.Requires(cmd != null);
            Contract.Requires(keyFunc != null);
            Contract.Ensures(Contract.Result<Dictionary<TKey, TValue>>() != null);
            using (var reader = cmd.ExecuteReader())
            {
                return reader.ToDictionary(keyFunc);
            }
        }

        /// <summary>Executes the <paramref name="cmd"/> and reads all the records into a dictionary, using the supplied <paramref name="keyFunc"/> to generate the key</summary>
        public static async Task<Dictionary<TKey, TValue>> ToDictionaryAsync<TKey, TValue>(this SqlCommand cmd, Func<TValue, TKey> keyFunc)
        {
            Contract.Requires(cmd != null);
            Contract.Requires(keyFunc != null);
            Contract.Ensures(Contract.Result<Dictionary<TKey, TValue>>() != null);
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                return await reader.ToDictionaryAsync(keyFunc);
            }
        }

        /// <summary>Executes the <paramref name="cmd"/> and reads all the records in a lookup, grouped by key, using the supplied <paramref name="keyFunc"/> to generate the key</summary>
        public static ILookup<TKey, TValue> ToLookup<TKey, TValue>(this IDbCommand cmd, Func<TValue, TKey> keyFunc)
        {
            Contract.Requires(cmd != null);
            Contract.Requires(keyFunc != null);
            Contract.Ensures(Contract.Result<ILookup<TKey, TValue>>() != null);
            using (var reader = cmd.ExecuteReader())
            {
                return reader.ToLookup(keyFunc);
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

        private static Action<IDbCommand, object> CreateAddParametersAction(Type type)
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
                if (!TypeMap.ContainsKey(prop.PropertyType)) continue;
                lines.Add(Expression.Assign(dataParam, Expression.Call(cmd, createParameter)));
                lines.Add(Expression.Assign(Expression.Property(dataParam, "ParameterName"), Expression.Constant("@" + prop.Name)));

                if (Types.IsStructured(prop.PropertyType))
                {
                    if (prop.PropertyType != typeof(TableType))
                        throw new NotSupportedException($"Parameter {dataParam.Name} implements {nameof(IEnumerable<SqlDataRecord>)} but type name is unknown.  Please wrap parameter by calling {nameof(EnumerableExtension.WithTypeName)}");

                    lines.Add(Expression.IfThen(
                        Expression.Not(Expression.TypeIs(cmd, typeof(SqlCommand))), 
                        Expression.Throw(Expression.New(typeof(NotSupportedException).GetConstructor(new [] { typeof(string) }), Expression.Constant("Structured parameters are supported only for SqlCommand")))
                        ));
                    lines.Add(Expression.Assign(Expression.Property(Expression.Convert(dataParam, typeof (SqlParameter)), "SqlDbType"), Expression.Constant(SqlDbType.Structured)));
                    lines.Add(Expression.Assign(Expression.Property(Expression.Convert(dataParam, typeof (SqlParameter)), "TypeName"), Expression.Property(Expression.Property(parameters, prop.Name), "TypeName")));
                }
                else
                {
                    lines.Add(Expression.Assign(Expression.Property(dataParam, "DbType"), Expression.Constant(TypeMap[prop.PropertyType])));
                }

                if (Types.CanBeNull(prop.PropertyType))
                {
                    lines.Add(Expression.IfThenElse(
                        Expression.Equal(Expression.Property(parameters, prop.Name), Expression.Constant(null)),
                        Expression.Assign(Expression.Property(dataParam, "Value"), Expression.Convert(Expression.Field(null, typeof (DBNull), "Value"), typeof (object))),
                        Expression.Assign(Expression.Property(dataParam, "Value"), PropertyValue(parameters, prop))
                        ));
                }
                else
                {
                    lines.Add(Expression.Assign(Expression.Property(dataParam, "Value"), PropertyValue(parameters, prop)));
                }

                lines.Add(Expression.Call(Expression.Property(cmd, "Parameters"), typeof(IList).GetMethod("Add", new [] {typeof(object)}), dataParam));
            }
            var block = Expression.Block(new[] { dataParam, parameters }, lines);
            return Expression.Lambda<Action<IDbCommand, object>>(block, cmd, obj).Compile();
        }

        private static UnaryExpression PropertyValue(ParameterExpression obj, PropertyInfo prop)
        {
            return Expression.Convert(Expression.Property(obj, prop.Name), typeof(object));
        }

    }

    static class EnumerableExtension
    {
        /// <summary>
        /// Used to add the SQL Server Table Type name to a parameter
        /// </summary>
        public static TableType WithTypeName(this IEnumerable<SqlDataRecord> records, string typeName)
        {
            Contract.Requires(records != null);
            Contract.Requires(typeName != null);
            Contract.Ensures(Contract.Result<IEnumerable<SqlDataRecord>>() != null);
            return new TableType(typeName, records);
        }
    }

    public class TableType : IEnumerable<SqlDataRecord>
    {
        public string TypeName { get; }
        public IEnumerable<SqlDataRecord> Records { get; }

        public TableType(string typeName, IEnumerable<SqlDataRecord> records)
        {
            Contract.Requires(typeName != null);
            Contract.Requires(records != null);
            TypeName = typeName;
            Records = records;
        }

        public IEnumerator<SqlDataRecord> GetEnumerator()
        {
            return Records.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) Records).GetEnumerator();
        }
    }
}