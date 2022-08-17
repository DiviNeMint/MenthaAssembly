﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace MenthaAssembly
{
    public class ConcurrentObservableCollection<T> : ConcurrentCollection<T>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        private readonly SynchronizationContext OriginalSynchronizationContext = SynchronizationContext.Current;
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

                OnPropertyChanged(IndexerName);
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, originalItem, Value, Index));
            });

        public override void Add(T item)
            => Handle(() =>
            {
                Items.Add(item);
                OnPropertyChanged(CountName);
                OnPropertyChanged(IndexerName);
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
            });
        public override void AddRange(IEnumerable<T> Items)
            => Handle(() =>
            {
                this.Items.AddRange(Items);
                OnPropertyChanged(CountName);
                OnPropertyChanged(IndexerName);
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            });

        public override bool Remove(T item)
            => Handle(() =>
            {
                if (Items.Remove(item))
                {
                    OnPropertyChanged(CountName);
                    OnPropertyChanged(IndexerName);
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
                    return true;
                }
                return false;
            });
        public override void Remove(IEnumerable<T> Items)
            => Handle(() =>
            {
                if (Items is not T[] &&
                    Items is not IList &&
                    Items is not ICollection)
                    Items = Items.ToArray();

                foreach (T item in Items)
                {
                    if (this.Items.Remove(item))
                        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
                }

                OnPropertyChanged(CountName);
                OnPropertyChanged(IndexerName);
            });
        public override void RemoveAt(int index)
            => Handle(() =>
            {
                T RemovedItem = Items[index];
                if (Items.Remove(RemovedItem))
                {
                    OnPropertyChanged(CountName);
                    OnPropertyChanged(IndexerName);
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, RemovedItem, index));
                }
            });

        public override void Insert(int index, T item)
            => Handle(() =>
            {
                Items.Insert(index, item);
                OnPropertyChanged(CountName);
                OnPropertyChanged(IndexerName);
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
            });

        public override void Clear()
            => Handle(() =>
            {
                Items.Clear();
                OnPropertyChanged(CountName);
                OnPropertyChanged(IndexerName);
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            });

        public void ForEach(Action<T> Action)
            => Handle(() =>
            {
                foreach (T item in Items)
                    Action(item);
            });

        private void OnPropertyChanged([CallerMemberName] string PropertyName = null)
        {
            if (SynchronizationContext.Current == OriginalSynchronizationContext)
                RaisePropertyChanged(PropertyName);
            else
                OriginalSynchronizationContext.Post((s) => RaisePropertyChanged(PropertyName), null);
        }
        protected internal void RaisePropertyChanged([CallerMemberName] string PropertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));

        private void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (SynchronizationContext.Current == OriginalSynchronizationContext)
                RaiseCollectionChanged(e);
            else
                OriginalSynchronizationContext.Post((s) => RaiseCollectionChanged(e), null);
        }
        protected internal void RaiseCollectionChanged(NotifyCollectionChangedEventArgs e)
            => CollectionChanged?.Invoke(this, e);

    }
}
