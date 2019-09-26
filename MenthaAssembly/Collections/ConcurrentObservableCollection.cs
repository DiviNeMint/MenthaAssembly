using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading;

namespace MenthaAssembly
{

    public class ConcurrentObservableCollection<T> : ObservableCollection<T>
    {
        protected SpinLock SpinLock = new SpinLock();

        public new void Add(T Item)
        {
            bool LockTaken = false;
            try
            {
                SpinLock.Enter(ref LockTaken);
                base.Add(Item);
            }
            finally
            {
                if (LockTaken)
                    SpinLock.Exit(false);
            }
        }

        public new void Insert(int Index, T Item)
        {
            bool LockTaken = false;
            try
            {
                SpinLock.Enter(ref LockTaken);
                base.Insert(Index, Item);
            }
            finally
            {
                if (LockTaken)
                    SpinLock.Exit(false);
            }
        }

        public new void Move(int OldIndex, int NewIndex)
        {
            bool LockTaken = false;
            try
            {
                SpinLock.Enter(ref LockTaken);
                base.Move(OldIndex, NewIndex);
            }
            finally
            {
                if (LockTaken)
                    SpinLock.Exit(false);
            }
        }

        public new int IndexOf(T Item)
        {
            bool LockTaken = false;
            try
            {
                SpinLock.Enter(ref LockTaken);
                return base.IndexOf(Item);
            }
            finally
            {
                if (LockTaken)
                    SpinLock.Exit(false);
            }
        }

        public new bool Remove(T Item)
        {
            bool LockTaken = false;
            try
            {
                SpinLock.Enter(ref LockTaken);
                return base.Remove(Item);
            }
            finally
            {
                if (LockTaken)
                    SpinLock.Exit(false);
            }
        }

        public new void RemoveAt(int Index)
        {
            bool LockTaken = false;
            try
            {
                SpinLock.Enter(ref LockTaken);
                base.RemoveAt(Index);
            }
            finally
            {
                if (LockTaken)
                    SpinLock.Exit(false);
            }
        }

        public new void Clear()
        {
            bool LockTaken = false;
            try
            {
                SpinLock.Enter(ref LockTaken);
                base.Clear();
            }
            finally
            {
                if (LockTaken)
                    SpinLock.Exit(false);
            }
        }

        public void DisposeItems()
        {
            bool LockTaken = false;
            try
            {
                SpinLock.Enter(ref LockTaken);
                foreach (T item in Items)
                {
                    if (item is IDisposable DisposableItem)
                        DisposableItem.Dispose();
                }
                base.Clear();
            }
            finally
            {
                if (LockTaken)
                    SpinLock.Exit(false);
            }
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
            => ConcurrentHelper.OnCollectionChanged(this, e);

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
            => ConcurrentHelper.OnPropertyChanged(this, e);

    }
}
