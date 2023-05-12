using System;

namespace MenthaAssembly.Media.Imaging
{
    /// <summary>
    /// Indicates that the pixel has no alpha channel.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public sealed class NonAlphaAttribute : Attribute
    {

    }
}