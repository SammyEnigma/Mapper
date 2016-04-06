using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Mapper
{
    public static class DataReaderExtensions
    {
        private static readonly MostlyReadDictionary<MetaData, Delegate> Methods = new MostlyReadDictionary<MetaData, Delegate>();

        /// <summary>Reads exactly one item from the reader</summary>
        /// <exception cref="InvalidOperationException"> when zero values read or more than one value can be read</exception>
        public static T ReadSingle<T>(this IDataReader reader)
        {
            Contract.Requires(reader != null);
            Contract.Requires(reader.IsClosed == false);
            Contract.Ensures(Contract.Result<T>() != null);
            if (!reader.Read()) throw new InvalidOperationException("Expected one value to be read but reader is empty");
            var map = GetMappingFunc<T>(reader);
            var single = map(reader);
            if (reader.Read()) throw new InvalidOperationException("Expected one value to be read but more than one value can be read");
            return single;
        }

        /// <summary>Reads exactly one item from the reader</summary>
        /// <exception cref="InvalidOperationException"> when zero values read or more than one value can be read</exception>
        public static async Task<T> ReadSingleAsync<T>(this SqlDataReader reader)
        {
            Contract.Requires(reader != null);
            Contract.Requires(reader.IsClosed == false);
            Contract.Ensures(Contract.Result<T>() != null);
            if (! await reader.ReadAsync()) throw new InvalidOperationException("Expected one value to be read but reader is empty");
            var map = GetMappingFunc<T>(reader);
            var single = map(reader);
            if (await reader.ReadAsync()) throw new InvalidOperationException("Expected one value to be read but more than one value can be read");
            return single;
        }

        /// <summary>Reads zero or one items from the reader</summary>
        /// <remarks>Returns the default vaue of T if no values be read, i.e may return null</remarks>
        public static T ReadSingleOrDefault<T>(this IDataReader reader)
        {
            Contract.Requires(reader != null);
            Contract.Requires(reader.IsClosed == false);
            if (!reader.Read()) return default(T);
            var map = GetMappingFunc<T>(reader);
            return map(reader);
        }

        /// <summary>Reads zero or one items from the reader</summary>
        /// <remarks>Returns the default vaue of T if no values be read, i.e may return null</remarks>
        public static async Task<T> ReadSingleOrDefaultAsync<T>(this SqlDataReader reader)
        {
            Contract.Requires(reader != null);
            Contract.Requires(reader.IsClosed == false);
            if (! await reader.ReadAsync()) return default(T);
            var map = GetMappingFunc<T>(reader);
            return map(reader);
        }

        /// <summary>Reads all the records in the reader into a list</summary>
        public static List<T> ReadList<T>(this IDataReader reader)
        {
            Contract.Requires(reader != null);
            Contract.Requires(reader.IsClosed == false);
            Contract.Ensures(Contract.Result<List<T>>() != null);
            var map = GetMappingFunc<T>(reader);
            var list = new List<T>();
            while (reader.Read())
            {
                list.Add(map(reader));
            }
            return list;
        }

        /// <summary>Reads all the records in the reader into a list</summary>
        public static async Task<List<T>> ReadListAsync<T>(this SqlDataReader reader)
        {
            Contract.Requires(reader != null);
            Contract.Requires(reader.IsClosed == false);
            Contract.Ensures(Contract.Result<List<T>>() != null);
            var map = GetMappingFunc<T>(reader);
            var list = new List<T>();
            while (await reader.ReadAsync())
            {
                list.Add(map(reader));
            }
            return list;
        }

        /// <summary>Reads all the records in the reader into a dictionary, using the supplied <paramref name="keyFunc"/> to generate the key</summary>
        public static Dictionary<TKey, TValue> ReadDictionary<TKey, TValue>(this IDataReader reader, Func<TValue, TKey> keyFunc)
        {
            Contract.Requires(reader != null);
            Contract.Requires(keyFunc != null);
            Contract.Requires(reader.IsClosed == false);
            Contract.Ensures(Contract.Result<Dictionary<TKey, TValue>>() != null);
            var map = GetMappingFunc<TValue>(reader);
            var dict = new Dictionary<TKey, TValue>();
            while (reader.Read())
            {
                TValue value = map(reader);
                TKey key = keyFunc(value);
                dict.Add(key, value);
            }
            return dict;
        }

        /// <summary>Reads all the records in the reader into a dictionary, using the supplied <paramref name="keyFunc"/> to generate the key</summary>
        public static async Task<Dictionary<TKey, TValue>> ReadDictionaryAsync<TKey, TValue>(this SqlDataReader reader, Func<TValue, TKey> keyFunc)
        {
            Contract.Requires(reader != null);
            Contract.Requires(keyFunc != null);
            Contract.Requires(reader.IsClosed == false);
            Contract.Ensures(Contract.Result<Dictionary<TKey, TValue>>() != null);
            var map = GetMappingFunc<TValue>(reader);
            var dict = new Dictionary<TKey, TValue>();
            while (await reader.ReadAsync())
            {
                TValue value = map(reader);
                TKey key = keyFunc(value);
                dict.Add(key, value);
            }
            return dict;
        }

        /// <summary>Reads all the records in the lookup, group by key, using the supplied <paramref name="keyFunc"/> to generate the key</summary>
        public static HashLookup<TKey, TValue> ReadLookup<TKey, TValue>(this IDataReader reader, Func<TValue, TKey> keyFunc)
        {
            Contract.Requires(reader != null);
            Contract.Requires(keyFunc != null);
            Contract.Requires(reader.IsClosed == false);
            var map = GetMappingFunc<TValue>(reader);
            var lookup = new HashLookup<TKey, TValue>();
            while (reader.Read())
            {
                TValue value = map(reader);
                TKey key = keyFunc(value);
                lookup.Add(key, value);
            }
            return lookup;
        }

        /// <summary>Reads all the records in the lookup, group by key, using the supplied <paramref name="keyFunc"/> to generate the key</summary>
        public static async Task<HashLookup<TKey, TValue>> ReadLookupAsync<TKey, TValue>(this SqlDataReader reader, Func<TValue, TKey> keyFunc)
        {
            Contract.Requires(reader != null);
            Contract.Requires(keyFunc != null);
            Contract.Requires(reader.IsClosed == false);
            var map = GetMappingFunc<TValue>(reader);
            var lookup = new HashLookup<TKey, TValue>();
            while (await reader.ReadAsync())
            {
                TValue value = map(reader);
                TKey key = keyFunc(value);
                lookup.Add(key, value);
            }
            return lookup;
        }

        internal static Func<IDataReader, T> GetMappingFunc<T>(IDataReader reader)
        {
            Delegate func = GetOrCreateMappingFunc(typeof(T), reader);
            return (Func<IDataReader, T>)func;
        }

        private static Delegate GetOrCreateMappingFunc(Type typeT, IDataReader reader)
        {
            var columns = CreateColumnList(reader);
            return Methods.GetOrAdd(new MetaData(typeT, columns), md => CreateMapFunc(md.Target, md.Columns));
        }

        private static Column[] CreateColumnList(IDataReader reader)
        {
            var columns = new Column[reader.FieldCount];
            for (int i = 0; i < reader.FieldCount; i++)
            {
                var name = reader.GetName(i);
                var type = reader.GetFieldType(i);
                columns[i] = new Column(i, name, type);
            }
            return columns;
        }

        private static Delegate CreateMapFunc(Type typeT, IReadOnlyCollection<Column> columns)
        {
            var map = CreateMemberToColumnMap(columns, typeT);
            var readerParam = Expression.Parameter(typeof (IDataReader), "reader");
            var resultParam = Expression.Parameter(typeT, "result");
            var block = CreateMapBlock(typeT, map, readerParam, resultParam);
            var func = typeof(Func<,>).MakeGenericType(new[] { typeof(IDataReader), typeT });
            return Expression.Lambda(func, block, new[] { readerParam }).Compile();
        }

        internal static Dictionary<MemberInfo, Column> CreateMemberToColumnMap(IReadOnlyCollection<Column> columns, Type type)
        {
            var map = new Dictionary<MemberInfo, Column>();
            var bestMatch = NewMatchFunc(columns);
            foreach (var member in WriteablePublicFieldsAndProperties(type))
            {
                var col = bestMatch(member.Name);
                if (col.Name != null) map.Add(member, col);
            }
            return map;
        }

        private static Func<string, Column> NewMatchFunc(IReadOnlyCollection<Column> columns)
        {
            var nameToColumns = columns
                .SelectMany(col => Names.CandidateNames(col.Name, col.Type), (col, name) => new { col, name })
                .OrderByDescending(x => x.col.Name.Length) // make sure columns with the longest names match first
                .ToLookup(x => x.name, x => x.col, StringComparer.OrdinalIgnoreCase);
            return name => nameToColumns[name].FirstOrDefault();
        }

        private static IEnumerable<MemberInfo> WriteablePublicFieldsAndProperties(Type type)
        {
            const BindingFlags PublicInstance = BindingFlags.Public | BindingFlags.Instance;
            return type.GetFields(PublicInstance).Where(field => !field.IsInitOnly).Cast<MemberInfo>()
                   .Concat(type.GetProperties(PublicInstance).Where(prop => prop.CanWrite));
        }

        private static BlockExpression CreateMapBlock(Type type, Dictionary<MemberInfo, Column> map, ParameterExpression reader, ParameterExpression result)
        {
            Contract.Requires(type != null);
            Contract.Requires(map != null);
            Contract.Ensures(Contract.Result<BlockExpression>() != null);

            var constructor = type.GetConstructor(Type.EmptyTypes);
            Contract.Assert(constructor != null);
            var lines = new List<Expression>{ Expression.Assign(result, Expression.New(constructor))  };
            lines.AddRange(map
                .Where(pair => Types.AreCompatible(pair.Value.Type, Types.PropertyOrFieldType(pair.Key)))
                .Select(pair => AssignDefaultOrValue(reader, pair.Value, pair.Key, result)));
            lines.Add(result); // the return value
            return Expression.Block(new[] { result }, lines);
        }

        private static ConditionalExpression AssignDefaultOrValue(ParameterExpression reader, Column col, MemberInfo member, ParameterExpression result)
        {
            return Expression.IfThenElse(
                Expression.IsTrue(Expression.Call(reader, typeof(IDataRecord).GetMethod("IsDBNull", new[] { typeof(int) }), Expression.Constant(col.Ordinal))), 
                Expression.Assign(Expression.PropertyOrField(result, member.Name), Expression.Default(PropertyOrFieldType(member))), 
                AssignValue(member, col, result, reader));
        }

        private static Type PropertyOrFieldType(MemberInfo member)
        {
            var prop = member as PropertyInfo;
            var field = member as FieldInfo;
            return prop?.PropertyType ?? field.FieldType;
        }

        private static Expression AssignValue(MemberInfo member, Column col, ParameterExpression result, ParameterExpression reader)
        {
            if (col.Type == typeof (byte[]))
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

        private static Expression AssignRowVersionTimestampValue(MemberInfo member, Column col, ParameterExpression result, ParameterExpression reader)
        {
            //BIG assumption here is all byte[] fields are SQL Server Timestamp (RowVersion) fields
            var buffer = Expression.Parameter(typeof (byte[]), "buffer");
            var getBytes = typeof (IDataRecord).GetMethod("GetBytes", new[] {typeof (int), typeof (long), typeof (byte[]), typeof (int), typeof (int)});
            return Expression.Block(
                new[] {buffer},
                Expression.Assign(buffer, Expression.NewArrayBounds(typeof (byte), Expression.Constant(4))),
                Expression.Call(reader, getBytes, Expression.Constant(col.Ordinal), Expression.Constant(0L), buffer, Expression.Constant(0), Expression.Constant(4)),
                Expression.Assign(Expression.PropertyOrField(result, member.Name), buffer)
                );
        }

        private static MethodInfo DataReaderGetMethod(Type columnType)
        {
            Contract.Requires(columnType != null);
            Contract.Ensures(Contract.Result<MethodInfo>() != null);
            if (columnType == typeof (byte))
                return typeof (IDataRecord).GetMethod("GetByte", new[] {typeof (int)});
            if (columnType == typeof (short))
                return typeof (IDataRecord).GetMethod("GetInt16", new[] {typeof (int)});
            if (columnType == typeof (int))
                return typeof (IDataRecord).GetMethod("GetInt32", new[] {typeof (int)});
            if (columnType == typeof (long))
                return typeof (IDataRecord).GetMethod("GetInt64", new[] {typeof (int)});
            if (columnType == typeof (bool))
                return typeof (IDataRecord).GetMethod("GetBoolean", new[] {typeof (int)});
            if (columnType == typeof (string))
                return typeof (IDataRecord).GetMethod("GetString", new[] {typeof (int)});
            if (columnType == typeof (DateTime))
                return typeof (IDataRecord).GetMethod("GetDateTime", new[] {typeof (int)});
            if (columnType == typeof (float))
                return typeof (IDataRecord).GetMethod("GetSingle", new[] {typeof (int)});
            if (columnType == typeof (double))
                return typeof (IDataRecord).GetMethod("GetDouble", new[] {typeof (int)});
            if (columnType == typeof (decimal))
                return typeof (IDataRecord).GetMethod("GetDecimal", new[] {typeof (int)});
            if (columnType == typeof (decimal))
                return typeof (IDataRecord).GetMethod("GetDecimal", new[] {typeof (int)});
            if (columnType == typeof(char))
                return typeof(IDataRecord).GetMethod("GetChar", new[] { typeof(int) });
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
                if (Columns == null || Columns.Count == 0) return other.Columns == null || other.Columns.Count == 0;
                if (Columns.Count != other.Columns.Count) return false;
                for (int i = 0; i < Columns.Count; i++)
                {
                    if (!Columns[i].Equals(other.Columns[i])) return false;
                }
                return true;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                return obj is MetaData && Equals((MetaData) obj);
            }

            public override int GetHashCode()
            {
                int hash = Target.GetHashCode();
                if (Columns == null || Columns.Count == 0) return hash;
                hash *= Columns.Count;
                hash *= Columns[0].Name.GetHashCode();
                hash *= Columns[Columns.Count - 1].Name.GetHashCode();
                return hash;
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

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            return obj is Column && Equals((Column) obj);
        }

        public bool Equals(Column other)
        {
            return Ordinal == other.Ordinal
                && Name == other.Name
                && Type == other.Type;
        }

        public override int GetHashCode()
        {
            return Ordinal;
        }

    }

}