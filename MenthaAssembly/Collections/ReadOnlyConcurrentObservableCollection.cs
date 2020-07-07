using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace MenthaAssembly
{
    public class ReadOnlyConcurrentObservableCollection<T> : ReadOnlyCollection<T>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        public ReadOnlyConcurrentObservableCollection(ConcurrentObservableCollection<T> list) : base(list)
        {
            list.CollectionChanged += (s, e) => this.CollectionChanged?.Invoke(this, e);
            list.PropertyChanged += (s, e) => this.PropertyChanged?.Invoke(this, e);
        }

    }
}
