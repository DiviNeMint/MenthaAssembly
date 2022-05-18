using System;

namespace MenthaAssembly.Network.Primitives
{
    public abstract class IOCPBase : IDisposable
    {
        private IOCPPool _Pool = new IOCPPool();
        protected IOCPPool Pool => _Pool;

        public int BufferSize
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(GetType().Name);

                return Pool.BufferSize;
            }

            set
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(GetType().Name);

                Pool.BufferSize = value;
            }
        }

        private bool IsDisposed = false;
        public virtual void Dispose()
        {
            if (IsDisposed)
                return;

            try
            {
                _Pool.Dispose();
                _Pool = null;
            }
            finally
            {
                IsDisposed = true;
            }
        }

    }
}
