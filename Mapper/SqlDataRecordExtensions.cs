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

            var key = new TypeAndMetaData(typeof (T), metaData);
            var map = (Func<SqlMetaData[], T, SqlDataRecord>)Methods.GetOrAdd(key, data => CreateMappingFunc<T>(data.MetaData));
            return items.Select(item => map(metaData, item));
        }

        private static Func<SqlMetaData[], T, SqlDataRecord> CreateMappingFunc<T>(SqlMetaData[] metaData)
        {
            var result = Expression.Parameter(typeof (SqlDataRecord), "rec");
            var metaDataParam = Expression.Parameter(typeof (SqlMetaData[]), "metaData");
            var item = Expression.Parameter(typeof (T), "item");
            var constructorInfo = typeof (SqlDataRecord).GetConstructor(new[] {typeof (SqlMetaData[])});
            var lines = new List<Expression>
            {
                Expression.Assign(result, Expression.New(constructorInfo, metaDataParam))
            };
            var propertiesAndFields = Types.ReadablePropertiesAndFields<T>();

            var getDbNullMethod = typeof(SqlDataRecord).GetMethod("SetDbNull", new[] { typeof(int) });
            Contract.Assert(getDbNullMethod != null);
            for (int i = 0; i < metaData.Length; i++)
            {
                var col = metaData[i];
                var colType = Types.DBTypeToType[col.DbType];
                var member = FindMember(col, colType, propertiesAndFields);
                if (member == null) continue;
                var sourceType = Types.PropertyOrFieldType(member);
                if (Types.CanBeNull(sourceType))
                {
                    lines.Add(Expression.IfThenElse(
                                Expression.Equal(Expression.PropertyOrField(item, member.Name), Expression.Constant(null)),
                                Expression.Call(result, getDbNullMethod, Expression.Constant(i)),
                                SetValue(result, colType, i, item, member.Name)
                            ));
                }
                else
                {
                    lines.Add(SetValue(result, colType, i, item, member.Name));
                }
            }
            var block = Expression.Block(new[] {result}, lines);
            return Expression.Lambda<Func<SqlMetaData[], T, SqlDataRecord>>(block, metaDataParam, item).Compile();
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

        private static MethodCallExpression SetValue(ParameterExpression result, Type colType, int ordinal, ParameterExpression item, string propertyOrFieldName)
        {
            if (colType == typeof (byte[]))
            {
                var setBytes = typeof (SqlDataRecord).GetMethod("SetBytes", new[] {typeof(int), typeof(long), typeof(byte[]), typeof(int), typeof(int) });
                Contract.Assert(setBytes != null);
                return Expression.Call(result, setBytes, Expression.Constant(ordinal), Expression.Constant(0L), Expression.PropertyOrField(item, propertyOrFieldName), Expression.Constant(0), Expression.PropertyOrField(Expression.PropertyOrField(item, propertyOrFieldName), "Length"));
            }

            var setMethod = typeof (SqlDataRecord).GetMethod("Set" + colType.Name, new[] {typeof (int), colType});
            Contract.Assert(setMethod != null);
            return Expression.Call(result, setMethod, Expression.Constant(ordinal), Expression.PropertyOrField(item, propertyOrFieldName));
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