using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;

namespace MenthaAssembly
{
    public class ConcurrentObservableCollection<T> : ConcurrentCollection<T>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        private readonly SynchronizationContext OriginalSynchronizationContext = SynchronizationContext.Current;

        public event PropertyChangedEventHandler PropertyChanged;
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public ConcurrentObservableCollection() : base()
        {
        }
        public ConcurrentObservableCollection(IEnumerable<T> Items) : base(Items)
        {
        }

        protected override void SetItem(int Index, T Value)
            => Handle(() =>
            {
                T originalItem = BaseCollection[Index];
                BaseCollection[Index] = Value;

                OnPropertyChanged("Item[]");
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, originalItem, Value, Index));
            });
        protected override void SetItem(int Index, object Value)
            => Handle(() =>
            {
                object originalItem = BaseList[Index];
                BaseList[Index] = Value;

                OnPropertyChanged("Item[]");
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, originalItem, Value, Index));
            });

        public override void Add(T item)
            => Handle(() =>
            {
                BaseCollection.Add(item);
                OnPropertyChanged(nameof(Count));
                OnPropertyChanged("Item[]");
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
            });
        public override int Add(object value)
            => Handle(() =>
            {
                int Result = BaseList.Add(value);
                if (Result > -1)
                {
                    OnPropertyChanged(nameof(Count));
                    OnPropertyChanged("Item[]");
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, value, Result));
                }
                return Result;
            });

        public override bool Remove(T item)
            => Handle(() =>
            {
                if (BaseCollection.Remove(item))
                {
                    OnPropertyChanged(nameof(Count));
                    OnPropertyChanged("Item[]");
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
                    return true;
                }
                return false;
            });
        public override void Remove(object value)
            => Handle(() =>
            {
                int Index = BaseList.IndexOf(value);
                if (Index > -1)
                {
                    T RemovedItem = BaseCollection[Index];
                    if (BaseCollection.Remove(RemovedItem))
                    {
                        OnPropertyChanged(nameof(Count));
                        OnPropertyChanged("Item[]");
                        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, RemovedItem, Index));
                    }
                }
            });
        public override void RemoveAt(int index)
            => Handle(() =>
            {
                T RemovedItem = BaseCollection[index];
                if (BaseCollection.Remove(RemovedItem))
                {
                    OnPropertyChanged(nameof(Count));
                    OnPropertyChanged("Item[]");
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, RemovedItem, index));
                }
            });

        public override void Insert(int index, T item)
            => Handle(() =>
            {
                BaseCollection.Insert(index, item);
                OnPropertyChanged(nameof(Count));
                OnPropertyChanged("Item[]");
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
            });
        public override void Insert(int index, object value)
            => Handle(() =>
            {
                BaseList.Insert(index, value);
                OnPropertyChanged(nameof(Count));
                OnPropertyChanged("Item[]");
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, value, index));
            });

        public override void Clear()
            => Handle(() =>
            {
                BaseCollection.Clear();
                OnPropertyChanged(nameof(Count));
                OnPropertyChanged("Item[]");
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            });

        public void ForEach(Action<T> Action)
            => Handle(() =>
            {
                foreach (T item in BaseCollection)
                    Action(item);
            });

        private void OnPropertyChanged([CallerMemberName] string PropertyName = null)
        {
            if (SynchronizationContext.Current == OriginalSynchronizationContext)
            {
                RaisePropertyChanged(PropertyName);
                return;
            }

            OriginalSynchronizationContext.Post((s) => RaisePropertyChanged(PropertyName), null);
        }
        internal protected void RaisePropertyChanged([CallerMemberName] string PropertyName = null)
            => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));

        private void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (SynchronizationContext.Current == OriginalSynchronizationContext)
            {
                RaiseCollectionChanged(e);
                return;
            }

            OriginalSynchronizationContext.Post((s) => RaiseCollectionChanged(e), null);
        }
        internal protected void RaiseCollectionChanged(NotifyCollectionChangedEventArgs e)
            => this.CollectionChanged?.Invoke(this, e);

    }
}
