using System.Collections.ObjectModel;
using System.Linq;

namespace System.Collections.Generic
{
    /// <summary>
    /// Provides the base class for a generic read-only collection.
    /// </summary>
    /// <typeparam name="U">The type of element in collection.</typeparam>
    /// <typeparam name="T">The inherited type of element.</typeparam>
    [Serializable]
    public class ReadOnlyCollection<U, T> : IList<T>, IList, IReadOnlyList<T>
        where U : T
    {
        private readonly ReadOnlyCollection<U> Items;

        /// <summary>
        /// Gets the element at the specified index.
        /// </summary>
        /// <param name="Index">The zero-based index of the element to get.</param>
        /// <returns>The element at the specified index.</returns>
        public U this[int Index]
            => Items[Index];

        public int Count
            => Items.Count;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadOnlyCollection{U, T}"/> class that is a read-only wrapper around the specified list.
        /// </summary>
        /// <param name="Items">The list to wrap.</param>
        public ReadOnlyCollection(IList<U> Items)
        {
            this.Items = new(Items);
        }

        /// <summary>
        /// Searches for the specified object and returns the zero-based index of the first occurrence within the entire <see cref="ReadOnlyCollection{U, T}"/>.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="IList{U}"/>.
        /// The value can be null for reference types.</param>
        /// <returns>The zero-based index of the first occurrence of item within the entire <see cref="ReadOnlyCollection{U, T}"/>, if found; otherwise, -1.</returns>
        public int IndexOf(U item)
            => Items.IndexOf(item);

        /// <summary>
        /// Determines whether an element is in the <see cref="ReadOnlyCollection{U, T}"/>.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="ReadOnlyCollection{U, T}"/>.
        /// The value can be null for reference types.</param>
        /// <returns>true if value is found in the <see cref="ReadOnlyCollection{U, T}"/>; otherwise, false.</returns>
        public bool Contains(U item)
            => Items.Contains(item);

        /// <summary>
        /// Copies the entire <see cref="ReadOnlyCollection{U, T}"/> to a compatible one-dimensional <see cref="Array"/>, starting at the specified index of the target array.
        /// </summary>
        /// <param name="Array">The one-dimensional <see cref="Array"/> that is the destination of the elements copied from <see cref="ReadOnlyCollection{U, T}"/>. 
        /// The <see cref="Array"/> must have zero-based indexing.</param>
        /// <param name="Index">The zero-based index in array at which copying begins.</param>
        public void CopyTo(U[] Array, int Index)
            => Items.CopyTo(Array, Index);

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="ReadOnlyCollection{U, T}"/>.
        /// </summary>
        /// <returns>An <see cref="IEnumerator{U}"/> for the <see cref="ReadOnlyCollection{U, T}"/>.</returns>
        public IEnumerator<U> GetEnumerator()
            => Items.GetEnumerator();
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
            => Items.OfType<T>().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator()
            => Items.GetEnumerator();

        public override string ToString()
            => Items.ToString();

        #region IList
        object IList.this[int index]
        {
            get => Items[index];
            set => ((IList)Items)[index] = value;
        }
        T IList<T>.this[int index]
        {
            get => Items[index];
            set => ((IList)Items)[index] = value;
        }

        bool IList.IsFixedSize
            => true;

        bool IList.IsReadOnly
            => true;

        int IList.Add(object value)
            => ((IList)Items).Add(value);

        void IList.Insert(int index, object value)
            => ((IList)Items).Insert(index, value);
        void IList<T>.Insert(int index, T item)
            => ((IList)Items).Insert(index, item);

        void IList.Remove(object value)
            => ((IList)Items).Remove(value);
        bool ICollection<T>.Remove(T item)
            => ((ICollection<U>)Items).Remove((U)item);

        void IList.RemoveAt(int index)
            => ((IList)Items).RemoveAt(index);
        void IList<T>.RemoveAt(int index)
            => ((IList)Items).RemoveAt(index);

        void IList.Clear()
            => ((IList)Items).Clear();

        int IList.IndexOf(object value)
            => ((IList)Items).IndexOf((U)value);
        int IList<T>.IndexOf(T item)
            => ((IList)Items).IndexOf(item);

        bool IList.Contains(object value)
            => value is U Item && Items.Contains(Item);

        T IReadOnlyList<T>.this[int index]
            => this[index];

        #endregion

        #region ICollection
        bool ICollection<T>.IsReadOnly
            => true;

        bool ICollection.IsSynchronized
            => false;

        object ICollection.SyncRoot
            => ((ICollection)Items).SyncRoot;

        public bool IsReadOnly => ((ICollection<U>)Items).IsReadOnly;

        void ICollection<T>.Add(T item)
            => ((ICollection<U>)Items).Add((U)item);

        void ICollection<T>.Clear()
            => ((ICollection<U>)Items).Clear();

        bool ICollection<T>.Contains(T value)
            => value is U Item && Items.Contains(Item);

        void ICollection<T>.CopyTo(T[] array, int arrayIndex)
            => ((ICollection)Items).CopyTo(array, arrayIndex);

        void ICollection.CopyTo(Array array, int index)
            => ((ICollection)Items).CopyTo(array, index);

        #endregion

    }
}