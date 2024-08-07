using System;

[Flags]
public enum LibraryType
{
    Managed = 1,
    Unmanaged = 2,
    x86 = 4,
    x64 = 8,
    Unknown = 128
}