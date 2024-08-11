using System;

namespace MenthaAssembly.IO
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class CodecIgnoreAttribute() : Attribute
    {
    }
}