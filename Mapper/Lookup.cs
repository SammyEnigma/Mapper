// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Mapper
{
    /// <summary>
    /// A key to many value dictionary data type
    /// </summary>
    public class Lookup<TKey, TElement> : ILookup<TKey, TElement>
    {
        private static readonly TElement[] Empty = new TElement[0];
        private readonly IEqualityComparer<TKey> _comparer;
        private Grouping<TKey, TElement>[] _groupings;
        private Grouping<TKey, TElement> _lastGrouping;
        private int _count;

        public Lookup(IEqualityComparer<TKey> comparer = null)
        {
            _comparer = comparer ?? EqualityComparer<TKey>.Default;
            _groupings = new Grouping<TKey, TElement>[7];
        }

        public int Count => _count;

        public void Add(TKey key, TElement item)
        {
            GetGrouping(key, create: true).Add(item);
        }

        /// <summary>Gets the <see cref="T:System.Collections.Generic.IEnumerable`1"/> sequence of values indexed by a specified key.</summary>
        /// <returns>The <see cref="T:System.Collections.Generic.IEnumerable`1"/> sequence of values indexed by the specified key.</returns>
        /// <param name="key">The key of the desired sequence of values.</param>
        /// <remarks>Returns an empty sequence if the key is not present</remarks>
        public IEnumerable<TElement> this[TKey key]
        {
            get
            {
                Grouping<TKey, TElement> grouping = GetGrouping(key, create: false);
                return grouping ?? (IEnumerable<TElement>) Empty;
            }
        }

        public bool Contains(TKey key)
        {
            return GetGrouping(key, create: false) != null;
        }

        public IEnumerator<IGrouping<TKey, TElement>> GetEnumerator()
        {
            Grouping<TKey, TElement> g = _lastGrouping;
            if (g != null)
            {
                do
                {
                    g = g._next;
                    yield return g;
                }
                while (g != _lastGrouping);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        internal int InternalGetHashCode(TKey key)
        {
            // Handle comparer implementations that throw when passed null
            return (key == null) ? 0 : _comparer.GetHashCode(key) & 0x7FFFFFFF;
        }

        internal Grouping<TKey, TElement> GetGrouping(TKey key, bool create)
        {
            int hashCode = InternalGetHashCode(key);
            for (Grouping<TKey, TElement> grouping = _groupings[hashCode % _groupings.Length]; grouping != null; grouping = grouping._hashNext)
            {
                if (grouping._hashCode == hashCode && _comparer.Equals(grouping._key, key))
                {
                    return grouping;
                }
            }

            if (!create)
                return null;

            if (_count == _groupings.Length)
            {
                Resize();
            }

            int index = hashCode%_groupings.Length;
            var newGrouping = new Grouping<TKey, TElement>
            {
                _key = key,
                _hashCode = hashCode,
                _elements = new TElement[1],
                _hashNext = _groupings[index]
            };
            _groupings[index] = newGrouping;
            if (_lastGrouping == null)
            {
                newGrouping._next = newGrouping;
            }
            else
            {
                newGrouping._next = _lastGrouping._next;
                _lastGrouping._next = newGrouping;
            }

            _lastGrouping = newGrouping;
            _count++;
            return newGrouping;
        }

        private void Resize()
        {
            int newSize = checked((_count * 2) + 1);
            var newGroupings = new Grouping<TKey, TElement>[newSize];
            var g = _lastGrouping;
            do
            {
                g = g._next;
                int index = g._hashCode % newSize;
                g._hashNext = newGroupings[index];
                newGroupings[index] = g;
            }
            while (g != _lastGrouping);

            _groupings = newGroupings;
        }
    }
}