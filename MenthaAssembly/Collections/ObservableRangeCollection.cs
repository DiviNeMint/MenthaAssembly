using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace System.Collections.Generic
{
    public class ObservableRangeCollection<T> : ObservableCollection<T>
    {
        private const string CountString = "Count";
        private const string IndexerName = "Item[]";

        public ObservableRangeCollection() : base()
        {
        }
        public ObservableRangeCollection(IEnumerable<T> Collection) : base(Collection)
        {
        }
        public ObservableRangeCollection(List<T> List) : base(List)
        {
        }

        public virtual void AddRange(IEnumerable<T> Items)
        {
            CheckReentrancy();

            foreach (T Item in Items)
                base.Items.Add(Item);

            OnPropertyChanged(CountString);
            OnPropertyChanged(IndexerName);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public virtual void RemoveRange(IEnumerable<T> Items)
        {
            CheckReentrancy();

            if (Items is not T[] and
                not IList and
                not ICollection)
                Items = Items.ToArray();

            foreach (T Item in Items)
                base.Items.Remove(Item);

            OnPropertyChanged(CountString);
            OnPropertyChanged(IndexerName);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        protected void OnPropertyChanged(string PropertyName)
            => base.OnPropertyChanged(new PropertyChangedEventArgs(PropertyName));

    }
}