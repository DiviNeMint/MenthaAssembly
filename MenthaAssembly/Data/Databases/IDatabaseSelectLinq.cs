using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace MenthaAssembly.Data
{
    public interface IDatabaseSelectLinq<T> : IAsyncEnumerable<T>
    {
        IDatabaseSelectLinq<T> Select(Expression<Func<T, object>> Selector);

        IDatabaseSelectLinq<T> Where(Expression<Func<T, bool>> Prediction);

    }

}
