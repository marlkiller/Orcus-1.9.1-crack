using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Orcus.Server.Core.Utilities
{
    public class ActionsQueue<T> : IEnumerable<T>, ICollection
    {
        public delegate void ProcessItemDelegate(T item);

        private readonly object _executionLock = new object();
        private readonly Queue<T> _internalQueue;
        private readonly ProcessItemDelegate _processItemDelegate;
        private readonly List<T> _activeExecutions;

        protected ActionsQueue(ProcessItemDelegate processItemDelegate, Queue<T> internalQueue)
        {
            _processItemDelegate = processItemDelegate;
            _internalQueue = internalQueue;
            _activeExecutions = new List<T>();
        }

        public ActionsQueue(ProcessItemDelegate processItemDelegate) : this(processItemDelegate, new Queue<T>())
        {
        }

        public ActionsQueue(int capacity, ProcessItemDelegate processItemDelegate)
            : this(processItemDelegate, new Queue<T>(capacity))
        {
        }

        public ActionsQueue(IEnumerable<T> collection, ProcessItemDelegate processItemDelegate)
            : this(processItemDelegate, new Queue<T>(collection))
        {
        }

        public int ParallelExecutionLimit { get; set; }

        public void CopyTo(Array array, int index)
        {
            ((ICollection) _internalQueue).CopyTo(array, index);
        }

        public int Count => _internalQueue.Count;
        object ICollection.SyncRoot => ((ICollection) _internalQueue).SyncRoot;
        bool ICollection.IsSynchronized => ((ICollection) _internalQueue).IsSynchronized;

        public IEnumerator<T> GetEnumerator()
        {
            return _internalQueue.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _internalQueue.GetEnumerator();
        }

        public void Enqueue(T item)
        {
            lock (_executionLock)
            {
                if (_activeExecutions.Count >= ParallelExecutionLimit)
                {
                    _internalQueue.Enqueue(item);
                }
                else
                {
                    ProcessItem(item);
                }
            }
        }

        private void ProcessItem(T item)
        {
            _activeExecutions.Add(item);
            new Thread(() =>
            {
                try
                {
                    _processItemDelegate(item);
                }
                finally 
                {
                    lock (_executionLock)
                    {
                        _activeExecutions.Remove(item);
                        if (_internalQueue.Count > 0 && _activeExecutions.Count < ParallelExecutionLimit)
                        {
                            var nextItem = _internalQueue.Dequeue();
                            ProcessItem(nextItem);
                        }
                    }
                }
            }).Start();
        }
    }
}