using System;
using System.IO;
using System.Runtime.InteropServices;

namespace MenthaAssembly
{
    public unsafe class DynamicLibrary : IDisposable
    {
        #region Windows API
        [DllImport("kernel32.dll")]
        private static extern IntPtr LoadLibrary(string Path);

        [DllImport("kernel32.dll")]
        private static extern bool FreeLibrary(IntPtr hModule);

        #endregion

        #region Misc

        //private struct IMAGE_DOS_HEADER
        //{
        //    public ushort e_magic;      // Magic number
        //    public ushort e_cblp;       // Bytes on last page of file
        //    public ushort e_cp;         // Pages in file
        //    public ushort e_crlc;       // Relocations
        //    public ushort e_cparhdr;    // Size of header in paragraphs
        //    public ushort e_minalloc;   // Minimum extra paragraphs needed
        //    public ushort e_maxalloc;   // Maximum extra paragraphs needed
        //    public ushort e_ss;         // Initial (relative) SS value
        //    public ushort e_sp;         // Initial SP value
        //    public ushort e_csum;       // Checksum
        //    public ushort e_ip;         // Initial IP value
        //    public ushort e_cs;         // Initial (relative) CS value
        //    public ushort e_lfarlc;     // File address of relocation table
        //    public ushort e_ovno;       // Overlay number

        //    public ushort e_res1;       // Reserved words
        //    public ushort e_res2;
        //    public ushort e_res3;
        //    public ushort e_res4;

        //    public ushort e_oemid;      // OEM identifier (for e_oeminfo)
        //    public ushort e_oeminfo;    // OEM information; e_oemid specific

        //    public ushort d_res5;       // Reserved words
        //    public ushort d_res6;
        //    public ushort d_res7;
        //    public ushort d_res8;
        //    public ushort d_res9;
        //    public ushort d_res10;
        //    public ushort d_res11;
        //    public ushort d_res12;
        //    public ushort d_res13;
        //    public ushort d_res14;

        //    public int e_lfanew;        // File address of new exe header
        //}

        //private struct IMAGE_FILE_HEADER
        //{
        //    /// <summary>
        //    /// The architecture type of the computer. 
        //    /// An image file can only be run on the specified computer or a system that emulates the specified computer. 
        //    /// </summary>
        //    public MachineType Machine;

        //    /// <summary>
        //    /// The number of sections. 
        //    /// This indicates the size of the section table, which immediately follows the headers. 
        //    /// Note that the Windows loader limits the number of sections to 96.
        //    /// </summary>
        //    public ushort NumberOfSections;

        //    /// <summary>
        //    /// The low 32 bits of the time stamp of the image. 
        //    /// This represents the date and time the image was created by the linker. 
        //    /// The value is represented in the number of seconds elapsed since midnight (00:00:00), January 1, 1970, Universal Coordinated Time, according to the system clock.
        //    /// </summary>
        //    public uint TimeDateStamp;

        //    /// <summary>
        //    /// The offset of the symbol table, in bytes, or zero if no COFF symbol table exists.
        //    /// </summary>
        //    public uint PointerToSymbolTable;

        //    /// <summary>
        //    /// The number of symbols in the symbol table.
        //    /// </summary>
        //    public uint NumberOfSymbols;

        //    /// <summary>
        //    /// The size of the optional header, in bytes. This value should be 0 for object files.
        //    /// </summary>
        //    public ushort SizeOfOptionalHeader;

        //    /// <summary>
        //    /// The characteristics of the image.
        //    /// </summary>
        //    public FileCharFlags Characteristics;

        //}

        //private enum MachineType : ushort
        //{
        //    x86 = 0x014c,
        //    x64 = 0x8664,
        //    Itanium = 0x0200,
        //}

        [Flags]
        private enum FileCharFlags : ushort
        {
            /// <summary>
            /// Relocation information was stripped from the file. The file must be loaded at its preferred base address. If the base address is not available, the loader reports an error.
            /// </summary>
            RELOCS_STRIPPED = 0x0001,

            /// <summary>
            /// The file is executable (there are no unresolved external references).
            /// </summary>
            EXECUTABLE_IMAGE = 0x0002,

            /// <summary>
            /// COFF line numbers were stripped from the file.
            /// </summary>
            LINE_NUMS_STRIPPED = 0x0004,

            /// <summary>
            /// COFF symbol table entries were stripped from file.
            /// </summary>
            LOCAL_SYMS_STRIPPED = 0x0008,

            /// <summary>
            /// Aggressively trim the working set.This value is obsolete.
            /// </summary>
            AGGRESIVE_WS_TRIM = 0x0010,

            /// <summary>
            /// The application can handle addresses larger than 2 GB.
            /// </summary>
            LARGE_ADDRESS_AWARE = 0x0020,

            /// <summary>
            /// The bytes of the word are reversed.This flag is obsolete.
            /// </summary>
            BYTES_REVERSED_LO = 0x0080,

            /// <summary>
            /// The computer supports 32-bit words.
            /// </summary>
            x32BIT_MACHINE = 0x0100,

            /// <summary>
            /// Debugging information was removed and stored separately in another file.
            /// </summary>
            DEBUG_STRIPPED = 0x0200,

            /// <summary>
            /// If the image is on removable media, copy it to and run it from the swap file.
            /// </summary>
            REMOVABLE_RUN_FROM_SWAP = 0x0400,

            /// <summary>
            /// If the image is on the network, copy it to and run it from the swap file.
            /// </summary>
            NET_RUN_FROM_SWAP = 0x0800,

            /// <summary>
            /// The image is a system file.
            /// </summary>
            System = 0x1000,

            /// <summary>
            /// The image is a DLL file.While it is an executable file, it cannot be run directly.
            /// </summary>
            Dll = 0x2000,

            /// <summary>
            /// The file should be run only on a uniprocessor computer.
            /// </summary>
            UP_System_Only = 0x4000,

            /// <summary>
            /// The bytes of the word are reversed.This flag is obsolete.
            /// </summary>
            Bytes_Reversed_HI = 0x8000,
        }

        private enum FilePlatform : ushort
        {
            /// <summary>
            /// The file is an executable image.
            /// </summary>
            x86 = 0x10b,

            /// <summary>
            /// The file is an executable image.
            /// </summary>
            x64 = 0x20b,

            /// <summary>
            /// The file is a ROM image.
            /// </summary>
            ROM_OPTIONAL_HDR_MAGIC = 0x107,
        }

        private class LibraryIntPtr : SafeHandle
        {
            private bool _IsInvalid = false;
            public override bool IsInvalid => _IsInvalid;

            public LibraryIntPtr(IntPtr Handle) : base(Handle, true)
            {

            }

            protected override bool ReleaseHandle()
            {
                FreeLibrary(handle);
                _IsInvalid = true;
                return true;
            }
        }

        #endregion

        public string Path { get; }

        public LibraryType Type { get; }

        protected DynamicLibrary(string Path, LibraryType Type)
        {
            this.Path = Path;
            this.Type = Type;
        }

        public virtual void Dispose()
            => GC.SuppressFinalize(this);

        public static DynamicLibrary Load(string Path)
        {
            if (!File.Exists(Path))
                throw new FileNotFoundException(string.Empty, Path);

            LibraryType Type = ParseType(Path);

            if ((Type & LibraryType.Managed) > 0)
                return new ManagedLibrary(Path, Type);

            if (Environment.Is64BitProcess != (Type & LibraryType.x64) > 0)
                throw new BadImageFormatException($"This unmanaged library can't run in {(Environment.Is64BitProcess ? "x64" : "x86")} process.");

            IntPtr pLibrary = LoadLibrary(Path);
            if (pLibrary == IntPtr.Zero)
                throw new ApplicationException($"There was an error during dll loading : {Path}, ErrorCode : {Marshal.GetLastWin32Error()}");

            return new UnmanagedLibrary(Path, new LibraryIntPtr(pLibrary), Type);
        }
        public static bool TryLoad(string Path, out DynamicLibrary Library)
        {
            if (!File.Exists(Path))
            {
                Library = null;
                return false;
            }

            LibraryType Type = ParseType(Path);

            if ((Type & LibraryType.Managed) > 0)
            {
                Library = new ManagedLibrary(Path, Type);
                return true;
            }

            if (Environment.Is64BitProcess != (Type & LibraryType.x64) > 0)
            {
                Library = null;
                return false;
            }

            IntPtr pLibrary = LoadLibrary(Path);
            if (pLibrary == IntPtr.Zero)
            {
                Library = null;
                return false;
            }

            Library = new UnmanagedLibrary(Path, new LibraryIntPtr(pLibrary), Type);
            return true;
        }

        private static LibraryType ParseType(string Path)
        {
            // PE Struct
            //https://web.archive.org/web/20160202125049/http://blogs.msdn.com/b/kstanton/archive/2004/03/31/105060.aspx

            using FileStream s = new FileStream(Path, FileMode.Open);
            // Skip to DosHeader.e_lfanew
            s.Seek(60, SeekOrigin.Begin);

            // FileHeader Position
            int FileHeaderPosition = s.Read<int>() + sizeof(uint);

            // Skip to FileHeader.Characteristics
            s.Seek(FileHeaderPosition + 18, SeekOrigin.Begin);

            FileCharFlags FileFlags = s.Read<FileCharFlags>();
            FilePlatform p = s.Read<FilePlatform>();

            LibraryType r;
            switch (p)
            {
                case FilePlatform.x86:
                    // Skip to OptionalHeader.DataDirectory.Size
                    s.Seek(206 + sizeof(uint), SeekOrigin.Current);

                    r = LibraryType.x86;
                    break;
                case FilePlatform.x64:
                    // Skip to OptionalHeader.DataDirectory.Size
                    s.Seek(222 + sizeof(uint), SeekOrigin.Current);

                    r = LibraryType.x64;
                    break;
                case FilePlatform.ROM_OPTIONAL_HDR_MAGIC:
                default:
                    return LibraryType.Unknown;
            }
            int DataDirectorySize = s.Read<int>();

            return r | (DataDirectorySize > 0 ? LibraryType.Managed : LibraryType.Unmanaged);
        }

    }
}