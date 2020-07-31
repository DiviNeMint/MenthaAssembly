using System.Collections.Concurrent;

namespace MenthaAssembly.Utils
{
    public class Pool<T>
    {
        private readonly ConcurrentQueue<T> PoolBase = new ConcurrentQueue<T>();

        public virtual void Enqueue(ref T Item)
            => PoolBase.Enqueue(Item);

        public virtual bool TryDequeue(out T Item)
            => PoolBase.TryDequeue(out Item);

    }
}
