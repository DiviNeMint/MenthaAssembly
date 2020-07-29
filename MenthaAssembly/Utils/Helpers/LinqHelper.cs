using System.Collections;
using System.Collections.Generic;

namespace System.Linq
{
    public static class LinqHelper
    {
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

        public static TSource? FirstOrNull<TSource>(this IEnumerable<TSource> Source, Func<TSource, bool> Predicate)
            where TSource : struct
        {
            foreach (TSource Item in Source)
                if (Predicate(Item))
                    return Item;

            return null;
        }

        public static bool Any(this IEnumerable Source, object Item)
        {
            foreach (object i in Source)
                if (Item.Equals(i))
                    return true;

            return false;
        }
        public static bool Any<T>(this IEnumerable Source, T Item)
        {
            foreach (T i in Source.OfType<T>())
                if (Item.Equals(i))
                    return true;

            return false;
        }

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

    }
}
