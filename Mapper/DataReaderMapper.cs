using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace BusterWood.Mapper
{
    public class DataReaderMapper
    {
        static readonly MostlyReadDictionary<MetaData, Delegate> Methods = new MostlyReadDictionary<MetaData, Delegate>();

        internal Delegate GetOrCreateMappingFunc(Type typeT, DbDataReader reader)
        {
            Contract.Requires(typeT != null);
            Contract.Requires(reader != null);

            var columns = CreateColumnList(reader);
            return Methods.GetOrAdd(new MetaData(typeT, columns), md => CreateMapFunc(md.Target, md.Columns));
        }

        internal static Column[] CreateColumnList(DbDataReader reader)
        {
            Contract.Requires(reader != null);
            Contract.Ensures(Contract.Result<Column[]>() != null);

            var columns = new Column[reader.FieldCount];
            for (int i = 0; i < columns.Length; i++)
            {
                columns[i] = new Column(i, reader.GetName(i), reader.GetFieldType(i), reader.GetDataTypeName(i));
            }
            return columns;
        }

        static Delegate CreateMapFunc(Type typeT, IReadOnlyCollection<Column> columns)
        {
            Contract.Requires(columns != null);
            Contract.Requires(typeT != null);
            Contract.Ensures(Contract.Result<Delegate>() != null);

            if (typeT.IsPrimitiveOrEnum() || typeT.IsNullable()
                 || typeT == typeof(string) || typeT == typeof(Guid) || typeT == typeof(DateTime) || typeT == typeof(DateTimeOffset)
                || (typeT.IsClass == false && typeT.Name.Contains("Id")) && typeT.Namespace.Contains("BusterWood")) // special case for Id, IntId, LongId and GuidId structs
            {
                return CreatePrimativeMapFunc(typeT, columns);
            }

            MappingResult<Thing, Thing> result = Mapping.CreateFromSource(columns.Cast<Thing>().ToList(), Types.WriteablePublicThings(typeT), typeT.Name);
            if (result.Mapped.Count == 0)
                throw new InvalidOperationException("No columns were mapped to type " + typeT);
            var readerParam = Expression.Parameter(typeof(DbDataReader), "reader");
            var resultParam = Expression.Parameter(typeT, "result");
            var block = CreateMapBlock(typeT, result.Mapped, readerParam, resultParam);
            var func = typeof(Func<,>).MakeGenericType(new[] { typeof(DbDataReader), typeT });
            return Expression.Lambda(func, block, new[] { readerParam }).Compile();
        }

        static Delegate CreatePrimativeMapFunc(Type typeT, IReadOnlyCollection<Column> columns)
        {
            Contract.Requires(typeT != null);
            Contract.Requires(columns != null);
            Contract.Requires(columns.Count > 0);

            var col0 = columns.First();
            if (!Types.AreCompatible(col0.Type, typeT))
                Mapping._trace.OnNext($"Cannot map column {col0.Name} to {typeT} as column type {col0.Type} is not compatible");

            var readerParam = Expression.Parameter(typeof(DbDataReader), "reader");
            var resultParam = Expression.Parameter(typeT, "result");

            bool readAsObject;
            var getMethod = DataReaderGetMethod(col0.Type, out readAsObject);
            Expression value = Expression.Call(readerParam, getMethod, Expression.Constant(0));

            if (readAsObject)
                value = Expression.Convert(value, col0.Type);

            if (col0.Type != typeT)
                value = ConvertOrCastValue(value, col0.Type, typeT);

            var body = Expression.Condition(
                Expression.IsTrue(Expression.Call(readerParam, typeof(DbDataReader).GetMethod("IsDBNull", new[] { typeof(int) }), Expression.Constant(0))),
                Expression.Default(typeT),
                value);

            var func = typeof(Func<,>).MakeGenericType(new[] { typeof(DbDataReader), typeT });
            return Expression.Lambda(func, body, new[] { readerParam }).Compile();
        }

        static BlockExpression CreateMapBlock(Type type, IReadOnlyList<Mapping<Thing, Thing>> mappping, ParameterExpression reader, ParameterExpression result)
        {
            Contract.Requires(type != null);
            Contract.Requires(mappping != null);
            Contract.Ensures(Contract.Result<BlockExpression>() != null);
            var ctor = type.IsClass ? (Expression)Expression.New(type.GetConstructor(Type.EmptyTypes)) : Expression.Default(type);
            Contract.Assert(ctor != null);
            var lines = new List<Expression> { Expression.Assign(result, ctor) };
            foreach (var map in mappping)
            {
                lines.Add(AssignDefaultOrValue(reader, (Column)map.From, map.To, result));
            }
            lines.Add(result); // the return value
            return Expression.Block(new[] { result }, lines);
        }

        static ConditionalExpression AssignDefaultOrValue(ParameterExpression reader, Column from, Thing to, ParameterExpression result)
        {
            return Expression.IfThenElse(
                Expression.IsTrue(Expression.Call(reader, typeof(DbDataReader).GetMethod("IsDBNull", new[] { typeof(int) }), Expression.Constant(from.Ordinal))),
                Expression.Assign(Expression.PropertyOrField(result, to.Name), Expression.Default(to.Type)),
                AssignValue(from, to, result, reader));
        }

        static Expression AssignValue(Column from, Thing to, ParameterExpression result, ParameterExpression reader)
        {
            if (from.Type == typeof(byte[]))
            {
                if (string.Equals("timestamp", from.DataTypeName, StringComparison.OrdinalIgnoreCase) || string.Equals("rowversion", from.DataTypeName, StringComparison.OrdinalIgnoreCase))
                    return AssignRowVersionTimestampValue(from, to, result, reader);
                return AssignByteArray(from, to, result, reader);
            }

            bool readAsObject;
            var getMethod = DataReaderGetMethod(from.Type, out readAsObject);
            Expression value = Expression.Call(reader, getMethod, Expression.Constant(from.Ordinal));
            if (readAsObject)
                value = Expression.Convert(value, from.Type);

            if (from.Type != to.Type)
            {
                value = ConvertOrCastValue(value, from.Type, to.Type);
            }
            return Expression.Assign(Expression.PropertyOrField(result, to.Name), value);
        }

        private static Expression ConvertOrCastValue(Expression value, Type fromType, Type outType)
        {
            if (outType.IsNullable())
            {
                var outArgType = outType.GetGenericArguments()[0];
                if (fromType == outArgType)
                    return Expression.Convert(value, outType); // e.g. int to int?

                var castMethod2 = Types.GetExplicitCastOperator(fromType, outArgType);
                if (castMethod2 != null)
                {
                    return Expression.Convert(Expression.Call(castMethod2, value), outType);
                }
            }

            var castMethod = Types.GetExplicitCastOperator(fromType, outType);
            if (castMethod != null)
            {
                return Expression.Call(castMethod, value);
            }

            return Expression.Convert(value, outType);
        }

        static Expression AssignRowVersionTimestampValue(Column from, Thing to, ParameterExpression result, ParameterExpression reader)
        {
            var buffer = Expression.Parameter(typeof(byte[]), "buffer");
            var getBytes = typeof(DbDataReader).GetMethod("GetBytes", new[] { typeof(int), typeof(long), typeof(byte[]), typeof(int), typeof(int) });
            return Expression.Block(
                new[] { buffer },
                Expression.Assign(buffer, Expression.NewArrayBounds(typeof(byte), Expression.Constant(8))),
                Expression.Call(reader, getBytes, Expression.Constant(from.Ordinal), Expression.Constant(0L), buffer, Expression.Constant(0), Expression.Constant(8)),
                Expression.Assign(Expression.PropertyOrField(result, to.Name), buffer)
            );
        }
        static Expression AssignByteArray(Column from, Thing to, ParameterExpression result, ParameterExpression reader)
        {
            var stream = Expression.Parameter(typeof(System.IO.Stream), "stream");
            var ms = Expression.Parameter(typeof(System.IO.MemoryStream), "ms");
            var getStream = typeof(DbDataReader).GetMethod("GetStream", new[] { typeof(int) });
            var copyTo = typeof(System.IO.Stream).GetMethod("CopyTo", new[] { typeof(System.IO.Stream) });
            var toArray = typeof(System.IO.MemoryStream).GetMethod("ToArray");
            var dispose = typeof(System.IO.Stream).GetMethod("Dispose");
            return Expression.Block(
                new[] { stream, ms },
                Expression.Assign(ms, Expression.New(typeof(System.IO.MemoryStream))),
                Expression.Assign(stream, Expression.Call(reader, getStream, Expression.Constant(from.Ordinal))),
                Expression.Call(stream, copyTo, ms),
                Expression.Assign(Expression.PropertyOrField(result, to.Name), Expression.Call(ms, toArray)),
                Expression.Call(ms, dispose),
                Expression.Call(stream, dispose)
            );
        }

        static MethodInfo DataReaderGetMethod(Type columnType, out bool readAsObject)
        {
            Contract.Requires(columnType != null);
            Contract.Ensures(Contract.Result<MethodInfo>() != null);
            var dataReader = typeof(DbDataReader);
            readAsObject = false;
            var ordinal = new[] { typeof(int) };
            if (columnType == typeof(byte))
                return dataReader.GetMethod("GetByte", ordinal);
            if (columnType == typeof(short))
                return dataReader.GetMethod("GetInt16", ordinal);
            if (columnType == typeof(int))
                return dataReader.GetMethod("GetInt32", ordinal);
            if (columnType == typeof(long))
                return dataReader.GetMethod("GetInt64", ordinal);
            if (columnType == typeof(bool))
                return dataReader.GetMethod("GetBoolean", ordinal);
            if (columnType == typeof(string))
                return dataReader.GetMethod("GetString", ordinal);
            if (columnType == typeof(DateTime))
                return dataReader.GetMethod("GetDateTime", ordinal);
            if (columnType == typeof(float))
                return dataReader.GetMethod("GetSingle", ordinal);
            if (columnType == typeof(double))
                return dataReader.GetMethod("GetDouble", ordinal);
            if (columnType == typeof(decimal))
                return dataReader.GetMethod("GetDecimal", ordinal);
            if (columnType == typeof(decimal))
                return dataReader.GetMethod("GetDecimal", ordinal);
            if (columnType == typeof(char))
                return dataReader.GetMethod("GetChar", ordinal);
            if (columnType == typeof(Guid))
                return dataReader.GetMethod("GetGuid", ordinal);
            if (columnType == typeof(DateTimeOffset))
            {
                readAsObject = true;
                return dataReader.GetMethod("GetValue", ordinal);
            }
            throw new NotSupportedException(columnType.ToString());
        }

        internal struct MetaData : IEquatable<MetaData>
        {
            public readonly Type Target;
            public readonly IReadOnlyList<Column> Columns;

            public MetaData(Type target, IReadOnlyList<Column> columns)
            {
                Columns = columns;
                Target = target;
            }

            public bool Equals(MetaData other)
            {
                if (Target != other.Target) return false;
                if (Columns.Count != other.Columns.Count) return false;
                for (int i = 0; i < Columns.Count; i++)
                {
                    if (!Columns[i].Equals(other.Columns[i])) return false;
                }
                return true;
            }

            public override bool Equals(object obj) => obj is MetaData && Equals((MetaData)obj);

            public override int GetHashCode()
            {
                unchecked
                {
                    int hash = Target.GetHashCode();
                    if (Columns.Count == 0) return hash;
                    hash *= Columns.Count;
                    hash *= Columns[0].Name.GetHashCode();
                    hash *= Columns[Columns.Count - 1].Name.GetHashCode();
                    return hash;
                }
            }

        }

    }

 
}