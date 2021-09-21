using System;
using System.Runtime.InteropServices;

namespace MenthaAssembly.OpenCL
{
    public class OpenCLKernelArgument : IDisposable
    {
        public OpenCLArgumentIOMode IOMode { get; }

        public object Argument { get; }

        private GCHandle GCHandle;

#pragma warning disable IDE1006 // 命名樣式
        public IntPtr pArgument
#pragma warning restore IDE1006 // 命名樣式
        {
            get
            {
                if (Argument is IntPtr pArg)
                    return pArg;

                if (!GCHandle.IsAllocated)
                    GCHandle = GCHandle.Alloc(Argument, GCHandleType.Pinned);

                return GCHandle.AddrOfPinnedObject();
            }
        }

        public long Size { get; }

        public virtual bool IsArray { get; }

        public virtual long ArrayLength { get; }

        public OpenCLKernelArgument(object Argument) : this(Argument, Argument is Array ? OpenCLArgumentIOMode.InOut : OpenCLArgumentIOMode.In)
        {

        }
        public OpenCLKernelArgument(object Argument, OpenCLArgumentIOMode IOMode)
        {
            this.Argument = Argument;

            if (Argument is Array ArrayArg)
            {
                Size = Marshal.SizeOf(ArrayArg.GetValue(0).GetType()) * ArrayArg.Length;
                ArrayLength = ArrayArg.Length;
                IsArray = true;
                this.IOMode = IOMode;
            }
            else
            {
                if ((IOMode & OpenCLArgumentIOMode.Out) > 0)
                    throw new ArgumentException($"{nameof(IOMode)} can't include {nameof(OpenCLArgumentIOMode)}.{nameof(OpenCLArgumentIOMode.Out)}.");

                Size = Marshal.SizeOf(Argument);
                ArrayLength = -1;
                IsArray = false;
                this.IOMode = IOMode;
            }
        }
        public OpenCLKernelArgument(IntPtr pArgument, long Size, OpenCLArgumentIOMode IOMode)
        {
            Argument = pArgument;
            this.Size = Size;
            ArrayLength = -1;
            IsArray = false;
            this.IOMode = IOMode;
        }

        private bool IsDisposed;
        public void Dispose()
        {
            try
            {
                if (IsDisposed)
                    return;

                if (GCHandle.IsAllocated)
                    GCHandle.Free();

                IsDisposed = true;
            }
            finally
            {
                GC.SuppressFinalize(this);
            }
        }
    }
}
