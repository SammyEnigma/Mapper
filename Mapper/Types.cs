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

        public static bool IsNullable(Type type)
        {
            if (type.IsPrimitive) return false;
            if (type.IsConstructedGenericType && type.GetGenericTypeDefinition() == typeof (Nullable<>)) return true;
            if (type.IsEnum) return false;
            if (!type.IsClass) return false;
            return true;
        }

        public static bool IsStructured(Type type)
        {
            return typeof (IEnumerable<SqlDataRecord>).IsAssignableFrom(type);
        }
    }
}