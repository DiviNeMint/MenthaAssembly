using System;

namespace MenthaAssembly.IO
{
    [AttributeUsage(AttributeTargets.Constructor)]
    public sealed class CodecConstructorAttribute(params string[] MemberNames) : Attribute
    {
        public string[] MemberNames { get; } = MemberNames;
    }
}