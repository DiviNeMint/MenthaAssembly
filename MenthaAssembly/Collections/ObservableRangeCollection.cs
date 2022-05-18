using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace MenthaAssembly
{
    public class ObservableRangeCollection<T> : ObservableCollection<T>
    {
        private const string CountString = "Count";
        private const string IndexerName = "Item[]";

        public virtual void AddRange(IEnumerable<T> Items)
        {
            CheckReentrancy();

            foreach (T Item in Items)
                base.Items.Add(Item);

            OnPropertyChanged(CountString);
            OnPropertyChanged(IndexerName);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, Items is IList ListItems ? ListItems : Items.ToList()));
        }

        protected void OnPropertyChanged(string PropertyName)
            => base.OnPropertyChanged(new PropertyChangedEventArgs(PropertyName));

    }

}
