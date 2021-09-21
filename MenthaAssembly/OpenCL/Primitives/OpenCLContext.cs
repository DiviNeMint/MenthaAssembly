using MenthaAssembly.OpenCL.Primitives;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace MenthaAssembly.OpenCL
{
    internal class OpenCLContext : IDisposable
    {
        public IntPtr Handle { get; }

        public ReadOnlyCollection<OpenCLDevice> Devices { get; }

        public List<OpenCLContextProperty> Properties { get; }

        public OpenCLContextCallback Callback;

        public OpenCLContext(IEnumerable<OpenCLDevice> Devices, IEnumerable<OpenCLContextProperty> Properties, OpenCLContextCallback Callback, IntPtr UserData)
        {
            this.Devices = new ReadOnlyCollection<OpenCLDevice>(Devices?.ToArray() ?? new OpenCLDevice[0]);
            this.Properties = new List<OpenCLContextProperty>(Properties);
            this.Callback = Callback;

            IntPtr[] pDevices = Devices?.Select(i => i.Handle).ToArray() ?? new IntPtr[0],
                     pProperties = new IntPtr[Properties?.Count() * 2 + 1 ?? 0];

            int Index = 0;
            foreach (OpenCLContextProperty Property in Properties)
            {
                pProperties[Index++] = new IntPtr((int)Property.Name);
                pProperties[Index++] = Property.Handle;
            }


            Handle = OpenCLCore.CreateContext(pProperties, pDevices.Length, pDevices, Callback, UserData, out OpenCLErrorCode ResultCode);

            if (ResultCode != OpenCLErrorCode.Success)
                throw new OpenCLException(ResultCode);
        }

        public OpenCLCommandQueue CreateCommandQueue(OpenCLDevice Device, OpenCLCommandQueueFlags Flags)
            => new OpenCLCommandQueue(this, Device, Flags);

        public OpenCLMemory CreateBuffer(OpenCLMemoryFlags Flags, IntPtr pHost, long Size)
            => new OpenCLMemory(this, Flags, pHost, Size);

        private bool IsDisposed;
        public void Dispose()
        {
            try
            {
                if (IsDisposed)
                    return;

                OpenCLErrorCode Code = OpenCLCore.ReleaseContext(Handle);
                if (Code != OpenCLErrorCode.Success)
                    Debug.WriteLine($"{nameof(OpenCLContext)} Release fail.\r\n" +
                                    $"ErrorCode : {Code}");

                IsDisposed = true;
            }
            finally
            {
                GC.SuppressFinalize(this);
            }
        }

        ~OpenCLContext()
        {
            Dispose();
        }
    }
}
