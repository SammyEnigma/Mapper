using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading;

namespace Mapper
{
    /// <summary>A dictionary that is mostly read from, hardly ever written too</summary>
    class MostlyReadDictionary<TKey, TValue>
    {
        readonly ReaderWriterLockSlim _rwLock = new ReaderWriterLockSlim();
        readonly Dictionary<TKey, TValue> _map = new Dictionary<TKey, TValue>();

        public TValue GetOrAdd(TKey key, Func<TKey, TValue> addFunc)
        {
            Contract.Requires(key != null);
            Contract.Requires(addFunc != null);
            TValue value;

            // simple case of we already have a value for the key
            _rwLock.EnterReadLock();
            try
            {
                if (_map.TryGetValue(key, out value)) return value;
            }
            finally
            {
                _rwLock.ExitReadLock();
            }

            // no value for the key
            _rwLock.EnterUpgradeableReadLock();
            try
            {
                // some other thread may have just added it
                if (_map.TryGetValue(key, out value)) return value;

                // the function may take "some time" so evalutate it outside of the write lock so other threads can still read the dictionary
                value = addFunc(key);

                _rwLock.EnterWriteLock();
                try
                {
                    _map[key] = value;
                }
                finally
                {
                    _rwLock.ExitWriteLock();
                }
                return value;
            }
            finally
            {
                _rwLock.ExitUpgradeableReadLock();
            }
        }
    }
}