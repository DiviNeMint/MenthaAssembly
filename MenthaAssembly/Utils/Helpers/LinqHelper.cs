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

        public static bool Contains<TSource>(this IEnumerable<TSource> Source, Func<TSource, bool> Predicate)
        {
            foreach (TSource Item in Source)
                if (Predicate(Item))
                    return true;

            return false;
        }


    }
}
