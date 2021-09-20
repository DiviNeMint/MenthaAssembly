using MenthaAssembly.OpenCL.Primitives;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace MenthaAssembly.OpenCL
{
    public class OpenCLKernel : IDisposable
    {
        public IntPtr Handle { get; }

        internal OpenCLProgram Program { get; }

        public string FunctionName { get; }

        internal OpenCLKernel(IntPtr pKernel, OpenCLProgram Program)
        {
            this.Handle = pKernel;
            this.Program = Program;
            this.FunctionName = GetProgramBuildInfo(OpenCLKernelProperty.FunctionName, new byte[512]);
        }

        private string GetProgramBuildInfo(OpenCLKernelProperty Property, byte[] Buffer)
        {
            OpenCLCore.GetKernelInfo(this.Handle, Property, Buffer.Length, Buffer, out int ReadSize);
            return Encoding.ASCII.GetString(Buffer, 0, ReadSize).Trim('\0');
        }

        public void Invoke(params object[] Arguments)
            => Invoke(Program.Context.Devices.FirstOrDefault(), Arguments.Select(i => i is OpenCLKernelArgument Arg ? Arg : new OpenCLKernelArgument(i)).ToArray());
        public void Invoke(params OpenCLKernelArgument[] Arguments)
            => Invoke(Program.Context.Devices.FirstOrDefault(), Arguments);
        public void Invoke(long[] GlobalWorkOffset, long[] GlobalWorkSize, long[] LocalWorkSize, params OpenCLKernelArgument[] Arguments)
            => Invoke(Program.Context.Devices.FirstOrDefault(), GlobalWorkOffset, GlobalWorkSize, LocalWorkSize, Arguments);
        public void Invoke(OpenCLDevice Device, params object[] Arguments)
            => Invoke(Device, Arguments.Select(i => i is OpenCLKernelArgument Arg ? Arg : new OpenCLKernelArgument(i)).ToArray());
        public void Invoke(OpenCLDevice Device, params OpenCLKernelArgument[] Arguments)
            => Invoke(Device, null, new long[] { Arguments.FirstOrDefault(i => i.IsArray).ArrayLength }, null, Arguments);
        public unsafe void Invoke(OpenCLDevice Device, long[] GlobalWorkOffset, long[] GlobalWorkSize, long[] LocalWorkSize, params OpenCLKernelArgument[] Arguments)
        {
            if (Device is null)
                throw new ArgumentNullException(nameof(Device));

            if (Arguments.Length == 0)
                throw new ArgumentNullException(nameof(Arguments));

            // SerArguments
            OpenCLMemory[] Memories = new OpenCLMemory[Arguments.Length];
            for (int i = 0; i < Arguments.Length; i++)
            {
                OpenCLKernelArgument Arg = Arguments[i];

                OpenCLErrorCode ResultCode;
                if (Arg.IsArray || (Arg.IOMode & OpenCLArgumentIOMode.Out) > 0)
                {
                    OpenCLMemoryFlags MemFlags = OpenCLMemoryFlags.ReadWrite | ((Arg.IOMode & OpenCLArgumentIOMode.In) > 0 ? OpenCLMemoryFlags.CopyHostPointer : OpenCLMemoryFlags.AllocateHostPointer);
                    OpenCLMemory Memory = Program.Context.CreateBuffer(MemFlags, Arg.pArgument, Arg.Size);

                    IntPtr pMemory = Memory.Handle;
                    Memories[i] = Memory;
                    ResultCode = OpenCLCore.SetKernelArg(this.Handle, i, new IntPtr(sizeof(IntPtr)), ref pMemory);
                }
                else
                {
                    ResultCode = OpenCLCore.SetKernelArg(this.Handle, i, new IntPtr(Arg.Size), Arg.pArgument);
                }

                if (ResultCode != OpenCLErrorCode.Success)
                    throw new OpenCLException(ResultCode);
            }

            OpenCLCommandQueue Commands = Program.Context.CreateCommandQueue(Device, OpenCLCommandQueueFlags.None);
            try
            {
                // Execute
                Commands.Execute(this, GlobalWorkOffset, GlobalWorkSize, LocalWorkSize);

                // Read Result
                for (int i = 0; i < Arguments.Length; i++)
                {
                    OpenCLKernelArgument Arg = Arguments[i];

                    if ((Arg.IOMode & OpenCLArgumentIOMode.Out) > 0)
                        Commands.ReadFromMemory(Memories[i], Arg.pArgument, 0, Arg.Size, true);

                    Memories[i]?.Dispose();
                }

            }
            finally
            {
                Commands.Finish();
                Commands.Dispose();
            }
        }

        //public unsafe void SetArgument<T>(int Index, T Value, bool IsInput)
        //    where T : unmanaged
        //{
        //    if (IsDisposed)
        //        throw new ObjectDisposedException(nameof(OpenCLKernel));

        //    OpenCLErrorCode ResultCode = OpenCLCore.SetKernelArg(this.Handle, Index, new IntPtr(sizeof(T)), &Value);
        //    if (ResultCode != OpenCLErrorCode.Success)
        //        throw new OpenCLException(ResultCode);
        //}

        //public unsafe void SetArgument<T>(int Index, T[] Array, bool IsInput)
        //    where T : unmanaged
        //{
        //    if (IsDisposed)
        //        throw new ObjectDisposedException(nameof(OpenCLKernel));

        //    OpenCLMemory Memory = Program.Context.CreateBuffer(Array, IsInput);
        //    SetArgument(Index, Memory.Handle);
        //}

        //private unsafe void SetArgument(int Index, IntPtr Handle)
        //{
        //    OpenCLErrorCode ResultCode = OpenCLCore.SetKernelArg(this.Handle, Index, new IntPtr(sizeof(IntPtr)), Handle);
        //    if (ResultCode != OpenCLErrorCode.Success)
        //        throw new OpenCLException(ResultCode);
        //}



        //private bool IsDisposed;
        //public void Dispose()
        //{
        //    if (IsDisposed)
        //        return;

        //    OpenCLErrorCode Code = OpenCLCore.ReleaseKernel(Handle);
        //    if (Code != OpenCLErrorCode.Success)
        //        Debug.WriteLine($"{nameof(OpenCLKernel)} Release fail.\r\n" +
        //                        $"ErrorCode : {Code}");

        //    IsDisposed = true;
        //}

        private bool IsDisposed;
        public void Dispose()
        {
            try
            {
                if (IsDisposed)
                    return;

                OpenCLErrorCode Code = OpenCLCore.ReleaseKernel(Handle);
                if (Code != OpenCLErrorCode.Success)
                    Debug.WriteLine($"{nameof(OpenCLKernel)} Release fail.\r\n" +
                                    $"ErrorCode : {Code}");

                IsDisposed = true;
            }
            finally
            {
                GC.SuppressFinalize(this);
            }
        }

        ~OpenCLKernel()
        {
            Dispose();
        }
    }
}
