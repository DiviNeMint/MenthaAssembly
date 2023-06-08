using System;
using System.Collections.Concurrent;

namespace MenthaAssembly.Utils
{
    public class Pool<T> : IDisposable
    {
        private ConcurrentQueue<T> PoolBase = new();

        public virtual void Enqueue(T Item)
        {
            if (IsDisposed)
                return;

            PoolBase.Enqueue(Item);
        }

        public virtual bool TryDequeue(out T Item)
        {
            if (IsDisposed)
            {
                Item = default;
                return false;
            }

            return PoolBase.TryDequeue(out Item);
        }

        private bool IsDisposed = false;
        public virtual void Dispose()
        {
            if (IsDisposed)
                return;

            try
            {
                PoolBase = null;
            }
            finally
            {
                IsDisposed = true;
            }
        }

    }
}