using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using Microsoft.SqlServer.Server;

namespace BusterWood.Mapper
{
    static class Types
    {
        internal static readonly Dictionary<Type, DbType> TypeToDbType;
        internal static readonly Dictionary<DbType, Type> DBTypeToType;

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

        public static Type TypeFromSqlTypeName(string sqlType)
        {
            switch (sqlType)
            {
                case "char":
                case "nchar":
                case "varchar":
                case "nvarchar":
                    return typeof(string);
                case "numeric":
                    return typeof(decimal);
                case "bit":
                    return typeof(bool);
                case "tinyint":
                    return typeof(byte);
                case "smallint":
                    return typeof(short);
                case "int":
                    return typeof(int);
                case "bigint":
                    return typeof(long);
                case "datetime":
                case "datetime2":
                    return typeof(DateTime);
                case "timestamp":
                    return typeof(byte[]);
                case "uniqueidentifier":
                    return typeof(Guid);
                default:
                    throw new ArgumentOutOfRangeException(nameof(sqlType), sqlType, "Unknown SQL type");
            }
        }

        [Pure]
        internal static bool AreCompatible(Type inType, Type outType) => inType == outType || CanBeCast(inType, outType);

        [Pure]
        internal static bool CanBeCast(Type inType, Type outType)
        {
            return outType.IsAssignableFrom(inType)
                || (inType.IsPrimitiveOrEnum() && IsNullable(outType) && outType.GetGenericArguments()[0].IsPrimitiveOrEnum())
                   || (inType.IsPrimitive && outType.IsPrimitive)
                   || (outType.IsEnum && inType.IsPrimitive) // enum assignment is not handled in "IsAssignableFrom"
                   || (outType.IsEnum && inType.IsEnum) 
                   || (outType.IsPrimitive && inType.IsEnum);
        }

        [Pure]
        internal static bool AreInSomeSenseCompatible(Type inType, Type outType)
        {
            return AreCompatible(inType, outType) || (IsNullable(inType) && AreCompatible(inType.GetGenericArguments()[0], outType));
        }

        [Pure]
        internal static Type PropertyOrFieldType(this MemberInfo member)
        {
            Contract.Requires(member != null);
            var prop = member as PropertyInfo;
            if (prop != null) return prop.PropertyType;
            return ((FieldInfo) member).FieldType;
        }

        [Pure]
        internal static bool CanBeNull(Type type)
        {
            if (type.IsPrimitive) return false;
            if (IsNullable(type)) return true;
            if (type.IsEnum) return false;
            if (!type.IsClass) return false;
            return true;
        }

        [Pure]
        internal static bool IsNullable(this Type type)
        {
            Contract.Requires(type != null);
            return type.IsConstructedGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        [Pure]
        internal static bool IsNullablePrimitiveOrEnum(this Type type)
        {
            Contract.Requires(type != null);
            if (IsPrimitiveOrEnum(type)) return true;
            return type.IsConstructedGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) && IsPrimitiveOrEnum(type.GetGenericArguments()[0]);
        }

        [Pure]
        internal static bool IsPrimitiveOrEnum(this Type type)
        {
            Contract.Requires(type != null);
            return type.IsPrimitive || type.IsEnum;
        }

        [Pure]
        internal static bool IsStructured(Type type)
        {
            Contract.Requires(type != null);            
            return type == typeof(TableType) || typeof(IEnumerable<SqlDataRecord>).IsAssignableFrom(type);
        }

        [Pure]
        internal static Type NullableOf(this Type type)
        {
            Contract.Requires(type != null);
            Contract.Requires(type.IsGenericType);
            Contract.Requires(type.GetGenericTypeDefinition() == typeof(Nullable<>));
            return type.GetGenericArguments()[0];
        }

        internal static Dictionary<string, MemberInfo> WritablePropertiesAndFields<T>()
        {
            return WriteablePublicFieldsAndProperties(typeof(T)).ToDictionary(p => p.Name, p => p, StringComparer.OrdinalIgnoreCase);
        }

        internal static IReadOnlyCollection<Thing> WriteablePublicThings(Type type)
        {
            const BindingFlags PublicInstance = BindingFlags.Public | BindingFlags.Instance;
            var fields = type.GetFields(PublicInstance).Where(field => !field.IsInitOnly).Select(fi => (Thing)new Field(fi));
            var props = type.GetProperties(PublicInstance).Where(prop => prop.CanWrite).Select(pi => (Thing)new Property(pi));
            return new List<Thing>(fields.Concat(props));
        }

        internal static IReadOnlyCollection<Thing> ReadablePublicThings(Type type)
        {
            const BindingFlags PublicInstance = BindingFlags.Public | BindingFlags.Instance;
            var fields = type.GetFields(PublicInstance).Select(fi => (Thing)new Field(fi));
            var props = type.GetProperties(PublicInstance).Where(prop => prop.CanRead).Select(pi => (Thing)new Property(pi));
            return new List<Thing>(fields.Concat(props));
        }

        internal static ConstructorInfo LongestConstructor(Type type)
        {
            return type.GetConstructors().OrderByDescending(ci => ci.GetParameters().Length).FirstOrDefault();
        }

        internal static IReadOnlyCollection<Thing> ConstructorThings(ConstructorInfo ctor)
        {
            return ctor.GetParameters().Select(pi => (Thing)new Parameter(pi)).ToList();
        }

        internal static IEnumerable<MemberInfo> WriteablePublicFieldsAndProperties(Type type)
        {
            Contract.Requires(type != null);
            const BindingFlags PublicInstance = BindingFlags.Public | BindingFlags.Instance;
            return type.GetFields(PublicInstance).Where(field => !field.IsInitOnly).Cast<MemberInfo>()
                   .Concat(type.GetProperties(PublicInstance).Where(prop => prop.CanWrite));
        }

        internal static Dictionary<string, MemberInfo> ReadablePropertiesAndFields<T>() => ReadablePropertiesAndFieldsDictionary(typeof(T));

        internal static Dictionary<string, MemberInfo> ReadablePropertiesAndFieldsDictionary(Type typeT)
        {
            return ReadablePublicFieldsAndProperties(typeT).ToDictionary(p => p.Name, p => p, StringComparer.OrdinalIgnoreCase);
        }

        internal static IEnumerable<MemberInfo> ReadablePublicFieldsAndProperties(Type type)
        {
            Contract.Requires(type != null);
            const BindingFlags PublicInstance = BindingFlags.Public | BindingFlags.Instance;
            return type.GetFields(PublicInstance).Cast<MemberInfo>()
                   .Concat(type.GetProperties(PublicInstance).Where(prop => prop.CanRead));
        }

    }

    [Flags]
    enum TypeFlags
    {
        Primative,
        Enum,
        NullablePrimative,
        NullableEnum,
    }
}