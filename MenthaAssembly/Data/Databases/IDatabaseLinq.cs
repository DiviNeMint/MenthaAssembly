using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace MenthaAssembly.Data
{
    public interface IDatabaseLinq<T> : IAsyncEnumerable<T>
    {
        IDatabaseSelectLinq<T> Select(Expression<Func<T, object>> Selector);

        IDatabaseLinq<T> Where(Expression<Func<T, bool>> Prediction);

        void Remove();
        Task RemoveAsync();

    }

}
