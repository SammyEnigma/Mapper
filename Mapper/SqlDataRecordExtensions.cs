using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.SqlServer.Server;

namespace BusterWood.Mapper
{
    public static partial class Extensions
    {
        static readonly MostlyReadDictionary<TypeAndMetaData, Delegate> Methods = new MostlyReadDictionary<TypeAndMetaData, Delegate>();

        /// <summary>
        /// Converts a sequence of <paramref name="items"/> into a sequence of <see cref="SqlDataRecord"/> using the supplied <paramref name="metaData"/>
        /// </summary>
        internal static IEnumerable<SqlDataRecord> ToDataRecords<T>(this IEnumerable<T> items, SqlMetaData[] metaData)
        {
            Contract.Requires(items != null);
            Contract.Requires(metaData != null);
            Contract.Requires(metaData.Length > 0);
            var key = new TypeAndMetaData(typeof(T), metaData);
            var map = (Func<SqlMetaData[], T, SqlDataRecord>)GetOrAddFunc(key, typeof(T));
            return items.Select(item => map(metaData, item));
        }

        /// <summary>
        /// Converts a sequence of <paramref name="items"/> into a <see cref="TableType"/> containing a sequence of <see cref="SqlDataRecord"/> using the supplied <paramref name="metaData"/>
        /// </summary>
        public static TableType ToTableType<T>(this IEnumerable<T> items, SqlMetaData[] metaData, string tableTypeName)
        {
            Contract.Requires(items != null);
            Contract.Requires(metaData != null);
            Contract.Requires(metaData.Length > 0);
            Contract.Requires(!string.IsNullOrWhiteSpace(tableTypeName));
            Contract.Ensures(Contract.Result<IEnumerable<SqlDataRecord>>() != null);
            var key = new TypeAndMetaData(typeof(T), metaData);
            var map = (Func<SqlMetaData[], T, SqlDataRecord>)GetOrAddFunc(key, typeof(T));
            if (items.Any())
                return new TableType(tableTypeName, items.Select(item => map(metaData, item)));
            else
                return new TableType(tableTypeName, null); // SQL will throw and exception if you try to pass an empty enumeration
        }

        /// <summary>
        /// Converts a sequence of <paramref name="items"/> into a <see cref="TableType"/> containing a sequence of <see cref="SqlDataRecord"/> using the supplied <paramref name="metaData"/>
        /// </summary>
        /// <remarks><paramref name="extraAction"/> can be used to set additional values on each <see cref="SqlDataRecord"/></remarks>
        public static TableType ToTableType<T>(this IEnumerable<T> items, SqlMetaData[] metaData, string tableTypeName, Action<SqlDataRecord, T> extraAction)
        {
            Contract.Requires(items != null);
            Contract.Requires(metaData != null);
            Contract.Requires(metaData.Length > 0);
            Contract.Requires(!string.IsNullOrWhiteSpace(tableTypeName));
            Contract.Ensures(Contract.Result<IEnumerable<SqlDataRecord>>() != null);
            var key = new TypeAndMetaData(typeof(T), metaData);
            var map = (Func<SqlMetaData[], T, SqlDataRecord>)GetOrAddFunc(key, typeof(T));
            if (items.Any())
                return new TableType(tableTypeName, ConvertItemsToRecords(items, metaData, map, extraAction));
            else
                return new TableType(tableTypeName, null); // SQL will throw and exception if you try to pass an empty enumeration
        }

        static IEnumerable<SqlDataRecord> ConvertItemsToRecords<T>(IEnumerable<T> items, SqlMetaData[] metaData, Func<SqlMetaData[], T, SqlDataRecord> map, Action<SqlDataRecord, T> extraAction)
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
            Contract.Requires(metaData.Length > 0);
            Contract.Requires(!string.IsNullOrWhiteSpace(tableTypeName));
            Contract.Requires(extraAction != null);
            Contract.Ensures(Contract.Result<IEnumerable<SqlDataRecord>>() != null);
            var key = new TypeAndMetaData(typeof(T), metaData);
            var typeT = typeof(T);
            var map = (Func<SqlMetaData[], T, SqlDataRecord>)GetOrAddFunc(key, typeT);
            if (items.Any())
                return new TableType(tableTypeName, Records(items, metaData, map, extraAction));
            else
                return new TableType(tableTypeName, null); // SQL will throw and exception if you try to pass an empty enumeration
        }

        static IEnumerable<SqlDataRecord> Records<T>(IEnumerable<T> items, SqlMetaData[] metaData, Func<SqlMetaData[], T, SqlDataRecord> map, Action<SqlDataRecord, T, int> extraAction)
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

        /// <summary>Used to add the SQL Server Table Type name to a parameter</summary>
        public static TableType WithTypeName(this IEnumerable<SqlDataRecord> records, string tableTypeName)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(tableTypeName));
            Contract.Ensures(Contract.Result<IEnumerable<SqlDataRecord>>() != null);
            return new TableType(tableTypeName, records);
        }

        static Delegate GetOrAddFunc(TypeAndMetaData key, Type typeT) => Methods.GetOrAdd(key, data => CreateMappingFunc(typeT, data.MetaData));

        static Delegate CreateMappingFunc(Type typeT, SqlMetaData[] metaData)
        {
            if (Types.IsPrimitiveOrEnum(typeT) && metaData.Length == 1)
                return CreatePrimativeMapping(typeT, metaData[0]);
            if (typeT == typeof(string) && metaData.Length == 1)
                return CreatePrimativeMapping(typeT, metaData[0]);
            else
                return CreateTypeMapping(typeT, metaData);
        }

        private static Delegate CreatePrimativeMapping(Type typeT, SqlMetaData sqlMetaData)
        {
            var dest = new Column(0, sqlMetaData.Name, Types.DBTypeToType[sqlMetaData.DbType]);
            LambdaExpression lambdaExpression = CreatePrimativeLambda(typeT, dest);
            return lambdaExpression.Compile();
        }

        static LambdaExpression CreatePrimativeLambda(Type typeT, Column col)
        {
            var result = Expression.Parameter(typeof(SqlDataRecord), "rec");
            var metaDataParam = Expression.Parameter(typeof(SqlMetaData[]), "metaData");
            var item = Expression.Parameter(typeT, "item");
            var constructorInfo = typeof(SqlDataRecord).GetConstructor(new[] { typeof(SqlMetaData[]) });
            var lines = new List<Expression>
            {
                Expression.Assign(result, Expression.New(constructorInfo, metaDataParam))
            };

            var setNullMethod = typeof(SqlDataRecord).GetMethod("SetDBNull", new[] { typeof(int) });
            Contract.Assert(setNullMethod != null);

            var setValueExp = SetValue(result, col.Type, col, item);
            if (setValueExp == null)
                throw new InvalidOperationException($"Cannot map from {typeT} to {col.Type}");

            if (Types.CanBeNull(typeT))
            {
                lines.Add(Expression.IfThenElse(
                            Expression.Equal(item, Expression.Constant(null)),
                            Expression.Call(result, setNullMethod, Expression.Constant(col.Ordinal)),
                            setValueExp
                        ));
            }
            else
            {
                lines.Add(setValueExp);
            }
            lines.Add(result);
            var block = Expression.Block(new[] { result }, lines);
            var func = typeof(Func<,,>).MakeGenericType(typeof(SqlMetaData[]), typeT, typeof(SqlDataRecord));
            var lambdaExpression = Expression.Lambda(func, block, metaDataParam, item);
            return lambdaExpression;
        }

        private static Delegate CreateTypeMapping(Type typeT, SqlMetaData[] metaData)
        {
            var columns = metaData.Select((md, i) => (Thing)new Column(i, md.Name, Types.DBTypeToType[md.DbType])).ToList();
            var result = Mapping.CreateFromDestination(Types.ReadablePublicThings(typeT), columns, typeT.Name);
            LambdaExpression lambdaExpression = CreateMappingLambda(typeT, result.Mapped);
            return lambdaExpression.Compile();
        }

        static LambdaExpression CreateMappingLambda(Type typeT, List<Mapping<Thing, Thing>> mapping)
        {
            var result = Expression.Parameter(typeof(SqlDataRecord), "rec");
            var metaDataParam = Expression.Parameter(typeof(SqlMetaData[]), "metaData");
            var item = Expression.Parameter(typeT, "item");
            var constructorInfo = typeof(SqlDataRecord).GetConstructor(new[] { typeof(SqlMetaData[]) });
            var lines = new List<Expression>
            {
                Expression.Assign(result, Expression.New(constructorInfo, metaDataParam))
            };
            var propertiesAndFields = Types.ReadablePropertiesAndFieldsDictionary(typeT);

            var setNullMethod = typeof(SqlDataRecord).GetMethod("SetDBNull", new[] { typeof(int) });
            Contract.Assert(setNullMethod != null);

            foreach (var map in mapping)
            {
                var col = (Column)map.To;
                var setValueExp = SetValue(result, item, map.From, col);
                if (setValueExp == null)
                    continue;

                if (Types.CanBeNull(map.From.Type))
                {
                    lines.Add(Expression.IfThenElse(
                                Expression.Equal(Expression.PropertyOrField(item, map.From.Name), Expression.Constant(null)),
                                Expression.Call(result, setNullMethod, Expression.Constant(col.Ordinal)),
                                setValueExp
                            ));
                }
                else
                {
                    lines.Add(setValueExp);
                }
            }
            lines.Add(result);
            var block = Expression.Block(new[] { result }, lines);
            var func = typeof(Func<,,>).MakeGenericType(typeof(SqlMetaData[]), typeT, typeof(SqlDataRecord));
            var lambdaExpression = Expression.Lambda(func, block, metaDataParam, item);
            return lambdaExpression;
        }

        static MethodCallExpression SetValue(ParameterExpression result, ParameterExpression item, Thing from, Column to)
        {
            Expression value = Expression.PropertyOrField(item, from.Name);
            return SetValue(result, from.Type, to, value);
        }

        private static MethodCallExpression SetValue(ParameterExpression result, Type fromType, Column to, Expression value)
        {
            if (to.Type != fromType)
            {
                // type if not the same, can it be assigned?
                if (Types.CanBeCast(fromType, to.Type))
                    value = Expression.Convert(value, to.Type);
                else if (Types.IsNullable(fromType) && fromType.GetGenericArguments()[0] == to.Type)
                    value = Expression.Convert(value, to.Type);
                else if (Types.IsNullable(fromType) && Types.CanBeCast(fromType.GetGenericArguments()[0], to.Type))
                    value = Expression.Convert(value, to.Type);
                else
                    return null;
            }

            if (to.Type == typeof(byte[]))
            {
                var setBytes = typeof(SqlDataRecord).GetMethod("SetBytes", new[] { typeof(int), typeof(long), typeof(byte[]), typeof(int), typeof(int) });
                Contract.Assert(setBytes != null);
                return Expression.Call(result, setBytes, Expression.Constant(to.Ordinal), Expression.Constant(0L), value, Expression.Constant(0), Expression.PropertyOrField(value, "Length"));
            }

            var setMethod = typeof(SqlDataRecord).GetMethod(SetMethodName(to.Type), new[] { typeof(int), to.Type });
            Contract.Assert(setMethod != null);
            return Expression.Call(result, setMethod, Expression.Constant(to.Ordinal), value);
        }

        static string SetMethodName(Type colType)
        {
            if (Types.IsNullable(colType)) colType = colType.GetGenericArguments()[0];
            if (colType == typeof(float)) return "SetFloat";
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

        public override bool Equals(object obj) => obj is TypeAndMetaData && Equals((TypeAndMetaData)obj);

        public override int GetHashCode()
        {
            unchecked
            {
                return (Type.GetHashCode() *397) ^ MetaData.Length;
            }
        }
    }

    public class TableType : IEnumerable<SqlDataRecord>
    {
        /// <summary>The name of the SQL Server table type</summary>
        public string TypeName { get; }

        /// <summary>The encoded <see cref="SqlDataRecord"/></summary>
        public IEnumerable<SqlDataRecord> Records { get; }

        public TableType(string typeName, IEnumerable<SqlDataRecord> records)
        {
            Contract.Requires(typeName != null);
            // note: records must be null, an empty enumeration will cause a SQL exception
            TypeName = typeName;
            Records = records;
        }

        /// <summary>Returns an enumerator that iterates through the collection.</summary>
        public IEnumerator<SqlDataRecord> GetEnumerator() => Records.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

}