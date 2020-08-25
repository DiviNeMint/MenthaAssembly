using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security;

namespace MenthaAssembly.OpenCL.Primitives
{


    /// <summary>
    /// A callback function that can be registered by the application to report information on errors that occur in the <see cref="OpenCLContext"/>.
    /// </summary>
    /// <param name="ErrorMessage"> An error string. </param>
    /// <param name="clDataPtr"> A pointer to binary data that is returned by the OpenCL implementation that can be used to log additional information helpful in debugging the error.</param>
    /// <param name="clDataSize"> The size of the binary data that is returned by the OpenCL. </param>
    /// <param name="pUserData"> The pointer to the optional user data specified in <paramref name="pUserData"/> argument of <see cref="OpenCLContext"/> constructor. </param>
    /// <remarks> This callback function may be called asynchronously by the OpenCL implementation. It is the application's responsibility to ensure that the callback function is thread-safe. </remarks>
    internal delegate void OpenCLContextCallback(string ErrorMessage, IntPtr clDataPtr, IntPtr clDataSize, IntPtr pUserData);

    /// <summary>
    /// A callback function that can be registered by the application to report the <see cref="ComputeProgram"/> build status.
    /// </summary>
    /// <param name="pProgram"> The handle of the <see cref="ComputeProgram"/> being built. </param>
    /// <param name="pUserData"> The pointer to the optional user data specified in <paramref name="pUserData"/> argument of <see cref="ComputeProgram.Build"/>. </param>
    /// <remarks> This callback function may be called asynchronously by the OpenCL implementation. It is the application's responsibility to ensure that the callback function is thread-safe. </remarks>
    internal delegate void OpenCLProgramBuildCallback(IntPtr pProgram, IntPtr pUserData);

    /// <summary>
    /// Contains bindings to the OpenCL 1.0 functions.
    /// </summary>
    /// <remarks> See the OpenCL specification for documentation regarding these functions. </remarks>
    [SuppressUnmanagedCodeSecurity]
    internal class OpenCLCore
    {
#if OSX
        private const string LibName = "/System/Library/Frameworks/OpenCL.framework/OpenCL";
#elif LINUX
        private const string LibName = "libOpenCL.so";
#else
        private const string LibName = "OpenCL";
#endif

        [DllImport(LibName, EntryPoint = "clGetPlatformIDs")]
        internal static extern OpenCLErrorCode GetPlatformIDs(int num_entries, [Out, MarshalAs(UnmanagedType.LPArray)] IntPtr[] pPlatforms, out int num_platforms);

        [DllImport(LibName, EntryPoint = "clGetPlatformInfo")]
        internal static extern OpenCLErrorCode GetPlatformInfo(IntPtr pPlatform, OpenCLPlatformProperty Property, int BufferSize, byte[] Buffer, out int ReadSize);

        [DllImport(LibName, EntryPoint = "clGetDeviceIDs")]
        internal static extern OpenCLErrorCode GetDeviceIDs(IntPtr pPlatform, OpenCLDeviceTypes DeviceType, int num_entries, [Out, MarshalAs(UnmanagedType.LPArray)] IntPtr[] pDevices, out int num_devices);

        [DllImport(LibName, EntryPoint = "clGetDeviceInfo")]
        internal static extern OpenCLErrorCode GetDeviceInfo(IntPtr pDevice, OpenCLDeviceProperty Property, int BufferSize, byte[] Buffer, out int ReadSize);

        [DllImport(LibName, EntryPoint = "clGetDeviceInfo")]
        internal static extern OpenCLErrorCode GetDeviceInfo(IntPtr pDevice, OpenCLDeviceProperty Property, int BufferSize, IntPtr Buffer, out int ReadSize);

        [DllImport(LibName, EntryPoint = "clCreateContext")]
        internal static extern IntPtr CreateContext([MarshalAs(UnmanagedType.LPArray)] IntPtr[] pProperties, int num_devices, [MarshalAs(UnmanagedType.LPArray)] IntPtr[] pDevices, OpenCLContextCallback Callback, IntPtr pUserData, out OpenCLErrorCode ResultCode);

        [DllImport(LibName, EntryPoint = "clReleaseContext")]
        internal static extern OpenCLErrorCode ReleaseContext(IntPtr pContext);

        [DllImport(LibName, EntryPoint = "clCreateProgramWithSource")]
        internal static extern IntPtr CreateProgramWithSource(IntPtr pContext, int Count, string[] Sources, [MarshalAs(UnmanagedType.LPArray)] IntPtr[] lengths, out OpenCLErrorCode ResultCode);

        //[DllImport(LibName, EntryPoint = "clCreateProgramWithBinary")]
        //internal static extern CLProgramHandle CreateProgramWithBinary(
        //    IntPtr pContext,
        //    int num_devices,
        //    [MarshalAs(UnmanagedType.LPArray)] CLDeviceHandle[] device_list,
        //    [MarshalAs(UnmanagedType.LPArray)] IntPtr[] lengths,
        //    [MarshalAs(UnmanagedType.LPArray)] IntPtr[] binaries,
        //    [MarshalAs(UnmanagedType.LPArray)] int[] binary_status,
        //    out OpenCLErrorCode ResultCode);

        //[DllImport(LibName, EntryPoint = "clRetainProgram")]
        //internal static extern OpenCLErrorCode RetainProgram(CLProgramHandle program);

        [DllImport(LibName, EntryPoint = "clBuildProgram")]
        internal static extern OpenCLErrorCode BuildProgram(IntPtr pProgram, int num_devices, [MarshalAs(UnmanagedType.LPArray)] IntPtr[] pDevices, string Options, OpenCLProgramBuildCallback Callback, IntPtr pUserData);

        [DllImport(LibName, EntryPoint = "clGetProgramBuildInfo")]
        internal static extern OpenCLErrorCode GetProgramBuildInfo(IntPtr pProgram, IntPtr pDevice, OpenCLProgramBuildProperty Property, int BufferSize, byte[] Buffer, out int ReadSize);

        [DllImport(LibName, EntryPoint = "clGetProgramBuildInfo")]
        internal static extern OpenCLErrorCode GetProgramBuildInfo(IntPtr pProgram, IntPtr pDevice, OpenCLProgramBuildProperty Property, int BufferSize, IntPtr Buffer, out int ReadSize);

        [DllImport(LibName, EntryPoint = "clReleaseProgram")]
        internal static extern OpenCLErrorCode ReleaseProgram(IntPtr pProgram);


        //[DllImport(LibName, EntryPoint = "clUnloadCompiler")]
        //internal static extern OpenCLErrorCode UnloadCompiler();

        //[DllImport(LibName, EntryPoint = "clGetProgramInfo")]
        //internal static extern OpenCLErrorCode GetProgramInfo(
        //    CLProgramHandle program,
        //    ComputeProgramInfo param_name,
        //    IntPtr param_value_size,
        //    IntPtr param_value,
        //    out IntPtr param_value_size_ret);

        [DllImport(LibName, EntryPoint = "clCreateKernelsInProgram")]
        internal static extern OpenCLErrorCode CreateKernelsInProgram(IntPtr pProgram, int num_kernels, [Out, MarshalAs(UnmanagedType.LPArray)] IntPtr[] pKernels, out int ReadSize);

        [DllImport(LibName, EntryPoint = "clCreateKernel")]
        internal static extern IntPtr CreateKernel(IntPtr pProgram, string kernel_name, out OpenCLErrorCode ResultCode);

        [DllImport(LibName, EntryPoint = "clGetKernelInfo")]
        internal static extern OpenCLErrorCode GetKernelInfo(IntPtr pKernel, OpenCLKernelProperty Property, int BufferSize, byte[] Buffer, out int ReadSize);

        //[DllImport(LibName, EntryPoint = "clGetKernelWorkGroupInfo")]
        //internal static extern OpenCLErrorCode GetKernelWorkGroupInfo(
        //    IntPtr pKernel,
        //    IntPtr pDevice,
        //    OpenCLKernelWorkGroupInfo param_name,
        //    IntPtr param_value_size,
        //    IntPtr param_value,
        //    out IntPtr param_value_size_ret);

        //[DllImport(LibName, EntryPoint = "clRetainKernel")]
        //internal static extern OpenCLErrorCode RetainKernel(IntPtr pKernel);

        [DllImport(LibName, EntryPoint = "clSetKernelArg")]
        internal static extern OpenCLErrorCode SetKernelArg(IntPtr pKernel, int ArgIndex, IntPtr ArgSize, ref IntPtr pMemory);

        [DllImport(LibName, EntryPoint = "clSetKernelArg")]
        internal static extern OpenCLErrorCode SetKernelArg(IntPtr pKernel, int ArgIndex, IntPtr ArgSize, IntPtr pArgValue);

        //[DllImport(LibName, EntryPoint = "clSetKernelArg")]
        //internal static extern OpenCLErrorCode SetKernelArg(
        //    IntPtr pKernel,
        //    int ArgIndex,
        //    IntPtr ArgSize,
        //    ref CLMemoryHandle ArgValue);

        //[DllImport(LibName, EntryPoint = "clSetKernelArg")]
        //internal static extern OpenCLErrorCode SetKernelArg(
        //    IntPtr pKernel,
        //    int ArgIndex,
        //    IntPtr ArgSize,
        //    ref byte ArgValue);

        //[DllImport(LibName, EntryPoint = "clSetKernelArg")]
        //internal static extern OpenCLErrorCode SetKernelArg(
        //    IntPtr pKernel,
        //    int ArgIndex,
        //    IntPtr ArgSize,
        //    ref ushort ArgValue);

        //[DllImport(LibName, EntryPoint = "clSetKernelArg")]
        //internal static extern OpenCLErrorCode SetKernelArg(
        //    IntPtr pKernel,
        //    int ArgIndex,
        //    IntPtr ArgSize,
        //    ref short ArgValue);

        //[DllImport(LibName, EntryPoint = "clSetKernelArg")]
        //internal static extern OpenCLErrorCode SetKernelArg(
        //    IntPtr pKernel,
        //    int ArgIndex,
        //    IntPtr ArgSize,
        //    ref uint ArgValue);

        //[DllImport(LibName, EntryPoint = "clSetKernelArg")]
        //internal static extern OpenCLErrorCode SetKernelArg(
        //    IntPtr pKernel,
        //    int ArgIndex,
        //    IntPtr ArgSize,
        //    ref int ArgValue);

        //[DllImport(LibName, EntryPoint = "clSetKernelArg")]
        //internal static extern OpenCLErrorCode SetKernelArg(
        //    IntPtr pKernel,
        //    int ArgIndex,
        //    IntPtr ArgSize,
        //    ref ulong ArgValue);

        //[DllImport(LibName, EntryPoint = "clSetKernelArg")]
        //internal static extern OpenCLErrorCode SetKernelArg(
        //    IntPtr pKernel,
        //    int ArgIndex,
        //    IntPtr ArgSize,
        //    ref long ArgValue);

        //[DllImport(LibName, EntryPoint = "clSetKernelArg")]
        //internal static extern OpenCLErrorCode SetKernelArg(
        //    IntPtr pKernel,
        //    int ArgIndex,
        //    IntPtr ArgSize,
        //    ref float ArgValue);

        //[DllImport(LibName, EntryPoint = "clSetKernelArg")]
        //internal static extern OpenCLErrorCode SetKernelArg(
        //    IntPtr pKernel,
        //    int ArgIndex,
        //    IntPtr ArgSize,
        //    ref double ArgValue);


        [DllImport(LibName, EntryPoint = "clReleaseKernel")]
        internal static extern OpenCLErrorCode ReleaseKernel(IntPtr kernel);

        [DllImport(LibName, EntryPoint = "clCreateCommandQueue")]
        internal static extern IntPtr CreateCommandQueue(IntPtr pContext, IntPtr pDevice, OpenCLCommandQueueFlags Flags, out OpenCLErrorCode ResultCode);

        [DllImport(LibName, EntryPoint = "clGetCommandQueueInfo")]
        internal static extern OpenCLErrorCode GetCommandQueueInfo(IntPtr pCommandQueue, OpenCLCommandQueueProperty Property, int BufferSize, byte[] Buffer, out int ReadSize);

        [DllImport(LibName, EntryPoint = "clSetCommandQueueProperty")]
        internal static extern OpenCLErrorCode SetCommandQueueProperty(IntPtr pCommandQueue, OpenCLCommandQueueProperty Property, [MarshalAs(UnmanagedType.Bool)] bool Enable, out OpenCLCommandQueueProperty OldProperties);

        //[DllImport(LibName, EntryPoint = "clFlush")]
        //internal static extern OpenCLErrorCode Flush(IntPtr pCommandQueue);

        [DllImport(LibName, EntryPoint = "clFinish")]
        internal static extern OpenCLErrorCode Finish(IntPtr pCommandQueue);

        //[DllImport(LibName, EntryPoint = "clRetainCommandQueue")]
        //internal static extern OpenCLErrorCode RetainCommandQueue(IntPtr pCommandQueue);

        [DllImport(LibName, EntryPoint = "clReleaseCommandQueue")]
        internal static extern OpenCLErrorCode ReleaseCommandQueue(IntPtr pCommandQueue);

        [DllImport(LibName, EntryPoint = "clCreateBuffer")]
        internal static extern IntPtr CreateBuffer(IntPtr pContext, OpenCLMemoryFlags Flags, IntPtr Size, IntPtr pHost, out OpenCLErrorCode ResultCode);

        [DllImport(LibName, EntryPoint = "clEnqueueReadBuffer")]
        internal static extern OpenCLErrorCode EnqueueReadBuffer(IntPtr pCommandQueue, IntPtr pBuffer, [MarshalAs(UnmanagedType.Bool)] bool BlockingRead,
                                                                 IntPtr Offset, IntPtr DestSize, IntPtr pDest,
                                                                 int num_events, [MarshalAs(UnmanagedType.LPArray)] IntPtr[] pEvents, [Out, MarshalAs(UnmanagedType.LPArray, SizeConst = 1)] IntPtr[] pNewEvents);

        //[DllImport(LibName, EntryPoint = "clCreateImage2D")]
        //internal static extern CLMemoryHandle CreateImage2D(
        //    IntPtr pContext,
        //    OpenCLMemoryFlags flags,
        //    ref ComputeImageFormat image_format,
        //    IntPtr image_width,
        //    IntPtr image_height,
        //    IntPtr image_row_pitch,
        //    IntPtr host_ptr,
        //    out OpenCLErrorCode ResultCode);

        //[DllImport(LibName, EntryPoint = "clCreateImage3D")]
        //internal static extern CLMemoryHandle CreateImage3D(
        //    IntPtr pContext,
        //    OpenCLMemoryFlags flags,
        //    ref ComputeImageFormat image_format,
        //    IntPtr image_width,
        //    IntPtr image_height,
        //    IntPtr image_depth,
        //    IntPtr image_row_pitch,
        //    IntPtr image_slice_pitch,
        //    IntPtr host_ptr,
        //    out OpenCLErrorCode ResultCode);

        //[DllImport(LibName, EntryPoint = "clRetainMemObject")]
        //internal static extern OpenCLErrorCode RetainMemObject(IntPtr pMemObj);

        [DllImport(LibName, EntryPoint = "clReleaseMemObject")]
        internal static extern OpenCLErrorCode ReleaseMemObject(IntPtr pMemObj);

        //[DllImport(LibName, EntryPoint = "clGetSupportedImageFormats")]
        //internal static extern OpenCLErrorCode GetSupportedImageFormats(
        //    IntPtr pContext,
        //    OpenCLMemoryFlags flags,
        //    OpenCLMemoryType image_type,
        //    int num_entries,
        //    [Out, MarshalAs(UnmanagedType.LPArray)] ComputeImageFormat[] image_formats,
        //    out int num_image_formats);

        //[DllImport(LibName, EntryPoint = "clGetMemObjectInfo")]
        //internal static extern OpenCLErrorCode GetMemObjectInfo(
        //    IntPtr pMemObj,
        //    OpenCLMemoryInfo param_name,
        //    IntPtr param_value_size,
        //    IntPtr param_value,
        //    out IntPtr param_value_size_ret);

        //[DllImport(LibName, EntryPoint = "clGetImageInfo")]
        //internal static extern OpenCLErrorCode GetImageInfo(
        //    CLMemoryHandle image,
        //    ComputeImageInfo param_name,
        //    IntPtr param_value_size,
        //    IntPtr param_value,
        //    out IntPtr param_value_size_ret);

        //[DllImport(LibName, EntryPoint = "clCreateSampler")]
        //internal static extern CLSamplerHandle CreateSampler(
        //    IntPtr pContext,
        //    [MarshalAs(UnmanagedType.Bool)] bool normalized_coords,
        //    ComputeImageAddressing addressing_mode,
        //    ComputeImageFiltering filter_mode,
        //    out OpenCLErrorCode ResultCode);

        //[DllImport(LibName, EntryPoint = "clRetainSampler")]
        //internal static extern OpenCLErrorCode RetainSampler(
        //    CLSamplerHandle sample);

        //[DllImport(LibName, EntryPoint = "clReleaseSampler")]
        //internal static extern OpenCLErrorCode ReleaseSampler(
        //    CLSamplerHandle sample);

        //[DllImport(LibName, EntryPoint = "clGetSamplerInfo")]
        //internal static extern OpenCLErrorCode GetSamplerInfo(
        //    CLSamplerHandle sample,
        //    ComputeSamplerInfo param_name,
        //    IntPtr param_value_size,
        //    IntPtr param_value,
        //    out IntPtr param_value_size_ret);


        //[DllImport(LibName, EntryPoint = "clWaitForEvents")]
        //internal static extern OpenCLErrorCode WaitForEvents(
        //    int num_events,
        //    [MarshalAs(UnmanagedType.LPArray)] CLEventHandle[] event_list);

        //[DllImport(LibName, EntryPoint = "clGetEventInfo")]
        //internal static extern OpenCLErrorCode GetEventInfo(
        //    CLEventHandle @event,
        //    ComputeEventInfo param_name,
        //    IntPtr param_value_size,
        //    IntPtr param_value,
        //    out IntPtr param_value_size_ret);

        //[DllImport(LibName, EntryPoint = "clRetainEvent")]
        //internal static extern OpenCLErrorCode RetainEvent(
        //    CLEventHandle @event);

        //[DllImport(LibName, EntryPoint = "clReleaseEvent")]
        //internal static extern OpenCLErrorCode ReleaseEvent(
        //    CLEventHandle @event);

        //[DllImport(LibName, EntryPoint = "clGetEventProfilingInfo")]
        //internal static extern OpenCLErrorCode GetEventProfilingInfo(
        //    CLEventHandle @event,
        //    ComputeCommandProfilingInfo param_name,
        //    IntPtr param_value_size,
        //    IntPtr param_value,
        //    out IntPtr param_value_size_ret);


        //[DllImport(LibName, EntryPoint = "clEnqueueWriteBuffer")]
        //internal static extern OpenCLErrorCode EnqueueWriteBuffer(
        //    IntPtr pCommandQueue,
        //    CLMemoryHandle buffer,
        //    [MarshalAs(UnmanagedType.Bool)] bool blocking_write,
        //    IntPtr offset,
        //    IntPtr cb,
        //    IntPtr ptr,
        //    int num_events_in_wait_list,
        //    [MarshalAs(UnmanagedType.LPArray)] CLEventHandle[] event_wait_list,
        //    [Out, MarshalAs(UnmanagedType.LPArray, SizeConst = 1)] CLEventHandle[] new_event);

        //[DllImport(LibName, EntryPoint = "clEnqueueCopyBuffer")]
        //internal static extern OpenCLErrorCode EnqueueCopyBuffer(
        //    IntPtr pCommandQueue,
        //    CLMemoryHandle src_buffer,
        //    CLMemoryHandle dst_buffer,
        //    IntPtr src_offset,
        //    IntPtr dst_offset,
        //    IntPtr cb,
        //    int num_events_in_wait_list,
        //    [MarshalAs(UnmanagedType.LPArray)] CLEventHandle[] event_wait_list,
        //    [Out, MarshalAs(UnmanagedType.LPArray, SizeConst = 1)] CLEventHandle[] new_event);

        //[DllImport(LibName, EntryPoint = "clEnqueueReadImage")]
        //internal static extern OpenCLErrorCode EnqueueReadImage(
        //    IntPtr pCommandQueue,
        //    CLMemoryHandle image,
        //    [MarshalAs(UnmanagedType.Bool)] bool blocking_read,
        //    ref SysIntX3 origin,
        //    ref SysIntX3 region,
        //    IntPtr row_pitch,
        //    IntPtr slice_pitch,
        //    IntPtr ptr,
        //    int num_events_in_wait_list,
        //    [MarshalAs(UnmanagedType.LPArray)] CLEventHandle[] event_wait_list,
        //    [Out, MarshalAs(UnmanagedType.LPArray, SizeConst = 1)] CLEventHandle[] new_event);

        //[DllImport(LibName, EntryPoint = "clEnqueueWriteImage")]
        //internal static extern OpenCLErrorCode EnqueueWriteImage(
        //    IntPtr pCommandQueue,
        //    CLMemoryHandle image,
        //    [MarshalAs(UnmanagedType.Bool)] bool blocking_write,
        //    ref SysIntX3 origin,
        //    ref SysIntX3 region,
        //    IntPtr input_row_pitch,
        //    IntPtr input_slice_pitch,
        //    IntPtr ptr,
        //    int num_events_in_wait_list,
        //    [MarshalAs(UnmanagedType.LPArray)] CLEventHandle[] event_wait_list,
        //    [Out, MarshalAs(UnmanagedType.LPArray, SizeConst = 1)] CLEventHandle[] new_event);

        //[DllImport(LibName, EntryPoint = "clEnqueueCopyImage")]
        //internal static extern OpenCLErrorCode EnqueueCopyImage(
        //    IntPtr pCommandQueue,
        //    CLMemoryHandle src_image,
        //    CLMemoryHandle dst_image,
        //    ref SysIntX3 src_origin,
        //    ref SysIntX3 dst_origin,
        //    ref SysIntX3 region,
        //    int num_events_in_wait_list,
        //    [MarshalAs(UnmanagedType.LPArray)] CLEventHandle[] event_wait_list,
        //    [Out, MarshalAs(UnmanagedType.LPArray, SizeConst = 1)] CLEventHandle[] new_event);

        //[DllImport(LibName, EntryPoint = "clEnqueueCopyImageToBuffer")]
        //internal static extern OpenCLErrorCode EnqueueCopyImageToBuffer(
        //    IntPtr pCommandQueue,
        //    CLMemoryHandle src_image,
        //    CLMemoryHandle dst_buffer,
        //    ref SysIntX3 src_origin,
        //    ref SysIntX3 region,
        //    IntPtr dst_offset,
        //    int num_events_in_wait_list,
        //    [MarshalAs(UnmanagedType.LPArray)] CLEventHandle[] event_wait_list,
        //    [Out, MarshalAs(UnmanagedType.LPArray, SizeConst = 1)] CLEventHandle[] new_event);

        //[DllImport(LibName, EntryPoint = "clEnqueueCopyBufferToImage")]
        //internal static extern OpenCLErrorCode EnqueueCopyBufferToImage(
        //    IntPtr pCommandQueue,
        //    CLMemoryHandle src_buffer,
        //    CLMemoryHandle dst_image,
        //    IntPtr src_offset,
        //    ref SysIntX3 dst_origin,
        //    ref SysIntX3 region,
        //    int num_events_in_wait_list,
        //    [MarshalAs(UnmanagedType.LPArray)] CLEventHandle[] event_wait_list,
        //    [Out, MarshalAs(UnmanagedType.LPArray, SizeConst = 1)] CLEventHandle[] new_event);

        //[DllImport(LibName, EntryPoint = "clEnqueueMapBuffer")]
        //internal static extern IntPtr EnqueueMapBuffer(
        //    IntPtr pCommandQueue,
        //    CLMemoryHandle buffer,
        //    [MarshalAs(UnmanagedType.Bool)] bool blocking_map,
        //    OpenCLMemoryMappingFlags map_flags,
        //    IntPtr offset,
        //    IntPtr cb,
        //    int num_events_in_wait_list,
        //    [MarshalAs(UnmanagedType.LPArray)] CLEventHandle[] event_wait_list,
        //    [Out, MarshalAs(UnmanagedType.LPArray, SizeConst = 1)] CLEventHandle[] new_event,
        //    out OpenCLErrorCode ResultCode);

        //[DllImport(LibName, EntryPoint = "clEnqueueMapImage")]
        //internal static extern IntPtr EnqueueMapImage(
        //    IntPtr pCommandQueue,
        //    CLMemoryHandle image,
        //    [MarshalAs(UnmanagedType.Bool)] bool blocking_map,
        //    OpenCLMemoryMappingFlags map_flags,
        //    ref SysIntX3 origin,
        //    ref SysIntX3 region,
        //    out IntPtr image_row_pitch,
        //    out IntPtr image_slice_pitch,
        //    int num_events_in_wait_list,
        //    [MarshalAs(UnmanagedType.LPArray)] CLEventHandle[] event_wait_list,
        //    [Out, MarshalAs(UnmanagedType.LPArray, SizeConst = 1)] CLEventHandle[] new_event,
        //    out OpenCLErrorCode ResultCode);

        //[DllImport(LibName, EntryPoint = "clEnqueueUnmapMemObject")]
        //internal static extern OpenCLErrorCode EnqueueUnmapMemObject(
        //    IntPtr pCommandQueue,
        //    IntPtr pMemObj,
        //    IntPtr mapped_ptr,
        //    int num_events_in_wait_list,
        //    [MarshalAs(UnmanagedType.LPArray)] CLEventHandle[] event_wait_list,
        //    [Out, MarshalAs(UnmanagedType.LPArray, SizeConst = 1)] CLEventHandle[] new_event);

        [DllImport(LibName, EntryPoint = "clEnqueueNDRangeKernel")]
        internal static extern OpenCLErrorCode EnqueueNDRangeKernel(IntPtr pCommandQueue, IntPtr pKernel,
                                                                    int work_dim,
                                                                    [MarshalAs(UnmanagedType.LPArray)] IntPtr[] global_work_offset,
                                                                    [MarshalAs(UnmanagedType.LPArray)] IntPtr[] global_work_size,
                                                                    [MarshalAs(UnmanagedType.LPArray)] IntPtr[] local_work_size,
                                                                    int num_events_in_wait_list,
                                                                    [MarshalAs(UnmanagedType.LPArray)] IntPtr[] event_wait_list,
                                                                    [Out, MarshalAs(UnmanagedType.LPArray, SizeConst = 1)] IntPtr[] new_event);

        //[DllImport(LibName, EntryPoint = "clEnqueueTask")]
        //internal static extern OpenCLErrorCode EnqueueTask(
        //    IntPtr pCommandQueue,
        //    IntPtr pKernel,
        //    int num_events_in_wait_list,
        //    [MarshalAs(UnmanagedType.LPArray)] CLEventHandle[] event_wait_list,
        //    [Out, MarshalAs(UnmanagedType.LPArray, SizeConst = 1)] CLEventHandle[] new_event);

        //[DllImport(LibName, EntryPoint = "clEnqueueNativeKernel")]
        //public extern static ComputeErrorCode EnqueueNativeKernel(
        //    IntPtr pCommandQueue,
        //    IntPtr user_func,
        //    IntPtr args,
        //    IntPtr cb_args,
        //    int num_mem_objects,
        //    IntPtr* mem_list,
        //    IntPtr* args_mem_loc,
        //    int num_events_in_wait_list,
        //    [MarshalAs(UnmanagedType.LPArray)] CLEventHandle[] event_wait_list,
        //    [Out, MarshalAs(UnmanagedType.LPArray, SizeConst=1)] CLEventHandle[] new_event);

        //[DllImport(LibName, EntryPoint = "clEnqueueMarker")]
        //internal static extern OpenCLErrorCode EnqueueMarker(
        //    IntPtr pCommandQueue,
        //    out CLEventHandle new_event);

        //[DllImport(LibName, EntryPoint = "clEnqueueWaitForEvents")]
        //internal static extern OpenCLErrorCode EnqueueWaitForEvents(
        //    IntPtr pCommandQueue,
        //    int num_events,
        //    [MarshalAs(UnmanagedType.LPArray)] CLEventHandle[] event_list);

        //[DllImport(LibName, EntryPoint = "clEnqueueBarrier")]
        //internal static extern OpenCLErrorCode EnqueueBarrier(
        //    IntPtr pCommandQueue);


        ///// <summary>
        ///// Gets the extension function address for the given function name,
        ///// or NULL if a valid function can not be found. The client must
        ///// check to make sure the address is not NULL, before using or
        ///// calling the returned function address.
        ///// </summary>
        //[DllImport(LibName, EntryPoint = "clGetExtensionFunctionAddress")]
        //internal static extern IntPtr GetExtensionFunctionAddress(
        //    String func_name);

        //// CL/GL Sharing API

        //[DllImport(LibName, EntryPoint = "clCreateFromGLBuffer")]
        //internal static extern CLMemoryHandle CreateFromGLBuffer(
        //    IntPtr pContext,
        //    OpenCLMemoryFlags flags,
        //    int bufobj,
        //    out OpenCLErrorCode ResultCode);

        //[DllImport(LibName, EntryPoint = "clCreateFromGLTexture2D")]
        //internal static extern CLMemoryHandle CreateFromGLTexture2D(
        //    IntPtr pContext,
        //    OpenCLMemoryFlags flags,
        //    int target,
        //    int miplevel,
        //    int texture,
        //    out OpenCLErrorCode ResultCode);

        //[DllImport(LibName, EntryPoint = "clCreateFromGLTexture3D")]
        //internal static extern CLMemoryHandle CreateFromGLTexture3D(
        //    IntPtr pContext,
        //    OpenCLMemoryFlags flags,
        //    int target,
        //    int miplevel,
        //    int texture,
        //    out OpenCLErrorCode ResultCode);

        //[DllImport(LibName, EntryPoint = "clCreateFromGLRenderbuffer")]
        //internal static extern CLMemoryHandle CreateFromGLRenderbuffer(
        //    IntPtr pContext,
        //    OpenCLMemoryFlags flags,
        //    int renderbuffer,
        //    out OpenCLErrorCode ResultCode);

        //[DllImport(LibName, EntryPoint = "clGetGLObjectInfo")]
        //internal static extern OpenCLErrorCode GetGLObjectInfo(
        //    IntPtr pMemObj,
        //    out ComputeGLObjectType gl_object_type,
        //    out int gl_object_name);

        //[DllImport(LibName, EntryPoint = "clGetGLTextureInfo")]
        //internal static extern OpenCLErrorCode GetGLTextureInfo(
        //    IntPtr pMemObj,
        //    ComputeGLTextureInfo param_name,
        //    IntPtr param_value_size,
        //    IntPtr param_value,
        //    out IntPtr param_value_size_ret);

        //[DllImport(LibName, EntryPoint = "clEnqueueAcquireGLObjects")]
        //internal static extern OpenCLErrorCode EnqueueAcquireGLObjects(
        //    IntPtr pCommandQueue,
        //    int num_objects,
        //    [MarshalAs(UnmanagedType.LPArray)] CLMemoryHandle[] mem_objects,
        //    int num_events_in_wait_list,
        //    [MarshalAs(UnmanagedType.LPArray)] CLEventHandle[] event_wait_list,
        //    [Out, MarshalAs(UnmanagedType.LPArray, SizeConst = 1)] CLEventHandle[] new_event);

        //[DllImport(LibName, EntryPoint = "clEnqueueReleaseGLObjects")]
        //internal static extern OpenCLErrorCode EnqueueReleaseGLObjects(
        //    IntPtr pCommandQueue,
        //    int num_objects,
        //    [MarshalAs(UnmanagedType.LPArray)] CLMemoryHandle[] mem_objects,
        //    int num_events_in_wait_list,
        //    [MarshalAs(UnmanagedType.LPArray)] CLEventHandle[] event_wait_list,
        //    [Out, MarshalAs(UnmanagedType.LPArray, SizeConst = 1)] CLEventHandle[] new_event);


        public static IEnumerable<OpenCLPlatform> Platforms => GetOpenCLPlatformInfos();
        private static IEnumerable<OpenCLPlatform> GetOpenCLPlatformInfos()
        {
            if (GetPlatformIDs(0, null, out int Count) == OpenCLErrorCode.Success)
            {
                IntPtr[] pPlatforms = new IntPtr[Count];

                if (GetPlatformIDs(Count, pPlatforms, out _) == OpenCLErrorCode.Success)
                    foreach (IntPtr pPlatform in pPlatforms)
                        yield return new OpenCLPlatform(pPlatform);
            }
        }

    }
}
