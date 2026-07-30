using System;

namespace MenthaAssembly.IO
{
    public sealed class CodecTypeResolveEventArgs(string AssemblyName, string TypeName) : EventArgs
    {
        public string AssemblyName { get; } = AssemblyName;

        public string TypeName { get; } = TypeName;

        public Type ResolvedType { get; set; }

    }
}
