using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reflection;
using Microsoft.SqlServer.Server;

namespace Mapper
{
    static class Types
    {
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

        public static bool IsStructured(Type type)
        {
            return typeof (IEnumerable<SqlDataRecord>).IsAssignableFrom(type);
        }
    }
}