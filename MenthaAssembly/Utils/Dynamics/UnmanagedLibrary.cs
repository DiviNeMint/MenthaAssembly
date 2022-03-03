using MenthaAssembly.Win32;
using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using static MenthaAssembly.Win32.System;

namespace MenthaAssembly
{
    public unsafe sealed class UnmanagedLibrary : DynamicLibrary
    {
        #region Code Load
        //// https://github.com/dretax/DynamicDllLoader/blob/master/DynamicDLLLoader/DynamicDllLoader.cs
        //// https://github.com/fancycode/MemoryModule/blob/master/MemoryModule.c
        //// https://www.joachim-bauch.de/tutorials/loading-a-dll-from-memory/

        //#region Windows Struct
        //[StructLayout(LayoutKind.Sequential)]
        //private struct IMAGE_EXPORT_DIRECTORY
        //{
        //    public uint Characteristics;
        //    public uint TimeDateStamp;
        //    public ushort MajorVersion;
        //    public ushort MinorVersion;
        //    public uint Name;
        //    public uint Base;
        //    public uint NumberOfFunctions;
        //    public uint NumberOfNames;
        //    public int AddressOfFunctions;     // RVA from base of image
        //    public int AddressOfNames;         // RVA from base of image
        //    public int AddressOfNameOrdinals;  // RVA from base of image
        //}

        ////[StructLayout(LayoutKind.Sequential)]
        ////private struct IMAGE_IMPORT_BY_NAME
        ////{
        ////    public short Hint;
        ////    public byte Name;
        ////}

        ////[StructLayout(LayoutKind.Sequential)]
        ////private struct MemoryModule
        ////{
        ////    public IMAGE_NT_HEADERS Headers;
        ////    public IntPtr CodeBase;
        ////    public IntPtr Modules;
        ////    public int NumModules;
        ////    public int Initialized;
        ////}

        //[StructLayout(LayoutKind.Sequential)]
        //private struct IMAGE_BASE_RELOCATION
        //{
        //    public int VirtualAddress;
        //    public int SizeOfBlock;
        //}

        //[StructLayout(LayoutKind.Sequential)]
        //private struct IMAGE_IMPORT_DESCRIPTOR
        //{
        //    /// <summary>
        //    /// 0 for terminating null import descriptor; RVA to original unbound IAT (PIMAGE_THUNK_DATA)
        //    /// </summary>
        //    public int Characteristics;

        //    /// <summary>
        //    /// 0 if not bound, -1 if bound, and real date\time stamp in IMAGE_DIRECTORY_ENTRY_BOUND_IMPORT (new BIND); O.W. date/time stamp of DLL bound to (Old BIND)
        //    /// </summary>
        //    public uint TimeDateStamp;

        //    /// <summary>
        //    /// -1 if no forwarders
        //    /// </summary>
        //    public uint ForwarderChain;

        //    public int Name;

        //    /// <summary>
        //    /// RVA to IAT (if bound this IAT has actual addresses)
        //    /// </summary>
        //    public int FirstThunk;
        //}

        ////[StructLayout(LayoutKind.Sequential, Pack = 4)]
        ////private struct IMAGE_SECTION_HEADER
        ////{
        ////    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        ////    public byte[] Name;

        ////    //union 
        ////    //{    
        ////    //    DWORD PhysicalAddress;    
        ////    //    DWORD VirtualSize;  
        ////    //} Misc;  
        ////    public uint PhysicalAddress;

        ////    //public uint VirtualSize;
        ////    public uint VirtualAddress;
        ////    public int SizeOfRawData;
        ////    public uint PointerToRawData;
        ////    public uint PointerToRelocations;
        ////    public uint PointerToLinenumbers;
        ////    public short NumberOfRelocations;
        ////    public short NumberOfLinenumbers;
        ////    public uint Characteristics;
        ////}

        //[StructLayout(LayoutKind.Explicit)]
        //public struct IMAGE_SECTION_HEADER
        //{
        //    [FieldOffset(0)]
        //    public fixed byte Name[8];

        //    [FieldOffset(8)]
        //    public uint VirtualSize;

        //    [FieldOffset(8)]
        //    public uint PhysicalAddress;

        //    [FieldOffset(12)]
        //    public uint VirtualAddress;

        //    [FieldOffset(16)]
        //    public uint SizeOfRawData;

        //    [FieldOffset(20)]
        //    public uint PointerToRawData;

        //    [FieldOffset(24)]
        //    public uint PointerToRelocations;

        //    [FieldOffset(28)]
        //    public uint PointerToLinenumbers;

        //    [FieldOffset(32)]
        //    public ushort NumberOfRelocations;

        //    [FieldOffset(34)]
        //    public ushort NumberOfLinenumbers;

        //    [FieldOffset(36)]
        //    public DataSectionFlags Characteristics;

        //}

        //[Flags]
        //public enum DataSectionFlags : uint
        //{
        //    /// <summary>
        //    /// Reserved for future use.
        //    /// </summary>
        //    TypeReg = 0x00000000,
        //    /// <summary>
        //    /// Reserved for future use.
        //    /// </summary>
        //    TypeDsect = 0x00000001,
        //    /// <summary>
        //    /// Reserved for future use.
        //    /// </summary>
        //    TypeNoLoad = 0x00000002,
        //    /// <summary>
        //    /// Reserved for future use.
        //    /// </summary>
        //    TypeGroup = 0x00000004,
        //    /// <summary>
        //    /// The section should not be padded to the next boundary. This flag is obsolete and is replaced by IMAGE_SCN_ALIGN_1BYTES. This is valid only for object files.
        //    /// </summary>
        //    TypeNoPadded = 0x00000008,
        //    /// <summary>
        //    /// Reserved for future use.
        //    /// </summary>
        //    TypeCopy = 0x00000010,
        //    /// <summary>
        //    /// The section contains executable code.
        //    /// </summary>
        //    ContentCode = 0x00000020,
        //    /// <summary>
        //    /// The section contains initialized data.
        //    /// </summary>
        //    ContentInitializedData = 0x00000040,
        //    /// <summary>
        //    /// The section contains uninitialized data.
        //    /// </summary>
        //    ContentUninitializedData = 0x00000080,
        //    /// <summary>
        //    /// Reserved for future use.
        //    /// </summary>
        //    LinkOther = 0x00000100,
        //    /// <summary>
        //    /// The section contains comments or other information. The .drectve section has this type. This is valid for object files only.
        //    /// </summary>
        //    LinkInfo = 0x00000200,
        //    /// <summary>
        //    /// Reserved for future use.
        //    /// </summary>
        //    TypeOver = 0x00000400,
        //    /// <summary>
        //    /// The section will not become part of the image. This is valid only for object files.
        //    /// </summary>
        //    LinkRemove = 0x00000800,
        //    /// <summary>
        //    /// The section contains COMDAT data. For more information, see section 5.5.6, COMDAT Sections (Object Only). This is valid only for object files.
        //    /// </summary>
        //    LinkComDat = 0x00001000,
        //    /// <summary>
        //    /// Reset speculative exceptions handling bits in the TLB entries for this section.
        //    /// </summary>
        //    NoDeferSpecExceptions = 0x00004000,
        //    /// <summary>
        //    /// The section contains data referenced through the global pointer (GP).
        //    /// </summary>
        //    RelativeGP = 0x00008000,
        //    /// <summary>
        //    /// Reserved for future use.
        //    /// </summary>
        //    MemPurgeable = 0x00020000,
        //    /// <summary>
        //    /// Reserved for future use.
        //    /// </summary>
        //    Memory16Bit = 0x00020000,
        //    /// <summary>
        //    /// Reserved for future use.
        //    /// </summary>
        //    MemoryLocked = 0x00040000,
        //    /// <summary>
        //    /// Reserved for future use.
        //    /// </summary>
        //    MemoryPreload = 0x00080000,
        //    /// <summary>
        //    /// Align data on a 1-byte boundary. Valid only for object files.
        //    /// </summary>
        //    Align1Bytes = 0x00100000,
        //    /// <summary>
        //    /// Align data on a 2-byte boundary. Valid only for object files.
        //    /// </summary>
        //    Align2Bytes = 0x00200000,
        //    /// <summary>
        //    /// Align data on a 4-byte boundary. Valid only for object files.
        //    /// </summary>
        //    Align4Bytes = 0x00300000,
        //    /// <summary>
        //    /// Align data on an 8-byte boundary. Valid only for object files.
        //    /// </summary>
        //    Align8Bytes = 0x00400000,
        //    /// <summary>
        //    /// Align data on a 16-byte boundary. Valid only for object files.
        //    /// </summary>
        //    Align16Bytes = 0x00500000,
        //    /// <summary>
        //    /// Align data on a 32-byte boundary. Valid only for object files.
        //    /// </summary>
        //    Align32Bytes = 0x00600000,
        //    /// <summary>
        //    /// Align data on a 64-byte boundary. Valid only for object files.
        //    /// </summary>
        //    Align64Bytes = 0x00700000,
        //    /// <summary>
        //    /// Align data on a 128-byte boundary. Valid only for object files.
        //    /// </summary>
        //    Align128Bytes = 0x00800000,
        //    /// <summary>
        //    /// Align data on a 256-byte boundary. Valid only for object files.
        //    /// </summary>
        //    Align256Bytes = 0x00900000,
        //    /// <summary>
        //    /// Align data on a 512-byte boundary. Valid only for object files.
        //    /// </summary>
        //    Align512Bytes = 0x00A00000,
        //    /// <summary>
        //    /// Align data on a 1024-byte boundary. Valid only for object files.
        //    /// </summary>
        //    Align1024Bytes = 0x00B00000,
        //    /// <summary>
        //    /// Align data on a 2048-byte boundary. Valid only for object files.
        //    /// </summary>
        //    Align2048Bytes = 0x00C00000,
        //    /// <summary>
        //    /// Align data on a 4096-byte boundary. Valid only for object files.
        //    /// </summary>
        //    Align4096Bytes = 0x00D00000,
        //    /// <summary>
        //    /// Align data on an 8192-byte boundary. Valid only for object files.
        //    /// </summary>
        //    Align8192Bytes = 0x00E00000,
        //    /// <summary>
        //    /// The section contains extended relocations.
        //    /// </summary>
        //    LinkExtendedRelocationOverflow = 0x01000000,
        //    /// <summary>
        //    /// The section can be discarded as needed.
        //    /// </summary>
        //    MemoryDiscardable = 0x02000000,
        //    /// <summary>
        //    /// The section cannot be cached.
        //    /// </summary>
        //    MemoryNotCached = 0x04000000,
        //    /// <summary>
        //    /// The section is not pageable.
        //    /// </summary>
        //    MemoryNotPaged = 0x08000000,
        //    /// <summary>
        //    /// The section can be shared in memory.
        //    /// </summary>
        //    MemoryShared = 0x10000000,
        //    /// <summary>
        //    /// The section can be executed as code.
        //    /// </summary>
        //    MemoryExecute = 0x20000000,
        //    /// <summary>
        //    /// The section can be read.
        //    /// </summary>
        //    MemoryRead = 0x40000000,
        //    /// <summary>
        //    /// The section can be written to.
        //    /// </summary>
        //    MemoryWrite = 0x80000000
        //}

        //[StructLayout(LayoutKind.Sequential, Pack = 4)]
        //public struct IMAGE_DOS_HEADER
        //{
        //    public ushort e_magic; // Magic number
        //    public ushort e_cblp; // bytes on last page of file
        //    public ushort e_cp; // Pages in file
        //    public ushort e_crlc; // Relocations
        //    public ushort e_cparhdr; // Size of header in paragraphs
        //    public ushort e_minalloc; // Minimum extra paragraphs needed
        //    public ushort e_maxalloc; // Maximum extra paragraphs needed
        //    public ushort e_ss; // Initial (relative) SS value
        //    public ushort e_sp; // Initial SP value
        //    public ushort e_csum; // Checksum
        //    public ushort e_ip; // Initial IP value
        //    public ushort e_cs; // Initial (relative) CS value
        //    public ushort e_lfarlc; // File address of relocation table
        //    public ushort e_ovno; // Overlay number

        //    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        //    public ushort[] e_res1; // Reserved words

        //    public ushort e_oemid; // OEM identifier (for e_oeminfo)
        //    public ushort e_oeminfo; // OEM information; e_oemid specific

        //    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        //    public ushort[] e_res2; // Reserved words

        //    public int e_lfanew; // File address of new exe header
        //}

        ////[StructLayout(LayoutKind.Sequential)]
        ////public struct IMAGE_NT_HEADERS
        ////{
        ////    public uint Signature;
        ////    public IMAGE_FILE_HEADER FileHeader;
        ////    public IMAGE_OPTIONAL_HEADER32 OptionalHeader;
        ////}

        ////[StructLayout(LayoutKind.Sequential, Pack = 4)]
        ////public struct IMAGE_OPTIONAL_HEADER32
        ////{

        ////    Standard fields.


        ////    public ushort Magic;
        ////    public byte MajorLinkerVersion;
        ////    public byte MinorLinkerVersion;
        ////    public int SizeOfCode;
        ////    public int SizeOfInitializedData;
        ////    public int SizeOfUninitializedData;
        ////    public int AddressOfEntryPoint;
        ////    public uint BaseOfCode;

        ////    public uint BaseOfData;


        ////    NT additional fields.


        ////    public uint ImageBase;
        ////    public int SectionAlignment;
        ////    public uint FileAlignment;
        ////    public ushort MajorOperatingSystemVersion;
        ////    public ushort MinorOperatingSystemVersion;
        ////    public ushort MajorImageVersion;
        ////    public ushort MinorImageVersion;
        ////    public ushort MajorSubsystemVersion;
        ////    public ushort MinorSubsystemVersion;
        ////    public uint Win32VersionValue;
        ////    public int SizeOfImage;
        ////    public int SizeOfHeaders;
        ////    public uint CheckSum;
        ////    public ushort Subsystem;
        ////    public ushort DllCharacteristics;
        ////    public uint SizeOfStackReserve;
        ////    public uint SizeOfStackCommit;
        ////    public uint SizeOfHeapReserve;
        ////    public uint SizeOfHeapCommit;
        ////    public uint LoaderFlags;
        ////    public uint NumberOfRvaAndSizes;

        ////    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        ////    public IMAGE_DATA_DIRECTORY[] DataDirectory;
        ////}

        //[StructLayout(LayoutKind.Explicit)]
        //public struct IMAGE_OPTIONAL_HEADER32
        //{
        //    [FieldOffset(0)]
        //    public MagicType Magic;

        //    [FieldOffset(2)]
        //    public byte MajorLinkerVersion;

        //    [FieldOffset(3)]
        //    public byte MinorLinkerVersion;

        //    [FieldOffset(4)]
        //    public uint SizeOfCode;

        //    [FieldOffset(8)]
        //    public uint SizeOfInitializedData;

        //    [FieldOffset(12)]
        //    public uint SizeOfUninitializedData;

        //    [FieldOffset(16)]
        //    public uint AddressOfEntryPoint;

        //    [FieldOffset(20)]
        //    public uint BaseOfCode;

        //    // PE32 contains this additional field
        //    [FieldOffset(24)]
        //    public uint BaseOfData;

        //    [FieldOffset(28)]
        //    public uint ImageBase;

        //    [FieldOffset(32)]
        //    public uint SectionAlignment;

        //    [FieldOffset(36)]
        //    public uint FileAlignment;

        //    [FieldOffset(40)]
        //    public ushort MajorOperatingSystemVersion;

        //    [FieldOffset(42)]
        //    public ushort MinorOperatingSystemVersion;

        //    [FieldOffset(44)]
        //    public ushort MajorImageVersion;

        //    [FieldOffset(46)]
        //    public ushort MinorImageVersion;

        //    [FieldOffset(48)]
        //    public ushort MajorSubsystemVersion;

        //    [FieldOffset(50)]
        //    public ushort MinorSubsystemVersion;

        //    [FieldOffset(52)]
        //    public uint Win32VersionValue;

        //    [FieldOffset(56)]
        //    public uint SizeOfImage;

        //    [FieldOffset(60)]
        //    public uint SizeOfHeaders;

        //    [FieldOffset(64)]
        //    public uint CheckSum;

        //    [FieldOffset(68)]
        //    public SubSystemType Subsystem;

        //    [FieldOffset(70)]
        //    public DllCharacteristicsType DllCharacteristics;

        //    [FieldOffset(72)]
        //    public uint SizeOfStackReserve;

        //    [FieldOffset(76)]
        //    public uint SizeOfStackCommit;

        //    [FieldOffset(80)]
        //    public uint SizeOfHeapReserve;

        //    [FieldOffset(84)]
        //    public uint SizeOfHeapCommit;

        //    [FieldOffset(88)]
        //    public uint LoaderFlags;

        //    [FieldOffset(92)]
        //    public uint NumberOfRvaAndSizes;

        //    [FieldOffset(96)]
        //    public IMAGE_DATA_DIRECTORY ExportTable;

        //    [FieldOffset(104)]
        //    public IMAGE_DATA_DIRECTORY ImportTable;

        //    [FieldOffset(112)]
        //    public IMAGE_DATA_DIRECTORY ResourceTable;

        //    [FieldOffset(120)]
        //    public IMAGE_DATA_DIRECTORY ExceptionTable;

        //    [FieldOffset(128)]
        //    public IMAGE_DATA_DIRECTORY CertificateTable;

        //    [FieldOffset(136)]
        //    public IMAGE_DATA_DIRECTORY BaseRelocationTable;

        //    [FieldOffset(144)]
        //    public IMAGE_DATA_DIRECTORY Debug;

        //    [FieldOffset(152)]
        //    public IMAGE_DATA_DIRECTORY Architecture;

        //    [FieldOffset(160)]
        //    public IMAGE_DATA_DIRECTORY GlobalPtr;

        //    [FieldOffset(168)]
        //    public IMAGE_DATA_DIRECTORY TLSTable;

        //    [FieldOffset(176)]
        //    public IMAGE_DATA_DIRECTORY LoadConfigTable;

        //    [FieldOffset(184)]
        //    public IMAGE_DATA_DIRECTORY BoundImport;

        //    [FieldOffset(192)]
        //    public IMAGE_DATA_DIRECTORY IAT;

        //    [FieldOffset(200)]
        //    public IMAGE_DATA_DIRECTORY DelayImportDescriptor;

        //    [FieldOffset(208)]
        //    public IMAGE_DATA_DIRECTORY CLRRuntimeHeader;

        //    [FieldOffset(216)]
        //    public IMAGE_DATA_DIRECTORY Reserved;
        //}

        //[StructLayout(LayoutKind.Explicit)]
        //public struct IMAGE_OPTIONAL_HEADER64
        //{
        //    [FieldOffset(0)]
        //    public MachineType Magic;

        //    [FieldOffset(2)]
        //    public byte MajorLinkerVersion;

        //    [FieldOffset(3)]
        //    public byte MinorLinkerVersion;

        //    [FieldOffset(4)]
        //    public uint SizeOfCode;

        //    [FieldOffset(8)]
        //    public uint SizeOfInitializedData;

        //    [FieldOffset(12)]
        //    public uint SizeOfUninitializedData;

        //    [FieldOffset(16)]
        //    public uint AddressOfEntryPoint;

        //    [FieldOffset(20)]
        //    public uint BaseOfCode;

        //    [FieldOffset(24)]
        //    public ulong ImageBase;

        //    [FieldOffset(32)]
        //    public uint SectionAlignment;

        //    [FieldOffset(36)]
        //    public uint FileAlignment;

        //    [FieldOffset(40)]
        //    public ushort MajorOperatingSystemVersion;

        //    [FieldOffset(42)]
        //    public ushort MinorOperatingSystemVersion;

        //    [FieldOffset(44)]
        //    public ushort MajorImageVersion;

        //    [FieldOffset(46)]
        //    public ushort MinorImageVersion;

        //    [FieldOffset(48)]
        //    public ushort MajorSubsystemVersion;

        //    [FieldOffset(50)]
        //    public ushort MinorSubsystemVersion;

        //    [FieldOffset(52)]
        //    public uint Win32VersionValue;

        //    [FieldOffset(56)]
        //    public uint SizeOfImage;

        //    [FieldOffset(60)]
        //    public uint SizeOfHeaders;

        //    [FieldOffset(64)]
        //    public uint CheckSum;

        //    [FieldOffset(68)]
        //    public SubSystemType Subsystem;

        //    [FieldOffset(70)]
        //    public DllCharacteristicsType DllCharacteristics;

        //    [FieldOffset(72)]
        //    public ulong SizeOfStackReserve;

        //    [FieldOffset(80)]
        //    public ulong SizeOfStackCommit;

        //    [FieldOffset(88)]
        //    public ulong SizeOfHeapReserve;

        //    [FieldOffset(96)]
        //    public ulong SizeOfHeapCommit;

        //    [FieldOffset(104)]
        //    public uint LoaderFlags;

        //    [FieldOffset(108)]
        //    public uint NumberOfRvaAndSizes;

        //    [FieldOffset(112)]
        //    public IMAGE_DATA_DIRECTORY ExportTable;

        //    [FieldOffset(120)]
        //    public IMAGE_DATA_DIRECTORY ImportTable;

        //    [FieldOffset(128)]
        //    public IMAGE_DATA_DIRECTORY ResourceTable;

        //    [FieldOffset(136)]
        //    public IMAGE_DATA_DIRECTORY ExceptionTable;

        //    [FieldOffset(144)]
        //    public IMAGE_DATA_DIRECTORY CertificateTable;

        //    [FieldOffset(152)]
        //    public IMAGE_DATA_DIRECTORY BaseRelocationTable;

        //    [FieldOffset(160)]
        //    public IMAGE_DATA_DIRECTORY Debug;

        //    [FieldOffset(168)]
        //    public IMAGE_DATA_DIRECTORY Architecture;

        //    [FieldOffset(176)]
        //    public IMAGE_DATA_DIRECTORY GlobalPtr;

        //    [FieldOffset(184)]
        //    public IMAGE_DATA_DIRECTORY TLSTable;

        //    [FieldOffset(192)]
        //    public IMAGE_DATA_DIRECTORY LoadConfigTable;

        //    [FieldOffset(200)]
        //    public IMAGE_DATA_DIRECTORY BoundImport;

        //    [FieldOffset(208)]
        //    public IMAGE_DATA_DIRECTORY IAT;

        //    [FieldOffset(216)]
        //    public IMAGE_DATA_DIRECTORY DelayImportDescriptor;

        //    [FieldOffset(224)]
        //    public IMAGE_DATA_DIRECTORY CLRRuntimeHeader;

        //    [FieldOffset(232)]
        //    public IMAGE_DATA_DIRECTORY Reserved;
        //}

        //public enum MachineType : ushort
        //{
        //    /// <summary>
        //    /// The content of this field is assumed to be applicable to any machine type
        //    /// </summary>
        //    Unknown = 0x0000,
        //    /// <summary>
        //    /// Intel 386 or later processors and compatible processors
        //    /// </summary>
        //    I386 = 0x014c,
        //    R3000 = 0x0162,
        //    /// <summary>
        //    ///  MIPS little endian
        //    /// </summary>
        //    R4000 = 0x0166,
        //    R10000 = 0x0168,
        //    /// <summary>
        //    /// MIPS little-endian WCE v2
        //    /// </summary>
        //    WCEMIPSV2 = 0x0169,
        //    /// <summary>
        //    /// Alpha AXP
        //    /// </summary>
        //    Alpha = 0x0184,
        //    /// <summary>
        //    /// Hitachi SH3
        //    /// </summary>
        //    SH3 = 0x01a2,
        //    /// <summary>
        //    /// Hitachi SH3 DSP
        //    /// </summary>
        //    SH3DSP = 0x01a3,
        //    /// <summary>
        //    /// Hitachi SH4
        //    /// </summary>
        //    SH4 = 0x01a6,
        //    /// <summary>
        //    /// Hitachi SH5
        //    /// </summary>
        //    SH5 = 0x01a8,
        //    /// <summary>
        //    /// ARM little endian
        //    /// </summary>
        //    ARM = 0x01c0,
        //    /// <summary>
        //    /// Thumb
        //    /// </summary>
        //    Thumb = 0x01c2,
        //    /// <summary>
        //    /// ARM Thumb-2 little endian
        //    /// </summary>
        //    ARMNT = 0x01c4,
        //    /// <summary>
        //    /// Matsushita AM33
        //    /// </summary>
        //    AM33 = 0x01d3,
        //    /// <summary>
        //    /// Power PC little endian
        //    /// </summary>
        //    PowerPC = 0x01f0,
        //    /// <summary>
        //    /// Power PC with floating point support
        //    /// </summary>
        //    PowerPCFP = 0x01f1,
        //    /// <summary>
        //    /// Intel Itanium processor family
        //    /// </summary>
        //    IA64 = 0x0200,
        //    /// <summary>
        //    /// MIPS16
        //    /// </summary>
        //    MIPS16 = 0x0266,
        //    /// <summary>
        //    /// Motorola 68000 series
        //    /// </summary>
        //    M68K = 0x0268,
        //    /// <summary>
        //    /// Alpha AXP 64-bit
        //    /// </summary>
        //    Alpha64 = 0x0284,
        //    /// <summary>
        //    /// MIPS with FPU
        //    /// </summary>
        //    MIPSFPU = 0x0366,
        //    /// <summary>
        //    /// MIPS16 with FPU
        //    /// </summary>
        //    MIPSFPU16 = 0x0466,
        //    /// <summary>
        //    /// EFI byte code
        //    /// </summary>
        //    EBC = 0x0ebc,
        //    /// <summary>
        //    /// RISC-V 32-bit address space
        //    /// </summary>
        //    RISCV32 = 0x5032,
        //    /// <summary>
        //    /// RISC-V 64-bit address space
        //    /// </summary>
        //    RISCV64 = 0x5064,
        //    /// <summary>
        //    /// RISC-V 128-bit address space
        //    /// </summary>
        //    RISCV128 = 0x5128,
        //    /// <summary>
        //    /// x64
        //    /// </summary>
        //    AMD64 = 0x8664,
        //    /// <summary>
        //    /// ARM64 little endian
        //    /// </summary>
        //    ARM64 = 0xaa64,
        //    /// <summary>
        //    /// LoongArch 32-bit processor family
        //    /// </summary>
        //    LoongArch32 = 0x6232,
        //    /// <summary>
        //    /// LoongArch 64-bit processor family
        //    /// </summary>
        //    LoongArch64 = 0x6264,
        //    /// <summary>
        //    /// Mitsubishi M32R little endian
        //    /// </summary>
        //    M32R = 0x9041
        //}
        //public enum MagicType : ushort
        //{
        //    HDR32_MAGIC = 0x10b,
        //    HDR64_MAGIC = 0x20b
        //}
        //public enum SubSystemType : ushort
        //{
        //    Unknown = 0,
        //    Native = 1,
        //    Windows_GUI = 2,
        //    Windows_CUI = 3,
        //    POSIX_CUI = 7,
        //    Windows_CE_GUI = 9,
        //    EFI_Application = 10,
        //    EFI_Boot_Service_Driver = 11,
        //    EFI_Runtime_Driver = 12,
        //    EFI_ROM = 13,
        //    XBOX = 14
        //}
        //public enum DllCharacteristicsType : ushort
        //{
        //    RES_0 = 0x0001,
        //    RES_1 = 0x0002,
        //    RES_2 = 0x0004,
        //    RES_3 = 0x0008,
        //    Dynamic_Base = 0x0040,
        //    Force_Integrity = 0x0080,
        //    NX_Compat = 0x0100,
        //    NO_Isolation = 0x0200,
        //    NO_SEH = 0x0400,
        //    NO_Bind = 0x0800,
        //    RES_4 = 0x1000,
        //    WDM_Driver = 0x2000,
        //    Terminal_Server_Aware = 0x8000
        //}

        //[StructLayout(LayoutKind.Sequential, Pack = 4)]
        //public struct IMAGE_DATA_DIRECTORY
        //{
        //    public int VirtualAddress;
        //    public int Size;
        //}

        //[StructLayout(LayoutKind.Sequential, Pack = 4)]
        //public struct IMAGE_FILE_HEADER
        //{
        //    public ushort Machine;
        //    public ushort NumberOfSections;
        //    public uint TimeDateStamp;
        //    public uint PointerToSymbolTable;
        //    public uint NumberOfSymbols;
        //    public ushort SizeOfOptionalHeader;
        //    public ushort Characteristics;
        //}

        //internal static class Win32Imports
        //{
        //    [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        //    public static extern uint GetProcAddress(IntPtr hModule, string procName);

        //    [DllImport("kernel32")]
        //    public static extern uint GetLastError();

        //    [DllImport("kernel32.dll")]
        //    public static extern IntPtr GetProcAddress(IntPtr module, IntPtr ordinal);

        //    [DllImport("kernel32.dll", SetLastError = true)]
        //    internal static extern bool VirtualProtect(IntPtr lpAddress, int dwSize, uint flNewProtect, out uint lpflOldProtect);
        //}

        //internal static class PointerHelpers
        //{
        //    public static T ToStruct<T>(byte[] data) where T : struct
        //    {
        //        unsafe
        //        {
        //            fixed (byte* p = &data[0])
        //            {
        //                return (T)Marshal.PtrToStructure(new IntPtr(p), typeof(T));
        //            }
        //        }
        //    }

        //    public static T ToStruct<T>(byte[] data, uint from) where T : struct
        //    {
        //        unsafe
        //        {
        //            fixed (byte* p = &data[from])
        //            {
        //                return (T)Marshal.PtrToStructure(new IntPtr(p), typeof(T));
        //            }
        //        }
        //    }

        //    public static T ToStruct<T>(IntPtr ptr, uint from) where T : struct
        //    {
        //        return (T)Marshal.PtrToStructure(ptr + (int)from, typeof(T));
        //    }
        //}

        //[UnmanagedFunctionPointer(CallingConvention.StdCall)]
        //unsafe delegate bool fnDllEntry(IntPtr instance, uint reason, IntPtr reserved);

        //#endregion

        //public static bool LoadLibrary(byte[] Data)
        //{
        //    fixed (byte* pData = &Data[0])
        //    {
        //        // DosHeader.e_lfanew
        //        int e_lfanew = *(int*)(pData + 60);

        //        // FileHeader
        //        IMAGE_FILE_HEADER* pFileHeader = (IMAGE_FILE_HEADER*)(pData + e_lfanew + 4);

        //        // Check if the file is Dll.
        //        if ((pFileHeader->Characteristics & 0x2000) == 0)
        //            return false;

        //        byte* pTemp = pData + e_lfanew + 24;
        //        IntPtr pImageBase;
        //        uint SizeOfImage,
        //             SizeOfHeaders,
        //             SectionAlignment;
        //        IMAGE_DATA_DIRECTORY ImportTable,
        //                             BaseRelocationTable;

        //        // Check the dll's Platform.
        //        ushort Platform = *(ushort*)pTemp;
        //        if (Platform == 0x10b)
        //        {
        //            // x86
        //            IMAGE_OPTIONAL_HEADER32* pOptionalHeader = (IMAGE_OPTIONAL_HEADER32*)pTemp;
        //            pImageBase = new IntPtr(pOptionalHeader->ImageBase);
        //            SizeOfImage = pOptionalHeader->SizeOfImage;
        //            SizeOfHeaders = pOptionalHeader->SizeOfHeaders;
        //            SectionAlignment = pOptionalHeader->SectionAlignment;
        //            ImportTable = pOptionalHeader->ImportTable;
        //            BaseRelocationTable = pOptionalHeader->BaseRelocationTable;
        //        }
        //        else if (Platform == 0x20b)
        //        {
        //            // x64
        //            IMAGE_OPTIONAL_HEADER64* pOptionalHeader = (IMAGE_OPTIONAL_HEADER64*)pTemp;
        //            pImageBase = new IntPtr((long)pOptionalHeader->ImageBase);
        //            SizeOfImage = pOptionalHeader->SizeOfImage;
        //            SizeOfHeaders = pOptionalHeader->SizeOfHeaders;
        //            SectionAlignment = pOptionalHeader->SectionAlignment;
        //            ImportTable = pOptionalHeader->ImportTable;
        //            BaseRelocationTable = pOptionalHeader->BaseRelocationTable;
        //        }
        //        else
        //        {
        //            // Unknown
        //            return false;
        //        }

        //        IntPtr Code = VirtualAlloc(pImageBase, SizeOfImage, MemAllocType.Reserve, MemProtectType.Page_ReadWrite);

        //        if (Code == IntPtr.Zero)
        //            Code = VirtualAlloc(Code, SizeOfImage, MemAllocType.Reserve, MemProtectType.Page_ReadWrite);

        //        IntPtr CodeBase = Code;

        //        Code = VirtualAlloc(Code, SizeOfImage, MemAllocType.Commit, MemProtectType.Page_ReadWrite);

        //        IntPtr headers = VirtualAlloc(Code, SizeOfHeaders, MemAllocType.Commit, MemProtectType.Page_ReadWrite);
        //        Marshal.Copy(Data, 0, headers, (int)(e_lfanew + SizeOfHeaders));


        //        bool Success = CopySections(Data,
        //                                    CodeBase,
        //                                    (IMAGE_SECTION_HEADER*)(headers + e_lfanew + 24 + pFileHeader->SizeOfOptionalHeader),
        //                                    pFileHeader->NumberOfSections,
        //                                    SectionAlignment);
        //        if (!Success)
        //            return false;

        //        long LocationDelta = Code.ToInt64() - pImageBase.ToInt64();
        //        if (LocationDelta != 0)
        //            PerformBaseRelocation(CodeBase, BaseRelocationTable, LocationDelta);

        //        BuildImportTable(CodeBase, ImportTable);


        //    }



        //    //fnDllEntry dllEntry;
        //    //IMAGE_DOS_HEADER dosHeader = PointerHelpers.ToStruct<IMAGE_DOS_HEADER>(Data);

        //    //IMAGE_NT_HEADERS oldHeader = PointerHelpers.ToStruct<IMAGE_NT_HEADERS>(Data, (uint)dosHeader.e_lfanew);

        //    //IntPtr code = VirtualAlloc(new IntPtr(oldHeader.OptionalHeader.ImageBase), oldHeader.OptionalHeader.SizeOfImage, MemAllocType.Reserve, MemProtectType.Page_ReadWrite);

        //    //if (code == IntPtr.Zero)
        //    //    code = VirtualAlloc(code, oldHeader.OptionalHeader.SizeOfImage, MemAllocType.Reserve, MemProtectType.Page_ReadWrite);

        //    //module = new MemoryModule { CodeBase = code, NumModules = 0, Modules = IntPtr.Zero, Initialized = 0 };

        //    //VirtualAlloc(code, oldHeader.OptionalHeader.SizeOfImage, MemAllocType.Commit, MemProtectType.Page_ReadWrite);

        //    //IntPtr headers = VirtualAlloc(code, oldHeader.OptionalHeader.SizeOfHeaders, MemAllocType.Commit, MemProtectType.Page_ReadWrite);

        //    //Marshal.Copy(Data, 0, headers, dosHeader.e_lfanew + oldHeader.OptionalHeader.SizeOfHeaders);

        //    //module.Headers = PointerHelpers.ToStruct<IMAGE_NT_HEADERS>(headers, (uint)dosHeader.e_lfanew);
        //    //module.Headers.OptionalHeader.ImageBase = (uint)code;

        //    //CopySections(Data, oldHeader, headers, dosHeader);

        //    //uint locationDelta = (uint)(code - (int)oldHeader.OptionalHeader.ImageBase);

        //    //if (locationDelta != 0)
        //    //    PerformBaseRelocation(locationDelta);

        //    //BuildImportTable();
        //    //FinalizeSections(headers, dosHeader, oldHeader);

        //    //try
        //    //{
        //    //    fnDllEntry dllEntry = (fnDllEntry)Marshal.GetDelegateForFunctionPointer(module.CodeBase + module.Headers.OptionalHeader.AddressOfEntryPoint, typeof(fnDllEntry));
        //    //    return dllEntry(code, 1, IntPtr.Zero);
        //    //}
        //    //catch
        //    //{
        //    //}

        //    return false;
        //}

        //private static bool CopySections(byte[] Datas, IntPtr CodeBase, IMAGE_SECTION_HEADER* Section, int NumberOfSections, uint SectionAlignment)
        //{
        //    for (int i = 0; i < NumberOfSections; i++, Section++)
        //    {
        //        IntPtr Dest;
        //        if (Section->SizeOfRawData == 0)
        //        {
        //            if (SectionAlignment > 0)
        //            {
        //                Dest = VirtualAlloc(CodeBase + (int)Section->VirtualAddress, SectionAlignment, MemAllocType.Commit, MemProtectType.Page_ReadWrite);

        //                if (Dest == IntPtr.Zero)
        //                    return false;

        //                // On 64bit systems we truncate to 32bit here but expand again later when "PhysicalAddress" is used.
        //                Section->PhysicalAddress = (uint)(Dest.ToInt64() & 0xFFFFFFFF);

        //                // Clear Datas
        //                byte* pDest = (byte*)Dest;
        //                for (int j = 0; j < SectionAlignment; j++)
        //                    *pDest++ = 0;

        //                //byte[] datazz = new byte[SectionAlignment];
        //                //Marshal.Copy(datazz, 0, dest, (int)SectionAlignment);
        //            }

        //            continue;
        //        }

        //        Dest = VirtualAlloc(CodeBase + (int)Section->VirtualAddress, Section->SizeOfRawData, MemAllocType.Commit, MemProtectType.Page_ReadWrite);

        //        if (Dest == IntPtr.Zero)
        //            return false;

        //        // On 64bit systems we truncate to 32bit here but expand again later when "PhysicalAddress" is used.
        //        Section->PhysicalAddress = (uint)(Dest.ToInt64() & 0xFFFFFFFF);

        //        // Fill Datas
        //        Marshal.Copy(Datas, (int)Section->PointerToRawData, Dest, (int)Section->SizeOfRawData);
        //    }

        //    return true;
        //}

        //private static void PerformBaseRelocation(IntPtr CodeBase, IMAGE_DATA_DIRECTORY Directory, long Delta)
        //{
        //    if (Directory.Size > 0)
        //    {
        //        IntPtr pRelocation0 = CodeBase + Directory.VirtualAddress;
        //        int SizeOfBase = sizeof(IMAGE_BASE_RELOCATION);// Marshal.SizeOf(typeof(IMAGE_BASE_RELOCATION));

        //        IMAGE_BASE_RELOCATION* pRelocation = (IMAGE_BASE_RELOCATION*)pRelocation0;
        //        while (pRelocation->VirtualAddress > 0)
        //        {
        //            IntPtr Dest = CodeBase + pRelocation->VirtualAddress;
        //            ushort* relInfo = (ushort*)(pRelocation0 + SizeOfBase);
        //            for (int i = 0; i < (pRelocation->SizeOfBlock - SizeOfBase) / 2; i++, relInfo++)
        //            {
        //                int type = *relInfo >> 12;

        //                // IMAGE_REL_BASED_ABSOLUTE
        //                // skip relocation
        //                if (type == 0)
        //                    break;

        //                // IMAGE_REL_BASED_HIGHLOW
        //                // change complete 32 bit address
        //                if (type == 3)
        //                {
        //                    uint* patchAddrHl = (uint*)(Dest + (*relInfo & 0xfff));
        //                    *patchAddrHl += (uint)Delta;
        //                }

        //                // IMAGE_REL_BASED_DIR64
        //                else if (type == 10)
        //                {
        //                    ulong* patchAddrHl = (ulong*)(Dest + (*relInfo & 0xfff));
        //                    *patchAddrHl += (ulong)Delta;
        //                }
        //            }

        //            pRelocation0 += pRelocation->SizeOfBlock;
        //            pRelocation = (IMAGE_BASE_RELOCATION*)pRelocation0;
        //        }
        //    }
        //}

        //private static bool BuildImportTable(IntPtr CodeBase, IMAGE_DATA_DIRECTORY Directory)
        //{
        //    //int ucount = GetModuleCount();
        //    //module.Modules = Marshal.AllocHGlobal((ucount) * sizeof(int));
        //    //int pcount = 0;
        //    if (Directory.Size > 0)
        //    {
        //        IMAGE_IMPORT_DESCRIPTOR* pImportDesc = (IMAGE_IMPORT_DESCRIPTOR*)(CodeBase + Directory.VirtualAddress);
        //        while (pImportDesc->Name > 0)
        //        {
        //            IntPtr str = CodeBase + pImportDesc->Name;
        //            string tmp = Marshal.PtrToStringAnsi(str);

        //            uint* thunkRef, funcRef;

        //            IntPtr handle = Win32.System.LoadLibrary(tmp);

        //            if (handle == IntPtr.Zero)
        //                return false;

        //            int Characteristics = pImportDesc->Characteristics,
        //                FirstThunk = pImportDesc->FirstThunk;
        //            if (Characteristics > 0)
        //            {
        //                IntPtr thunkRefAddr = CodeBase + Characteristics;
        //                thunkRef = (uint*)thunkRefAddr;
        //                funcRef = (uint*)(CodeBase + FirstThunk);
        //            }
        //            else
        //            {
        //                thunkRef = (uint*)(CodeBase + FirstThunk);
        //                funcRef = (uint*)(CodeBase + FirstThunk);
        //            }

        //            for (; *thunkRef > 0; thunkRef++, funcRef++)
        //            {
        //                if ((*thunkRef & 0x80000000) != 0)
        //                {
        //                    *funcRef = (uint)Win32Imports.GetProcAddress(handle, new IntPtr(*thunkRef & 0xffff));
        //                }
        //                else
        //                {
        //                    IntPtr str2 = CodeBase + (int)*thunkRef + 2;
        //                    string tmpaa = Marshal.PtrToStringAnsi(str2);
        //                    *funcRef = Win32Imports.GetProcAddress(handle, tmpaa);
        //                }

        //                if (*funcRef == 0)
        //                    return false;
        //            }

        //            //pcount++;
        //            //importDesc = PointerHelpers.ToStruct<IMAGE_IMPORT_DESCRIPTOR>(codeBase, directory.VirtualAddress + (uint)(Marshal.SizeOf(typeof(IMAGE_IMPORT_DESCRIPTOR)) * pcount));

        //            pImportDesc++;
        //        }
        //    }

        //    return true;
        //}

        //public static IntPtr GetProcAddress(IntPtr CodeBase, IMAGE_DATA_DIRECTORY Directory, string Name)
        //{
        //    if (Directory.Size == 0)
        //        return IntPtr.Zero;

        //    IMAGE_EXPORT_DIRECTORY* pExports = (IMAGE_EXPORT_DIRECTORY*)(CodeBase + Directory.VirtualAddress);

        //    int* nameRef = (int*)(CodeBase + pExports->AddressOfNames);
        //    ushort* ordinal = (ushort*)(CodeBase + pExports->AddressOfNameOrdinals);
        //    for (uint i = 0; i < pExports->NumberOfNames; i++, nameRef++, ordinal++)
        //    {
        //        IntPtr str = CodeBase + *nameRef;
        //        string tmp = Marshal.PtrToStringAnsi(str);

        //        if (tmp == Name)
        //            return CodeBase + *(int*)(CodeBase + pExports->AddressOfFunctions + *ordinal * 4);
        //    }

        //    return IntPtr.Zero;
        //}

        ////public int GetModuleCount()
        ////{
        ////    int count = 0;
        ////    IntPtr CodeBase = module.CodeBase;
        ////    IMAGE_DATA_DIRECTORY directory = module.Headers.OptionalHeader.DataDirectory[1];
        ////    if (directory.Size > 0)
        ////    {
        ////        IMAGE_IMPORT_DESCRIPTOR* pImportDesc = (IMAGE_IMPORT_DESCRIPTOR*)(CodeBase + directory.VirtualAddress);
        ////        while (pImportDesc->Name > 0)
        ////        {
        ////            IntPtr str = CodeBase + pImportDesc->Name;
        ////            string tmp = Marshal.PtrToStringAnsi(str);
        ////            IntPtr handle = Win32.System.LoadLibrary(tmp);

        ////            if (handle == IntPtr.Zero)
        ////                break;

        ////            //if (handle == -1)
        ////            //{
        ////            //    break;
        ////            //}

        ////            pImportDesc++;
        ////        }
        ////    }

        ////    return count;
        ////}

        ////static readonly int[][][] ProtectionFlags = new int[2][][];

        ////public void FinalizeSections(IntPtr headers, IMAGE_DOS_HEADER dosHeader, IMAGE_NT_HEADERS oldHeaders)
        ////{
        ////    ProtectionFlags[0] = new int[2][];
        ////    ProtectionFlags[1] = new int[2][];
        ////    ProtectionFlags[0][0] = new int[2];
        ////    ProtectionFlags[0][1] = new int[2];
        ////    ProtectionFlags[1][0] = new int[2];
        ////    ProtectionFlags[1][1] = new int[2];
        ////    ProtectionFlags[0][0][0] = 0x01;
        ////    ProtectionFlags[0][0][1] = 0x08;
        ////    ProtectionFlags[0][1][0] = 0x02;
        ////    ProtectionFlags[0][1][1] = 0x04;
        ////    ProtectionFlags[1][0][0] = 0x10;
        ////    ProtectionFlags[1][0][1] = 0x80;
        ////    ProtectionFlags[1][1][0] = 0x20;
        ////    ProtectionFlags[1][1][1] = 0x40;

        ////    IMAGE_SECTION_HEADER section = PointerHelpers.ToStruct<IMAGE_SECTION_HEADER>(headers,
        ////        (uint)(24 + dosHeader.e_lfanew + oldHeaders.FileHeader.SizeOfOptionalHeader));
        ////    for (int i = 0; i < module.Headers.FileHeader.NumberOfSections; i++)
        ////    {
        ////        //Console.WriteLine("Finalizing " + Encoding.UTF8.GetString(section.Name));
        ////        int executable = (section.Characteristics & 0x20000000) != 0 ? 1 : 0;
        ////        int readable = (section.Characteristics & 0x40000000) != 0 ? 1 : 0;
        ////        int writeable = (section.Characteristics & 0x80000000) != 0 ? 1 : 0;

        ////        if ((section.Characteristics & 0x02000000) > 0)
        ////        {
        ////            bool aa = VirtualFree(new IntPtr(section.PhysicalAddress), section.SizeOfRawData, MemFreeType.Decommit);
        ////            continue;
        ////        }

        ////        uint protect = (uint)ProtectionFlags[executable][readable][writeable];

        ////        if ((section.Characteristics & 0x04000000) > 0)
        ////            protect |= 0x200;
        ////        int size = section.SizeOfRawData;
        ////        if (size == 0)
        ////        {
        ////            if ((section.Characteristics & 0x00000040) > 0)
        ////                size = module.Headers.OptionalHeader.SizeOfInitializedData;
        ////            else if ((section.Characteristics & 0x00000080) > 0)
        ////                size = module.Headers.OptionalHeader.SizeOfUninitializedData;
        ////        }

        ////        if (size > 0)
        ////        {
        ////            if (!Win32Imports.VirtualProtect(new IntPtr(section.PhysicalAddress), section.SizeOfRawData, protect, out uint oldProtect))
        ////            {
        ////            }
        ////        }

        ////        section = PointerHelpers.ToStruct<IMAGE_SECTION_HEADER>(headers, (uint)(24 + dosHeader.e_lfanew + oldHeaders.FileHeader.SizeOfOptionalHeader + Marshal.SizeOf(typeof(IMAGE_SECTION_HEADER)) * (i + 1)));
        ////    }
        ////}

        ////private MemoryModule module;

        #endregion

        #region Windows API
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string ProcedureName);

        #endregion

        private readonly ConcurrentDictionary<string, Delegate> MethodInfos = new ConcurrentDictionary<string, Delegate>();

        private readonly SafeHandle pLibrary;
        internal UnmanagedLibrary(string Path, SafeHandle pLibrary, LibraryType Type) : base(Path, Type)
        {
            this.pLibrary = pLibrary;
        }

        public TDelegate GetMethod<TDelegate>(string FunctionName)
            where TDelegate : Delegate
        {
            if (MethodInfos.TryGetValue(FunctionName, out Delegate MethodBase) &&
                MethodBase is TDelegate Method)
                return Method;

            if (pLibrary.IsInvalid)
                return null;

            IntPtr pProc = GetProcAddress(pLibrary.DangerousGetHandle(), FunctionName);

            Method = Marshal.GetDelegateForFunctionPointer<TDelegate>(pProc);
            MethodInfos.AddOrUpdate(FunctionName, Method, (k, v) => Method);
            return Method;
        }

        public void Invoke<TDelegate>(string FunctionName, params object[] Args)
            where TDelegate : Delegate
            => GetMethod<TDelegate>(FunctionName).DynamicInvoke(Args);
        public TResult Invoke<TDelegate, TResult>(string FunctionName, params object[] Args)
            where TDelegate : Delegate
            => (TResult)GetMethod<TDelegate>(FunctionName).DynamicInvoke(Args);

        public override void Dispose()
        {
            pLibrary.Dispose();
            base.Dispose();
        }

    }
}