using MenthaAssembly.OpenCL.Primitives;
using System;
using System.Diagnostics;

namespace MenthaAssembly.OpenCL
{
    internal class OpenCLMemory : IDisposable
    {
        public IntPtr Handle { protected set; get; }

        public OpenCLContext Context { get; }

        public OpenCLMemoryFlags Flags { get; }

        public long Size { protected set; get; }

        private readonly IntPtr pHost;
        protected OpenCLMemory(OpenCLContext Context, OpenCLMemoryFlags Flags)
        {
            this.Context = Context;
            this.Flags = Flags;
        }
        public OpenCLMemory(OpenCLContext Context, OpenCLMemoryFlags Flags, IntPtr pHost, long Size) : this(Context, Flags)
        {
            this.Size = Size;
            this.pHost = pHost;

            this.Handle = OpenCLCore.CreateBuffer(Context.Handle, Flags, new IntPtr(Size),
                                                  (Flags & (OpenCLMemoryFlags.CopyHostPointer | OpenCLMemoryFlags.UseHostPointer)) > 0 ? pHost : IntPtr.Zero,
                                                  out OpenCLErrorCode ResultCode);
            if (ResultCode != OpenCLErrorCode.Success)
                throw new OpenCLException(ResultCode);
        }

        private bool IsDisposed;
        public void Dispose()
        {
            try
            {
                if (IsDisposed)
                    return;

                OpenCLErrorCode Code = OpenCLCore.ReleaseMemObject(Handle);
                if (Code != OpenCLErrorCode.Success)
                    Debug.WriteLine($"{nameof(OpenCLMemory)} Release fail.\r\n" +
                                    $"ErrorCode : {Code}");

                IsDisposed = true;
            }
            finally
            {
                GC.SuppressFinalize(this);
            }
        }

        ~OpenCLMemory()
        {
            Dispose();
        }
    }
}
