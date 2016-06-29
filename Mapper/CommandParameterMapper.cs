using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reflection;

namespace Mapper
{
    class CommandParameterMapper
    {
        readonly MostlyReadDictionary<Type, Action<DbCommand, object>> Methods = new MostlyReadDictionary<Type, Action<DbCommand, object>>();

        public DbCommand AddParameters(DbCommand cmd, object parameters)
        {
            Contract.Requires(cmd != null);
            Contract.Requires(parameters != null);
            Contract.Ensures(Contract.Result<DbCommand>() == cmd);
            Action<DbCommand, object> action = Methods.GetOrAdd(parameters.GetType(), CreateAddParametersAction);
            action(cmd, parameters);
            return cmd;
        }

        static Action<DbCommand, object> CreateAddParametersAction(Type type)
        {
            var obj = Expression.Parameter(typeof(object), "obj");
            var parameters = Expression.Parameter(type, "parameters");
            var cmd = Expression.Parameter(typeof(DbCommand), "cmd");
            var dataParam = Expression.Parameter(typeof(DbParameter), "dataParam");
            var lines = new List<Expression>
            {
                Expression.Assign(parameters, Expression.Convert(obj, type))
            };
            var createParameter = typeof(DbCommand).GetMethod("CreateParameter", Type.EmptyTypes);
            foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var propertyType = prop.PropertyType;
                if (!IsSupportedType(propertyType)) continue;
                lines.Add(Expression.Assign(dataParam, Expression.Call(cmd, createParameter)));
                lines.Add(Expression.Assign(Expression.Property(dataParam, "ParameterName"), Expression.Constant("@" + prop.Name)));

                if (Types.IsStructured(propertyType))
                {
                    if (propertyType != typeof(TableType))
                        throw new NotSupportedException($"Parameter {dataParam.Name} implements {nameof(IEnumerable<SqlDataRecord>)} but type name is unknown.  Please wrap parameter by calling {nameof(Extensions.WithTypeName)}");

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

                lines.Add(Expression.Call(Expression.Property(cmd, "Parameters"), typeof(DbParameterCollection).GetMethod("Add", new[] { typeof(object) }), dataParam));
            }
            var block = Expression.Block(new[] { dataParam, parameters }, lines);
            return Expression.Lambda<Action<DbCommand, object>>(block, cmd, obj).Compile();
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
