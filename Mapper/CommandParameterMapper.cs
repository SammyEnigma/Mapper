using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reflection;

namespace BusterWood.Mapper
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
            foreach (var prop in Types.ReadablePublicFieldsAndProperties(type))
            {
                var fieldOrProperty = Types.PropertyOrFieldType(prop);
                if (!IsSupportedType(fieldOrProperty)) continue;
                lines.Add(Expression.Assign(dataParam, Expression.Call(cmd, createParameter)));
                lines.Add(Expression.Assign(Expression.Property(dataParam, "ParameterName"), Expression.Constant("@" + prop.Name)));

                if (Types.IsStructured(fieldOrProperty))
                {
                    if (fieldOrProperty != typeof(TableType))
                        throw new NotSupportedException($"Parameter {dataParam.Name} implements {nameof(IEnumerable<SqlDataRecord>)} but type name is unknown.  Please wrap parameter by calling {nameof(Extensions.WithTypeName)}");

                    lines.Add(Expression.IfThen(
                        Expression.Not(Expression.TypeIs(cmd, typeof(DbCommand))),
                        Expression.Throw(Expression.New(typeof(NotSupportedException).GetConstructor(new[] { typeof(string) }), Expression.Constant("Structured parameters are supported only for SqlCommand")))
                        ));
                    lines.Add(Expression.Assign(Expression.Property(Expression.Convert(dataParam, typeof(SqlParameter)), "SqlDbType"), Expression.Constant(SqlDbType.Structured)));
                    lines.Add(Expression.Assign(Expression.Property(Expression.Convert(dataParam, typeof(SqlParameter)), "TypeName"), Expression.Property(Expression.Property(parameters, prop.Name), "TypeName")));
                }
                else if (fieldOrProperty.IsEnum || Types.IsNullableEnum(fieldOrProperty))
                {
                    lines.Add(Expression.Assign(Expression.Property(dataParam, "DbType"), Expression.Constant(DbType.Int32)));
                }
                else
                {
                    lines.Add(Expression.Assign(Expression.Property(dataParam, "DbType"), Expression.Constant(Types.TypeToDbType[fieldOrProperty])));
                }

                if (fieldOrProperty == typeof(TableType))
                {
                    // check if records is null
                    var tt = Expression.PropertyOrField(parameters, prop.Name);
                    var records = Expression.Property(Expression.Convert(tt, typeof(TableType)), "Records");
                    lines.Add(
                        Expression.Assign(Expression.Property(dataParam, "Value"), Expression.Convert(records, typeof(object))
                        //Expression.Condition(
                        //    Expression.Equal(records, Expression.Constant(null)),
                        //    Expression.Convert(Expression.Field(null, typeof(DBNull), "Value"), typeof(object)),
                        //    Expression.Convert(records, typeof(object))
                        //)
                        )
                    );
                }
                else if (Types.CanBeNull(fieldOrProperty))
                {
                    lines.Add(
                        Expression.Assign(
                            Expression.Property(dataParam, "Value"),
                            Expression.Condition(
                                Expression.Equal(Expression.Property(parameters, prop.Name), Expression.Constant(null)),
                                Expression.Convert(Expression.Field(null, typeof(DBNull), "Value"), typeof(object)),
                                PropertyValueCastToObject(parameters, prop)
                            )
                        )
                    );
                }
                else
                {
                    lines.Add(Expression.Assign(Expression.Property(dataParam, "Value"), PropertyValueCastToObject(parameters, prop)));
                }

                lines.Add(Expression.Call(Expression.Property(cmd, "Parameters"), typeof(DbParameterCollection).GetMethod("Add", new[] { typeof(object) }), dataParam));
            }
            var block = Expression.Block(new[] { dataParam, parameters }, lines);
            return Expression.Lambda<Action<DbCommand, object>>(block, cmd, obj).Compile();
        }

        static bool IsSupportedType(Type propertyType)
        {
            return Types.IsStructured(propertyType) || Types.TypeToDbType.ContainsKey(propertyType) || propertyType.IsEnum || Types.IsNullableEnum(propertyType);
        }

        static UnaryExpression PropertyValueCastToObject(ParameterExpression obj, MemberInfo propOrField)
        {
            Expression value = Expression.PropertyOrField(obj, propOrField.Name);
            var type = Types.PropertyOrFieldType(propOrField);
            if (type.IsEnum || Types.IsNullableEnum(type))
            {
                value = Expression.Convert(value, typeof(int));
            }
            return Expression.Convert(value, typeof(object));
        }

    }
}
