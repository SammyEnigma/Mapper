using System;
using System.Collections.Generic;
using System.Threading;

namespace Mapper
{
    /// <summary>
    /// Represents an object that is both an observable sequence as well as an observer.
    /// Each notification is broadcasted to all subscribed observers.
    /// </summary>
    /// <typeparam name="T">The type of the elements processed by the subject.</typeparam>
    sealed class Subject<T> : IObservable<T>, IObserver<T>
    {
        readonly object gate = new object();
        bool isDisposed;
        bool isStopped;
        List<IObserver<T>> observers;
        Exception exception;

        /// <summary>
        /// Creates a subject.
        /// </summary>
        public Subject()
        {
            observers = new List<IObserver<T>>();
        }

        /// <summary>
        /// Notifies all subscribed observers about the end of the sequence.
        /// </summary>
        public void OnCompleted()
        {
            List<IObserver<T>> os = null;
            lock (gate)
            {
                CheckDisposed();

                if (!isStopped)
                {
                    os = observers;
                    observers = null;
                    isStopped = true;
                }
            }

            if (os != null)
                foreach (var o in os)
                    o.OnCompleted();
        }

        /// <summary>
        /// Notifies all subscribed observers with the exception.
        /// </summary>
        /// <param name="error">The exception to send to all subscribed observers.</param>
        /// <exception cref="ArgumentNullException"><paramref name="error"/> is null.</exception>
        public void OnError(Exception error)
        {
            if (error == null)
                throw new ArgumentNullException("error");

            List<IObserver<T>> os = null;
            lock (gate)
            {
                CheckDisposed();

                if (!isStopped)
                {
                    os = observers;
                    observers = null;
                    isStopped = true;
                    exception = error;
                }
            }

            if (os != null)
                foreach (var o in os)
                    o.OnError(error);
        }

        /// <summary>
        /// Notifies all subscribed observers with the value.
        /// </summary>
        /// <param name="value">The value to send to all subscribed observers.</param>
        public void OnNext(T value)
        {
            List<IObserver<T>> os = null;
            lock (gate)
            {
                CheckDisposed();

                if (!isStopped)
                {
                    os = observers;
                }
            }

            if (os != null)
                foreach (var o in os)
                    o.OnNext(value);
        }

        /// <summary>
        /// Subscribes an observer to the subject.
        /// </summary>
        /// <param name="observer">Observer to subscribe to the subject.</param>
        /// <remarks>IDisposable object that can be used to unsubscribe the observer from the subject.</remarks>
        /// <exception cref="ArgumentNullException"><paramref name="observer"/> is null.</exception>
        public IDisposable Subscribe(IObserver<T> observer)
        {
            if (observer == null)
                throw new ArgumentNullException("observer");

            lock (gate)
            {
                CheckDisposed();

                if (!isStopped)
                {
                    observers = new List<IObserver<T>>(observers);
                    observers.Add(observer);
                    return new Subscription(this, observer);
                }
                else if (exception != null)
                {
                    observer.OnError(exception);
                    return Disposable.Empty;
                }
                else
                {
                    observer.OnCompleted();
                    return Disposable.Empty;
                }
            }
        }

        void Unsubscribe(IObserver<T> observer)
        {
            lock (gate)
            {
                if (observers != null)
                {
                    observers = new List<IObserver<T>>(observers);
                    observers.Remove(observer);
                }
            }
        }

        sealed class Subscription : IDisposable
        {
            Subject<T> subject;
            IObserver<T> observer;

            public Subscription(Subject<T> subject, IObserver<T> observer)
            {
                this.subject = subject;
                this.observer = observer;
            }

            public void Dispose()
            {
                var o = Interlocked.Exchange(ref observer, null);
                if (o != null)
                {
                    subject.Unsubscribe(o);
                    subject = null;
                }
            }
        }

        void CheckDisposed()
        {
            if (isDisposed)
                throw new ObjectDisposedException(string.Empty);
        }

        /// <summary>
        /// Unsubscribe all observers and release resources.
        /// </summary>
        public void Dispose()
        {
            lock (gate)
            {
                isDisposed = true;
                observers = null;
            }
        }
    }

    sealed class Disposable : IDisposable
    {
        public static readonly Disposable Empty = new Disposable();

        public void Dispose()
        {
        }
    }
}
