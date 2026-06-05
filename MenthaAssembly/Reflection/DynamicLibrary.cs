using System;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;
using System.Threading;

namespace MenthaAssembly
{
    /// <summary>
    /// Represents a dynamically loaded managed or unmanaged library.
    /// </summary>
    /// <remarks>
    /// Libraries are pooled by full path. Loading the same path returns the same instance and increments an internal load count.
    /// Each successful load must be released by calling <see cref="Dispose"/>.
    /// </remarks>
    public abstract class DynamicLibrary(string Filename, LibraryType Type) : IDisposable
    {
        private static readonly ConcurrentDictionary<string, DynamicLibrary> LibraryPool = new(StringComparer.OrdinalIgnoreCase);

        private int _referenceCount = 1;

        /// <summary>
        /// Gets the full path of the loaded library file.
        /// </summary>
        public string Filename { get; } = Path.GetFullPath(Filename);

        /// <summary>
        /// Gets the detected library type.
        /// </summary>
        public LibraryType Type { get; } = Type;

        /// <summary>
        /// Loads the specified dynamic library.
        /// </summary>
        /// <remarks>
        /// The pool returns the same library instance for the same path.
        /// Each successful <see cref="Load(string)"/> or <see cref="TryLoad(string, out DynamicLibrary)"/> call must be paired with one <see cref="Dispose"/> call.
        /// </remarks>
        /// <param name="Fullname">The full path of the library file to load.</param>
        /// <returns>The loaded dynamic library.</returns>
        /// <exception cref="FileNotFoundException">The library file does not exist.</exception>
        /// <exception cref="BadImageFormatException">The file is not a supported dynamic library.</exception>
        public static DynamicLibrary Load(string Fullname)
            => Load(Fullname, null);
        /// <summary>
        /// Loads the specified dynamic library and uses the specified resolver for managed assembly dependencies.
        /// </summary>
        /// <remarks>
        /// The pool returns the same library instance for the same path.
        /// Each successful <see cref="Load(string, Func{AssemblyName, Assembly})"/> or <see cref="TryLoad(string, out DynamicLibrary, Func{AssemblyName, Assembly})"/> call must be paired with one <see cref="Dispose"/> call.
        /// </remarks>
        /// <param name="Fullname">The full path of the library file to load.</param>
        /// <param name="Resolver">The resolver used to resolve managed assembly dependencies.</param>
        /// <returns>The loaded dynamic library.</returns>
        /// <exception cref="FileNotFoundException">The library file does not exist.</exception>
        /// <exception cref="BadImageFormatException">The file is not a supported dynamic library.</exception>
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
        /// <summary>
        /// Attempts to load the specified dynamic library.
        /// </summary>
        /// <remarks>
        /// The pool returns the same library instance for the same path.
        /// Each successful <see cref="Load(string)"/> or <see cref="TryLoad(string, out DynamicLibrary)"/> call must be paired with one <see cref="Dispose"/> call.
        /// </remarks>
        /// <param name="Fullname">The full path of the library file to load.</param>
        /// <param name="Library">When this method returns, contains the loaded dynamic library if loading succeeded; otherwise, <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the library was loaded; otherwise, <see langword="false"/>.</returns>
        public static bool TryLoad(string Fullname, out DynamicLibrary Library)
            => TryLoad(Fullname, out Library, null);
        /// <summary>
        /// Attempts to load the specified dynamic library and uses the specified resolver for managed assembly dependencies.
        /// </summary>
        /// <remarks>
        /// The pool returns the same library instance for the same path.
        /// Each successful <see cref="Load(string, Func{AssemblyName, Assembly})"/> or <see cref="TryLoad(string, out DynamicLibrary, Func{AssemblyName, Assembly})"/> call must be paired with one <see cref="Dispose"/> call.
        /// </remarks>
        /// <param name="Fullname">The full path of the library file to load.</param>
        /// <param name="Library">When this method returns, contains the loaded dynamic library if loading succeeded; otherwise, <see langword="null"/>.</param>
        /// <param name="Resolver">The resolver used to resolve managed assembly dependencies.</param>
        /// <returns><see langword="true"/> if the library was loaded; otherwise, <see langword="false"/>.</returns>
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
        /// <summary>
        /// Releases one successful load reference for this library.
        /// </summary>
        /// <remarks>
        /// The underlying library is released only when the pooled load count reaches zero.
        /// </remarks>
        public void Dispose()
        {
            // Dispose releases one successful Load/TryLoad reference.
            // The real library is released only after the pooled count reaches zero.
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