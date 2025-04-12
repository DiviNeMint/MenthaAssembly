using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace System.Collections.Generic
{
    [Serializable]
    public class ConcurrentObservableCollection<T> : ConcurrentCollection<T>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        private const string CountName = nameof(Count);
        private const string IndexerName = "Item[]";

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
                T originalItem = Items[Index];
                Items[Index] = Value;

                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, originalItem, Value, Index));
                OnPropertyChanged(IndexerName);
            });

        public override void Add(T item)
            => Handle(() =>
            {
                Items.Add(item);
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
                OnPropertyChanged(CountName);
                OnPropertyChanged(IndexerName);
            });
        public override void AddRange(IEnumerable<T> Items)
            => Handle(() =>
            {
                this.Items.AddRange(Items);
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                OnPropertyChanged(CountName);
                OnPropertyChanged(IndexerName);
            });

        public override bool Remove(T item)
            => Handle(() =>
            {
                if (!Items.Remove(item))
                    return false;

                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
                OnPropertyChanged(CountName);
                OnPropertyChanged(IndexerName);
                return true;
            });
        public override void Remove(Predicate<T> Predict)
            => Handle(() =>
            {
                for (int i = Items.Count - 1; i >= 0; i--)
                {
                    T RemovedItem = Items[i];
                    if (Predict(RemovedItem))
                    {
                        Items.RemoveAt(i);
                        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, RemovedItem, i));
                    }
                }

                OnPropertyChanged(CountName);
                OnPropertyChanged(IndexerName);
            });
        public override void Remove(IEnumerable<T> Items)
            => Handle(() =>
            {
                if (Items is not T[] and not IList and not ICollection)
                    Items = Items.ToArray();

                foreach (T item in Items)
                    if (this.Items.Remove(item))
                        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));

                OnPropertyChanged(CountName);
                OnPropertyChanged(IndexerName);
            });
        public override void RemoveAt(int index)
            => Handle(() =>
            {
                T RemovedItem = Items[index];
                if (Items.Remove(RemovedItem))
                {
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, RemovedItem, index));
                    OnPropertyChanged(CountName);
                    OnPropertyChanged(IndexerName);
                }
            });

        public override bool TryRemove(Predicate<T> Predict, out T Item)
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
                            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, Temp, i));

                            OnPropertyChanged(CountName);
                            OnPropertyChanged(IndexerName);
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

        public override void Insert(int index, T item)
            => Handle(() =>
            {
                Items.Insert(index, item);
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
                OnPropertyChanged(CountName);
                OnPropertyChanged(IndexerName);
            });

        public override void Clear()
            => Handle(() =>
            {
                Items.Clear();
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                OnPropertyChanged(CountName);
                OnPropertyChanged(IndexerName);
            });

        protected internal override void Handle(Action Action)
            => ReflectionHelper.Invoke(Action);
        protected internal override U Handle<U>(Func<U> Func)
            => ReflectionHelper.Invoke(Func);

        protected void OnPropertyChanged([CallerMemberName] string PropertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        protected void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
            => CollectionChanged?.Invoke(this, e);

    }

    //[Serializable]
    //public class ConcurrentObservableCollection<T> : ConcurrentCollection<T>, INotifyCollectionChanged, INotifyPropertyChanged
    //{
    //    private readonly SynchronizationContext OriginalSynchronizationContext = ReflectionHelper.Invoke(() => SynchronizationContext.Current);
    //    private const string CountName = nameof(Count);
    //    private const string IndexerName = "Item[]";

    //    public event PropertyChangedEventHandler PropertyChanged;
    //    public event NotifyCollectionChangedEventHandler CollectionChanged;

    //    public ConcurrentObservableCollection() : base()
    //    {
    //    }
    //    public ConcurrentObservableCollection(IEnumerable<T> Items) : base(Items)
    //    {
    //    }

    //    protected override void SetItem(int Index, T Value)
    //        => Handle(() =>
    //        {
    //            T originalItem = Items[Index];
    //            Items[Index] = Value;

    //            RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, originalItem, Value, Index));
    //            RaisePropertyChanged(IndexerName);
    //        });

    //    public override void Add(T item)
    //        => Handle(() =>
    //        {
    //            Items.Add(item);
    //            RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
    //            RaisePropertyChanged(CountName);
    //            RaisePropertyChanged(IndexerName);
    //        });
    //    public override void AddRange(IEnumerable<T> Items)
    //        => Handle(() =>
    //        {
    //            this.Items.AddRange(Items);
    //            RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    //            RaisePropertyChanged(CountName);
    //            RaisePropertyChanged(IndexerName);
    //        });

    //    public override bool Remove(T item)
    //        => Handle(() =>
    //        {
    //            if (!Items.Remove(item))
    //                return false;

    //            RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
    //            RaisePropertyChanged(CountName);
    //            RaisePropertyChanged(IndexerName);
    //            return true;
    //        });
    //    public override void Remove(Predicate<T> Predict)
    //        => Handle(() =>
    //        {
    //            for (int i = Items.Count - 1; i >= 0; i--)
    //            {
    //                T RemovedItem = Items[i];
    //                if (Predict(RemovedItem))
    //                {
    //                    Items.RemoveAt(i);
    //                    RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, RemovedItem, i));
    //                }
    //            }

    //            RaisePropertyChanged(CountName);
    //            RaisePropertyChanged(IndexerName);
    //        });
    //    public override void Remove(IEnumerable<T> Items)
    //        => Handle(() =>
    //        {
    //            if (Items is not T[] and not IList and not ICollection)
    //                Items = Items.ToArray();

    //            foreach (T item in Items)
    //                if (this.Items.Remove(item))
    //                    RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));

    //            RaisePropertyChanged(CountName);
    //            RaisePropertyChanged(IndexerName);
    //        });
    //    public override void RemoveAt(int index)
    //        => Handle(() =>
    //        {
    //            T RemovedItem = Items[index];
    //            if (Items.Remove(RemovedItem))
    //            {
    //                RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, RemovedItem, index));
    //                RaisePropertyChanged(CountName);
    //                RaisePropertyChanged(IndexerName);
    //            }
    //        });

    //    public override bool TryRemove(Predicate<T> Predict, out T Item)
    //    {
    //        bool InternalTryRemove(out T Item)
    //        {
    //            for (int i = Items.Count - 1; i >= 0; i--)
    //            {
    //                Item = Items[i];
    //                if (Predict(Item))
    //                {
    //                    Items.RemoveAt(i);
    //                    RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, Item, i));

    //                    RaisePropertyChanged(CountName);
    //                    RaisePropertyChanged(IndexerName);
    //                    return true;
    //                }
    //            }

    //            Item = default;
    //            return false;
    //        }

    //        return Handle(InternalTryRemove, out Item);
    //    }

    //    public override void Insert(int index, T item)
    //        => Handle(() =>
    //        {
    //            Items.Insert(index, item);
    //            RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
    //            RaisePropertyChanged(CountName);
    //            RaisePropertyChanged(IndexerName);
    //        });

    //    public override void Clear()
    //        => Handle(() =>
    //        {
    //            Items.Clear();
    //            RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    //            RaisePropertyChanged(CountName);
    //            RaisePropertyChanged(IndexerName);
    //        });

    //    protected internal void RaisePropertyChanged([CallerMemberName] string PropertyName = null)
    //    {
    //        if (PropertyChanged is null)
    //            return;

    //        if (SynchronizationContext.Current == OriginalSynchronizationContext)
    //            OnPropertyChanged(PropertyName);
    //        else
    //            OriginalSynchronizationContext.Send((s) => OnPropertyChanged(PropertyName), null);
    //    }
    //    private void OnPropertyChanged(string PropertyName)
    //        => PropertyChanged.Invoke(this, new PropertyChangedEventArgs(PropertyName));

    //    protected internal void RaiseCollectionChanged(NotifyCollectionChangedEventArgs e)
    //    {
    //        if (CollectionChanged is null)
    //            return;

    //        if (SynchronizationContext.Current == OriginalSynchronizationContext)
    //            OnCollectionChanged(e);
    //        else
    //            OriginalSynchronizationContext.Send((s) => OnCollectionChanged(e), null);
    //    }
    //    private void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    //        => CollectionChanged.Invoke(this, e);

    //}
}