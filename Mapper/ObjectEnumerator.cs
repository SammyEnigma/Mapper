﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BusterWood.Mapper
{
    public static class Seq
    {
        public static bool Contains<T>(this IEnumerable<T> items, Func<T, bool> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));
            if (items == null)
                return false;
            foreach (var item in items)
            {
                if (predicate(item))
                    return true;
            }
            return false;
        }

        public static IEnumerable<T> Lazy<T>(Func<T[]> itemFunc)
        {
            if (itemFunc == null)
                throw new ArgumentNullException(nameof(itemFunc));
            foreach (var item in itemFunc())
            {
                yield return item;
            }
        }
    }

    public static class ObjectEnumerator
    {       
        /// <summary>Turns an object into a sequence of key value pairs</summary>
        /// <remarks>Uses reflection, TODO: a version that uses LINQ expressions</remarks>
        public static IEnumerable<KeyValuePair<string, object>> AsSeq(this object item)
        {
            if (item == null)
                return Enumerable.Empty<KeyValuePair<string, object>>();

            var type = item.GetType();

            var fields = Seq.Lazy(() => type.GetFields())
                .Select(f => new KeyValuePair<string, object>(f.Name, f.GetValue(item)));

            var properties = Seq.Lazy(() => type.GetProperties())
                .Where(p => p.CanRead)
                .Select(p => new KeyValuePair<string, object>(p.Name, p.GetValue(item)));
            
            return fields.Concat(properties).Where(pair => pair.Value != null);
        }

        /// <summary>Turns a sequence of key value pairs into an object using the ctor or writeable fields and properties (if the ctor has zero parameters)</summary>
        /// <remarks>Uses reflection, TODO: a version that uses LINQ expressions</remarks>
        public static T New<T>(this IEnumerable<KeyValuePair<string, object>> values)
        {
            if (values == null)
                return default(T);

            var longestCtor = typeof(T).GetConstructors()
                .OrderByDescending(c => c.GetParameters().Length)
                .FirstOrDefault();

            if (longestCtor?.GetParameters().Length > 0)
                return NewCtor<T>(longestCtor, values);
            else
                return NewSetProperties<T>(values);
        }

        private static T NewSetProperties<T>(IEnumerable<KeyValuePair<string, object>> values)
        {
            var writeable = Types.WriteablePublicFieldsAndProperties(typeof(T)).ToDictionary(m => m.Name, StringComparer.OrdinalIgnoreCase); // case insensitive
            object item = Activator.CreateInstance(typeof(T)); // force boxing so setting values via reflection works for structs
            foreach (var pair in values)
            {
                MemberInfo member;
                if (writeable.TryGetValue(pair.Key, out member))
                {
                    member.SetValue(item, pair.Value);
                }
            }
            return (T)item; // unbox struct
        }

        private static void SetValue(this MemberInfo member, object obj, object value)
        {
            if (member is PropertyInfo)
                ((PropertyInfo)member).SetValue(obj, value);
            else
                ((FieldInfo)member).SetValue(obj, value);

        }

        private static T NewCtor<T>(ConstructorInfo ctor, IEnumerable<KeyValuePair<string, object>> values)
        {
            var map = values.ToDictionary(p => p.Key, p => p.Value, StringComparer.OrdinalIgnoreCase); // case insensitive

            var args = ctor.GetParameters()
                .Select(p => map.ContainsKey(p.Name) ? map[p.Name] : Default(p.ParameterType))
                .ToArray();

            return (T)ctor.Invoke(args);
        }

        private static object Default(Type type) => type.IsValueType ? Activator.CreateInstance(type) : null;
    }
}
