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

        private int BlockReentrancyCount;

        public event PropertyChangedEventHandler PropertyChanged;
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public ConcurrentObservableCollection()
            : base()
        {
            RegisterWpfCollectionSynchronization();
        }
        public ConcurrentObservableCollection(IEnumerable<T> Items)
            : base(Items)
        {
            RegisterWpfCollectionSynchronization();
        }
        private void RegisterWpfCollectionSynchronization()
        {
            AssemblyName WPFCore = Assembly.GetEntryAssembly()?
                                           .GetReferencedAssemblies()
                                           .Where(AssemblyHelper.IsDotNetAssembly)
                                           .FirstOrDefault(i => i.Name == "PresentationFramework");
            if (WPFCore is null)
                return;

            Assembly WPFAssembly = Assembly.Load(WPFCore);
            if (WPFAssembly.GetType("System.Windows.Data.BindingOperations", false) is Type BindingOperationsType &&
                BindingOperationsType.TryGetStaticMethod("EnableCollectionSynchronization", [typeof(IEnumerable), typeof(object)], out MethodInfo EnableMethod))
                ReflectionHelper.InvokeOnUIThread(() =>
                {
                    try
                    {
                        EnableMethod.Invoke(null, [this, SyncRoot]);
                    }
                    catch (TargetInvocationException ex)
                    {
                        throw ex.InnerException ?? ex;
                    }
                });
        }

        protected override void SetItem(int Index, T Value)
            => Handle(() =>
            {
                CheckReentrancy();

                T OriginalItem = Items[Index];
                if (EqualityComparer<T>.Default.Equals(OriginalItem, Value))
                    return;

                Items[Index] = Value;

                OnPropertyChanged(IndexerName);
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Replace,
                    Value,
                    OriginalItem,
                    Index));
            });

        public override void Add(T Item)
            => Handle(() =>
            {
                CheckReentrancy();

                int Index = Items.Count;
                Items.Add(Item);

                OnPropertyChanged(CountName);
                OnPropertyChanged(IndexerName);
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Add,
                    Item,
                    Index));
            });

        public override bool Remove(T Item)
            => Handle(() =>
            {
                CheckReentrancy();

                int Index = Items.IndexOf(Item);
                if (Index < 0)
                    return false;

                T RemovedItem = Items[Index];
                Items.RemoveAt(Index);

                OnPropertyChanged(CountName);
                OnPropertyChanged(IndexerName);
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Remove,
                    RemovedItem,
                    Index));
                return true;
            });

        public override void RemoveAt(int Index)
            => Handle(() =>
            {
                CheckReentrancy();

                T RemovedItem = Items[Index];
                Items.RemoveAt(Index);

                OnPropertyChanged(CountName);
                OnPropertyChanged(IndexerName);
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Remove,
                    RemovedItem,
                    Index));
            });

        public override void Insert(int Index, T Item)
            => Handle(() =>
            {
                CheckReentrancy();

                Items.Insert(Index, Item);

                OnPropertyChanged(CountName);
                OnPropertyChanged(IndexerName);
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Add,
                    Item,
                    Index));
            });


        public override void AddRange(IEnumerable<T> Items)
            => Handle(() =>
            {
                if (Items is null)
                    throw new ArgumentNullException(nameof(Items));

                List<T> Buffer = Items as List<T> ?? new List<T>(Items);
                if (Buffer.Count == 0)
                    return;

                CheckReentrancy();

                this.Items.AddRange(Buffer);

                OnPropertyChanged(CountName);
                OnPropertyChanged(IndexerName);
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            });

        public override void Remove(Predicate<T> Predict)
            => Handle(() =>
            {
                if (Predict is null)
                    throw new ArgumentNullException(nameof(Predict));

                CheckReentrancy();

                bool Changed = false;

                for (int i = Items.Count - 1; i >= 0; i--)
                {
                    if (!Predict(Items[i]))
                        continue;

                    Items.RemoveAt(i);
                    Changed = true;
                }

                if (!Changed)
                    return;

                OnPropertyChanged(CountName);
                OnPropertyChanged(IndexerName);
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            });

        public override void Remove(IEnumerable<T> Items)
            => Handle(() =>
            {
                if (Items is null)
                    throw new ArgumentNullException(nameof(Items));

                CheckReentrancy();

                bool Changed = false;
                IEnumerable<T> Buffer = Items is ICollection<T> ? Items : new List<T>(Items);

                foreach (T Item in Buffer)
                {
                    int Index = this.Items.IndexOf(Item);
                    if (Index < 0)
                        continue;

                    this.Items.RemoveAt(Index);
                    Changed = true;
                }

                if (!Changed)
                    return;

                OnPropertyChanged(CountName);
                OnPropertyChanged(IndexerName);
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            });

        public override void Clear()
            => Handle(() =>
            {
                if (Items.Count == 0)
                    return;

                CheckReentrancy();

                Items.Clear();

                OnPropertyChanged(CountName);
                OnPropertyChanged(IndexerName);
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            });


        protected virtual void OnPropertyChanged([CallerMemberName] string PropertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            BlockReentrancyCount++;
            try
            {
                CollectionChanged?.Invoke(this, e);
            }
            finally
            {
                BlockReentrancyCount--;
            }
        }

        protected void CheckReentrancy()
        {
            if (BlockReentrancyCount > 0 &&
                CollectionChanged != null &&
                CollectionChanged.GetInvocationList().Length > 1)
                throw new InvalidOperationException("Cannot modify the collection during a CollectionChanged event.");
        }

    }
}