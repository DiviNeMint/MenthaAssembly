using System;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;
using System.Threading;

namespace MenthaAssembly
{
    public abstract class DynamicLibrary(string Filename, LibraryType Type) : IDisposable
    {
        private static readonly ConcurrentDictionary<string, DynamicLibrary> LibraryPool = new(StringComparer.OrdinalIgnoreCase);

        private int _referenceCount = 1;

        public string Filename { get; } = Path.GetFullPath(Filename);

        public LibraryType Type { get; } = Type;

        internal int ReferenceCount
            => _referenceCount;

        public static DynamicLibrary Load(string Fullname)
            => Load(Fullname, null);
        public static DynamicLibrary Load(string Fullname, Func<AssemblyName, Assembly> Resolver)
        {
            if (!File.Exists(Fullname))
                throw new FileNotFoundException(string.Empty, Fullname);

            Fullname = Path.GetFullPath(Fullname);

            while (true)
            {
                if (LibraryPool.TryGetValue(Fullname, out DynamicLibrary existingLibrary))
                {
                    Interlocked.Increment(ref existingLibrary._referenceCount);
                    return existingLibrary;
                }

                LibraryType type = AssemblyHelper.GetLibraryType(Fullname);
                if ((type & LibraryType.Unknown) > 0)
                    throw new BadImageFormatException(string.Empty, Fullname);

                DynamicLibrary library;

                if ((type & LibraryType.Managed) > 0)
                {
                    library = new ManagedLibrary(Fullname, type, Resolver);
                }
                else
                {
                    if (Environment.Is64BitProcess != ((type & LibraryType.x64) > 0))
                        throw new BadImageFormatException($"This unmanaged library can't run in {(Environment.Is64BitProcess ? "x64" : "x86")} process.");

                    UnmanagedLibrary unmanaged = new(Fullname, type);
                    if (!unmanaged.IsLoaded)
                    {
                        unmanaged.OnDispose();
                        return null;
                    }

                    library = unmanaged;
                }

                if (LibraryPool.TryAdd(Fullname, library))
                    return library;

                library.OnDispose();
            }
        }
        public static bool TryLoad(string Fullname, out DynamicLibrary Library)
            => TryLoad(Fullname, out Library, null);
        public static bool TryLoad(string Fullname, out DynamicLibrary Library, Func<AssemblyName, Assembly> Resolver)
        {
            if (!File.Exists(Fullname))
            {
                Library = null;
                return false;
            }

            Fullname = Path.GetFullPath(Fullname);

            while (true)
            {
                if (LibraryPool.TryGetValue(Fullname, out DynamicLibrary existingLibrary))
                {
                    Interlocked.Increment(ref existingLibrary._referenceCount);
                    Library = existingLibrary;
                    return true;
                }

                LibraryType type = AssemblyHelper.GetLibraryType(Fullname);
                if ((type & LibraryType.Unknown) > 0)
                {
                    Library = null;
                    return false;
                }

                DynamicLibrary library;

                if ((type & LibraryType.Managed) > 0)
                {
                    library = new ManagedLibrary(Fullname, type, Resolver);
                }
                else
                {
                    if (Environment.Is64BitProcess != ((type & LibraryType.x64) > 0))
                    {
                        Library = null;
                        return false;
                    }

                    UnmanagedLibrary unmanaged = new(Fullname, type);
                    if (!unmanaged.IsLoaded)
                    {
                        unmanaged.OnDispose();
                        Library = null;
                        return false;
                    }

                    library = unmanaged;
                }

                if (LibraryPool.TryAdd(Fullname, library))
                {
                    Library = library;
                    return true;
                }

                library.OnDispose();
            }
        }

        private int _isDisposed;
        public void Dispose()
        {
            if (Interlocked.Exchange(ref _isDisposed, 1) != 0)
                return;

            if (LibraryPool.TryGetValue(Filename, out DynamicLibrary pooledLibrary) &&
                ReferenceEquals(pooledLibrary, this))
            {
                int remain = Interlocked.Decrement(ref _referenceCount);
                if (remain > 0)
                {
                    Interlocked.Exchange(ref _isDisposed, 0);
                    return;
                }

                LibraryPool.TryRemove(Filename, out _);
            }

            OnDispose();
            GC.SuppressFinalize(this);
        }
        protected abstract void OnDispose();

    }
}