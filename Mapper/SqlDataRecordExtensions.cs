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

        /// <summary>
        /// Converts a sequence of <paramref name="items"/> into a sequence of <see cref="SqlDataRecord"/> using the supplied <paramref name="metaData"/>
        /// </summary>
        internal static IEnumerable<SqlDataRecord> ToDataRecords<T>(this IEnumerable<T> items, SqlMetaData[] metaData)
        {
            Contract.Requires(items != null);
            Contract.Requires(metaData != null);
            Contract.Ensures(Contract.Result<IEnumerable<SqlDataRecord>>() != null);
            var key = new TypeAndMetaData(typeof(T), metaData);
            var typeT = typeof(T);
            var map = (Func<SqlMetaData[], T, SqlDataRecord>)GetOrAddFunc(key, typeT);
            return items.Select(item => map(metaData, item));
        }

        /// <summary>
        /// Converts a sequence of <paramref name="items"/> into a <see cref="TableType"/> containing a sequence of <see cref="SqlDataRecord"/> using the supplied <paramref name="metaData"/>
        /// </summary>
        public static TableType ToTableType<T>(this IEnumerable<T> items, SqlMetaData[] metaData, string tableTypeName)
        {
            Contract.Requires(items != null);
            Contract.Requires(metaData != null);
            Contract.Requires(tableTypeName != null);
            Contract.Ensures(Contract.Result<IEnumerable<SqlDataRecord>>() != null);
            var key = new TypeAndMetaData(typeof(T), metaData);
            var typeT = typeof(T);
            var map = (Func<SqlMetaData[], T, SqlDataRecord>)GetOrAddFunc(key, typeT);
            return new TableType(tableTypeName, items.Select(item => map(metaData, item)));
        }

        /// <summary>
        /// Converts a sequence of <paramref name="items"/> into a <see cref="TableType"/> containing a sequence of <see cref="SqlDataRecord"/> using the supplied <paramref name="metaData"/>
        /// </summary>
        /// <remarks><paramref name="extraAction"/> can be used to set additional values on each <see cref="SqlDataRecord"/></remarks>
        public static TableType ToTableType<T>(this IEnumerable<T> items, SqlMetaData[] metaData, string tableTypeName, Action<SqlDataRecord, T> extraAction)
        {
            Contract.Requires(items != null);
            Contract.Requires(metaData != null);
            Contract.Requires(tableTypeName != null);
            Contract.Ensures(Contract.Result<IEnumerable<SqlDataRecord>>() != null);
            var key = new TypeAndMetaData(typeof(T), metaData);
            var typeT = typeof(T);
            var map = (Func<SqlMetaData[], T, SqlDataRecord>)GetOrAddFunc(key, typeT);
            return new TableType(tableTypeName, Records(items, metaData, map, extraAction));
        }

        private static IEnumerable<SqlDataRecord> Records<T>(IEnumerable<T> items, SqlMetaData[] metaData, Func<SqlMetaData[], T, SqlDataRecord> map, Action<SqlDataRecord, T> extraAction)
        {
            foreach (var item in items)
            {
                var record = map(metaData, item);
                extraAction(record, item);
                yield return record;
            }
        }

        /// <summary>
        /// Converts a sequence of <paramref name="items"/> into a <see cref="TableType"/> containing a sequence of <see cref="SqlDataRecord"/> using the supplied <paramref name="metaData"/>
        /// </summary>
        /// <remarks><paramref name="extraAction"/> can be used to set additional values on each <see cref="SqlDataRecord"/> and gets passed the index of the record</remarks>
        public static TableType ToTableType<T>(this IEnumerable<T> items, SqlMetaData[] metaData, string tableTypeName, Action<SqlDataRecord, T, int> extraAction)
        {
            Contract.Requires(items != null);
            Contract.Requires(metaData != null);
            Contract.Requires(tableTypeName != null);
            Contract.Requires(extraAction != null);
            Contract.Ensures(Contract.Result<IEnumerable<SqlDataRecord>>() != null);
            var key = new TypeAndMetaData(typeof(T), metaData);
            var typeT = typeof(T);
            var map = (Func<SqlMetaData[], T, SqlDataRecord>)GetOrAddFunc(key, typeT);
            return new TableType(tableTypeName, Records(items, metaData, map, extraAction));
        }

        private static IEnumerable<SqlDataRecord> Records<T>(IEnumerable<T> items, SqlMetaData[] metaData, Func<SqlMetaData[], T, SqlDataRecord> map, Action<SqlDataRecord, T, int> extraAction)
        {
            int i = 0;
            foreach (var item in items)
            {
                var record = map(metaData, item);
                extraAction(record, item, i);
                yield return record;
                i++;
            }
        }

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

                var setValueExp = SetValue(result, outType, i, item, member);
                if (setValueExp == null)
                    continue;

                if (Types.CanBeNull(Types.PropertyOrFieldType(member)))
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

    internal struct TypeAndMetaData : IEquatable<TypeAndMetaData>
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
            if (Type != other.Type) return false;
            if (MetaData.Length != other.MetaData.Length) return false;
            for (int i = 0; i < MetaData.Length; i++)
            {
                var left = MetaData[i];
                var right = other.MetaData[i];
                if (left.Name != right.Name) return false;
                if (left.SqlDbType != right.SqlDbType) return false;
                if (left.MaxLength != right.MaxLength) return false;
            }
            return true;
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
                return ((Type?.GetHashCode() ?? 0)*397) ^ MetaData.Length;
            }
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
            return ((IEnumerable)Records).GetEnumerator();
        }
    }

}