using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace MenthaAssembly
{
    public class ConcurrentCollection<T> : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable, IList, ICollection, IReadOnlyList<T>, IReadOnlyCollection<T>
    {
        protected readonly object LockObject = new object();

        public virtual T this[int Index]
        {
            get => GetItem(Index);
            set => SetItem(Index, value);
        }

        public int Count
            => Items.Count;

        internal readonly List<T> Items = new List<T>();
        public ConcurrentCollection()
        {
            Items = new List<T>();
        }
        public ConcurrentCollection(IEnumerable<T> Items)
        {
            this.Items = new List<T>(Items);
        }

        protected virtual T GetItem(int Index)
            => Handle(() => Items[Index]);

        protected virtual void SetItem(int Index, T Value)
            => Handle(() => Items[Index] = Value);

        public virtual void Add(T Item)
            => Handle(() => Items.Add(Item));

        public virtual void AddRange(IEnumerable<T> Items)
            => Handle(() => this.Items.AddRange(Items));

        public virtual bool Remove(T Item)
            => Handle(() => Items.Remove(Item));

        public virtual void Remove(IEnumerable<T> Items)
            => Handle(() =>
            {
                if (Items is not T[] &&
                    Items is not IList &&
                    Items is not ICollection)
                    Items = Items.ToArray();

                foreach (T Item in Items)
                    this.Items.Remove(Item);
            });

        public virtual void RemoveAt(int Index)
            => Handle(() => Items.RemoveAt(Index));

        public virtual void Insert(int Index, T Item)
            => Handle(() => Items.Insert(Index, Item));

        public virtual void Clear()
            => Handle(() => Items.Clear());

        public virtual bool Contains(T Item)
            => Handle(() => Items.Contains(Item));

        public virtual void CopyTo(T[] Array, int ArrayIndex)
            => Handle(() => Items.CopyTo(Array, ArrayIndex));

        public virtual int IndexOf(T Item)
            => Handle(() => Items.IndexOf(Item));

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

        protected internal U Handle<U>(Func<U> Func)
        {
            bool Token = false;
            try
            {
                Monitor.Enter(LockObject, ref Token);
                return Func();
            }
            finally
            {
                if (Token)
                    Monitor.Exit(LockObject);
            }
        }
        protected internal void Handle(Action Action)
        {
            bool Token = false;
            try
            {
                Monitor.Enter(LockObject, ref Token);
                Action();
            }
            finally
            {
                if (Token)
                    Monitor.Exit(LockObject);
            }
        }

        public void Lock()
            => Monitor.Enter(LockObject);
        public void Unlock()
        {
            if (Monitor.IsEntered(LockObject))
                Monitor.Exit(LockObject);
        }

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