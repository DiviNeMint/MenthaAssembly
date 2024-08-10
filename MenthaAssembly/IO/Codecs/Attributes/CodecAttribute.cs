using System;

namespace MenthaAssembly.IO
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class CodecAttribute(Type CodecType) : Attribute
    {
        public Type CodecType { get; } = CodecType;
    }
}