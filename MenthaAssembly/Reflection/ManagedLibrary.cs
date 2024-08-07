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
        private readonly Assembly Assembly;
        private readonly string RootFolder;

#if NET6_0_OR_GREATER
        private readonly AssemblyLoadContext Context;
        internal ManagedLibrary(string Fullname, LibraryType Type) : base(Fullname, Type)
        {
            RootFolder = Path.GetDirectoryName(Fullname);

            Context = new AssemblyLoadContext(Guid.NewGuid().ToString(), true);
            Context.Resolving += OnResolving;
            Assembly = Context.LoadFromAssemblyPath(Filename);
        }
        private Assembly OnResolving(AssemblyLoadContext Context, AssemblyName Name)
        {
            string Filename = $"{Path.Combine(RootFolder, Name.Name)}.dll";
            if (File.Exists(Filename))
                return Context.LoadFromAssemblyPath(Filename);

            return null;
        }

        private bool IsDisposed;
        protected override void Dispose(bool IsDisposing)
        {
            if (!IsDisposed)
            {
                if (IsDisposing)
                    Context.Unload();

                IsDisposed = true;
            }
        }

#else
        private readonly AppDomain Domain;
        internal ManagedLibrary(string FullName, LibraryType Type) : base(FullName, Type)
        {
            RootFolder = Path.GetDirectoryName(FullName);

            Domain = AppDomain.CreateDomain(Guid.NewGuid().ToString());
            Domain.AssemblyResolve += OnDomainAssemblyResolve;

            byte[] RawDatas = File.ReadAllBytes(FullName);
            Assembly = Domain.Load(RawDatas);
        }
        private Assembly OnDomainAssemblyResolve(object sender, ResolveEventArgs e)
        {
            string Filename = $"{Path.Combine(RootFolder, e.Name)}.dll";
            if (File.Exists(Filename))
            {
                byte[] RawDatas = File.ReadAllBytes(Filename);
                return Domain.Load(RawDatas);
            }

            return null;
        }

        private bool IsDisposed;
        protected override void Dispose(bool IsDisposing)
        {
            if (!IsDisposed)
            {
                if (IsDisposing)
                    AppDomain.Unload(Domain);

                IsDisposed = true;
            }
        }
#endif

        public Type[] GetTypes()
            => Assembly.GetTypes();

        public IEnumerable<AssemblyInfo> EnumDependencyLibrarieyInfos()
        {
            string[] LoadedAssemblies = Assembly.GetEntryAssembly()
                                                .GetReferencedAssemblies()
                                                .Concat(Assembly.GetExecutingAssembly()
                                                                .GetReferencedAssemblies())
                                                .Select(i => i.Name)
                                                .Distinct()
                                                .ToArray();

            // Managed
            Dictionary<string, Assembly> Managed =
#if NET6_0_OR_GREATER
                AssemblyHelper.GetDependencyManagedNonSystemAssemblyTable(Assembly, Context);
#else
                AssemblyHelper.GetDependencyManagedNonSystemAssemblyTable(Assembly, Domain);
#endif
            foreach (string Key in Managed.Keys.Where(i => LoadedAssemblies.Contains(i)))
                Managed.Remove(Key);

            LibraryType ManagedType = (Environment.Is64BitProcess ? LibraryType.x64 : LibraryType.x86) | LibraryType.Managed;
            foreach (KeyValuePair<string, Assembly> Data in Managed)
                yield return new AssemblyInfo(Data.Key, Data.Value.Location, ManagedType);

            // Unmanage
            LibraryType UnmanagedType = (Environment.Is64BitProcess ? LibraryType.x64 : LibraryType.x86) | LibraryType.Unmanaged;
            IEnumerable<string> Unmanaged = Assembly.GetUnmanagedDependencyAssemblyNames()
                                                    .Concat(Managed.Values.TrySelectMany(i => i.GetUnmanagedDependencyAssemblyNames()));

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