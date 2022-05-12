using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace MenthaAssembly
{
    [ComVisible(false)]
    [DebuggerDisplay("Count = {Count}")]
    public class ObservableDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable, IDictionary, ICollection, IReadOnlyDictionary<TKey, TValue>, IReadOnlyCollection<KeyValuePair<TKey, TValue>>, ISerializable, IDeserializationCallback, INotifyCollectionChanged, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        private const string CountName = nameof(Count);

        protected readonly Dictionary<TKey, TValue> Base;

        public ICollection<TKey> Keys
            => Base.Keys;
        ICollection IDictionary.Keys
            => Base.Keys;
        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys
            => Base.Keys;

        public ICollection<TValue> Values
            => Base.Values;
        ICollection IDictionary.Values
            => Base.Values;
        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values
            => Base.Values;

        public int Count
            => Base.Count;

        public TValue this[TKey key]
        {
            get => Base[key];
            set
            {
                if (Base.ContainsKey(key))
                {
                    Base[key] = value;
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, new KeyValuePair<TKey, TValue>(key, value)));
                    return;
                }

                Add(key, value);
            }
        }
        object IDictionary.this[object key]
        {
            get => ((IDictionary)Base)[key];
            set
            {
                if (((IDictionary)Base).Contains(key))
                {
                    ((IDictionary)Base)[key] = value;
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, key));
                    return;
                }

                ((IDictionary)this).Add(key, value);
            }
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => ((ICollection<KeyValuePair<TKey, TValue>>)Base).IsReadOnly;
        bool IDictionary.IsReadOnly => ((IDictionary)Base).IsReadOnly;

        bool IDictionary.IsFixedSize => ((IDictionary)Base).IsFixedSize;

        object ICollection.SyncRoot => ((ICollection)Base).SyncRoot;

        bool ICollection.IsSynchronized => ((ICollection)Base).IsSynchronized;

        public ObservableDictionary()
        {
            Base = new Dictionary<TKey, TValue>();
        }
        public ObservableDictionary(int Capacity)
        {
            Base = new Dictionary<TKey, TValue>(Capacity);
        }
        public ObservableDictionary(IEqualityComparer<TKey> Comparer)
        {
            Base = new Dictionary<TKey, TValue>(Comparer);
        }
        public ObservableDictionary(IDictionary<TKey, TValue> Dictionary)
        {
            Base = new Dictionary<TKey, TValue>(Dictionary);
        }
        public ObservableDictionary(int Capacity, IEqualityComparer<TKey> Comparer)
        {
            Base = new Dictionary<TKey, TValue>(Capacity, Comparer);
        }
        public ObservableDictionary(IDictionary<TKey, TValue> Dictionary, IEqualityComparer<TKey> Comparer)
        {
            Base = new Dictionary<TKey, TValue>(Dictionary, Comparer);
        }

        public void Add(TKey key, TValue value)
        {
            Base.Add(key, value);
            OnPropertyChanged(CountName);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new KeyValuePair<TKey, TValue>(key, value)));
        }
        void IDictionary.Add(object key, object value)
            => Add((TKey)key, (TValue)value);
        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
        {
            Base.Add(item.Key, item.Value);
            OnPropertyChanged(CountName);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
        }

        public bool Remove(TKey key)
        {
            if (TryGetValue(key, out TValue value))
            {
                Base.Remove(key);
                OnPropertyChanged(CountName);
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, new KeyValuePair<TKey, TValue>(key, value)));
                return true;
            }

            return false;
        }
        void IDictionary.Remove(object key)
            => Remove((TKey)key);
        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        {
            bool Result = Base.Remove(item.Key);
            OnPropertyChanged(CountName);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
            return Result;
        }

        public void Clear()
        {
            Base.Clear();
            OnPropertyChanged(CountName);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
            => Base.Contains(item);
        bool IDictionary.Contains(object key)
            => ((IDictionary)Base).Contains(key);
        public bool ContainsKey(TKey key)
            => Base.ContainsKey(key);

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
            => ((ICollection<KeyValuePair<TKey, TValue>>)Base).CopyTo(array, arrayIndex);
        void ICollection.CopyTo(Array array, int index)
            => ((ICollection)Base).CopyTo(array, index);

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
            => Base.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator()
            => Base.GetEnumerator();
        IDictionaryEnumerator IDictionary.GetEnumerator()
            => Base.GetEnumerator();

        public bool TryGetValue(TKey key, out TValue value)
            => Base.TryGetValue(key, out value);

        public void GetObjectData(SerializationInfo info, StreamingContext context)
            => ((ISerializable)Base).GetObjectData(info, context);
        public void OnDeserialization(object sender)
            => ((IDeserializationCallback)Base).OnDeserialization(sender);

        protected void OnPropertyChanged([CallerMemberName] string PropertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));

        protected void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
            => CollectionChanged?.Invoke(this, e);

    }
}
