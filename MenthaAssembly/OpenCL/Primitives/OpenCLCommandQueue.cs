using MenthaAssembly.OpenCL.Primitives;
using System;
using System.Diagnostics;
using System.Linq;

namespace MenthaAssembly.OpenCL
{
    internal class OpenCLCommandQueue : IDisposable
    {
        public IntPtr Handle { get; }

        public OpenCLContext Context { get; }

        public OpenCLDevice Device { get; }

        public bool OutOfOrder { get; }

        public bool Profiling { get; }

        public OpenCLCommandQueue(OpenCLContext Context, OpenCLDevice Device, OpenCLCommandQueueFlags Flags)
        {
            Handle = OpenCLCore.CreateCommandQueue(Context.Handle, Device.Handle, Flags, out OpenCLErrorCode ResultCode);
            if (ResultCode != OpenCLErrorCode.Success)
                throw new OpenCLException(ResultCode);

            this.Device = Device;
            this.Context = Context;

            this.OutOfOrder = (Flags & OpenCLCommandQueueFlags.OutOfOrderExecution) == OpenCLCommandQueueFlags.OutOfOrderExecution;
            this.Profiling = (Flags & OpenCLCommandQueueFlags.Profiling) == OpenCLCommandQueueFlags.Profiling;
        }

        ///// <summary>
        ///// Enqueues a command to execute a range of <see cref="OpenCLKernel"/>s in parallel.
        ///// </summary>
        ///// <param name="Kernel"> The <see cref="OpenCLKernel"/> to execute. </param>
        ///// <param name="GlobalWorkOffset"> An array of values that describe the offset used to calculate the global ID of a work-item instead of having the global IDs always start at offset (0, 0,... 0). </param>
        ///// <param name="GlobalWorkSize"> An array of values that describe the number of global work-items in dimensions that will execute the kernel function. The total number of global work-items is computed as global_work_size[0] *...* global_work_size[work_dim - 1]. </param>
        ///// <param name="LocalWorkSize"> An array of values that describe the number of work-items that make up a work-group (also referred to as the size of the work-group) that will execute the <paramref name="Kernel"/>. The total number of work-items in a work-group is computed as local_work_size[0] *... * local_work_size[work_dim - 1]. </param>
        ///// <param name="Events"> A collection of events that need to complete before this particular command can be executed. If <paramref name="Events"/> is not <c>null</c> or read-only a new <see cref="OpenCLEvent"/> identifying this command is created and attached to the end of the collection. </param>
        //public void Execute(OpenCLKernel Kernel, long[] GlobalWorkOffset, long[] GlobalWorkSize, long[] LocalWorkSize, ICollection<OpenCLEventBase> Events)
        //{
        //    //CLEventHandle[] eventHandles = OpenCLTools.ExtractHandles(events, out var eventWaitListSize);
        //    //bool eventsWritable = events != null && !events.IsReadOnly;
        //    //CLEventHandle[] newEventHandle = eventsWritable ? new CLEventHandle[1] : null;

        //    //OpenCLErrorCode error = CL12.EnqueueNDRangeKernel(Handle, kernel.Handle, GlobalWorkSize.Length, OpenCLTools.ConvertArray(GlobalWorkOffset), OpenCLTools.ConvertArray(GlobalWorkSize), OpenCLTools.ConvertArray(localWorkSize), eventWaitListSize, eventHandles, newEventHandle);
        //    //OpenCLException.ThrowOnError(error);

        //    //if (eventsWritable)
        //    //    events.Add(new OpenCLEvent(newEventHandle[0], this));
        //}

        /// <summary>
        /// Enqueues a command to execute a range of <see cref="OpenCLKernel"/>s in parallel.
        /// </summary>
        /// <param name="Kernel"> The <see cref="OpenCLKernel"/> to execute. </param>
        /// <param name="GlobalWorkOffset"> An array of values that describe the offset used to calculate the global ID of a work-item instead of having the global IDs always start at offset (0, 0,... 0). </param>
        /// <param name="GlobalWorkSize"> An array of values that describe the number of global work-items in dimensions that will execute the kernel function. The total number of global work-items is computed as global_work_size[0] *...* global_work_size[work_dim - 1]. </param>
        /// <param name="LocalWorkSize"> An array of values that describe the number of work-items that make up a work-group (also referred to as the size of the work-group) that will execute the <paramref name="kernel"/>. The total number of work-items in a work-group is computed as local_work_size[0] *... * local_work_size[work_dim - 1]. </param>
        public void Execute(OpenCLKernel Kernel, long[] GlobalWorkOffset, long[] GlobalWorkSize, long[] LocalWorkSize)
        {
            OpenCLErrorCode ResultCode = OpenCLCore.EnqueueNDRangeKernel(Handle, Kernel.Handle,
                                                                         GlobalWorkSize?.Length ?? 0,
                                                                         GlobalWorkOffset?.Select(i => new IntPtr(i)).ToArray(),
                                                                         GlobalWorkSize?.Select(i => new IntPtr(i)).ToArray(),
                                                                         LocalWorkSize?.Select(i => new IntPtr(i)).ToArray(),
                                                                         0, null, null);

            if (ResultCode != OpenCLErrorCode.Success)
                throw new OpenCLException(ResultCode);
        }

        public void ReadFromMemory(OpenCLMemory Memory, IntPtr pDest, long Offset, long Length, bool Blocking)
        {
            OpenCLErrorCode ResultCode = OpenCLCore.EnqueueReadBuffer(Handle, Memory.Handle, Blocking, new IntPtr(Offset), new IntPtr(Length), pDest, 0, null, null);
            if (ResultCode != OpenCLErrorCode.Success)
                throw new OpenCLException(ResultCode);
        }


        /// <summary>
        /// Blocks until all previously enqueued commands are issued to the <see cref="OpenCLCommandQueue.Device"/> and have completed.
        /// </summary>
        public void Finish()
        {
            OpenCLErrorCode ResultCode = OpenCLCore.Finish(Handle);

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

                OpenCLErrorCode Code = OpenCLCore.ReleaseCommandQueue(Handle);
                if (Code != OpenCLErrorCode.Success)
                    Debug.WriteLine($"{nameof(OpenCLCommandQueue)} Release fail.\r\n" +
                                    $"ErrorCode : {Code}");

                IsDisposed = true;
            }
            finally
            {
                GC.SuppressFinalize(this);
            }
        }

        ~OpenCLCommandQueue()
        {
            Dispose();
        }
    }
}
