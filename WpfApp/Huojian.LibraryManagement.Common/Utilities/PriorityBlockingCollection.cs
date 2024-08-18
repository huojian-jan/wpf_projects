using System.Collections;
using System.Collections.Concurrent;

namespace ShadowBot.Common.Utilities
{
    public class PriorityBlockingCollection<TValue> : IEnumerable<TValue>, IEnumerable
    {
        private BlockingCollection<PriorityItem<TValue>> _collection;

        public PriorityBlockingCollection()
        {
            _collection = new BlockingCollection<PriorityItem<TValue>>(new PriorityList<TValue>());
        }

        public int Count => _collection.Count;

        public bool TryTake(out TValue value)
        {
            if (_collection.TryTake(out PriorityItem<TValue> item))
            {
                value = item.Value;
                return true;
            }
            else
            {
                value = default;
                return false;
            }
        }

        public TValue Take()
        {
            return _collection.Take().Value;
        }

        public bool TryQueueAtHead(TValue value)
        {
            return _collection.TryAdd(new PriorityItem<TValue>(true, value));
        }

        public void QueueAtHead(TValue value)
        {
            _collection.Add(new PriorityItem<TValue>(true, value));
        }

        public bool TryQueue(TValue value)
        {
            return _collection.TryAdd(new PriorityItem<TValue>(false, value));
        }

        public void Queue(TValue value)
        {
            _collection.Add(new PriorityItem<TValue>(false, value));
        }

        #region IEnumerable

        public IEnumerator<TValue> GetEnumerator()
        {
            return ((IEnumerable<PriorityItem<TValue>>)_collection)
                .Select(m => m.Value)
                .GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)this).GetEnumerator();
        }

        #endregion

        class PriorityItem<T>
        {
            public PriorityItem(bool prior, T value)
            {
                Prior = prior;
                Value = value;
            }

            public bool Prior { get; }

            public T Value { get; }
        }

        class PriorityList<T> : IProducerConsumerCollection<PriorityItem<T>>
        {

            //TODO(了解并发下面的类)
            //private readonly SynchronizedCollection<PriorityItem<T>> _collection;

            public PriorityList()
            {
                //_collection = new SynchronizedCollection<PriorityItem<T>>();
            }

            public int Count => 1;

            public object SyncRoot => default;

            public bool IsSynchronized => true;

            //public void CopyTo(PriorityItem<T>[] array, int index)
            //{
            //    _collection.CopyTo(array, index);
            //}

            //public void CopyTo(Array array, int index)
            //{
            //    ((ICollection)_collection).CopyTo(array, index);
            //}

            //public IEnumerator<PriorityItem<T>> GetEnumerator()
            //{
            //    return _collection.GetEnumerator();
            //}

            //public PriorityItem<T>[] ToArray()
            //{
            //    return _collection.ToArray();
            //}

            //public bool TryAdd(PriorityItem<T> item)
            //{
            //    if (item.Prior)
            //        _collection.Insert(0, item);
            //    else
            //        _collection.Add(item);
            //    return true;
            //}

            //public bool TryTake(out PriorityItem<T> item)
            //{
            //    lock (_collection.SyncRoot)
            //    {
            //        item = null;
            //        if (_collection.Count == 0)
            //            return false;
            //        item = _collection[0];
            //        _collection.RemoveAt(0);
            //        return true;
            //    }
            //}

            //IEnumerator IEnumerable.GetEnumerator()
            //{
            //    return ((IEnumerable)_collection).GetEnumerator();
            //}
        }
    }
}
