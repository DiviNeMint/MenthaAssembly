using MenthaAssembly.OpenCL.Primitives;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace MenthaAssembly.OpenCL
{
    public class OpenCLDevice
    {
        public IntPtr Handle { get; }

        public string Name { get; }

        public OpenCLPlatform Platform { get; }

        #region OpenCL 1.0
        public long AddressBits { get; }

        public bool Available { get; }

        public OpenCLCommandQueueFlags CommandQueueFlags { get; }

        public bool CompilerAvailable { get; }

        public string DriverVersion { get; }

        public bool EndianLittle { get; }

        public bool ErrorCorrectionSupport { get; }

        public OpenCLExecutionCapabilities ExecutionCapabilities { get; }

        public ReadOnlyCollection<string> Extensions { get; }

        public long GlobalMemoryCacheLineSize { get; }

        public long GlobalMemoryCacheSize { get; }

        public OpenCLMemoryCacheType GlobalMemoryCacheType { get; }

        public long GlobalMemorySize { get; }

        public long Image2DMaxHeight { get; }

        public long Image2DMaxWidth { get; }

        public long Image3DMaxDepth { get; }

        public long Image3DMaxHeight { get; }

        public long Image3DMaxWidth { get; }

        public bool ImageSupport { get; }

        public long LocalMemorySize { get; }

        public OpenCLLocalMemoryType LocalMemoryType { get; }

        public long MaxClockFrequency { get; }

        public long MaxComputeUnits { get; }

        public long MaxConstantArguments { get; }

        public long MaxConstantBufferSize { get; }

        public long MaxMemoryAllocationSize { get; }

        public long MaxParameterSize { get; }

        public long MaxReadImageArguments { get; }

        public long MaxSamplers { get; }

        public long MaxWorkGroupSize { get; }

        public long MaxWorkItemDimensions { get; }

        public ReadOnlyCollection<long> MaxWorkItemSizes { get; }

        public long MaxWriteImageArguments { get; }

        public long MemoryBaseAddressAlignment { get; }

        public long MinDataTypeAlignmentSize { get; }

        public long PreferredVectorWidthChar { get; }

        public long PreferredVectorWidthDouble
            => GetDeviceInfo<uint>(OpenCLDeviceProperty.PreferredVectorWidthDouble);

        public long PreferredVectorWidthFloat { get; }

        public long PreferredVectorWidthHalf
            => GetDeviceInfo<uint>(OpenCLDeviceProperty.PreferredVectorWidthHalf);

        public long PreferredVectorWidthInt { get; }

        public long PreferredVectorWidthLong { get; }

        public long PreferredVectorWidthShort { get; }

        public string Profile { get; }

        public long ProfilingTimerResolution { get; }

        public OpenCLSingleCapabilities SingleCapabilities { get; }

        public OpenCLDeviceTypes Type { get; }

        public string Vendor { get; }

        public long VendorId { get; }

        public Version Version
        {
            get
            {
                string[] verstring = VersionString.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                return new Version(verstring[1]);
            }
        }

        public string VersionString { get; }

        #endregion

        #region OpenCL 1.1
        public bool HostUnifiedMemory
            => GetDeviceInfo<bool>(OpenCLDeviceProperty.HostUnifiedMemory);

        public long NativeVectorWidthChar
            => GetDeviceInfo<long>(OpenCLDeviceProperty.NativeVectorWidthChar);

        public long NativeVectorWidthDouble
            => GetDeviceInfo<long>(OpenCLDeviceProperty.NativeVectorWidthDouble);

        public long NativeVectorWidthFloat
            => GetDeviceInfo<long>(OpenCLDeviceProperty.NativeVectorWidthFloat);

        public long NativeVectorWidthHalf
            => GetDeviceInfo<long>(OpenCLDeviceProperty.NativeVectorWidthHalf);

        public long NativeVectorWidthInt
            => GetDeviceInfo<long>(OpenCLDeviceProperty.NativeVectorWidthInt);

        public long NativeVectorWidthLong
            => GetDeviceInfo<long>(OpenCLDeviceProperty.NativeVectorWidthLong);

        public long NativeVectorWidthShort
            => GetDeviceInfo<long>(OpenCLDeviceProperty.NativeVectorWidthShort);

        public Version OpenCLCVersion
        {
            get
            {
                string[] verstring = OpenCLCVersionString.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                return new Version(verstring[2]);
            }
        }

        public string OpenCLCVersionString => GetDeviceInfo(OpenCLDeviceProperty.OpenCLCVersion, new byte[256]);

        #endregion

        public OpenCLDevice(OpenCLPlatform Platform, IntPtr pDevice)
        {
            this.Platform = Platform;
            this.Handle = pDevice;

            byte[] Buffer = new byte[512];

            AddressBits = GetDeviceInfo<uint>(OpenCLDeviceProperty.AddressBits);
            Available = GetDeviceInfo<bool>(OpenCLDeviceProperty.Available);
            CompilerAvailable = GetDeviceInfo<bool>(OpenCLDeviceProperty.CompilerAvailable);
            DriverVersion = GetDeviceInfo(OpenCLDeviceProperty.DriverVersion, Buffer);
            EndianLittle = GetDeviceInfo<bool>(OpenCLDeviceProperty.EndianLittle);
            ErrorCorrectionSupport = GetDeviceInfo<bool>(OpenCLDeviceProperty.ErrorCorrectionSupport);
            ExecutionCapabilities = (OpenCLExecutionCapabilities)GetDeviceInfo<long>(OpenCLDeviceProperty.ExecutionCapabilities);

            Extensions = new ReadOnlyCollection<string>(GetDeviceInfo(OpenCLDeviceProperty.Extensions, Buffer).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));

            GlobalMemoryCacheLineSize = GetDeviceInfo<uint>(OpenCLDeviceProperty.GlobalMemoryCachelineSize);
            GlobalMemoryCacheSize = (long)GetDeviceInfo<ulong>(OpenCLDeviceProperty.GlobalMemoryCacheSize);
            GlobalMemoryCacheType = (OpenCLMemoryCacheType)GetDeviceInfo<long>(OpenCLDeviceProperty.GlobalMemoryCacheType);
            GlobalMemorySize = (long)GetDeviceInfo<ulong>(OpenCLDeviceProperty.GlobalMemorySize);

            Image2DMaxHeight = (long)GetDeviceInfo<IntPtr>(OpenCLDeviceProperty.Image2DMaxHeight);
            Image2DMaxWidth = (long)GetDeviceInfo<IntPtr>(OpenCLDeviceProperty.Image2DMaxWidth);

            Image3DMaxDepth = (long)GetDeviceInfo<IntPtr>(OpenCLDeviceProperty.Image3DMaxDepth);
            Image3DMaxHeight = (long)GetDeviceInfo<IntPtr>(OpenCLDeviceProperty.Image3DMaxHeight);
            Image3DMaxWidth = (long)GetDeviceInfo<IntPtr>(OpenCLDeviceProperty.Image3DMaxWidth);

            ImageSupport = GetDeviceInfo<bool>(OpenCLDeviceProperty.ImageSupport);

            LocalMemorySize = (long)GetDeviceInfo<ulong>(OpenCLDeviceProperty.LocalMemorySize);
            LocalMemoryType = (OpenCLLocalMemoryType)GetDeviceInfo<long>(OpenCLDeviceProperty.LocalMemoryType);

            MaxClockFrequency = GetDeviceInfo<uint>(OpenCLDeviceProperty.MaxClockFrequency);
            MaxComputeUnits = GetDeviceInfo<uint>(OpenCLDeviceProperty.MaxComputeUnits);
            MaxConstantArguments = GetDeviceInfo<uint>(OpenCLDeviceProperty.MaxConstantArguments);
            MaxConstantBufferSize = (long)GetDeviceInfo<ulong>(OpenCLDeviceProperty.MaxConstantBufferSize);
            MaxMemoryAllocationSize = (long)GetDeviceInfo<ulong>(OpenCLDeviceProperty.MaxMemoryAllocationSize);
            MaxParameterSize = (long)GetDeviceInfo<IntPtr>(OpenCLDeviceProperty.MaxParameterSize);
            MaxReadImageArguments = GetDeviceInfo<uint>(OpenCLDeviceProperty.MaxReadImageArguments);
            MaxSamplers = GetDeviceInfo<uint>(OpenCLDeviceProperty.MaxSamplers);
            MaxWorkGroupSize = (long)GetDeviceInfo<IntPtr>(OpenCLDeviceProperty.MaxWorkGroupSize);
            MaxWorkItemDimensions = GetDeviceInfo<uint>(OpenCLDeviceProperty.MaxWorkItemDimensions);
            MaxWorkItemSizes = new ReadOnlyCollection<long>(GetArrayInfo(OpenCLDeviceProperty.MaxWorkItemSizes).Select(i => i.ToInt64()).ToArray());
            MaxWriteImageArguments = GetDeviceInfo<uint>(OpenCLDeviceProperty.MaxWriteImageArguments);
            MemoryBaseAddressAlignment = GetDeviceInfo<uint>(OpenCLDeviceProperty.MemoryBaseAddressAlignment);

#pragma warning disable CS0618 // 類型或成員已經過時
            MinDataTypeAlignmentSize = GetDeviceInfo<uint>(OpenCLDeviceProperty.MinDataTypeAlignmentSize);
#pragma warning restore CS0618 // 類型或成員已經過時

            Name = GetDeviceInfo(OpenCLDeviceProperty.Name, Buffer);

            PreferredVectorWidthChar = GetDeviceInfo<uint>(OpenCLDeviceProperty.PreferredVectorWidthChar);
            PreferredVectorWidthFloat = GetDeviceInfo<uint>(OpenCLDeviceProperty.PreferredVectorWidthFloat);
            PreferredVectorWidthInt = GetDeviceInfo<uint>(OpenCLDeviceProperty.PreferredVectorWidthInt);
            PreferredVectorWidthLong = GetDeviceInfo<uint>(OpenCLDeviceProperty.PreferredVectorWidthLong);
            PreferredVectorWidthShort = GetDeviceInfo<uint>(OpenCLDeviceProperty.PreferredVectorWidthShort);
            Profile = GetDeviceInfo(OpenCLDeviceProperty.Profile, Buffer);
            ProfilingTimerResolution = (long)GetDeviceInfo<IntPtr>(OpenCLDeviceProperty.ProfilingTimerResolution);
            CommandQueueFlags = (OpenCLCommandQueueFlags)GetDeviceInfo<long>(OpenCLDeviceProperty.CommandQueueProperties);
            SingleCapabilities = (OpenCLSingleCapabilities)GetDeviceInfo<long>(OpenCLDeviceProperty.SingleFPConfig);
            Type = (OpenCLDeviceTypes)GetDeviceInfo<long>(OpenCLDeviceProperty.Type);
            Vendor = GetDeviceInfo(OpenCLDeviceProperty.Vendor, Buffer);
            VendorId = GetDeviceInfo<uint>(OpenCLDeviceProperty.VendorId);
            VersionString = GetDeviceInfo(OpenCLDeviceProperty.Version, Buffer);
        }

        private string GetDeviceInfo(OpenCLDeviceProperty Flag, byte[] Buffer)
        {
            OpenCLCore.GetDeviceInfo(this.Handle, Flag, Buffer.Length, Buffer, out int ReadSize);
            return Encoding.ASCII.GetString(Buffer, 0, ReadSize).Trim('\0');
        }

        private unsafe T GetDeviceInfo<T>(OpenCLDeviceProperty Flag)
             where T : unmanaged
        {
            T Result = default;
            T* pResult = &Result;

            if (Result is bool)
            {
                *(bool*)pResult = GetDeviceInfo<int>(Flag) == 1;
                return Result;
            }

            OpenCLCore.GetDeviceInfo(this.Handle, Flag, sizeof(T), (IntPtr)pResult, out _);
            return Result;
        }

        private unsafe IntPtr[] GetArrayInfo(OpenCLDeviceProperty Flag)
        {
            if (OpenCLCore.GetDeviceInfo(this.Handle, Flag, 0, IntPtr.Zero, out int Length) == OpenCLErrorCode.Success)
            {
                IntPtr[] Buffer = new IntPtr[Length];
                GCHandle gcHandle = GCHandle.Alloc(Buffer, GCHandleType.Pinned);
                try
                {
                    if (OpenCLCore.GetDeviceInfo(this.Handle, Flag, Length, gcHandle.AddrOfPinnedObject(), out _) == OpenCLErrorCode.Success)
                        return Buffer;
                }
                finally
                {
                    gcHandle.Free();
                }
            }

            return new IntPtr[0];
        }

    }

}
