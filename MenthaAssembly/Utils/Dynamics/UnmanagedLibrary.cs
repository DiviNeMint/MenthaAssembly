using MenthaAssembly.Win32;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using static MenthaAssembly.Win32.System;

namespace MenthaAssembly
{
    // https://github.com/dretax/DynamicDllLoader/blob/master/DynamicDLLLoader/DynamicDllLoader.cs
    // https://github.com/fancycode/MemoryModule/blob/master/MemoryModule.c
    public sealed unsafe partial class UnmanagedLibrary : DynamicLibrary
    {
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private unsafe delegate bool DllEntryProc(IntPtr instance, uint reason, IntPtr reserved);

        private static readonly MemProtectType[][][] ProtectionFlags = new MemProtectType[][][]
        {
            new MemProtectType[][]
            {
                new MemProtectType[] { MemProtectType.Page_NoAccess, MemProtectType.Page_WriteCopy },
                new MemProtectType[] { MemProtectType.Page_ReadOnly, MemProtectType.Page_ReadWrite },
            },
            new MemProtectType[][]
            {
                new MemProtectType[] { MemProtectType.Page_Execute, MemProtectType.Page_Execute_WriteCopy },
                new MemProtectType[] { MemProtectType.Page_Execute_Read, MemProtectType.Page_Execute_ReadWrite },
            },
        };

        private int e_lfanew;
        private IntPtr CodeBase;
        private DllEntryProc DllEntry;
        private ImageDataDirectory ExportDirectory;

        private readonly ConcurrentDictionary<string, Delegate> MethodInfos = new ConcurrentDictionary<string, Delegate>();
        private readonly List<IntPtr> ImportLibraries = new List<IntPtr>();

        internal readonly bool IsLoaded;
        internal UnmanagedLibrary(string Path, LibraryType Type) : base(Path, Type)
        {
            IsLoaded = LoadLibrary(File.ReadAllBytes(Path));
            if (!IsLoaded)
                Dispose();
        }

        private bool LoadLibrary(byte[] Datas)
        {
            fixed (byte* pData = &Datas[0])
            {
                // DosHeader.e_lfanew
                e_lfanew = *(int*)(pData + 60);

                // FileHeader
                ImageFileHeader* pFileHeader = (ImageFileHeader*)(pData + e_lfanew + 4);

                //// It has checked by DynamicLibrary.
                //// Check if the file is Dll.
                //if ((pFileHeader->Characteristics & 0x2000) == 0)
                //    return false;

                byte* pTemp = pData + e_lfanew + 24;
                IntPtr pImageBase;
                uint SizeOfImage,
                     SizeOfHeaders,
                     SectionAlignment,
                     SizeOfInitializedData,
                     SizeOfUninitializedData,
                     AddressOfEntryPoint;

                ImageDataDirectory ImportDirectory,
                                     RelocationDirectory;

                // Check the dll's Platform.
                ushort Platform = *(ushort*)pTemp;
                if (Platform == 0x10b)
                {
                    // x86
                    ImageOptionalHeader32* pOptionalHeader = (ImageOptionalHeader32*)pTemp;
                    pImageBase = new IntPtr(pOptionalHeader->ImageBase);
                    SizeOfImage = pOptionalHeader->SizeOfImage;
                    SizeOfHeaders = pOptionalHeader->SizeOfHeaders;
                    SectionAlignment = pOptionalHeader->SectionAlignment;
                    SizeOfInitializedData = pOptionalHeader->SizeOfInitializedData;
                    SizeOfUninitializedData = pOptionalHeader->SizeOfUninitializedData;

                    ImportDirectory = pOptionalHeader->ImportTable;
                    RelocationDirectory = pOptionalHeader->BaseRelocationTable;
                    ExportDirectory = pOptionalHeader->ExportTable;

                    AddressOfEntryPoint = pOptionalHeader->AddressOfEntryPoint;
                }
                else if (Platform == 0x20b)
                {
                    // x64
                    ImageOptionalHeader64* pOptionalHeader = (ImageOptionalHeader64*)pTemp;
                    pImageBase = new IntPtr((long)pOptionalHeader->ImageBase);
                    SizeOfImage = pOptionalHeader->SizeOfImage;
                    SizeOfHeaders = pOptionalHeader->SizeOfHeaders;
                    SectionAlignment = pOptionalHeader->SectionAlignment;
                    SizeOfInitializedData = pOptionalHeader->SizeOfInitializedData;
                    SizeOfUninitializedData = pOptionalHeader->SizeOfUninitializedData;

                    ImportDirectory = pOptionalHeader->ImportTable;
                    RelocationDirectory = pOptionalHeader->BaseRelocationTable;
                    ExportDirectory = pOptionalHeader->ExportTable;

                    AddressOfEntryPoint = pOptionalHeader->AddressOfEntryPoint;
                }
                else
                {
                    // Unknown
                    return false;
                }

                CodeBase = VirtualAlloc(pImageBase, SizeOfImage, MemAllocType.Reserve, MemProtectType.Page_ReadWrite);

                if (CodeBase == IntPtr.Zero)
                    CodeBase = VirtualAlloc(CodeBase, SizeOfImage, MemAllocType.Reserve, MemProtectType.Page_ReadWrite);

                VirtualAlloc(CodeBase, SizeOfImage, MemAllocType.Commit, MemProtectType.Page_ReadWrite);
                VirtualAlloc(CodeBase, SizeOfHeaders, MemAllocType.Commit, MemProtectType.Page_ReadWrite);

                Marshal.Copy(Datas, 0, CodeBase, (int)(e_lfanew + SizeOfHeaders));

                ImageSectionHeader* pSectionHeader = (ImageSectionHeader*)(CodeBase + e_lfanew + 24 + pFileHeader->SizeOfOptionalHeader);

                if (!CopySections(Datas, pSectionHeader, pFileHeader->NumberOfSections, SectionAlignment))
                    return false;

                long LocationDelta = CodeBase.ToInt64() - pImageBase.ToInt64();
                if (LocationDelta != 0)
                    PerformBaseRelocation(RelocationDirectory, LocationDelta);

                if (!BuildImportTable(ImportDirectory))
                    return false;

                if (!FinalizeSections(pSectionHeader, pFileHeader->NumberOfSections, SizeOfInitializedData, SizeOfUninitializedData))
                    return false;

                try
                {
                    DllEntry = Marshal.GetDelegateForFunctionPointer<DllEntryProc>(CodeBase + (int)AddressOfEntryPoint);
                    return DllEntry(CodeBase, 1, IntPtr.Zero);
                }
                catch
                {
                }
            }

            return false;
        }

        private bool CopySections(byte[] Datas, ImageSectionHeader* pSection, int NumberOfSections, uint SectionAlignment)
        {
            for (int i = 0; i < NumberOfSections; i++, pSection++)
            {
                IntPtr Dest;
                if (pSection->SizeOfRawData == 0)
                {
                    if (SectionAlignment > 0)
                    {
                        Dest = VirtualAlloc(CodeBase + (int)pSection->VirtualAddress, SectionAlignment, MemAllocType.Commit, MemProtectType.Page_ReadWrite);

                        if (Dest == IntPtr.Zero)
                            return false;

                        pSection->PhysicalAddress = Dest;

                        // Clear Datas
                        byte* pDest = (byte*)Dest;
                        for (int j = 0; j < SectionAlignment; j++)
                            *pDest++ = 0;

                        //byte[] datazz = new byte[SectionAlignment];
                        //Marshal.Copy(datazz, 0, dest, (int)SectionAlignment);
                    }

                    continue;
                }

                Dest = VirtualAlloc(CodeBase + (int)pSection->VirtualAddress, pSection->SizeOfRawData, MemAllocType.Commit, MemProtectType.Page_ReadWrite);

                if (Dest == IntPtr.Zero)
                    return false;

                pSection->PhysicalAddress = Dest;

                // Fill Datas
                Marshal.Copy(Datas, (int)pSection->PointerToRawData, Dest, (int)pSection->SizeOfRawData);
            }

            return true;
        }

        private void PerformBaseRelocation(ImageDataDirectory RelocationDirectory, long Delta)
        {
            if (RelocationDirectory.Size > 0)
            {
                IntPtr pRelocation0 = CodeBase + RelocationDirectory.VirtualAddress;
                int SizeOfBase = sizeof(ImageBaseRelocation);

                ImageBaseRelocation* pRelocation = (ImageBaseRelocation*)pRelocation0;
                while (pRelocation->VirtualAddress > 0)
                {
                    IntPtr Dest = CodeBase + pRelocation->VirtualAddress;
                    ushort* relInfo = (ushort*)(pRelocation0 + SizeOfBase);
                    for (int i = 0; i < (pRelocation->SizeOfBlock - SizeOfBase) / 2; i++, relInfo++)
                    {
                        int type = *relInfo >> 12;

                        // IMAGE_REL_BASED_ABSOLUTE
                        // skip relocation
                        if (type == 0)
                            break;

                        // IMAGE_REL_BASED_HIGHLOW
                        // change complete 32 bit address
                        if (type == 3)
                        {
                            uint* patchAddrHl = (uint*)(Dest + (*relInfo & 0xfff));
                            *patchAddrHl += (uint)Delta;
                        }

                        // IMAGE_REL_BASED_DIR64
                        else if (type == 10)
                        {
                            ulong* patchAddrHl = (ulong*)(Dest + (*relInfo & 0xfff));
                            *patchAddrHl += (ulong)Delta;
                        }
                    }

                    pRelocation0 += pRelocation->SizeOfBlock;
                    pRelocation = (ImageBaseRelocation*)pRelocation0;
                }
            }
        }

        private bool BuildImportTable(ImageDataDirectory ImportDirectory)
        {
            if (ImportDirectory.Size > 0)
            {
                ImageImportDescriptor* pImportDesc = (ImageImportDescriptor*)(CodeBase + ImportDirectory.VirtualAddress);
                while (pImportDesc->Name > 0)
                {
                    IntPtr pStr = CodeBase + pImportDesc->Name;
                    string Temp = Marshal.PtrToStringAnsi(pStr);

                    IntPtr* thunkRef, funcRef;

                    IntPtr pLibrary = Win32.System.LoadLibrary(Temp);

                    if (pLibrary == IntPtr.Zero)
                        return false;

                    if (!ImportLibraries.Contains(pLibrary))
                        ImportLibraries.Add(pLibrary);

                    int Characteristics = pImportDesc->Characteristics,
                        FirstThunk = pImportDesc->FirstThunk;
                    if (Characteristics > 0)
                    {
                        IntPtr thunkRefAddr = CodeBase + Characteristics;
                        thunkRef = (IntPtr*)thunkRefAddr;
                        funcRef = (IntPtr*)(CodeBase + FirstThunk);
                    }
                    else
                    {
                        thunkRef = (IntPtr*)(CodeBase + FirstThunk);
                        funcRef = (IntPtr*)(CodeBase + FirstThunk);
                    }

                    for (; *thunkRef != IntPtr.Zero; thunkRef++, funcRef++)
                    {
                        if ((thunkRef->ToInt64() & 0x80000000) != 0)
                        {
                            *funcRef = Win32.System.GetProcAddress(pLibrary, new IntPtr(thunkRef->ToInt64() & 0xFFFF));
                        }
                        else
                        {
                            //[StructLayout(LayoutKind.Sequential)]
                            //private struct IMAGE_IMPORT_BY_NAME
                            //{
                            //    public short Hint;
                            //    public byte Name;
                            //}

                            pStr = new IntPtr(CodeBase.ToInt64() + thunkRef->ToInt64() + 2);
                            Temp = Marshal.PtrToStringAnsi(pStr);
                            *funcRef = Win32.System.GetProcAddress(pLibrary, Temp);
                        }

                        if (*funcRef == IntPtr.Zero)
                            return false;
                    }

                    pImportDesc++;
                }
            }

            return true;
        }

        private bool FinalizeSections(ImageSectionHeader* pSection, ushort NumberOfSections, uint SizeOfInitializedData, uint SizeOfUninitializedData)
        {
            for (ushort i = 0; i < NumberOfSections; i++, pSection++)
            {
                if ((pSection->Characteristics & ImageDataSectionFlags.MemoryDiscardable) > 0)
                {
                    VirtualFree(pSection->PhysicalAddress, pSection->SizeOfRawData, MemFreeType.Decommit);
                    continue;
                }

                uint size = pSection->SizeOfRawData;
                if (size == 0)
                {
                    if ((pSection->Characteristics & ImageDataSectionFlags.ContentInitializedData) > 0)
                        size = SizeOfInitializedData;
                    else if ((pSection->Characteristics & ImageDataSectionFlags.ContentUninitializedData) > 0)
                        size = SizeOfUninitializedData;
                }

                if (size > 0)
                {
                    int executable = (pSection->Characteristics & ImageDataSectionFlags.MemoryExecute) != 0 ? 1 : 0,
                        readable = (pSection->Characteristics & ImageDataSectionFlags.MemoryRead) != 0 ? 1 : 0,
                        writeable = (pSection->Characteristics & ImageDataSectionFlags.MemoryWrite) != 0 ? 1 : 0;

                    MemProtectType protect = ProtectionFlags[executable][readable][writeable];
                    if ((pSection->Characteristics & ImageDataSectionFlags.MemoryNotCached) > 0)
                        protect |= MemProtectType.Page_NoCache;

                    if (!VirtualProtect(pSection->PhysicalAddress, pSection->SizeOfRawData, protect, out IntPtr oldProtect))
                        return false;
                }

            }

            return true;
        }

        public TDelegate GetMethod<TDelegate>(string FunctionName)
            where TDelegate : Delegate
        {
            if (MethodInfos.TryGetValue(FunctionName, out Delegate MethodBase) &&
                MethodBase is TDelegate Method)
                return Method;

            IntPtr pProc = GetProcAddress(FunctionName);

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

        private IntPtr GetProcAddress(string Name)
        {
            if (ExportDirectory.Size == 0)
                return IntPtr.Zero;

            ImageExportDirectory* pExports = (ImageExportDirectory*)(CodeBase + ExportDirectory.VirtualAddress);

            int* nameRef = (int*)(CodeBase + pExports->AddressOfNames);
            ushort* ordinal = (ushort*)(CodeBase + pExports->AddressOfNameOrdinals);
            for (uint i = 0; i < pExports->NumberOfNames; i++, nameRef++, ordinal++)
            {
                IntPtr pStr = CodeBase + *nameRef;
                string tmp = Marshal.PtrToStringAnsi(pStr);

                if (tmp == Name)
                    return CodeBase + *(int*)(CodeBase + pExports->AddressOfFunctions + *ordinal * 4);
            }

            return IntPtr.Zero;
        }

        public IEnumerable<string> GetMethodNames()
        {
            if (ExportDirectory.Size == 0)
                yield break;

            ImageExportDirectory pExports = (CodeBase + ExportDirectory.VirtualAddress).Cast<ImageExportDirectory>();

            IntPtr nameRef = CodeBase + pExports.AddressOfNames;
            for (uint i = 0; i < pExports.NumberOfNames; i++, nameRef += 4)
            {
                IntPtr pStr = CodeBase + nameRef.Cast<int>();
                yield return Marshal.PtrToStringAnsi(pStr);
            }
        }

        private bool IsDisposed;
        private void Dispose(bool IsDisposing)
        {
            if (!IsDisposed)
            {
                if (IsDisposing)
                {
                    DllEntry?.Invoke(CodeBase, 0, IntPtr.Zero);
                    DllEntry = null;
                }

                foreach (IntPtr pLibrary in ImportLibraries)
                    FreeLibrary(pLibrary);

                ImportLibraries.Clear();

                if (CodeBase != IntPtr.Zero)
                {
                    VirtualFree(CodeBase, 0, MemFreeType.Release);
                    CodeBase = IntPtr.Zero;
                }

                IsDisposed = true;
            }
        }

        ~UnmanagedLibrary()
        {
            Dispose(false);
        }

        public override void Dispose()
        {
            Dispose(true);
            base.Dispose();
        }

        #region Window API Version (Old)

        //private readonly ConcurrentDictionary<string, Delegate> MethodInfos = new ConcurrentDictionary<string, Delegate>();

        //private readonly SafeHandle pLibrary;
        //internal UnmanagedLibrary(string Path, SafeHandle pLibrary, LibraryType Type) : base(Path, Type)
        //{
        //    this.pLibrary = pLibrary;
        //}

        //public TDelegate GetMethod<TDelegate>(string FunctionName)
        //    where TDelegate : Delegate
        //{
        //    if (MethodInfos.TryGetValue(FunctionName, out Delegate MethodBase) &&
        //        MethodBase is TDelegate Method)
        //        return Method;

        //    if (pLibrary.IsInvalid)
        //        return null;

        //    IntPtr pProc = GetProcAddress(pLibrary.DangerousGetHandle(), FunctionName);

        //    Method = Marshal.GetDelegateForFunctionPointer<TDelegate>(pProc);
        //    MethodInfos.AddOrUpdate(FunctionName, Method, (k, v) => Method);
        //    return Method;
        //}

        //public void Invoke<TDelegate>(string FunctionName, params object[] Args)
        //    where TDelegate : Delegate
        //    => GetMethod<TDelegate>(FunctionName).DynamicInvoke(Args);
        //public TResult Invoke<TDelegate, TResult>(string FunctionName, params object[] Args)
        //    where TDelegate : Delegate
        //    => (TResult)GetMethod<TDelegate>(FunctionName).DynamicInvoke(Args);

        //public override void Dispose()
        //{
        //    pLibrary.Dispose();
        //    base.Dispose();
        //}

        #endregion

    }
}