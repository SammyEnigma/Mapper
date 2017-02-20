using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

namespace BusterWood.Mapper
{
    public struct DataSequence<T> : IEnumerable<T>, IDisposable
    {
        readonly Func<DbDataReader, T> map;
        readonly DbDataReader reader;
        readonly Action<DbDataReader, T> extraAction;

        public DataSequence(DbDataReader reader, Action<DbDataReader, T> extraAction)
        {
            Contract.Requires(reader != null);
            Contract.Requires(reader.IsClosed == false);
            this.reader = reader;
            this.extraAction = extraAction;
            map = Extensions.GetMappingFunc<T>(reader);
        }

        /// <remarks>The underlying <see cref="DbDataReader"/> is disposed the <see cref="DataSequenceEnumerator"/> has been enumerated, i.e. after a foreach() loop on it</remarks>
        public DataSequenceEnumerator GetEnumerator() => new DataSequenceEnumerator(this);

        /// <remarks>The underlying <see cref="DbDataReader"/> is disposed the <see cref="DataSequenceEnumerator"/> has been enumerated, i.e. after a foreach() loop on it</remarks>
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

        /// <remarks>The underlying <see cref="DbDataReader"/> is disposed the <see cref="DataSequenceEnumerator"/> has been enumerated, i.e. after a foreach() loop on it</remarks>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>Reads the next value from the underlying data reader</summary>
        /// <returns>Default for T if there is no next record</returns>
        public async Task<T> NextOrDefaultAsync() => await reader.ReadAsync() ? map(reader) : default(T);

        /// <summary>
        /// Not call is normally need for this, enumerating this struct will close the reader
        /// </summary>
        void IDisposable.Dispose()
        {
            reader.Dispose();
        }

        public struct DataSequenceEnumerator : IEnumerator<T>
        {
            readonly DataSequence<T> _seq;

            internal DataSequenceEnumerator(DataSequence<T> seq)
            {
                _seq = seq;
                Current = default(T);
            }

            /// <summary>Gets the element in the collection at the current position of the enumerator.</summary>
            /// <returns>The element in the collection at the current position of the enumerator.</returns>
            public T Current { get; private set; }

            object IEnumerator.Current => Current;

            /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
            public void Dispose()
            {
                _seq.reader.Dispose();
                Current = default(T);
            }

            /// <summary>Advances the enumerator to the next element of the collection.</summary>
            /// <returns>true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.</returns>
            public bool MoveNext()
            {
                if (_seq.reader.Read())
                {
                    Current = _seq.map(_seq.reader);
                    _seq.extraAction?.Invoke(_seq.reader, Current);
                    return true;
                }
                else
                {
                    Current = default(T);
                    return false;
                }
            }

            /// <summary>Sets the enumerator to its initial position, which is before the first element in the collection.</summary>
            /// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception>
            public void Reset()
            {
                throw new InvalidOperationException();
            }
        }
    }

}
