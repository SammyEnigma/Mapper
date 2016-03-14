using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.SqlServer.Server;

namespace Mapper
{
    public static class SqlDataRecordExtensions
    {
        private static readonly MostlyReadDictionary<TypeAndMetaData, Delegate> Methods = new MostlyReadDictionary<TypeAndMetaData, Delegate>();

        public static IEnumerable<SqlDataRecord> ToDataRecords<T>(this IEnumerable<T> items, SqlMetaData[] metaData)
        {
            Contract.Requires(items != null);
            Contract.Requires(metaData != null);
            Contract.Ensures(Contract.Result<IEnumerable<SqlDataRecord>>() != null);
            var key = new TypeAndMetaData(typeof(T), metaData);
            var typeT = typeof(T);
            var map = (Func<SqlMetaData[], T, SqlDataRecord>)GetOrAddFunc(key, typeT);
            return items.Select(item => map(metaData, item));
        }

        private static Delegate GetOrAddFunc(TypeAndMetaData key, Type typeT)
        {
            return Methods.GetOrAdd(key, data => CreateMappingFunc(typeT, data.MetaData));
        }

        private static Delegate CreateMappingFunc(Type typeT, SqlMetaData[] metaData)
        {
            var result = Expression.Parameter(typeof (SqlDataRecord), "rec");
            var metaDataParam = Expression.Parameter(typeof (SqlMetaData[]), "metaData");
            var item = Expression.Parameter(typeT, "item");
            var constructorInfo = typeof (SqlDataRecord).GetConstructor(new[] {typeof (SqlMetaData[])});
            var lines = new List<Expression>
            {
                Expression.Assign(result, Expression.New(constructorInfo, metaDataParam))
            };
            var propertiesAndFields = Types.ReadablePropertiesAndFields(typeT);

            var setNullMethod = typeof(SqlDataRecord).GetMethod("SetDBNull", new[] { typeof(int) });
            Contract.Assert(setNullMethod != null);
            for (int i = 0; i < metaData.Length; i++)
            {
                var col = metaData[i];
                var outType = Types.DBTypeToType[col.DbType];
                var member = FindMember(col, outType, propertiesAndFields);
                if (member == null)
                    continue;
                var inType = Types.PropertyOrFieldType(member);
                Expression value = Expression.PropertyOrField(item, member.Name);
                if (outType != inType)
                {
                    // type if not the same, can it be assigned?
                    if (Types.CanBeCast(inType, outType))
                        value = Expression.Convert(value, outType);
                    else if (Types.IsNullable(inType) && inType.GetGenericArguments()[0] == outType)
                        value = Expression.Convert(value, outType);
                    else if (Types.IsNullable(inType) && Types.CanBeCast(inType.GetGenericArguments()[0], outType))
                        value = Expression.Convert(value, outType);
                    else
                        continue;
                }
                var setValueExp = SetValue(result, outType, i, item, member);
                if (setValueExp == null)
                    continue;

                if (Types.CanBeNull(inType))
                {
                    lines.Add(Expression.IfThenElse(
                                Expression.Equal(Expression.PropertyOrField(item, member.Name), Expression.Constant(null)),
                                Expression.Call(result, setNullMethod, Expression.Constant(i)),
                                setValueExp
                            ));
                }
                else
                {
                    lines.Add(setValueExp);
                }
            }
            lines.Add(result);
            var block = Expression.Block(new[] {result}, lines);
            var func = typeof(Func<,,>).MakeGenericType(typeof(SqlMetaData[]), typeT, typeof(SqlDataRecord));
            return Expression.Lambda(func, block, metaDataParam, item).Compile();
        }

        private static MemberInfo FindMember(SqlMetaData col, Type colType, IDictionary<string, MemberInfo> propertiesAndFields)
        {
            foreach (var name in Names.CandidateNames(col.Name, colType))
            {
                MemberInfo member;
                if (propertiesAndFields.TryGetValue(name, out member))
                    return member;
            }
            return null;
        }

        private static MethodCallExpression SetValue(ParameterExpression result, Type colType, int ordinal, ParameterExpression item, MemberInfo member)
        {
            var inType = Types.PropertyOrFieldType(member);
            Expression value = Expression.PropertyOrField(item, member.Name);
            if (colType != inType)
            {
                // type if not the same, can it be assigned?
                if (Types.CanBeCast(inType, colType))
                    value = Expression.Convert(value, colType);
                else if (Types.IsNullable(inType) && inType.GetGenericArguments()[0] == colType)
                    value = Expression.Convert(value, colType);
                else if (Types.IsNullable(inType) && Types.CanBeCast(inType.GetGenericArguments()[0], colType))
                    value = Expression.Convert(value, colType);
                else
                    return null;
            }

            if (colType == typeof(byte[]))
            {
                var setBytes = typeof(SqlDataRecord).GetMethod("SetBytes", new[] { typeof(int), typeof(long), typeof(byte[]), typeof(int), typeof(int) });
                Contract.Assert(setBytes != null);
                return Expression.Call(result, setBytes, Expression.Constant(ordinal), Expression.Constant(0L), value, Expression.Constant(0), Expression.PropertyOrField(value, "Length"));
            }

            var setMethod = typeof(SqlDataRecord).GetMethod(SetMethodName(colType), new[] { typeof(int), colType });
            Contract.Assert(setMethod != null);
            return Expression.Call(result, setMethod, Expression.Constant(ordinal), value);
        }

        private static string SetMethodName(Type colType)
        {
            if (Types.IsNullable(colType)) colType = colType.GetGenericArguments()[0];
            if (colType == typeof(Single)) return "SetFloat";
            return "Set" + colType.Name;
        }
    }

    struct TypeAndMetaData : IEquatable<TypeAndMetaData>
    {
        public readonly Type Type;
        public readonly SqlMetaData[] MetaData;

        public TypeAndMetaData(Type type, SqlMetaData[] metaData)
        {
            Type = type;
            MetaData = metaData;
        }

        public bool Equals(TypeAndMetaData other)
        {
            return Type == other.Type && StructuralComparisons.StructuralEqualityComparer.Equals(MetaData, other.MetaData);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is TypeAndMetaData && Equals((TypeAndMetaData) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Type?.GetHashCode() ?? 0)*397) ^ (MetaData?.GetHashCode() ?? 0);
            }
        }
    }

}