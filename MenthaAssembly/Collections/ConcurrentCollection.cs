using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;

namespace MenthaAssembly
{
    public class ConcurrentCollection<T> : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable, IList, ICollection, IReadOnlyList<T>, IReadOnlyCollection<T>
    {
        protected readonly object LockObject = new object();

        protected readonly Collection<T> BaseCollection;
        protected readonly IList BaseList;

        public ConcurrentCollection()
        {
            this.BaseCollection = new Collection<T>();
            this.BaseList = BaseCollection;
        }
        public ConcurrentCollection(IList<T> List)
        {
            this.BaseCollection = new Collection<T>(List);
            this.BaseList = BaseCollection;
        }

        public virtual T this[int Index]
        {
            get => GetItem(Index);
            set => SetItem(Index, value);
        }
        object IList.this[int Index]
        {
            get => GetItem(Index);
            set => SetItem(Index, value);
        }

        public int Count
            => Handle(() => BaseCollection.Count);

        public bool IsReadOnly
            => false;

        public bool IsFixedSize
            => false;

        public object SyncRoot
            => BaseCollection;

        public bool IsSynchronized
            => ((ICollection)BaseCollection).IsSynchronized;

        protected T GetItem(int Index)
            => Handle(() => BaseCollection[Index]);

        protected virtual void SetItem(int Index, T Value)
            => Handle(() => BaseCollection[Index] = Value);
        protected virtual void SetItem(int Index, object Value)
            => Handle(() => BaseList[Index] = Value);

        public virtual void Add(T Item)
            => Handle(() => BaseCollection.Add(Item));
        public virtual int Add(object Value)
            => Handle(() => BaseList.Add(Value));

        public virtual bool Remove(T Item)
            => Handle(() => BaseCollection.Remove(Item));
        public virtual void Remove(object Value)
            => Handle(() => BaseList.Remove(Value));
        public virtual void RemoveAt(int Index)
            => Handle(() => BaseCollection.RemoveAt(Index));

        public virtual void Insert(int Index, T Item)
            => Handle(() => BaseCollection.Insert(Index, Item));
        public virtual void Insert(int Index, object Value)
            => Handle(() => BaseList.Insert(Index, Value));

        public virtual void Clear()
            => Handle(() => BaseCollection.Clear());

        public bool Contains(T Item)
            => Handle(() => BaseCollection.Contains(Item));
        public bool Contains(object Value)
            => Handle(() => BaseList.Contains(Value));

        public virtual void CopyTo(T[] Array, int ArrayIndex)
            => Handle(() => BaseCollection.CopyTo(Array, ArrayIndex));
        public virtual void CopyTo(Array Array, int Index)
            => Handle(() => ((ICollection)BaseCollection).CopyTo(Array, Index));

        public int IndexOf(T Item)
            => Handle(() => BaseCollection.IndexOf(Item));
        public int IndexOf(object Value)
            => Handle(() => BaseList.IndexOf(Value));

        public IEnumerator<T> GetEnumerator()
            => BaseCollection.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator()
            => BaseCollection.GetEnumerator();

        internal protected U Handle<U>(Func<U> Func)
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
        internal protected void Handle(Action Action)
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

        public override string ToString()
            => $"Count = {this.Count}";

    }
}
