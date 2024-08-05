using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace System.Collections.Generic
{
    /// <summary>
    /// Read-only wrapper around an ObservableCollection.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    [Serializable]
    public class ReadOnlyObservableCollection<T> : ReadOnlyCollection<T>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadOnlyObservableCollection{T}"/> class that serves as a wrapper around the specified <see cref="IList{T}"/> which must inherit from <see cref="INotifyCollectionChanged"/> and <see cref="INotifyPropertyChanged"/>
        /// </summary>
        /// <param name="Collection">The <see cref="IList{T}"/> with which to create this instance of the <see cref="ReadOnlyObservableCollection{T}"/> class.</param>
        public ReadOnlyObservableCollection(IList<T> Collection) : base(Collection)
        {
            ((INotifyCollectionChanged)Collection).CollectionChanged += (s, e) => OnCollectionChanged(e);
            ((INotifyPropertyChanged)Collection).PropertyChanged += (s, e) => OnPropertyChanged(e);
        }

        /// <summary>
        /// raise CollectionChanged event to any listeners
        /// </summary>
        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
            => CollectionChanged?.Invoke(this, e);

        /// <summary>
        /// raise PropertyChanged event to any listeners
        /// </summary>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
            => PropertyChanged?.Invoke(this, e);

    }

    /// <summary>
    /// Read-only wrapper around an ObservableCollection.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    [Serializable]
    public class ReadOnlyObservableCollection<U, T> : ReadOnlyCollection<U, T>, INotifyCollectionChanged, INotifyPropertyChanged
        where U : T
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadOnlyObservableCollection{U, T}"/> class that serves as a wrapper around the specified <see cref="IList{U}"/> which must inherit from <see cref="INotifyCollectionChanged"/> and <see cref="INotifyPropertyChanged"/>
        /// </summary>
        /// <param name="Collection">The <see cref="IList{U}"/> with which to create this instance of the <see cref="ReadOnlyObservableCollection{U, T}"/> class.</param>
        public ReadOnlyObservableCollection(IList<U> Collection) : base(Collection)
        {
            ((INotifyCollectionChanged)Collection).CollectionChanged += (s, e) => OnCollectionChanged(e);
            ((INotifyPropertyChanged)Collection).PropertyChanged += (s, e) => OnPropertyChanged(e);
        }

        /// <summary>
        /// raise CollectionChanged event to any listeners
        /// </summary>
        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
            => CollectionChanged?.Invoke(this, e);

        /// <summary>
        /// raise PropertyChanged event to any listeners
        /// </summary>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
            => PropertyChanged?.Invoke(this, e);

    }

}