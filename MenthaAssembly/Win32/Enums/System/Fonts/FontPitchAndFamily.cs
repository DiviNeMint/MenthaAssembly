using System;

namespace MenthaAssembly.Win32
{
    [Flags]
    internal enum FontPitchAndFamily : byte
    {
        Default = 0,
        Fixed = 1,
        Variable = 2,

        Roman = 16,
        Swiss = 32,
        Modern = 48,
        Script = 64,
        DecoRative = 80,
    }
}
