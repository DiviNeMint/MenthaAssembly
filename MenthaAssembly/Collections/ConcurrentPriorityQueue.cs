using MenthaAssembly;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;

namespace System.Collections.Generic
{
    /// <summary>
    /// Represents a thread-safe collection that allows elements to be enqueued with a priority and dequeued in priority order.
    /// </summary>
    /// <typeparam name="TElement">The type of elements in the queue.</typeparam>
    /// <typeparam name="TPriority">The type of priority associated with each element.</typeparam>
    public sealed class ConcurrentPriorityQueue<TElement, TPriority>
    {
        private readonly List<(TPriority, ConcurrentQueue<TElement>)> Collection;

        /// <summary>
        ///  Gets the number of elements contained in the <see cref="ConcurrentPriorityQueue{TElement, TPriority}"/>.
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// Gets a value that indicates whether the <see cref="ConcurrentPriorityQueue{T}"/> is empty.
        /// </summary>
        public bool IsEmpty
            => Count == 0;

        /// <summary>
        ///  Gets the priority comparer used by the <see cref="ConcurrentPriorityQueue{TElement, TPriority}"/>.
        /// </summary>
        public IComparer<TPriority> Comparer { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentPriorityQueue{TElement, TPriority}"/> class.
        /// </summary>
        public ConcurrentPriorityQueue()
        {
            Collection = [];
            Comparer = Comparer<TPriority>.Default;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentPriorityQueue{TElement, TPriority}"/> class with the specified comparer.
        /// </summary>
        /// <param name="comparer">The comparer used to compare the priorities of the elements.</param>
        public ConcurrentPriorityQueue(IComparer<TPriority> comparer)
        {
            Collection = [];
            Comparer = comparer ?? Comparer<TPriority>.Default;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentPriorityQueue{TElement, TPriority}"/> class with the specified items and comparer.
        /// </summary>
        /// <param name="items">The items to be added to the queue with their corresponding priorities.</param>
        /// <param name="comparer">The comparer used to compare the priorities of the elements.</param>
        public ConcurrentPriorityQueue(IEnumerable<(TElement, TPriority)> items, IComparer<TPriority> comparer)
        {
            Collection = [];
            Comparer = comparer ?? Comparer<TPriority>.Default;

            foreach ((TElement Element, TPriority Priority) in items)
                InternalEnqueue(Element, Priority);
        }

        /// <summary>
        /// Enqueues an element with the specified priority.
        /// </summary>
        /// <param name="item">The element to enqueue.</param>
        /// <param name="priority">The priority associated with the element.</param>
        public void Enqueue(TElement item, TPriority priority)
            => Handle(() => InternalEnqueue(item, priority));
        private void InternalEnqueue(TElement item, TPriority priority)
        {
            ConcurrentQueue<TElement> queue = InternalGetOrAddQueue(priority);
            queue.Enqueue(item);
            Count++;
        }

        /// <summary>
        /// Tries to dequeue an element from the queue.
        /// </summary>
        /// <param name="result">When this method returns, contains the dequeued element, if the operation succeeded; otherwise, the default value for the element type.</param>
        /// <returns><c>true</c> if an element was successfully dequeued; otherwise, <c>false</c>.</returns>
        public bool TryDequeue(out TElement result)
            => Handle(InternalTryDequeue, out result);
        private bool InternalTryDequeue(out TElement result)
        {
            if (IsEmpty)
            {
                result = default;
                return false;
            }

            (TPriority priority, ConcurrentQueue<TElement> queue) = Collection[0];
            if (!queue.TryDequeue(out result))
                return false;

            // Checks if the queue is empty
            if (queue.IsEmpty)
                Collection.RemoveAt(0);

            Count--;
            return true;
        }

        /// <summary>
        /// Tries to peek at the element with the highest priority without removing it from the queue.
        /// </summary>
        /// <param name="result">When this method returns, contains the element with the highest priority, if the operation succeeded; otherwise, the default value for the element type.</param>
        /// <returns><c>true</c> if an element was successfully peeked; otherwise, <c>false</c>.</returns>
        public bool TryPeek(out TElement result)
            => Handle(InternalTryPeek, out result);
        private bool InternalTryPeek(out TElement result)
        {
            if (IsEmpty)
            {
                result = default;
                return false;
            }

            (TPriority priority, ConcurrentQueue<TElement> queue) = Collection[0];
            return queue.TryPeek(out result);
        }

        /// <summary>
        /// Removes all elements from the queue.
        /// </summary>
        public void Clear()
            => Handle(() =>
            {
                Collection.Clear();
                Count = 0;
            });

        /// <summary>
        /// Copies the elements stored in the<see cref="ConcurrentPriorityQueue{TElement, TPriority}"/> to a new array.
        /// </summary>
        /// <returns>An array containing all the elements in the queue.</returns>
        public TElement[] ToArray()
            => Handle(() => Collection.SelectMany(i => i.Item2).ToArray());

        /// <summary>
        /// Copies the elements stored in the<see cref="ConcurrentPriorityQueue{TElement, TPriority}"/> to a new list.
        /// </summary>
        /// <returns>A list containing all the elements in the queue.</returns>
        public List<TElement> ToList()
            => Handle(() => Collection.SelectMany(i => i.Item2).ToList());

        private ConcurrentQueue<TElement> InternalGetOrAddQueue(TPriority priority)
        {
            int index = 0;
            int count = Collection.Count;
            for (; index < count; index++)
            {
                (TPriority p, ConcurrentQueue<TElement> q) = Collection[index];

                int compare = Comparer.Compare(p, priority);
                if (compare == 0)
                    return q;

                if (compare > 0)
                    break;
            }

            ConcurrentQueue<TElement> newQueue = new();
            Collection.Insert(index, (priority, newQueue));
            return newQueue;
        }

        private readonly object LockObj = new();
        internal U Handle<U>(Func<U> func)
        {
            bool token = false;
            try
            {
                Monitor.Enter(LockObj, ref token);
                return func();
            }
            finally
            {
                if (token)
                    Monitor.Exit(LockObj);
            }
        }
        internal U Handle<T, U>(OFunc<T, U> func, out T p)
        {
            bool token = false;
            try
            {
                Monitor.Enter(LockObj, ref token);
                return func(out p);
            }
            finally
            {
                if (token)
                    Monitor.Exit(LockObj);
            }
        }
        internal void Handle(Action action)
        {
            bool token = false;
            try
            {
                Monitor.Enter(LockObj, ref token);
                action();
            }
            finally
            {
                if (token)
                    Monitor.Exit(LockObj);
            }
        }

    }
}