using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Huojian.LibraryManagement.Common.ObjectModel
{
    public class NotifyCollection<T> : Collection<T>, INotifyCollectionChanged
    {
        public NotifyCollection()
            : base()
        { }


        public void InsertRange(int index, IEnumerable<T> collection)
        {
            var addedItems = collection.ToArray();
            int insertIndex = index;
            foreach (var item in addedItems)
            {
                base.InsertItem(insertIndex++, item);
                AttachItemChanged(item);
            }

            OnItemAdded(index, addedItems);
        }

        public void RemoveRange(int index, int count)
        {
            var removedItems = new T[count];

            for (int i = index; i < count; i++)
            {
                var removedItem = this[i];
                base.RemoveAt(i);
                removedItems[i - index] = removedItem;
                DettachItemChanged(removedItem);
            }

            OnItemRemoved(index, removedItems);
        }

        protected override void InsertItem(int index, T item)
        {
            base.InsertItem(index, item);

            AttachItemChanged(item);
            OnItemAdded(index, new T[] { item });
        }

        protected override void ClearItems()
        {
            var removedItems = Items.ToArray();

            base.ClearItems();

            foreach (var removedItem in removedItems)
                DettachItemChanged(removedItem);

            OnItemRemoved(0, removedItems);
        }

        protected override void RemoveItem(int index)
        {
            T removedItem = this[index];

            base.RemoveItem(index);

            DettachItemChanged(removedItem);

            OnItemRemoved(index, new T[] { removedItem });
        }

        protected override void SetItem(int index, T item)
        {
            T oldItem = this[index];
            base.SetItem(index, item);

            DettachItemChanged(oldItem);
            AttachItemChanged(item);

            OnItemReplaced(index, item, oldItem);
        }

        protected void CopyFrom(IEnumerable<T> collection)
        {
            IList<T> items = Items;
            if (collection != null && items != null)
            {
                using (IEnumerator<T> enumerator = collection.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        items.Add(enumerator.Current);
                    }
                }
            }
        }

        protected virtual void OnItemAdded(int insertIndex, T[] addedItem)
        {
            RaiseCollectionChanged(new NotifyCollectionChangedEventArgs<T>
            {
                ChangedMode = NotifyCollectionChangedMode.Add,
                Added = new NotifyCollectionAdded<T>(insertIndex, addedItem)
            });
            _notifyCollectionChanged?.Invoke(this,
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, addedItem, insertIndex));
        }

        protected virtual void OnItemRemoved(int removedStartIndex, T[] removedItems)
        {
            RaiseCollectionChanged(new NotifyCollectionChangedEventArgs<T>
            {
                ChangedMode = NotifyCollectionChangedMode.Remove,
                Removed = new NotifyCollectionRemoved<T>(removedStartIndex, removedItems)
            });
            _notifyCollectionChanged?.Invoke(this,
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removedItems, removedStartIndex));
        }

        protected virtual void OnItemReplaced(int updatedIndex, T newItem, T oldItem)
        {
            RaiseCollectionChanged(new NotifyCollectionChangedEventArgs<T>
            {
                ChangedMode = NotifyCollectionChangedMode.Replace,
                Replaced = new NotifyCollectionReplaced<T>(updatedIndex, newItem, oldItem)
            });
            _notifyCollectionChanged?.Invoke(this,
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItem, oldItem));
        }

        protected virtual void OnItemUpdated(int updatedIndex, T updatedItem, string propertyName)
        {
            RaiseCollectionChanged(new NotifyCollectionChangedEventArgs<T>
            {
                ChangedMode = NotifyCollectionChangedMode.Update,
                Updated = new NotifyCollectionUpdated<T>(updatedIndex, updatedItem, propertyName)
            });
        }

        protected void RaiseCollectionChanged(NotifyCollectionChangedEventArgs<T> args)
        {
            CollectionChanged?.Invoke(this, args);
        }

        protected void AttachItemChanged(T item)
        {
            if (item is INotifyPropertyChanged notifyItem)
                notifyItem.PropertyChanged += NotifyItem_PropertyChanged;
        }

        protected void DettachItemChanged(T item)
        {
            if (item is INotifyPropertyChanged notifyItem)
                notifyItem.PropertyChanged -= NotifyItem_PropertyChanged;
        }

        private void NotifyItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var item = (T)sender;
            var index = IndexOf(item);
            OnItemUpdated(index, item, e.PropertyName);
        }


        public event EventHandler<NotifyCollectionChangedEventArgs<T>> CollectionChanged;

        private NotifyCollectionChangedEventHandler _notifyCollectionChanged;

        event NotifyCollectionChangedEventHandler INotifyCollectionChanged.CollectionChanged
        {
            add
            {
                _notifyCollectionChanged += value;
            }
            remove
            {
                _notifyCollectionChanged -= value;
            }
        }
    }

    public enum NotifyCollectionChangedMode
    {
        Add,
        Remove,
        Update,
        Replace
    }

    public class NotifyCollectionChangedEventArgs<T> : EventArgs
    {
        public NotifyCollectionChangedMode ChangedMode { get; set; }

        public NotifyCollectionAdded<T> Added { get; set; }
        public NotifyCollectionRemoved<T> Removed { get; set; }
        public NotifyCollectionUpdated<T> Updated { get; set; }
        public NotifyCollectionReplaced<T> Replaced { get; set; }
    }

    public class NotifyCollectionAdded<T>
    {
        public NotifyCollectionAdded(int index, T[] items)
        {
            Index = index;
            Items = items;
        }

        public int Index { get; }

        public T[] Items { get; }
    }

    public class NotifyCollectionRemoved<T>
    {
        public NotifyCollectionRemoved(int index, T[] items)
        {
            Index = index;
            Items = items;
        }

        public int Index { get; }

        public T[] Items { get; }
    }

    public class NotifyCollectionUpdated<T>
    {
        public NotifyCollectionUpdated(int index, T item, string propertyName)
        {
            Index = index;
            Item = item;
            PropertyName = propertyName;
        }

        public int Index { get; }

        public T Item { get; }

        public string PropertyName { get; }
    }

    public class NotifyCollectionReplaced<T>
    {
        public NotifyCollectionReplaced(int index, T newItem, T oldItem)
        {
            Index = index;
            NewItem = newItem;
            OldItem = oldItem;
        }

        public int Index { get; }

        public T NewItem { get; }

        public T OldItem { get; }
    }
}
