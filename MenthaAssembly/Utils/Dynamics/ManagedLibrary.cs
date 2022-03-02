using System;
using System.IO;
using System.Reflection;

namespace MenthaAssembly
{
    public sealed class ManagedLibrary : DynamicLibrary
    {
        private readonly AppDomain Domain;
        private readonly Assembly Assembly;
        internal ManagedLibrary(string Path, LibraryType Type) : base(Path, Type)
        {
            Domain = AppDomain.CreateDomain(Guid.NewGuid().ToString());
            //Domain.AssemblyResolve += OnDomainAssemblyResolve;

            byte[] RawDatas = File.ReadAllBytes(Path);

            Assembly = Domain.Load(RawDatas);
        }

        //private static Assembly OnDomainAssemblyResolve(object sender, ResolveEventArgs args) 
        //    => throw new NotImplementedException();

        public Type[] GetTypes()
            => Assembly.GetTypes();

        public override void Dispose()
        {
            AppDomain.Unload(Domain);
            base.Dispose();
        }
    }

}

