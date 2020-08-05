using System.Threading;

namespace MenthaAssembly.Utils
{
    public class UidPool : Pool<uint>
    {
        private readonly object LockObject = new object();
        private uint Counter = 0;

        public uint Dequeue()
        {
            if (base.TryDequeue(out uint Uid))
                return Uid;

            bool Token = false;
            try
            {
                Monitor.Enter(LockObject, ref Token);
                return Counter++;
            }
            finally
            {
                if (Token)
                    Monitor.Exit(LockObject);
            }
        }

        public override bool TryDequeue(out uint Uid)
        {
            Uid = Dequeue();
            return true;
        }

    }
}
