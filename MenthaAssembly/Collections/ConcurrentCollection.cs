using System.Linq;
using System.Threading;

namespace System.Collections.Generic
{
    /// <summary>
    /// Represents a thread-safe collection that can be accessed by multiple threads concurrently.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    [Serializable]
    public class ConcurrentCollection<T> : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable, IList, ICollection, IReadOnlyList<T>, IReadOnlyCollection<T>
    {
        public virtual T this[int Index]
        {
            get => GetItem(Index);
            set => SetItem(Index, value);
        }

        public int Count
            => Items.Count;

        protected readonly List<T> Items = [];
        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentCollection{T}"/> class that is empty and has the default initial capacity.
        /// </summary>
        public ConcurrentCollection()
        {
            Items = [];
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentCollection{T}"/> class that contains elements copied from the specified collection and has sufficient capacity to accommodate the number of elements copied.
        /// </summary>
        /// <param name="Items">The collection whose elements are copied to the new collection.</param>
        public ConcurrentCollection(IEnumerable<T> Items)
        {
            this.Items = new List<T>(Items);
        }

        protected virtual T GetItem(int Index)
            => (Monitor.IsEntered(LockObj) ? Items : [.. Items])[Index];

        protected virtual void SetItem(int Index, T Value)
            => Handle(() => Items[Index] = Value);

        public virtual void Add(T Item)
            => Handle(() => Items.Add(Item));

        /// <summary>
        /// Adds items to the <see cref="ConcurrentCollection{T}"/>.
        /// </summary>
        /// <param name="Items">The objects to add to the <see cref="ConcurrentCollection{T}"/>.</param>
        public virtual void AddRange(IEnumerable<T> Items)
            => Handle(() => this.Items.AddRange(Items));

        public virtual bool Remove(T Item)
            => Handle(() => Items.Remove(Item));

        /// <summary>
        /// Remove objects from <see cref="ConcurrentCollection{T}"/> that satisfy predictions.
        /// </summary>
        /// <param name="Predict">The predictions to be met.</param>
        public virtual void Remove(Predicate<T> Predict)
            => Handle(() =>
            {
                for (int i = Items.Count - 1; i >= 0; i--)
                    if (Predict(Items[i]))
                        Items.RemoveAt(i);
            });

        /// <summary>
        /// Removes all specific objects from <see cref="ConcurrentCollection{T}"/>.
        /// </summary>
        /// <param name="Items">The objects to remove from <see cref="ConcurrentCollection{T}"/>.</param>
        public virtual void Remove(IEnumerable<T> Items)
            => Handle(() =>
            {
                if (Items is not T[] and not IList and not ICollection)
                    Items = Items.ToArray();

                foreach (T Item in Items)
                    this.Items.Remove(Item);
            });

        public virtual void RemoveAt(int Index)
            => Handle(() => Items.RemoveAt(Index));

        /// <summary>
        /// Attempts to remove and return the value that satisfies the prediction from the <see cref="ConcurrentCollection{T}"/>.
        /// </summary>
        /// <param name="Predict">The prediction of the element to remove and return.</param>
        /// <param name="Item">When this method returns, contains the object removed from the <see cref="ConcurrentCollection{T}"/>, or the default value of the <typeparamref name="T"/> type if no object meets the prediction.</param>
        /// <returns>true if the object was removed successfully; otherwise, false.</returns>
        public virtual bool TryRemove(Predicate<T> Predict, out T Item)
        {
            T Temp = default;
            try
            {
                return Handle(() =>
                {
                    for (int i = Items.Count - 1; i >= 0; i--)
                    {
                        Temp = Items[i];
                        if (Predict(Temp))
                        {
                            Items.RemoveAt(i);
                            return true;
                        }
                    }

                    return false;
                });
            }
            finally
            {
                Item = Temp;
            }
        }

        public virtual void Insert(int Index, T Item)
            => Handle(() => Items.Insert(Index, Item));

        public virtual void Clear()
            => Handle(Items.Clear);

        public virtual bool Contains(T Item)
            => (Monitor.IsEntered(LockObj) ? Items : [.. Items]).Contains(Item);

        public virtual void CopyTo(T[] Array, int ArrayIndex)
            => (Monitor.IsEntered(LockObj) ? Items : [.. Items]).CopyTo(Array, ArrayIndex);

        public virtual int IndexOf(T Item)
            => (Monitor.IsEntered(LockObj) ? Items : [.. Items]).IndexOf(Item);

        public void ForEach(Action<T> Action)
            => Handle(() =>
            {
                foreach (T item in Items)
                    Action(item);
            });

        public IEnumerator<T> GetEnumerator()
            => Items.ToList().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator()
            => Items.ToArray().GetEnumerator();

        private readonly object LockObj = new();
        protected internal virtual U Handle<U>(Func<U> Func)
        {
            bool Token = false;
            try
            {
                Monitor.Enter(LockObj, ref Token);
                return Func();
            }
            finally
            {
                if (Token)
                    Monitor.Exit(LockObj);
            }
        }
        protected internal virtual void Handle(Action Action)
        {
            bool Token = false;
            try
            {
                Monitor.Enter(LockObj, ref Token);
                Action();
            }
            finally
            {
                if (Token)
                    Monitor.Exit(LockObj);
            }
        }

        //public virtual void Lock()
        //    => Monitor.Enter(LockObj);
        //public virtual void Unlock()
        //{
        //    if (Monitor.IsEntered(LockObj))
        //        Monitor.Exit(LockObj);
        //}

        #region IList

        object IList.this[int Index]
        {
            get => GetItem(Index);
            set
            {
                if (value is T i)
                    SetItem(Index, i);
            }
        }

        bool IList.IsFixedSize
            => ((IList)Items).IsFixedSize;

        bool IList.IsReadOnly
            => ((IList)Items).IsReadOnly;

        int IList.Add(object Value)
        {
            if (Value is T Item)
            {
                Add(Item);
                return Items.Count;
            }

            return -1;
        }

        void IList.Remove(object Item)
        {
            if (Item is T i)
                Handle(() => Remove(i));
        }

        void IList.Insert(int Index, object Value)
        {
            if (Value is T Item)
                Insert(Index, Item);
        }

        bool IList.Contains(object Item)
            => Item is T i && Contains(i);

        int IList.IndexOf(object Value)
            => Value is T Item ? IndexOf(Item) : -1;

        #endregion

        #region ICollection
        bool ICollection<T>.IsReadOnly
            => ((ICollection<T>)Items).IsReadOnly;

        bool ICollection.IsSynchronized
            => ((ICollection)Items).IsSynchronized;

        object ICollection.SyncRoot
            => ((ICollection)Items).SyncRoot;

        void ICollection.CopyTo(Array Array, int Index)
            => Handle(() => ((ICollection)Items).CopyTo(Array, Index));

        #endregion

        public override string ToString()
            => $"Count = {Count}";

    }
}