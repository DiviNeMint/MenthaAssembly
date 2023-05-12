using System;

namespace MenthaAssembly
{
    /// <summary>
    /// Indicates that the object is calculated.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = true)]
    public sealed class CalculatedAttribute : Attribute
    {

    }
}