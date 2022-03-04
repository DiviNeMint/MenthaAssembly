using MenthaAssembly.Win32;
using System;
using System.IO;

namespace MenthaAssembly
{
    public unsafe partial class DynamicLibrary : IDisposable
    {
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

            UnmanagedLibrary tLibrary = new UnmanagedLibrary(Path, Type);
            return tLibrary.IsLoaded ? tLibrary : null;

            //IntPtr pLibrary = LoadLibrary(Path);
            //if (pLibrary == IntPtr.Zero)
            //    throw new ApplicationException($"There was an error during dll loading : {Path}, ErrorCode : {Marshal.GetLastWin32Error()}");

            //return new UnmanagedLibrary(Path, new LibraryIntPtr(pLibrary), Type);
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

            UnmanagedLibrary tLibrary = new UnmanagedLibrary(Path, Type);
            if (!tLibrary.IsLoaded)
            {
                Library = null;
                return false;
            }

            Library = tLibrary;
            return true;

            //IntPtr pLibrary = LoadLibrary(Path);
            //if (pLibrary == IntPtr.Zero)
            //{
            //    Library = null;
            //    return false;
            //}

            //Library = new UnmanagedLibrary(Path, new LibraryIntPtr(pLibrary), Type);
            //return true;
        }

        private static LibraryType ParseType(string Path)
        {
            // PE Struct
            //https://web.archive.org/web/20160202125049/http://blogs.msdn.com/b/kstanton/archive/2004/03/31/105060.aspx

            using FileStream s = new FileStream(Path, FileMode.Open, FileAccess.Read);
            // Skip to DosHeader.e_lfanew
            s.Seek(60, SeekOrigin.Begin);

            // FileHeader Position
            int FileHeaderPosition = s.Read<int>() + sizeof(uint);

            // Skip to FileHeader.Characteristics
            s.Seek(FileHeaderPosition + 18, SeekOrigin.Begin);

            ImageFileCharFlags FileFlags = s.Read<ImageFileCharFlags>();
            if ((FileFlags & ImageFileCharFlags.Dll) == 0)
                return LibraryType.Unknown;

            ImageOptionalMagicType p = s.Read<ImageOptionalMagicType>();

            LibraryType r;
            switch (p)
            {
                case ImageOptionalMagicType.HDR32_MAGIC:
                    // Skip to OptionalHeader.DataDirectory.Size
                    s.Seek(206 + sizeof(uint), SeekOrigin.Current);

                    r = LibraryType.x86;
                    break;
                case ImageOptionalMagicType.HDR64_MAGIC:
                    // Skip to OptionalHeader.DataDirectory.Size
                    s.Seek(222 + sizeof(uint), SeekOrigin.Current);

                    r = LibraryType.x64;
                    break;
                case ImageOptionalMagicType.ROM_OPTIONAL_HDR_MAGIC:
                default:
                    return LibraryType.Unknown;
            }
            int DataDirectorySize = s.Read<int>();

            return r | (DataDirectorySize > 0 ? LibraryType.Managed : LibraryType.Unmanaged);
        }

        #region For Unmanaged Library Window API Version (Old)
        //private class LibraryIntPtr : SafeHandle
        //{
        //    private bool _IsInvalid = false;
        //    public override bool IsInvalid => _IsInvalid;

        //    public LibraryIntPtr(IntPtr Handle) : base(Handle, true)
        //    {

        //    }

        //    protected override bool ReleaseHandle()
        //    {
        //        FreeLibrary(handle);
        //        _IsInvalid = true;
        //        return true;
        //    }
        //}

        #endregion

    }
}