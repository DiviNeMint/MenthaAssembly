using System;

namespace MenthaAssembly.Devices
{
    [Flags]
    internal enum PrinterModeFlags
    {
        SPECVERSION = 0x0401,
        Orientation = 0x00000001,
        PaperSize = 0x00000002,
        PaperLength = 0x00000004,
        PaperWidth = 0x00000008,
        Scale = 0x00000010,
        Copies = 0x00000100,
        DefaultSource = 0x00000200,
        PrintQuality = 0x00000400,
        Color = 0x00000800,
        Duplex = 0x00001000,
        YResolution = 0x00002000,
        TTOption = 0x00004000,
        Collate = 0x00008000,
        FormName = 0x00010000,
        LogPixels = 0x00020000,
        BitsPerPel = 0x00040000,
        PelsWidth = 0x00080000,
        PelsHeight = 0x00100000,
        DisplayFlags = 0x00200000,
        DisplayFrequency = 0x00400000,
        PanningWidth = 0x00800000,
        PanningHeight = 0x01000000,
        ICMMethod = 0x02000000,
        ICMIntent = 0x04000000,
        MediaType = 0x08000000,
        DitherType = 0x10000000,
        ICCManufacturer = 0x20000000,
        ICCModel = 0x40000000,
    }
}
