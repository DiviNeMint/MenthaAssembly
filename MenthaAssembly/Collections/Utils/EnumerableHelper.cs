using System.Collections;
using System.Collections.Generic;

namespace System.Linq
{
    public static class EnumerableHelper
    {
        /// <summary>
        /// Returns the first element of a sequence, or a null value if the sequence contains no elements.
        /// </summary>
        /// <param name="Source">The sequence to return the first element of.</param>
        public static object FirstOrNull(this IEnumerable Source)
        {
            IEnumerator Enumerator = Source.GetEnumerator();
            return Enumerator.MoveNext() ? Enumerator.Current : null;
        }
        /// <summary>
        /// Returns the first element of a sequence, or a null value if the sequence contains no elements.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <param name="Source">The sequence to return the first element of.</param>
        public static TSource? FirstOrNull<TSource>(this IEnumerable<TSource> Source)
            where TSource : struct
        {
            foreach (TSource Item in Source)
                return Item;

            return null;
        }
        /// <summary>
        /// Returns the first element of the sequence that satisfies a condition or a null value if no such element is found.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <param name="Source">The sequence to return the first element of.</param>
        /// <param name="Predicate">A function to test each element for a condition.</param>
        public static TSource? FirstOrNull<TSource>(this IEnumerable<TSource> Source, Func<TSource, bool> Predicate)
            where TSource : struct
        {
            foreach (TSource Item in Source)
                if (Predicate(Item))
                    return Item;

            return null;
        }

        /// <summary>
        /// Determines whether a sequence contains a specified element by using the default equality comparer.
        /// </summary>
        /// <param name="Source">A sequence in which to locate a value.</param>
        /// <param name="Item">The item to locate in the sequence.</param>
        public static bool Contains(this IEnumerable Source, object Item)
        {
            foreach (object i in Source)
                if (Item.Equals(i))
                    return true;

            return false;
        }
        /// <summary>
        /// Determines whether a sequence contains a specified element by using the default equality comparer.
        /// </summary>
        /// <typeparam name="T">The type of the elements of source.</typeparam>
        /// <param name="Source">A sequence in which to locate a value.</param>
        /// <param name="Item">The item to locate in the sequence.</param>
        public static bool Contains<T>(this IEnumerable Source, T Item)
        {
            foreach (T i in Source.OfType<T>())
                if (Item.Equals(i))
                    return true;

            return false;
        }

        /// <summary>
        /// Searches for the specified object and returns the zero-based index of the first occurrence within the sequence.
        /// </summary>
        /// <param name="Source">A sequence in which to locate a value.</param>
        /// <param name="Item">The item to locate in the sequence.</param>
        public static int IndexOf(this IEnumerable Source, object Item)
        {
            int Index = 0;
            foreach (object i in Source)
            {
                if (Item.Equals(i))
                    return Index;

                Index++;
            }

            return -1;
        }
        /// <summary>
        /// Searches for the specified object and returns the zero-based index of the first occurrence within the sequence.
        /// </summary>
        /// <param name="Source">A sequence in which to locate a value.</param>
        /// <param name="Predicate">A function to test each element for a condition.</param>
        public static int IndexOf(this IEnumerable Source, Func<object, bool> Predicate)
        {
            int Index = 0;
            foreach (object i in Source)
            {
                if (Predicate(i))
                    return Index;
                Index++;
            }

            return -1;
        }
        /// <summary>
        /// Searches for the specified object and returns the zero-based index of the first occurrence within the sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <param name="Source">A sequence in which to locate a value.</param>
        /// <param name="Predicate">A function to test each element for a condition.</param>
        public static int IndexOf<TSource>(this IEnumerable<TSource> Source, Func<TSource, bool> Predicate)
        {
            int Index = 0;
            foreach (TSource Item in Source)
            {
                if (Predicate(Item))
                    return Index;
                Index++;
            }

            return -1;
        }

        /// <summary>
        /// Filters a sequence of values based on a predicate.
        /// </summary>
        /// <param name="Source">A sequence to filter.</param>
        /// <param name="Predicate">A function to test each element for a condition.</param>
        public static IEnumerable Where(this IEnumerable Source, Func<object, bool> Predicate)
        {
            foreach (object Item in Source)
                if (Predicate(Item))
                    yield return Item;
        }

        /// <summary>
        /// Checks the sequence has contents.
        /// </summary>
        /// <param name="Source">The sequence to be checked.</param>
        public static bool IsEmpty(this IEnumerable Source)
            => !Source.GetEnumerator().MoveNext();

//#if NETSTANDARD2_0
//        /// <summary>
//        /// Appends a value to the end of the sequence.
//        /// </summary>
//        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
//        /// <param name="Source">A sequence of values.</param>
//        /// <param name="Element">The value to append to source.</param>
//        public static IEnumerable<TSource> Append<TSource>(this IEnumerable<TSource> Source, TSource Element)
//        {
//            foreach (TSource Item in Source)
//                yield return Item;

//            yield return Element;
//        }

//        /// <summary>
//        /// Adds a value to the beginning of the sequence.
//        /// </summary>
//        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
//        /// <param name="Source">A sequence of values.</param>
//        /// <param name="Element">The value to prepend to source.</param>
//        public static IEnumerable<TSource> Prepend<TSource>(this IEnumerable<TSource> Source, TSource Element)
//        {
//            yield return Element;

//            foreach (TSource Item in Source)
//                yield return Item;
//        }

//#endif

        public static IEnumerable<TResult> Select<TResult>(this IEnumerable Source, Func<object, TResult> Selector)
        {
            foreach (object Item in Source)
                yield return Selector.Invoke(Item);
        }
        public static IEnumerable<TResult> TrySelect<TResult>(this IEnumerable Source, Func<object, TResult> Selector)
        {
            TResult Result;
            foreach (object Item in Source)
            {
                try
                {
                    Result = Selector.Invoke(Item);
                }
                catch// (Exception ex)
                {
                    //Console.WriteLine($"{ex.Message}\r\n{ex.Source}\r\n{ex.StackTrace}");
                    continue;
                }
                yield return Result;
            }
        }
        public static IEnumerable<TResult> TrySelect<TSource, TResult>(this IEnumerable<TSource> Source, Func<TSource, TResult> Selector)
        {
            TResult Result;
            foreach (TSource Item in Source)
            {
                try
                {
                    Result = Selector.Invoke(Item);
                }
                catch// (Exception ex)
                {
                    //Console.WriteLine($"{ex.Message}\r\n{ex.Source}\r\n{ex.StackTrace}");
                    continue;
                }
                yield return Result;
            }
        }
        public static IEnumerable<TResult> TrySelectMany<TSource, TResult>(this IEnumerable<TSource> Source, Func<TSource, IEnumerable<TResult>> Selector)
        {
            IEnumerable<TResult> Results;
            foreach (TSource Item in Source)
            {
                try
                {
                    Results = Selector.Invoke(Item);
                }
                catch// (Exception ex)
                {
                    //Console.WriteLine($"{ex.Message}\r\n{ex.Source}\r\n{ex.StackTrace}");
                    continue;
                }

                foreach (TResult Result in Results)
                    yield return Result;
            }
        }

        /// <summary>
        /// Invokes a transform function on each element of a generic sequence and returns the maximum resulting element.
        /// </summary>
        /// <typeparam name="TResult">The type of the elements of source.</typeparam>
        /// <typeparam name="TSource">The type of the value returned by selector.</typeparam>
        /// <param name="Source">A sequence of values to determine the maximum value of.</param>
        /// <param name="Selector">A transform function to apply to each element.</param>
        public static TResult MaxBy<TResult, TSource>(this IEnumerable<TResult> Source, Func<TResult, TSource> Selector)
        {
            if (Source is null)
                throw new ArgumentNullException($"Source");

            Comparer<TSource> comparer = Comparer<TSource>.Default;
            TResult Result = default;
            TSource Max = default,
                    x;
            if (Max is null)
            {
                foreach (TResult Item in Source)
                {
                    x = Selector(Item);
                    if (x != null && (Max == null || comparer.Compare(x, Max) > 0))
                    {
                        Result = Item;
                        Max = x;
                    }
                }

                return Result;
            }
            else
            {
                bool HasValue = false;
                foreach (TResult Item in Source)
                {
                    x = Selector(Item);
                    if (HasValue)
                    {
                        if (comparer.Compare(x, Max) > 0)
                            Max = x;
                    }
                    else
                    {
                        Max = x;
                        HasValue = true;
                    }
                }

                return HasValue ? Result : throw new InvalidOperationException("Sequence contains no elements");
            }

        }

        /// <summary>
        /// Invokes a transform function on each element of a generic sequence and returns the minimum resulting element.
        /// </summary>
        /// <typeparam name="TResult">The type of the elements of source.</typeparam>
        /// <typeparam name="TSource">The type of the value returned by selector.</typeparam>
        /// <param name="Source">A sequence of values to determine the minimum value of.</param>
        /// <param name="Selector">A transform function to apply to each element.</param>
        public static TResult MinBy<TResult, TSource>(this IEnumerable<TResult> Source, Func<TResult, TSource> Selector)
        {
            if (Source is null)
                throw new ArgumentNullException($"Source");

            Comparer<TSource> comparer = Comparer<TSource>.Default;
            TResult Result = default;
            TSource Min = default,
                    x;
            if (Min is null)
            {
                foreach (TResult Item in Source)
                {
                    x = Selector(Item);
                    if (x != null && (Min == null || comparer.Compare(x, Min) < 0))
                    {
                        Result = Item;
                        Min = x;
                    }
                }

                return Result;
            }
            else
            {
                bool HasValue = false;
                foreach (TResult Item in Source)
                {
                    x = Selector(Item);
                    if (HasValue)
                    {
                        if (comparer.Compare(x, Min) < 0)
                            Min = x;
                    }
                    else
                    {
                        Min = x;
                        HasValue = true;
                    }
                }

                return HasValue ? Result : throw new InvalidOperationException("Sequence contains no elements");
            }

        }

    }
}