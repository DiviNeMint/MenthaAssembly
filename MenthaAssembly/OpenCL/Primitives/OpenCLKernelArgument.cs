using System;
using System.Runtime.InteropServices;

namespace MenthaAssembly.OpenCL
{
    public class OpenCLKernelArgument
    {
        public OpenCLArgumentIOMode IOMode { get; }

        public object Argument { get; }

#pragma warning disable IDE1006 // 命名樣式
        public IntPtr pArgument
#pragma warning restore IDE1006 // 命名樣式
        {
            get
            {
                if (Argument is IntPtr pArg)
                    return pArg;

                return GCHandle.Alloc(Argument, GCHandleType.Pinned).AddrOfPinnedObject();
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
                this.Size = Marshal.SizeOf(ArrayArg.GetValue(0).GetType()) * ArrayArg.Length;
                this.ArrayLength = ArrayArg.Length;
                this.IsArray = true;
                this.IOMode = IOMode;
            }
            else
            {
                if ((IOMode & OpenCLArgumentIOMode.Out) > 0)
                    throw new ArgumentException($"{nameof(IOMode)} can't include {nameof(OpenCLArgumentIOMode)}.{nameof(OpenCLArgumentIOMode.Out)}.");

                this.Size = Marshal.SizeOf(Argument);
                this.ArrayLength = -1;
                this.IsArray = false;
                this.IOMode = IOMode;
            }
        }
        public OpenCLKernelArgument(IntPtr pArgument, long Size, OpenCLArgumentIOMode IOMode)
        {
            this.Argument = pArgument;
            this.Size = Size;
            this.ArrayLength = -1;
            this.IsArray = false;
            this.IOMode = IOMode;
        }

    }
}
