using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
#if NET6_0_OR_GREATER
using System.Runtime.Loader;
#endif

namespace MenthaAssembly
{
    /// <summary>
    /// Represents a dynamically loaded managed assembly.
    /// </summary>
    public sealed class ManagedLibrary : DynamicLibrary
    {
        private readonly string RootFolder;
        private readonly Func<AssemblyName, Assembly> Resolver;
        private readonly List<DynamicLibrary> DependencyLibraries = [];

        /// <summary>
        /// Gets the root directory used to resolve local assembly dependencies.
        /// </summary>
        public string RootDirectory => RootFolder;

        /// <summary>
        /// Gets the full path of the loaded assembly file.
        /// </summary>
        public string Location => Filename;

        private Assembly _Assembly;
        /// <summary>
        /// Gets the loaded assembly instance.
        /// </summary>
        public Assembly Assembly => _Assembly;

        /// <summary>
        /// Gets the identity of the loaded assembly.
        /// </summary>
        public AssemblyName Name => _Assembly.GetName();

        /// <summary>
        /// Gets the display name of the loaded assembly.
        /// </summary>
        public string FullName => _Assembly.FullName;

#if NET6_0_OR_GREATER
        private readonly AssemblyLoadContext Context;
        internal ManagedLibrary(string Fullname, LibraryType Type, Func<AssemblyName, Assembly> Resolver = null) : base(Fullname, Type)
        {
            RootFolder = Path.GetDirectoryName(Fullname);
            this.Resolver = Resolver;

            Context = new AssemblyLoadContext(Guid.NewGuid().ToString(), true);
            Context.Resolving += OnResolving;

            using FileStream Stream = new(Fullname, FileMode.Open, FileAccess.Read);
            using MemoryStream Memory = new();
            Stream.CopyTo(Memory);
            Memory.Position = 0;

            _Assembly = Context.LoadFromStream(Memory);
        }

        private Assembly OnResolving(AssemblyLoadContext Context, AssemblyName Name)
        {
            string Filename = Path.Combine(RootFolder, $"{Name.Name}.dll");
            if (DynamicLibrary.TryLoad(Filename, out DynamicLibrary Library) &&
                Library is ManagedLibrary Managed)
            {
                DependencyLibraries.Add(Managed);
                return Managed._Assembly;
            }

            return Resolver?.Invoke(Name);
        }

        private bool IsDisposed;
        protected override void OnDispose()
        {
            if (IsDisposed)
                return;

            foreach (DynamicLibrary Library in DependencyLibraries)
                Library.Dispose();

            DependencyLibraries.Clear();
            Context?.Unload();
            _Assembly = null;
            IsDisposed = true;
        }
#else
        private readonly AppDomain Domain;
        internal ManagedLibrary(string Fullname, LibraryType Type, Func<AssemblyName, Assembly> Resolver = null) : base(Fullname, Type)
        {
            RootFolder = Path.GetDirectoryName(Fullname);
            this.Resolver = Resolver;

            Domain = AppDomain.CreateDomain(Guid.NewGuid().ToString());
            Domain.AssemblyResolve += OnDomainAssemblyResolve;

            byte[] RawDatas = File.ReadAllBytes(Fullname);
            _Assembly = Domain.Load(RawDatas);
        }

        private Assembly OnDomainAssemblyResolve(object sender, ResolveEventArgs e)
        {
            AssemblyName Name = new(e.Name);

            string Filename = Path.Combine(RootFolder, $"{Name.Name}.dll");
            if (DynamicLibrary.TryLoad(Filename, out DynamicLibrary Library) &&
                Library is ManagedLibrary Managed)
            {
                DependencyLibraries.Add(Managed);
                return Managed._Assembly;
            }

            return Resolver?.Invoke(Name);
        }

        private bool IsDisposed;
        protected override void OnDispose()
        {
            if (IsDisposed)
                return;

            foreach (DynamicLibrary Library in DependencyLibraries)
                Library.Dispose();

            DependencyLibraries.Clear();
            if (Domain != null)
                AppDomain.Unload(Domain);

            _Assembly = null;
            IsDisposed = true;
        }
#endif

        /// <summary>
        /// Gets the assemblies referenced by the loaded assembly.
        /// </summary>
        /// <returns>An array of referenced assembly names.</returns>
        public AssemblyName[] GetReferencedAssemblies()
            => _Assembly.GetReferencedAssemblies();

        /// <summary>
        /// Gets a custom attribute of the specified type from the loaded assembly.
        /// </summary>
        /// <typeparam name="T">The type of custom attribute to retrieve.</typeparam>
        /// <returns>The custom attribute if it exists; otherwise, <see langword="null"/>.</returns>
        public T GetCustomAttribute<T>()
            where T : Attribute
            => _Assembly.GetCustomAttribute<T>();

        /// <summary>
        /// Gets custom attributes of the specified type from the loaded assembly.
        /// </summary>
        /// <param name="AttributeType">The type of custom attribute to retrieve.</param>
        /// <param name="Inherit">A value that indicates whether inherited attributes should be searched.</param>
        /// <returns>An array that contains the matching custom attributes.</returns>
        public object[] GetCustomAttributes(Type AttributeType, bool Inherit = false)
            => _Assembly.GetCustomAttributes(AttributeType, Inherit);

        /// <summary>
        /// Gets all types defined in the loaded assembly.
        /// </summary>
        /// <returns>An array that contains the types defined in the assembly.</returns>
        public Type[] GetTypes()
            => _Assembly.GetTypes();

        /// <summary>
        /// Attempts to get the types defined in the loaded assembly.
        /// </summary>
        /// <param name="Types">When this method returns, contains the loaded types that could be resolved.</param>
        /// <returns><see langword="true"/> if all types were resolved; otherwise, <see langword="false"/>.</returns>
        public bool TryGetTypes(out Type[] Types)
        {
            try
            {
                Types = _Assembly.GetTypes();
                return true;
            }
            catch (ReflectionTypeLoadException ex)
            {
                Types = [.. ex.Types.Where(i => i != null)];
                return false;
            }
        }

        /// <summary>
        /// Enumerates managed and unmanaged dependency library information for the loaded assembly.
        /// </summary>
        /// <returns>A sequence of dependency library information.</returns>
        public IEnumerable<AssemblyInfo> EnumDependencyLibraryInfos()
        {
            string[] LoadedAssemblies = Assembly.GetEntryAssembly()
                                                .GetReferencedAssemblies()
                                                .Concat(Assembly.GetExecutingAssembly()
                                                                .GetReferencedAssemblies())
                                                .Select(i => i.Name)
                                                .Distinct()
                                                .ToArray();

            Dictionary<string, string> Managed = AssemblyHelper.GetDependencyManagedAssemblyPathTable(_Assembly, RootFolder);
            foreach (string Key in Managed.Keys.Where(i => LoadedAssemblies.Contains(i)).ToArray())
                Managed.Remove(Key);

            LibraryType ManagedType = (Environment.Is64BitProcess ? LibraryType.x64 : LibraryType.x86) | LibraryType.Managed;
            foreach (KeyValuePair<string, string> Data in Managed)
                yield return new AssemblyInfo(Data.Key, Data.Value, ManagedType);

            LibraryType UnmanagedType = (Environment.Is64BitProcess ? LibraryType.x64 : LibraryType.x86) | LibraryType.Unmanaged;
            HashSet<string> Unmanaged = new(_Assembly.GetUnmanagedDependencyAssemblyNames(), StringComparer.OrdinalIgnoreCase);
            foreach (string ManagedPath in Managed.Values)
            {
                if (!AssemblyHelper.TryGetUnmanagedDependencyAssemblyNames(ManagedPath, out string[] Names))
                    continue;

                foreach (string Name in Names)
                    Unmanaged.Add(Name);
            }

            foreach (string UnmanagedName in Unmanaged)
            {
                string FullName = Path.Combine(RootFolder, UnmanagedName);
                if (!File.Exists(FullName))
                    FullName = AssemblyHelper.GetUnmanagedLibraryFullName(UnmanagedName);

                if (!string.IsNullOrEmpty(FullName))
                    yield return new AssemblyInfo(UnmanagedName, FullName, UnmanagedType);
            }
        }

    }
}
