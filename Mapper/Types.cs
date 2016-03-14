using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using Microsoft.SqlServer.Server;

namespace Mapper
{
    static class Types
    {
        public static readonly Dictionary<Type, DbType> TypeToDbType;
        public static readonly Dictionary<DbType, Type> DBTypeToType;

        static Types()
        {
            TypeToDbType = new Dictionary<Type, DbType>
            {
                [typeof(byte)] = DbType.Byte,
                [typeof(sbyte)] = DbType.SByte,
                [typeof(short)] = DbType.Int16,
                [typeof(ushort)] = DbType.UInt16,
                [typeof(int)] = DbType.Int32,
                [typeof(uint)] = DbType.UInt32,
                [typeof(long)] = DbType.Int64,
                [typeof(ulong)] = DbType.UInt64,
                [typeof(float)] = DbType.Single,
                [typeof(double)] = DbType.Double,
                [typeof(decimal)] = DbType.Decimal,
                [typeof(bool)] = DbType.Boolean,
                [typeof(string)] = DbType.String,
                [typeof(char)] = DbType.StringFixedLength,
                [typeof(Guid)] = DbType.Guid,
                [typeof(DateTime)] = DbType.DateTime,
                [typeof(DateTimeOffset)] = DbType.DateTimeOffset,
                [typeof(byte[])] = DbType.Binary,
                [typeof(byte?)] = DbType.Byte,
                [typeof(sbyte?)] = DbType.SByte,
                [typeof(short?)] = DbType.Int16,
                [typeof(ushort?)] = DbType.UInt16,
                [typeof(int?)] = DbType.Int32,
                [typeof(uint?)] = DbType.UInt32,
                [typeof(long?)] = DbType.Int64,
                [typeof(ulong?)] = DbType.UInt64,
                [typeof(float?)] = DbType.Single,
                [typeof(double?)] = DbType.Double,
                [typeof(decimal?)] = DbType.Decimal,
                [typeof(bool?)] = DbType.Boolean,
                [typeof(char?)] = DbType.StringFixedLength,
                [typeof(Guid?)] = DbType.Guid,
                [typeof(DateTime?)] = DbType.DateTime,
                [typeof(DateTimeOffset?)] = DbType.DateTimeOffset
            };
            DBTypeToType = new Dictionary<DbType, Type>
            {
                [DbType.AnsiString] = typeof(string),
                [DbType.AnsiStringFixedLength] = typeof(string),
            };
            foreach (var pair in TypeToDbType)
            {
                var type = pair.Key;
                if (type.IsGenericType) continue; // ignore nullables
                DBTypeToType.Add(pair.Value, type);
            }
        }

        public static bool AreCompatible(Type inType, Type outType)
        {
            return inType == outType || CanBeCast(inType, outType);
        }

        public static bool CanBeCast(Type inType, Type outType)
        {
            return outType.IsAssignableFrom(inType)
                   || (inType.IsPrimitive && outType.IsPrimitive)
                   || (outType.IsEnum && inType.IsPrimitive) // enum assignment is not handled in "IsAssignableFrom"
                   || (outType.IsPrimitive && inType.IsEnum);
        }

        public static Type PropertyOrFieldType(MemberInfo member)
        {
            Contract.Requires(member != null);
            var prop = member as PropertyInfo;
            if (prop != null) return prop.PropertyType;
            return ((FieldInfo) member).FieldType;
        }

        public static bool CanBeNull(Type type)
        {
            if (type.IsPrimitive) return false;
            if (IsNullable(type)) return true;
            if (type.IsEnum) return false;
            if (!type.IsClass) return false;
            return true;
        }

        public static bool IsNullable(Type type) {
            return type.IsConstructedGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        public static bool IsPrimitiveOrEnum(this Type type)
        {
            Contract.Requires(type != null);
            return type.IsPrimitive || type.IsEnum;
        }

        public static bool IsStructured(Type type)
        {
            Contract.Requires(type != null);
            return typeof (IEnumerable<SqlDataRecord>).IsAssignableFrom(type);
        }

        public static Type NullableOf(this Type type)
        {
            Contract.Requires(type != null);
            Contract.Requires(type.IsGenericType);
            Contract.Requires(type.GetGenericTypeDefinition() == typeof(Nullable<>));
            return type.GetGenericArguments()[0];
        }

        public static Dictionary<string, MemberInfo> WritablePropertiesAndFields<T>()
        {
            var map = typeof (T).GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => p.CanWrite)
                .Cast<MemberInfo>()
                .ToDictionary(p => p.Name, p => p, StringComparer.OrdinalIgnoreCase);
            foreach (var field in typeof (T).GetFields(BindingFlags.Instance | BindingFlags.Public).Where(f => !f.IsInitOnly))
            {
                map[field.Name] = field;
            }
            return map;
        }

        public static Dictionary<string, MemberInfo> ReadablePropertiesAndFields<T>()
        {
            var map = typeof (T).GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => p.CanRead)
                .Cast<MemberInfo>()
                .ToDictionary(p => p.Name, p => p, StringComparer.OrdinalIgnoreCase);
            foreach (var field in typeof (T).GetFields(BindingFlags.Instance | BindingFlags.Public))
            {
                map[field.Name] = field;
            }
            return map;
        }
    }

    public static class Names
    {
        public static List<string> CandidateNames(string name, Type type)
        {
            var names = new List<string>(2) { name };

            // special handling of xxxId to xxx for primitive types (int, long, etc)
            if (name.EndsWith("Id", StringComparison.OrdinalIgnoreCase))
            {
                if (type.IsPrimitiveOrEnum() || (Types.IsNullable(type) && type.NullableOf().IsPrimitiveOrEnum()))
                    names.Add(name.Substring(0, name.Length - 2));
            }

            // see if we need to replace underscores
            int max = names.Count;
            for (int i = 0; i < max; i++)
            {
                if (names[i].IndexOf('_') >= 0)
                    names.Add(names[i].Replace("_", ""));
            }
            return names;
        }
    }
}