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
    public sealed class ManagedLibrary : DynamicLibrary
    {
        private readonly string RootFolder;
        private readonly Assembly Assembly;
        private readonly Func<AssemblyName, Assembly> Resolver;

        public string RootDirectory => RootFolder;

        public string Location => Filename;

        public AssemblyName Name => Assembly.GetName();

        public string FullName => Assembly.FullName;

#if NET6_0_OR_GREATER
        private readonly AssemblyLoadContext Context;
        internal ManagedLibrary(string Fullname, LibraryType Type, Func<AssemblyName, Assembly> Resolver = null) : base(Fullname, Type)
        {
            RootFolder = Path.GetDirectoryName(Fullname);
            this.Resolver = Resolver;

            if (Resolver is null)
            {
                AssemblyName targetName = AssemblyName.GetAssemblyName(Fullname);
                Assembly hostAssembly = AppDomain.CurrentDomain.GetAssemblies()
                                                               .FirstOrDefault(i => string.Equals(i.FullName, targetName.FullName, StringComparison.Ordinal));
                if (hostAssembly != null)
                {
                    Assembly = hostAssembly;
                    return;
                }
            }

            Context = new AssemblyLoadContext(Guid.NewGuid().ToString(), true);
            Context.Resolving += OnResolving;

            using FileStream Stream = new(Fullname, FileMode.Open, FileAccess.Read);
            using MemoryStream Memory = new();
            Stream.CopyTo(Memory);
            Memory.Position = 0;

            Assembly = Context.LoadFromStream(Memory);
        }

        private Assembly OnResolving(AssemblyLoadContext Context, AssemblyName Name)
        {
            string Filename = Path.Combine(RootFolder, $"{Name.Name}.dll");
            if (File.Exists(Filename))
                return Context.LoadFromAssemblyPath(Filename);

            return Resolver?.Invoke(Name);
        }

        private bool IsDisposed;
        protected override void OnDispose()
        {
            if (IsDisposed)
                return;

            Context?.Unload();
            IsDisposed = true;
        }
#else
        private readonly AppDomain Domain;
        internal ManagedLibrary(string Fullname, LibraryType Type, Func<AssemblyName, Assembly> Resolver = null) : base(Fullname, Type)
        {
            RootFolder = Path.GetDirectoryName(Fullname);
            this.Resolver = Resolver;

            if (Resolver is null)
            {
                AssemblyName targetName = AssemblyName.GetAssemblyName(Fullname);
                Assembly hostAssembly = AppDomain.CurrentDomain.GetAssemblies()
                                                               .FirstOrDefault(i => string.Equals(i.FullName, targetName.FullName, StringComparison.Ordinal));
                if (hostAssembly != null)
                {
                    Assembly = hostAssembly;
                    return;
                }
            }

            Domain = AppDomain.CreateDomain(Guid.NewGuid().ToString());
            Domain.AssemblyResolve += OnDomainAssemblyResolve;

            byte[] RawDatas = File.ReadAllBytes(Fullname);
            Assembly = Domain.Load(RawDatas);
        }

        private Assembly OnDomainAssemblyResolve(object sender, ResolveEventArgs e)
        {
            AssemblyName Name = new(e.Name);

            string Filename = Path.Combine(RootFolder, $"{Name.Name}.dll");
            if (File.Exists(Filename))
            {
                byte[] RawDatas = File.ReadAllBytes(Filename);
                return Domain.Load(RawDatas);
            }

            return Resolver?.Invoke(Name);
        }

        private bool IsDisposed;
        protected override void OnDispose()
        {
            if (IsDisposed)
                return;

            if (Domain != null)
                AppDomain.Unload(Domain);

            IsDisposed = true;
        }
#endif

        public AssemblyName[] GetReferencedAssemblies()
            => Assembly.GetReferencedAssemblies();

        public T GetCustomAttribute<T>()
            where T : Attribute
            => Assembly.GetCustomAttribute<T>();

        public object[] GetCustomAttributes(Type AttributeType, bool Inherit = false)
            => Assembly.GetCustomAttributes(AttributeType, Inherit);

        public Type[] GetTypes()
            => Assembly.GetTypes();

        public bool TryGetTypes(out Type[] Types)
        {
            try
            {
                Types = Assembly.GetTypes();
                return true;
            }
            catch (ReflectionTypeLoadException ex)
            {
                Types = [.. ex.Types.Where(i => i != null)];
                return false;
            }
        }

        public IEnumerable<AssemblyInfo> EnumDependencyLibraryInfos()
        {
            string[] LoadedAssemblies = Assembly.GetEntryAssembly()
                                                .GetReferencedAssemblies()
                                                .Concat(Assembly.GetExecutingAssembly()
                                                                .GetReferencedAssemblies())
                                                .Select(i => i.Name)
                                                .Distinct()
                                                .ToArray();

            Dictionary<string, string> Managed = AssemblyHelper.GetDependencyManagedAssemblyPathTable(Assembly, RootFolder);
            foreach (string Key in Managed.Keys.Where(i => LoadedAssemblies.Contains(i)).ToArray())
                Managed.Remove(Key);

            LibraryType ManagedType = (Environment.Is64BitProcess ? LibraryType.x64 : LibraryType.x86) | LibraryType.Managed;
            foreach (KeyValuePair<string, string> Data in Managed)
                yield return new AssemblyInfo(Data.Key, Data.Value, ManagedType);

            LibraryType UnmanagedType = (Environment.Is64BitProcess ? LibraryType.x64 : LibraryType.x86) | LibraryType.Unmanaged;
            IEnumerable<string> Unmanaged = Assembly.GetUnmanagedDependencyAssemblyNames();

            foreach (string ManagedPath in Managed.Values)
            {
                try
                {
                    Assembly Dependency = Assembly.LoadFrom(ManagedPath);
                    Unmanaged = Unmanaged.Concat(Dependency.GetUnmanagedDependencyAssemblyNames());
                }
                catch
                {
                }
            }

            foreach (string UnmanagedName in Unmanaged.Distinct(StringComparer.OrdinalIgnoreCase))
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