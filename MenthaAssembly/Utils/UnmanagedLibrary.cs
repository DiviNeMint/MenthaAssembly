//using MenthaAssembly.Win32;
//using System;
//using System.Runtime.InteropServices;
//using static MenthaAssembly.Win32.System;

//namespace MenthaAssembly
//{
//    // https://github.com/dretax/DynamicDllLoader/blob/master/DynamicDLLLoader/DynamicDllLoader.cs
//    // https://www.joachim-bauch.de/tutorials/loading-a-dll-from-memory/
//    public unsafe class UnmanagedLibrary
//    {
//        [StructLayout(LayoutKind.Sequential)]
//        private struct IMAGE_EXPORT_DIRECTORY
//        {
//            public uint Characteristics;
//            public uint TimeDateStamp;
//            public ushort MajorVersion;
//            public ushort MinorVersion;
//            public uint Name;
//            public uint Base;
//            public uint NumberOfFunctions;
//            public uint NumberOfNames;
//            public int AddressOfFunctions;     // RVA from base of image
//            public int AddressOfNames;         // RVA from base of image
//            public int AddressOfNameOrdinals;  // RVA from base of image
//        }

//        //[StructLayout(LayoutKind.Sequential)]
//        //private struct IMAGE_IMPORT_BY_NAME
//        //{
//        //    public short Hint;
//        //    public byte Name;
//        //}

//        [StructLayout(LayoutKind.Sequential)]
//        private struct MemoryModule
//        {
//            public IMAGE_NT_HEADERS Headers;
//            public IntPtr CodeBase;
//            public IntPtr Modules;
//            public int NumModules;
//            public int Initialized;
//        }

//        [StructLayout(LayoutKind.Sequential)]
//        private struct IMAGE_BASE_RELOCATION
//        {
//            public int VirtualAddress;
//            public int SizeOfBlock;
//        }

//        [StructLayout(LayoutKind.Sequential)]
//        private struct IMAGE_IMPORT_DESCRIPTOR
//        {
//            /// <summary>
//            /// 0 for terminating null import descriptor; RVA to original unbound IAT (PIMAGE_THUNK_DATA)
//            /// </summary>
//            public int CharacteristicsOrOriginalFirstThunk;
//            /// <summary>
//            /// 0 if not bound, -1 if bound, and real date\time stamp in IMAGE_DIRECTORY_ENTRY_BOUND_IMPORT (new BIND); O.W. date/time stamp of DLL bound to (Old BIND)
//            /// </summary>
//            public uint TimeDateStamp;
//            /// <summary>
//            /// -1 if no forwarders
//            /// </summary>
//            public uint ForwarderChain;
//            public int Name;
//            /// <summary>
//            /// RVA to IAT (if bound this IAT has actual addresses)
//            /// </summary>
//            public int FirstThunk;
//        }

//        [StructLayout(LayoutKind.Sequential, Pack = 4)]
//        private struct IMAGE_SECTION_HEADER
//        {
//            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
//            public byte[] Name;

//            //union 
//            //{    
//            //    DWORD PhysicalAddress;    
//            //    DWORD VirtualSize;  
//            //} Misc;  
//            public uint PhysicalAddress;

//            //public uint VirtualSize;
//            public uint VirtualAddress;
//            public int SizeOfRawData;
//            public uint PointerToRawData;
//            public uint PointerToRelocations;
//            public uint PointerToLinenumbers;
//            public short NumberOfRelocations;
//            public short NumberOfLinenumbers;
//            public uint Characteristics;
//        }

//        [StructLayout(LayoutKind.Sequential, Pack = 4)]
//        public struct IMAGE_DOS_HEADER
//        {
//            public ushort e_magic; // Magic number
//            public ushort e_cblp; // bytes on last page of file
//            public ushort e_cp; // Pages in file
//            public ushort e_crlc; // Relocations
//            public ushort e_cparhdr; // Size of header in paragraphs
//            public ushort e_minalloc; // Minimum extra paragraphs needed
//            public ushort e_maxalloc; // Maximum extra paragraphs needed
//            public ushort e_ss; // Initial (relative) SS value
//            public ushort e_sp; // Initial SP value
//            public ushort e_csum; // Checksum
//            public ushort e_ip; // Initial IP value
//            public ushort e_cs; // Initial (relative) CS value
//            public ushort e_lfarlc; // File address of relocation table
//            public ushort e_ovno; // Overlay number

//            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
//            public ushort[] e_res1; // Reserved words

//            public ushort e_oemid; // OEM identifier (for e_oeminfo)
//            public ushort e_oeminfo; // OEM information; e_oemid specific

//            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
//            public ushort[] e_res2; // Reserved words

//            public int e_lfanew; // File address of new exe header
//        }

//        [StructLayout(LayoutKind.Sequential)]
//        public struct IMAGE_NT_HEADERS
//        {
//            public uint Signature;
//            public IMAGE_FILE_HEADER FileHeader;
//            public IMAGE_OPTIONAL_HEADER32 OptionalHeader;
//        }

//        [StructLayout(LayoutKind.Sequential, Pack = 4)]
//        public struct IMAGE_OPTIONAL_HEADER32
//        {
//            //
//            // Standard fields.
//            //
//            public ushort Magic;
//            public byte MajorLinkerVersion;
//            public byte MinorLinkerVersion;
//            public int SizeOfCode;
//            public int SizeOfInitializedData;
//            public int SizeOfUninitializedData;
//            public int AddressOfEntryPoint;
//            public uint BaseOfCode;

//            public uint BaseOfData;

//            //
//            // NT additional fields.
//            //
//            public uint ImageBase;
//            public int SectionAlignment;
//            public uint FileAlignment;
//            public ushort MajorOperatingSystemVersion;
//            public ushort MinorOperatingSystemVersion;
//            public ushort MajorImageVersion;
//            public ushort MinorImageVersion;
//            public ushort MajorSubsystemVersion;
//            public ushort MinorSubsystemVersion;
//            public uint Win32VersionValue;
//            public int SizeOfImage;
//            public int SizeOfHeaders;
//            public uint CheckSum;
//            public ushort Subsystem;
//            public ushort DllCharacteristics;
//            public uint SizeOfStackReserve;
//            public uint SizeOfStackCommit;
//            public uint SizeOfHeapReserve;
//            public uint SizeOfHeapCommit;
//            public uint LoaderFlags;
//            public uint NumberOfRvaAndSizes;

//            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
//            public IMAGE_DATA_DIRECTORY[] DataDirectory;
//        }

//        [StructLayout(LayoutKind.Sequential, Pack = 4)]
//        public struct IMAGE_DATA_DIRECTORY
//        {
//            public int VirtualAddress;
//            public int Size;
//        }

//        [StructLayout(LayoutKind.Sequential, Pack = 4)]
//        public struct IMAGE_FILE_HEADER
//        {
//            public ushort Machine;
//            public ushort NumberOfSections;
//            public uint TimeDateStamp;
//            public uint PointerToSymbolTable;
//            public uint NumberOfSymbols;
//            public ushort SizeOfOptionalHeader;
//            public ushort Characteristics;
//        }

//        internal static class Win32Imports
//        {
//            [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
//            public static extern uint GetProcAddress(IntPtr hModule, string procName);

//            [DllImport("kernel32")]
//            public static extern uint GetLastError();

//            [DllImport("kernel32.dll")]
//            public static extern IntPtr GetProcAddress(IntPtr module, IntPtr ordinal);

//            [DllImport("kernel32.dll", SetLastError = true)]
//            internal static extern bool VirtualProtect(IntPtr lpAddress, int dwSize, uint flNewProtect, out uint lpflOldProtect);
//        }

//        internal static class PointerHelpers
//        {
//            public static T ToStruct<T>(byte[] data) where T : struct
//            {
//                unsafe
//                {
//                    fixed (byte* p = &data[0])
//                    {
//                        return (T)Marshal.PtrToStructure(new IntPtr(p), typeof(T));
//                    }
//                }
//            }

//            public static T ToStruct<T>(byte[] data, uint from) where T : struct
//            {
//                unsafe
//                {
//                    fixed (byte* p = &data[from])
//                    {
//                        return (T)Marshal.PtrToStructure(new IntPtr(p), typeof(T));
//                    }
//                }
//            }

//            public static T ToStruct<T>(IntPtr ptr, uint from) where T : struct
//            {
//                return (T)Marshal.PtrToStructure(ptr + (int)from, typeof(T));
//            }
//        }

//        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
//        unsafe delegate bool fnDllEntry(IntPtr instance, uint reason, IntPtr reserved);

//        public bool LoadLibrary(byte[] Data)
//        {
//            //fnDllEntry dllEntry;
//            IMAGE_DOS_HEADER dosHeader = PointerHelpers.ToStruct<IMAGE_DOS_HEADER>(Data);

//            IMAGE_NT_HEADERS oldHeader = PointerHelpers.ToStruct<IMAGE_NT_HEADERS>(Data, (uint)dosHeader.e_lfanew);

//            IntPtr code = VirtualAlloc(new IntPtr(oldHeader.OptionalHeader.ImageBase), oldHeader.OptionalHeader.SizeOfImage, MemAllocType.Reserve, MemProtectType.Page_ReadWrite);

//            if (code == IntPtr.Zero)
//                code = VirtualAlloc(code, oldHeader.OptionalHeader.SizeOfImage, MemAllocType.Reserve, MemProtectType.Page_ReadWrite);

//            module = new MemoryModule { CodeBase = code, NumModules = 0, Modules = IntPtr.Zero, Initialized = 0 };

//            VirtualAlloc(code, oldHeader.OptionalHeader.SizeOfImage, MemAllocType.Commit, MemProtectType.Page_ReadWrite);

//            IntPtr headers = VirtualAlloc(code, oldHeader.OptionalHeader.SizeOfHeaders, MemAllocType.Commit, MemProtectType.Page_ReadWrite);

//            Marshal.Copy(Data, 0, headers, dosHeader.e_lfanew + oldHeader.OptionalHeader.SizeOfHeaders);

//            module.Headers = PointerHelpers.ToStruct<IMAGE_NT_HEADERS>(headers, (uint)dosHeader.e_lfanew);
//            module.Headers.OptionalHeader.ImageBase = (uint)code;

//            CopySections(Data, oldHeader, headers, dosHeader);

//            uint locationDelta = (uint)(code - (int)oldHeader.OptionalHeader.ImageBase);

//            if (locationDelta != 0)
//                PerformBaseRelocation(locationDelta);

//            BuildImportTable();
//            FinalizeSections(headers, dosHeader, oldHeader);

//            try
//            {
//                fnDllEntry dllEntry = (fnDllEntry)Marshal.GetDelegateForFunctionPointer(module.CodeBase + module.Headers.OptionalHeader.AddressOfEntryPoint, typeof(fnDllEntry));
//                return dllEntry(code, 1, IntPtr.Zero);
//            }
//            catch
//            {
//            }

//            return false;
//        }

//        public int GetModuleCount()
//        {
//            int count = 0;
//            IntPtr CodeBase = module.CodeBase;
//            IMAGE_DATA_DIRECTORY directory = module.Headers.OptionalHeader.DataDirectory[1];
//            if (directory.Size > 0)
//            {
//                IMAGE_IMPORT_DESCRIPTOR* pImportDesc = (IMAGE_IMPORT_DESCRIPTOR*)(CodeBase + directory.VirtualAddress);
//                while (pImportDesc->Name > 0)
//                {
//                    IntPtr str = CodeBase + pImportDesc->Name;
//                    string tmp = Marshal.PtrToStringAnsi(str);
//                    IntPtr handle = Win32.System.LoadLibrary(tmp);

//                    if (handle == IntPtr.Zero)
//                        break;

//                    //if (handle == -1)
//                    //{
//                    //    break;
//                    //}

//                    pImportDesc++;
//                }
//            }

//            return count;
//        }

//        public int BuildImportTable()
//        {
//            int ucount = GetModuleCount();
//            module.Modules = Marshal.AllocHGlobal((ucount) * sizeof(int));
//            //int pcount = 0;
//            int result = 1;
//            IntPtr CodeBase = module.CodeBase;
//            IMAGE_DATA_DIRECTORY directory = module.Headers.OptionalHeader.DataDirectory[1];
//            if (directory.Size > 0)
//            {
//                IMAGE_IMPORT_DESCRIPTOR* pImportDesc = (IMAGE_IMPORT_DESCRIPTOR*)(CodeBase + directory.VirtualAddress);
//                while (pImportDesc->Name > 0)
//                {
//                    IntPtr str = CodeBase + pImportDesc->Name;
//                    string tmp = Marshal.PtrToStringAnsi(str);

//                    uint* thunkRef, funcRef;

//                    IntPtr handle = Win32.System.LoadLibrary(tmp);

//                    if (handle == IntPtr.Zero)
//                    {
//                        result = 0;
//                        break;
//                    }
//                    //if (handle == -1)
//                    //{
//                    //    result = 0;
//                    //    break;
//                    //}

//                    int CharOrOriginalFirstThunk = pImportDesc->CharacteristicsOrOriginalFirstThunk,
//                        FirstThunk = pImportDesc->FirstThunk;
//                    if (CharOrOriginalFirstThunk > 0)
//                    {
//                        IntPtr thunkRefAddr = CodeBase + CharOrOriginalFirstThunk;
//                        thunkRef = (uint*)thunkRefAddr;
//                        funcRef = (uint*)(CodeBase + FirstThunk);
//                    }
//                    else
//                    {
//                        thunkRef = (uint*)(CodeBase + FirstThunk);
//                        funcRef = (uint*)(CodeBase + FirstThunk);
//                    }

//                    for (; *thunkRef > 0; thunkRef++, funcRef++)
//                    {
//                        if ((*thunkRef & 0x80000000) != 0)
//                        {
//                            *funcRef = (uint)Win32Imports.GetProcAddress(handle, new IntPtr(*thunkRef & 0xffff));
//                        }
//                        else
//                        {
//                            IntPtr str2 = CodeBase + (int)*thunkRef + 2;
//                            string tmpaa = Marshal.PtrToStringAnsi(str2);
//                            *funcRef = Win32Imports.GetProcAddress(handle, tmpaa);
//                        }

//                        if (*funcRef == 0)
//                        {
//                            result = 0;
//                            break;
//                        }
//                    }

//                    //pcount++;
//                    //importDesc = PointerHelpers.ToStruct<IMAGE_IMPORT_DESCRIPTOR>(codeBase, directory.VirtualAddress + (uint)(Marshal.SizeOf(typeof(IMAGE_IMPORT_DESCRIPTOR)) * pcount));

//                    pImportDesc++;
//                }
//            }

//            return result;
//        }

//        static readonly int[][][] ProtectionFlags = new int[2][][];

//        public void FinalizeSections(IntPtr headers, IMAGE_DOS_HEADER dosHeader, IMAGE_NT_HEADERS oldHeaders)
//        {
//            ProtectionFlags[0] = new int[2][];
//            ProtectionFlags[1] = new int[2][];
//            ProtectionFlags[0][0] = new int[2];
//            ProtectionFlags[0][1] = new int[2];
//            ProtectionFlags[1][0] = new int[2];
//            ProtectionFlags[1][1] = new int[2];
//            ProtectionFlags[0][0][0] = 0x01;
//            ProtectionFlags[0][0][1] = 0x08;
//            ProtectionFlags[0][1][0] = 0x02;
//            ProtectionFlags[0][1][1] = 0x04;
//            ProtectionFlags[1][0][0] = 0x10;
//            ProtectionFlags[1][0][1] = 0x80;
//            ProtectionFlags[1][1][0] = 0x20;
//            ProtectionFlags[1][1][1] = 0x40;

//            IMAGE_SECTION_HEADER section = PointerHelpers.ToStruct<IMAGE_SECTION_HEADER>(headers,
//                (uint)(24 + dosHeader.e_lfanew + oldHeaders.FileHeader.SizeOfOptionalHeader));
//            for (int i = 0; i < module.Headers.FileHeader.NumberOfSections; i++)
//            {
//                //Console.WriteLine("Finalizing " + Encoding.UTF8.GetString(section.Name));
//                int executable = (section.Characteristics & 0x20000000) != 0 ? 1 : 0;
//                int readable = (section.Characteristics & 0x40000000) != 0 ? 1 : 0;
//                int writeable = (section.Characteristics & 0x80000000) != 0 ? 1 : 0;

//                if ((section.Characteristics & 0x02000000) > 0)
//                {
//                    bool aa = VirtualFree(new IntPtr(section.PhysicalAddress), section.SizeOfRawData, MemFreeType.Decommit);
//                    continue;
//                }

//                uint protect = (uint)ProtectionFlags[executable][readable][writeable];

//                if ((section.Characteristics & 0x04000000) > 0)
//                    protect |= 0x200;
//                int size = section.SizeOfRawData;
//                if (size == 0)
//                {
//                    if ((section.Characteristics & 0x00000040) > 0)
//                        size = module.Headers.OptionalHeader.SizeOfInitializedData;
//                    else if ((section.Characteristics & 0x00000080) > 0)
//                        size = module.Headers.OptionalHeader.SizeOfUninitializedData;
//                }

//                if (size > 0)
//                {
//                    if (!Win32Imports.VirtualProtect(new IntPtr(section.PhysicalAddress), section.SizeOfRawData, protect, out uint oldProtect))
//                    {
//                    }
//                }

//                section = PointerHelpers.ToStruct<IMAGE_SECTION_HEADER>(headers, (uint)(24 + dosHeader.e_lfanew + oldHeaders.FileHeader.SizeOfOptionalHeader + Marshal.SizeOf(typeof(IMAGE_SECTION_HEADER)) * (i + 1)));
//            }
//        }

//        public void PerformBaseRelocation(uint delta)
//        {
//            IntPtr CodeBase = module.CodeBase;
//            IMAGE_DATA_DIRECTORY directory = module.Headers.OptionalHeader.DataDirectory[5];
//            if (directory.Size > 0)
//            {
//                int sizeOfBase = Marshal.SizeOf(typeof(IMAGE_BASE_RELOCATION));
//                IMAGE_BASE_RELOCATION* pRelocation = (IMAGE_BASE_RELOCATION*)(CodeBase + directory.VirtualAddress);
//                while (pRelocation->VirtualAddress > 0)
//                {
//                    IntPtr dest = CodeBase + pRelocation->VirtualAddress;
//                    ushort* relInfo = (ushort*)(CodeBase + directory.VirtualAddress + sizeOfBase);
//                    for (int i = 0; i < (pRelocation->SizeOfBlock - sizeOfBase) / 2; i++, relInfo++)
//                    {
//                        int type = *relInfo >> 12;
//                        if (type == 0x03)
//                        {
//                            uint* patchAddrHl = (uint*)(dest + (*relInfo & 0xfff));
//                            *patchAddrHl += delta;
//                        }
//                    }

//                    pRelocation++;
//                }
//            }
//        }

//        private MemoryModule module;

//        public IntPtr GetProcAddress(string name)
//        {
//            IntPtr CodeBase = module.CodeBase;
//            int idx = -1;
//            uint i;

//            IMAGE_DATA_DIRECTORY directory = module.Headers.OptionalHeader.DataDirectory[0];
//            if (directory.Size == 0)
//                return IntPtr.Zero;

//            IMAGE_EXPORT_DIRECTORY* pExports = (IMAGE_EXPORT_DIRECTORY*)(CodeBase + directory.VirtualAddress);

//            int* nameRef = (int*)(CodeBase + pExports->AddressOfNames);
//            ushort* ordinal = (ushort*)(CodeBase + pExports->AddressOfNameOrdinals);
//            for (i = 0; i < pExports->NumberOfNames; i++, nameRef++, ordinal++)
//            {
//                IntPtr str = CodeBase + *nameRef;
//                string tmp = Marshal.PtrToStringAnsi(str);
//                if (tmp == name)
//                {
//                    idx = *ordinal;
//                    break;
//                }
//            }

//            return CodeBase + *(int*)(CodeBase + (pExports->AddressOfFunctions + idx * 4));
//        }

//        public void CopySections(byte[] data, IMAGE_NT_HEADERS oldHeaders, IntPtr headers, IMAGE_DOS_HEADER dosHeader)
//        {
//            int i;
//            IntPtr codebase = module.CodeBase;
//            int SizeOfHeader = Marshal.SizeOf(typeof(IMAGE_SECTION_HEADER));
//            IMAGE_SECTION_HEADER section = PointerHelpers.ToStruct<IMAGE_SECTION_HEADER>(headers, (uint)(24 + dosHeader.e_lfanew + oldHeaders.FileHeader.SizeOfOptionalHeader));
//            for (i = 0; i < module.Headers.FileHeader.NumberOfSections; i++)
//            {
//                IntPtr dest;
//                if (section.SizeOfRawData == 0)
//                {
//                    int size = oldHeaders.OptionalHeader.SectionAlignment;
//                    if (size > 0)
//                    {
//                        dest = VirtualAlloc(codebase + (int)section.VirtualAddress, size, MemAllocType.Commit, MemProtectType.Page_ReadWrite);

//                        section.PhysicalAddress = (uint)dest;
//                        IntPtr write = headers + 32 + dosHeader.e_lfanew + oldHeaders.FileHeader.SizeOfOptionalHeader + SizeOfHeader * i;
//                        Marshal.WriteInt32(write, (int)dest);
//                        byte[] datazz = new byte[size + 1];
//                        Marshal.Copy(datazz, 0, dest, size);
//                    }

//                    section = PointerHelpers.ToStruct<IMAGE_SECTION_HEADER>(headers, (uint)(24 + dosHeader.e_lfanew + oldHeaders.FileHeader.SizeOfOptionalHeader + SizeOfHeader * (i + 1)));
//                    continue;
//                }

//                dest = VirtualAlloc(codebase + (int)section.VirtualAddress, section.SizeOfRawData, MemAllocType.Commit, MemProtectType.Page_ReadWrite);
//                Marshal.Copy(data, (int)section.PointerToRawData, dest, section.SizeOfRawData);
//                section.PhysicalAddress = (uint)dest;
//                IntPtr write2 = headers + 32 + dosHeader.e_lfanew + oldHeaders.FileHeader.SizeOfOptionalHeader + SizeOfHeader * i;
//                Marshal.WriteInt32(write2, (int)dest);

//                section = PointerHelpers.ToStruct<IMAGE_SECTION_HEADER>(headers, (uint)(24 + dosHeader.e_lfanew + oldHeaders.FileHeader.SizeOfOptionalHeader + SizeOfHeader * (i + 1)));
//            }
//        }
//    }
//}