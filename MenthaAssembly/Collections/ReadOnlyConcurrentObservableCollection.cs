using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace System.Collections.Generic
{
    public class ReadOnlyConcurrentObservableCollection<T> : ReadOnlyCollection<T>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        public ReadOnlyConcurrentObservableCollection(ConcurrentObservableCollection<T> List) : base(List)
        {
            List.CollectionChanged += (s, e) => CollectionChanged?.Invoke(this, e);
            List.PropertyChanged += (s, e) => PropertyChanged?.Invoke(this, e);
        }

    }
}