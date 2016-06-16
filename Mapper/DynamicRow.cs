using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;

namespace Mapper
{
    public class DynamicRow : IDynamicMetaObjectProvider, IReadOnlyDictionary<string, object>
    {
        readonly IReadOnlyDictionary<string, int> ordinalsByName;
        readonly object[] values;

        public DynamicRow(IReadOnlyDictionary<string, int> ordinalsByName, object[] values)
        {
            Contract.Requires(ordinalsByName != null);
            Contract.Requires(values != null);
            Contract.Requires(ordinalsByName.Count > 0);
            Contract.Requires(ordinalsByName.Count <= values.Length); // there might be less names due to duplicates
            Contract.Requires(Contract.ForAll(ordinalsByName, pair => pair.Value >= 0 && pair.Value < values.Length));

            this.ordinalsByName = ordinalsByName;
            this.values = values;
        }

        public object this[string key]
        {
            get
            {
                int ordinal;
                return ordinalsByName.TryGetValue(key, out ordinal) ? values[ordinal] : null;
            }
        }

        public object this[int ordinal] => values[ordinal];
        public int Count => values.Length;
        public IEnumerable<string> Keys => ordinalsByName.Keys;
        public IEnumerable<object> Values => values;
        public bool ContainsKey(string key) => ordinalsByName.ContainsKey(key);
        public DynamicMetaObject GetMetaObject(Expression parameter) => new DynamicRowMetaObject(parameter, BindingRestrictions.Empty, this);
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            foreach (var pair in ordinalsByName)
            {
                yield return new KeyValuePair<string, object>(pair.Key, values[pair.Value]);
            }
        }

        public bool TryGetValue(string key, out object value)
        {
            int ordinal;
            if (ordinalsByName.TryGetValue(key, out ordinal))
            {
                value = values[ordinal];
                return true;
            }
            value = null;
            return false;
        }

    }

    class DynamicRowMetaObject : DynamicMetaObject
    {
        static readonly MethodInfo GetValue = typeof(IReadOnlyDictionary<string, object>).GetProperty("Item").GetGetMethod();

        public DynamicRowMetaObject(Expression expression, BindingRestrictions restrictions) : base(expression, restrictions)
        {
        }

        public DynamicRowMetaObject(Expression expression, BindingRestrictions restrictions, object value) : base(expression, restrictions, value)
        {
        }

        DynamicMetaObject CallMethod(MethodInfo method, Expression[] parameters)
        {
            return new DynamicMetaObject(
                Expression.Call(Expression.Convert(Expression, LimitType), method, parameters),
                BindingRestrictions.GetTypeRestriction(Expression, LimitType)
            );
        }

        public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
        {
            return CallMethod(GetValue, new Expression[] { Expression.Constant(binder.Name) });
        }

        ///<remarks>Needed for Visual basic dynamic support</remarks>
        public override DynamicMetaObject BindInvokeMember(InvokeMemberBinder binder, DynamicMetaObject[] args)
        {
            return CallMethod(GetValue, new Expression[] { Expression.Constant(binder.Name) });
        }
    }

    public struct DynamicDataSequence : IEnumerable<DynamicRow>, IDisposable
    {
        readonly DbDataReader reader;
        readonly Dictionary<string, int> metadata;

        internal DynamicDataSequence(DbDataReader reader)
        {
            Contract.Requires(reader != null);
            this.reader = reader;

            metadata = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < reader.FieldCount; i++)
            {
                var name = reader.GetName(i);
                if (!metadata.ContainsKey(name))  // columns can have duplicate names, just add the first one
                    metadata.Add(name, i);
            }
        }

        public void Dispose()
        {
            reader.Dispose();
        }

        public IEnumerator<DynamicRow> GetEnumerator()
        {
            while (reader.Read())
            {
                var values = new object[metadata.Count];
                reader.GetValues(values);
                yield return new DynamicRow(metadata, values);
            }
            Dispose();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
