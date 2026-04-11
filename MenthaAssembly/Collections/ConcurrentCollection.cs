using System.Threading;

namespace System.Collections.Generic
{
    /// <summary>
    /// Represents a thread-safe collection that can be accessed by multiple threads concurrently.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    [Serializable]
    public class ConcurrentCollection<T> : IList<T>, IList, IReadOnlyList<T>
    {
        protected readonly List<T> Items;
        protected readonly object SyncRoot = new();

        public virtual T this[int Index]
        {
            get => GetItem(Index);
            set => SetItem(Index, value);
        }

        object IList.this[int Index]
        {
            get => GetItem(Index);
            set
            {
                if (value is T Item)
                    SetItem(Index, Item);
            }
        }

        public int Count
            => Handle(() => Items.Count);

        bool IList.IsReadOnly
            => false;
        bool ICollection<T>.IsReadOnly
            => false;

        bool IList.IsFixedSize
            => false;

        bool ICollection.IsSynchronized
            => true;

        object ICollection.SyncRoot
            => SyncRoot;

        public ConcurrentCollection()
        {
            Items = [];
        }
        public ConcurrentCollection(IEnumerable<T> Items)
        {
            if (Items is null)
                throw new ArgumentNullException(nameof(Items));

            this.Items = [.. Items];
        }

        protected virtual T GetItem(int Index)
            => Handle(() => Items[Index]);

        protected virtual void SetItem(int Index, T Value)
            => Handle(() => Items[Index] = Value);

        public virtual void Add(T Item)
            => Handle(() => Items.Add(Item));

        public virtual void AddRange(IEnumerable<T> Items)
            => Handle(() =>
            {
                if (Items is null)
                    throw new ArgumentNullException(nameof(Items));

                this.Items.AddRange(Items);
            });

        public virtual bool Remove(T Item)
            => Handle(() => Items.Remove(Item));

        public virtual void Remove(Predicate<T> Predict)
            => Handle(() =>
            {
                if (Predict is null)
                    throw new ArgumentNullException(nameof(Predict));

                for (int i = Items.Count - 1; i >= 0; i--)
                    if (Predict(Items[i]))
                        Items.RemoveAt(i);
            });

        public virtual void Remove(IEnumerable<T> Items)
            => Handle(() =>
            {
                if (Items is null)
                    throw new ArgumentNullException(nameof(Items));

                IEnumerable<T> Buffer = Items is ICollection<T> ? Items : new List<T>(Items);
                foreach (T Item in Buffer)
                    this.Items.Remove(Item);
            });

        public virtual void RemoveAt(int Index)
            => Handle(() => Items.RemoveAt(Index));

        public virtual bool TryRemove(Predicate<T> Predict, out T Item)
        {
            if (Predict is null)
                throw new ArgumentNullException(nameof(Predict));

            T Result = default(T);
            bool Removed = Handle(() =>
            {
                for (int i = Items.Count - 1; i >= 0; i--)
                {
                    T Current = Items[i];
                    if (!Predict(Current))
                        continue;

                    Items.RemoveAt(i);
                    Result = Current;
                    return true;
                }

                return false;
            });

            Item = Result;
            return Removed;
        }

        public virtual void Insert(int Index, T Item)
            => Handle(() => Items.Insert(Index, Item));

        public virtual void Clear()
            => Handle(() => Items.Clear());

        public virtual bool Contains(T Item)
            => Handle(() => Items.Contains(Item));

        public virtual int IndexOf(T Item)
            => Handle(() => Items.IndexOf(Item));

        public virtual void CopyTo(T[] Array, int ArrayIndex)
            => Handle(() => Items.CopyTo(Array, ArrayIndex));

        void ICollection.CopyTo(Array Array, int Index)
            => Handle(() => ((ICollection)Items.ToArray()).CopyTo(Array, Index));

        protected virtual T[] ToArray()
            => Handle(Items.ToArray);

        protected virtual List<T> ToList()
            => Handle(() => new List<T>(Items));

        public IEnumerator<T> GetEnumerator()
            => ((IEnumerable<T>)ToArray()).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => ToArray().GetEnumerator();

        int IList.Add(object Value)
        {
            if (Value is T Item)
            {
                Add(Item);
                return Count - 1;
            }

            return -1;
        }

        bool IList.Contains(object Value)
            => Value is T Item && Contains(Item);

        int IList.IndexOf(object Value)
            => Value is T Item ? IndexOf(Item) : -1;

        void IList.Insert(int Index, object Value)
        {
            if (Value is T Item)
                Insert(Index, Item);
        }

        void IList.Remove(object Value)
        {
            if (Value is T Item)
                Remove(Item);
        }

        protected internal virtual U Handle<U>(Func<U> Func)
        {
            if (Func is null)
                throw new ArgumentNullException(nameof(Func));

            bool LockTaken = false;
            try
            {
                Monitor.Enter(SyncRoot, ref LockTaken);
                return Func();
            }
            finally
            {
                if (LockTaken)
                    Monitor.Exit(SyncRoot);
            }
        }

        protected internal virtual void Handle(Action Action)
        {
            if (Action is null)
                throw new ArgumentNullException(nameof(Action));

            bool LockTaken = false;
            try
            {
                Monitor.Enter(SyncRoot, ref LockTaken);
                Action();
            }
            finally
            {
                if (LockTaken)
                    Monitor.Exit(SyncRoot);
            }
        }

        public override string ToString()
            => $"Count = {Count}";

    }
}