using System;
using System.IO;
using System.Reflection;

namespace MenthaAssembly
{
    public abstract unsafe class DynamicLibrary(string Filename, LibraryType Type) : IDisposable
    {
        public string Filename { get; } = Filename;

        public LibraryType Type { get; } = Type;

        protected abstract void Dispose(bool IsDisposing);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public static DynamicLibrary Load(string Fullname)
        {
            if (!File.Exists(Fullname))
                throw new FileNotFoundException(string.Empty, Fullname);

            LibraryType Type = AssemblyHelper.GetLibraryType(Fullname);
            if ((Type & LibraryType.Unknown) > 0)
                throw new BadImageFormatException(string.Empty, Fullname);

            if ((Type & LibraryType.Managed) > 0)
                return new ManagedLibrary(Fullname, Type);

            if (Environment.Is64BitProcess != (Type & LibraryType.x64) > 0)
                throw new BadImageFormatException($"This unmanaged library can't run in {(Environment.Is64BitProcess ? "x64" : "x86")} process.");

            UnmanagedLibrary tLibrary = new(Fullname, Type);
            return tLibrary.IsLoaded ? tLibrary : null;
        }
        public static bool TryLoad(string Fullname, out DynamicLibrary Library)
        {
            if (!File.Exists(Fullname))
            {
                Library = null;
                return false;
            }

            LibraryType Type = AssemblyHelper.GetLibraryType(Fullname);
            if ((Type & LibraryType.Unknown) > 0)
            {
                Library = null;
                return false;
            }

            if ((Type & LibraryType.Managed) > 0)
            {
                Library = new ManagedLibrary(Fullname, Type);
                return true;
            }

            if (Environment.Is64BitProcess != (Type & LibraryType.x64) > 0)
            {
                Library = null;
                return false;
            }

            UnmanagedLibrary tLibrary = new(Fullname, Type);
            if (!tLibrary.IsLoaded)
            {
                Library = null;
                return false;
            }

            Library = tLibrary;
            return true;
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