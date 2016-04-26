using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Mapper
{
    public class DataReaderMapper
    {
        static readonly MostlyReadDictionary<MetaData, Delegate> Methods = new MostlyReadDictionary<MetaData, Delegate>();

        static readonly Subject<string> _trace = new Subject<string>();

        public static IObservable<string> Trace => _trace;

        internal Delegate GetOrCreateMappingFunc(Type typeT, DbDataReader reader)
        {
            Contract.Requires(typeT != null);
            Contract.Requires(reader != null);

            var columns = CreateColumnList(reader);
            return Methods.GetOrAdd(new MetaData(typeT, columns), md => CreateMapFunc(md.Target, md.Columns));
        }

        static Column[] CreateColumnList(DbDataReader reader)
        {
            Contract.Requires(reader != null);
            Contract.Ensures(Contract.Result<Column[]>() != null);

            var columns = new Column[reader.FieldCount];
            for (int i = 0; i < columns.Length; i++)
            {
                columns[i] = new Column(i, reader.GetName(i), reader.GetFieldType(i));
            }
            return columns;
        }

        static Delegate CreateMapFunc(Type typeT, IReadOnlyCollection<Column> columns)
        {
            Contract.Requires(columns != null);
            Contract.Requires(typeT != null);
            Contract.Ensures(Contract.Result<Delegate>() != null);

            if (typeT.IsPrimitiveOrEnum() || typeT.IsNullable())
            {
                return CreatePrimativeMapFunc(typeT, columns);
            }

            var map = CreateMemberToColumnMap(columns, typeT);
            var readerParam = Expression.Parameter(typeof(DbDataReader), "reader");
            var resultParam = Expression.Parameter(typeT, "result");
            var block = CreateMapBlock(typeT, map, readerParam, resultParam);
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
                _trace.OnNext($"Cannot map column {col0.Name} to {typeT} as column type {col0.Type} is not compatible");

            var readerParam = Expression.Parameter(typeof(DbDataReader), "reader");
            var resultParam = Expression.Parameter(typeT, "result");

            var getMethod = DataReaderGetMethod(typeT);
            Expression value = Expression.Call(readerParam, getMethod, Expression.Constant(0));
            if (col0.Type != typeT)
            {
                value = Expression.Convert(value, typeT);
            }

            var body = Expression.Condition(
                Expression.IsTrue(Expression.Call(readerParam, typeof(DbDataReader).GetMethod("IsDBNull", new[] { typeof(int) }), Expression.Constant(0))),
                Expression.Default(typeT),
                value);

            var func = typeof(Func<,>).MakeGenericType(new[] { typeof(DbDataReader), typeT });
            return Expression.Lambda(func, body, new[] { readerParam }).Compile();
        }

        internal static Dictionary<MemberInfo, Column> CreateMemberToColumnMap(IReadOnlyCollection<Column> columns, Type type)
        {
            Contract.Requires(type != null);
            Contract.Requires(columns != null);

            var map = new Dictionary<MemberInfo, Column>();
            var bestMatch = NewMatchFunc(columns);
            foreach (var member in WriteablePublicFieldsAndProperties(type))
            {
                var col = bestMatch(member.Name);
                if (col.Name != null)
                    map.Add(member, col);
                else
                    _trace.OnNext($"Cannot find a column in the data reader for {type}.{member.Name}");
            }
            return map;
        }

        static Func<string, Column> NewMatchFunc(IReadOnlyCollection<Column> columns)
        {
            Contract.Requires(columns != null);
            var nameToColumns = columns
                .SelectMany(col => Names.CandidateNames(col.Name, col.Type), (col, name) => new { col, name })
                .OrderByDescending(x => x.col.Name.Length) // make sure columns with the longest names match first
                .ToLookup(x => x.name, x => x.col, StringComparer.OrdinalIgnoreCase);
            return name => nameToColumns[name].FirstOrDefault();
        }

        static IEnumerable<MemberInfo> WriteablePublicFieldsAndProperties(Type type)
        {
            Contract.Requires(type != null);
            const BindingFlags PublicInstance = BindingFlags.Public | BindingFlags.Instance;
            return type.GetFields(PublicInstance).Where(field => !field.IsInitOnly).Cast<MemberInfo>()
                   .Concat(type.GetProperties(PublicInstance).Where(prop => prop.CanWrite));
        }

        static BlockExpression CreateMapBlock(Type type, Dictionary<MemberInfo, Column> map, ParameterExpression reader, ParameterExpression result)
        {
            Contract.Requires(type != null);
            Contract.Requires(map != null);
            Contract.Ensures(Contract.Result<BlockExpression>() != null);

            var constructor = type.GetConstructor(Type.EmptyTypes);
            Contract.Assert(constructor != null);
            var lines = new List<Expression> { Expression.Assign(result, Expression.New(constructor)) };
            foreach (var pair in map)
            {
                var member = pair.Key;
                var col = pair.Value;
                if (Types.AreCompatible(col.Type, Types.PropertyOrFieldType(member)))
                    lines.Add(AssignDefaultOrValue(reader, col, member, result));
                else
                    _trace.OnNext($"Cannot map column {col.Name} to {type}.{member.Name} as column type {col.Type} is not compatible with {Types.PropertyOrFieldType(member)}");
            }
            lines.Add(result); // the return value
            return Expression.Block(new[] { result }, lines);
        }

        static ConditionalExpression AssignDefaultOrValue(ParameterExpression reader, Column col, MemberInfo member, ParameterExpression result)
        {
            return Expression.IfThenElse(
                Expression.IsTrue(Expression.Call(reader, typeof(DbDataReader).GetMethod("IsDBNull", new[] { typeof(int) }), Expression.Constant(col.Ordinal))),
                Expression.Assign(Expression.PropertyOrField(result, member.Name), Expression.Default(PropertyOrFieldType(member))),
                AssignValue(member, col, result, reader));
        }

        static Type PropertyOrFieldType(MemberInfo member) => (member as PropertyInfo)?.PropertyType ?? (member as FieldInfo).FieldType;

        static Expression AssignValue(MemberInfo member, Column col, ParameterExpression result, ParameterExpression reader)
        {
            if (col.Type == typeof(byte[]))
            {
                return AssignRowVersionTimestampValue(member, col, result, reader);
            }

            var getMethod = DataReaderGetMethod(col.Type);
            Expression value = Expression.Call(reader, getMethod, Expression.Constant(col.Ordinal));
            var outType = PropertyOrFieldType(member);
            if (col.Type != outType)
            {
                value = Expression.Convert(value, outType);
            }
            return Expression.Assign(Expression.PropertyOrField(result, member.Name), value);
        }

        static Expression AssignRowVersionTimestampValue(MemberInfo member, Column col, ParameterExpression result, ParameterExpression reader)
        {
            //BIG assumption here is all byte[] fields are SQL Server Timestamp (RowVersion) fields
            var buffer = Expression.Parameter(typeof(byte[]), "buffer");
            var getBytes = typeof(DbDataReader).GetMethod("GetBytes", new[] { typeof(int), typeof(long), typeof(byte[]), typeof(int), typeof(int) });
            return Expression.Block(
                new[] { buffer },
                Expression.Assign(buffer, Expression.NewArrayBounds(typeof(byte), Expression.Constant(4))),
                Expression.Call(reader, getBytes, Expression.Constant(col.Ordinal), Expression.Constant(0L), buffer, Expression.Constant(0), Expression.Constant(4)),
                Expression.Assign(Expression.PropertyOrField(result, member.Name), buffer)
            );
        }

        static MethodInfo DataReaderGetMethod(Type columnType)
        {
            Contract.Requires(columnType != null);
            Contract.Ensures(Contract.Result<MethodInfo>() != null);
            var dataReader = typeof(DbDataReader);
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
            throw new NotSupportedException(columnType.ToString());
        }

        struct MetaData : IEquatable<MetaData>
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
                    if (Columns[i] != other.Columns[i]) return false;
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

    struct Column : IEquatable<Column>
    {
        public int Ordinal { get; }
        public string Name { get; }
        public Type Type { get; }

        public Column(int ordinal, string name, Type type)
        {
            Ordinal = ordinal;
            Name = name;
            Type = type;
        }

        public override bool Equals(object obj) => obj is Column && Equals((Column)obj);
        public bool Equals(Column other) => Ordinal == other.Ordinal && Name == other.Name && Type == other.Type;
        public override int GetHashCode() => Ordinal;
        public static bool operator ==(Column left, Column right) => left.Equals(right);
        public static bool operator !=(Column left, Column right) => !left.Equals(right);
    }

}