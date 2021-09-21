using MenthaAssembly.OpenCL.Primitives;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace MenthaAssembly.OpenCL
{
    internal class OpenCLProgram : IDisposable
    {
        public IntPtr Handle { get; }

        public ReadOnlyCollection<string> Sources { get; }

        public OpenCLContext Context { get; }

        public OpenCLProgram(OpenCLContext Context, params string[] Sources)
        {
            Handle = OpenCLCore.CreateProgramWithSource(Context.Handle, Sources.Length, Sources, null, out OpenCLErrorCode ResultCode);

            if (ResultCode != OpenCLErrorCode.Success)
                throw new OpenCLException(ResultCode);

            this.Context = Context;
            this.Sources = new ReadOnlyCollection<string>(Sources);
        }

        private string BuildOptions;
        private OpenCLProgramBuildCallback Callback;

        /// <summary>
        /// Builds (compiles and links) a program executable from the program source or binary for all or some of the <see cref="OpenCLContext.Devices"/>.
        /// </summary>
        /// <param name="Devices"> 
        /// A subset or all of <see cref="OpenCLContext.Devices"/>.
        /// If <paramref name="Devices"/> is <c>null</c>, the executable is built for every item of <see cref="OpenCLContext.Devices"/> for which a source or a binary has been loaded.
        /// </param>
        /// <param name="Options">
        /// A set of options for the OpenCL compiler. </param>
        /// <param name="Callback"> 
        /// A delegate instance that represents a reference to a notification routine.
        /// This routine is a callback function that an application can register and which will be called when the program executable has been built (successfully or unsuccessfully).
        /// If <paramref name="Callback"/> is not <c>null</c>, <see cref="Build"/> does not need to wait for the build to complete and can return immediately.
        /// If <paramref name="Callback"/> is <c>null</c>, <see cref="Build"/> does not return until the build has completed.
        /// The callback function may be called asynchronously by the OpenCL implementation.
        /// It is the application's responsibility to ensure that the callback function is thread-safe and that the delegate instance doesn't get collected by the Garbage Collector until the build operation triggers the callback. </param>
        /// <param name="pUserData"> 
        /// Optional user data that will be passed to <paramref name="Callback"/>. 
        /// </param>
        public IEnumerable<OpenCLKernel> Build(IEnumerable<OpenCLDevice> Devices, string Options, OpenCLProgramBuildCallback Callback, IntPtr pUserData)
        {
            IntPtr[] pDevices = Devices?.Select(i => i.Handle).ToArray();

            this.BuildOptions = Options;
            this.Callback = Callback;

            OpenCLErrorCode ResultCode = OpenCLCore.BuildProgram(Handle, pDevices?.Length ?? 0, pDevices, Options, Callback, pUserData);
            if (ResultCode != OpenCLErrorCode.Success)
            {
                Debug.WriteLine($"{nameof(OpenCLProgram)} Build fail.\r\n" +
                                $"ErrorCode : {ResultCode}\r\n" +
                                $"Message   : {GetBuildLog(Devices.FirstOrDefault())}");
                yield break;
            }

            ResultCode = OpenCLCore.CreateKernelsInProgram(Handle, 0, null, out int Count);
            if (ResultCode != OpenCLErrorCode.Success)
            {
                Debug.WriteLine($"{nameof(OpenCLProgram)} CreateKernels fail.\r\n" +
                                $"ErrorCode : {ResultCode}");
                yield break;
            }

            IntPtr[] pKernels = new IntPtr[Count];

            ResultCode = OpenCLCore.CreateKernelsInProgram(Handle, Count, pKernels, out Count);
            if (ResultCode != OpenCLErrorCode.Success)
            {
                Debug.WriteLine($"{nameof(OpenCLProgram)} CreateKernels fail.\r\n" +
                                $"ErrorCode : {ResultCode}");
                yield break;
            }

            for (int i = 0; i < Count; i++)
                yield return new OpenCLKernel(pKernels[i], this);
        }

        public string GetBuildLog(OpenCLDevice Device)
            => GetProgramBuildInfo(Device.Handle, OpenCLProgramBuildProperty.BuildLog, new byte[512]);

        private string GetProgramBuildInfo(IntPtr pDevice, OpenCLProgramBuildProperty Flag, byte[] Buffer)
        {
            OpenCLCore.GetProgramBuildInfo(this.Handle, pDevice, Flag, Buffer.Length, Buffer, out int ReadSize);
            return Encoding.ASCII.GetString(Buffer, 0, ReadSize).Trim('\0');
        }
        private unsafe T GetProgramBuildInfo<T>(IntPtr pDevice, OpenCLProgramBuildProperty Flag)
             where T : unmanaged
        {
            T Result = default;
            T* pResult = &Result;
            OpenCLCore.GetProgramBuildInfo(this.Handle, pDevice, Flag, sizeof(T), (IntPtr)pResult, out _);
            return Result;
        }

        private bool IsDisposed;
        public void Dispose()
        {
            try
            {
                if (IsDisposed)
                    return;

                Context.Dispose();

                OpenCLErrorCode Code = OpenCLCore.ReleaseProgram(Handle);
                if (Code != OpenCLErrorCode.Success)
                    Debug.WriteLine($"{nameof(OpenCLProgram)} Release fail.\r\n" +
                                    $"ErrorCode : {Code}");

                IsDisposed = true;
            }
            finally
            {
                GC.SuppressFinalize(this);
            }
        }

        ~OpenCLProgram()
        {
            Dispose();
        }
    }
}
