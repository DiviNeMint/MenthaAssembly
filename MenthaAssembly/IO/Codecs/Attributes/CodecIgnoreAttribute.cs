using System;

namespace MenthaAssembly.IO
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Constructor)]
    public sealed class CodecIgnoreAttribute() : Attribute
    {
    }
}