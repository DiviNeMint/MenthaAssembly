using System;

namespace MenthaAssembly
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = true, AllowMultiple = false)]
    public sealed class CalculatedAttribute : Attribute
    {

    }
}