using System;

namespace MenthaAssembly.Devices
{
    /// <summary>
    /// device capabilities indices
    /// </summary>
    [Flags]
    internal enum DeviceCapabilitiesFlags : short
    {
        /// <summary>
        /// Returns the dmFields member of the printer driver's DEVMODE structure. The dmFields member indicates which members in the device-independent portion of the structure are supported by the printer driver.
        /// </summary>
        Fields = 1,

        /// <summary>
        /// Retrieves a list of supported paper sizes. The pOutput buffer receives an array of WORD values that indicate the available paper sizes for the printer. The return value indicates the number of entries in the array. For a list of the possible array values, see the description of the dmPaperSize member of the DEVMODE structure. If pOutput is NULL, the return value indicates the required number of entries in the array.
        /// </summary>
        Papers = 2,

        /// <summary>
        /// Retrieves the dimensions, in tenths of a millimeter, of each supported paper size. The pOutput buffer receives an array of POINT structures. Each structure contains the width (x-dimension) and length (y-dimension) of a paper size as if the paper were in the DMORIENT_PORTRAIT orientation. The return value indicates the number of entries in the array.
        /// </summary>
        PaperSize = 3,

        /// <summary>
        /// Returns the minimum paper size that the dmPaperLength and dmPaperWidth members of the printer driver's DEVMODE structure can specify. The LOWORD of the return value contains the minimum dmPaperWidth value, and the HIWORD contains the minimum dmPaperLength value.
        /// </summary>
        MinExtent = 4,

        /// <summary>
        /// Returns the maximum paper size that the dmPaperLength and dmPaperWidth members of the printer driver's DEVMODE structure can specify. The LOWORD of the return value contains the maximum dmPaperWidth value, and the HIWORD contains the maximum dmPaperLength value.
        /// </summary>
        MaxExtent = 5,

        /// <summary>
        /// Retrieves a list of available paper bins. The pOutput buffer receives an array of WORD values that indicate the available paper sources for the printer. The return value indicates the number of entries in the array. For a list of the possible array values, see the description of the dmDefaultSource member of the DEVMODE structure. If pOutput is NULL, the return value indicates the required number of entries in the array.
        /// </summary>
        Bins = 6,

        /// <summary>
        /// If the printer supports duplex printing, the return value is 1; otherwise, the return value is zero. The pOutput parameter is not used.
        /// </summary>
        Duplex = 7,

        /// <summary>
        /// Returns the dmSize member of the printer driver's DEVMODE structure.
        /// </summary>
        Size = 8,

        /// <summary>
        /// Returns the number of bytes required for the device-specific portion of the DEVMODE structure for the printer driver.
        /// </summary>
        Extra = 9,

        /// <summary>
        /// Returns the specification version to which the printer driver conforms.
        /// </summary>
        Version = 10,

        /// <summary>
        /// Returns the version number of the printer driver.
        /// </summary>
        Driver = 11,

        /// <summary>
        /// Retrieves the names of the printer's paper bins. The pOutput buffer receives an array of string buffers. Each string buffer is 24 characters long and contains the name of a paper bin. The return value indicates the number of entries in the array. The name strings are null-terminated unless the name is 24 characters long. If pOutput is NULL, the return value is the number of bin entries required.
        /// </summary>
        BinNames = 12,

        /// <summary>
        /// Retrieves a list of the resolutions supported by the printer. The pOutput buffer receives an array of LONG values. For each supported resolution, the array contains a pair of LONG values that specify the x and y dimensions of the resolution, in dots per inch. The return value indicates the number of supported resolutions. If pOutput is NULL, the return value indicates the number of supported resolutions.
        /// </summary>
        EnumResolutions = 13,

        /// <summary>
        /// Retrieves the names of any additional files that need to be loaded when a driver is installed. The pOutput buffer receives an array of string buffers. Each string buffer is 64 characters long and contains the name of a file. The return value indicates the number of entries in the array. The name strings are null-terminated unless the name is 64 characters long. If pOutput is NULL, the return value is the number of files.
        /// </summary>
        FileDependencies = 14,

        /// <summary>
        /// Retrieves the abilities of the driver to use TrueType fonts. For TrueType, the pOutput parameter should be NULL. The return value can be one or more of the following:
        ///DCTT_BITMAP      Device can print TrueType fonts as graphics.
        ///DCTT_DOWNLOAD    Device can download TrueType fonts.
        ///DCTT_SUBDEV      Device can substitute device fonts for TrueType fonts.
        /// </summary>
        TrueType = 15,

        /// <summary>
        /// Retrieves a list of supported paper names (for example, Letter or Legal). The pOutput buffer receives an array of string buffers. Each string buffer is 64 characters long and contains the name of a paper form. The return value indicates the number of entries in the array. The name strings are null-terminated unless the name is 64 characters long. If pOutput is NULL, the return value is the number of paper forms.
        /// </summary>
        PaperNames = 16,

        /// <summary>
        /// Returns the relationship between portrait and landscape orientations for a device, in terms of the number of degrees that portrait orientation is rotated counterclockwise to produce landscape orientation. The return value can be one of the following:
        ///0    No landscape orientation.
        ///90   Portrait is rotated 90 degrees to produce landscape.
        ///270  Portrait is rotated 270 degrees to produce landscape.
        /// </summary>
        Orientation = 17,

        /// <summary>
        /// Returns the number of copies the device can print.
        /// </summary>
        Copies = 18,

        BinAdjuct = 19,

        EMFCompliant = 20,

        DataTypeProduced = 21,

        /// <summary>
        /// If the printer supports collating, the return value is 1; otherwise, the return value is zero. The pOutput parameter is not used.
        /// </summary>
        Collate = 22,

        Manufacturer = 23,

        Model = 24,

        /// <summary>
        /// Retrieves a list of printer description languages supported by the printer.
        /// The pOutput buffer receives an array of string buffers. Each buffer is 32 characters long and contains the name of a printer description language.
        /// The return value indicates the number of entries in the array.
        /// The name strings are null-terminated unless the name is 32 characters long.
        /// If pOutput is NULL, the return value indicates the required number of array entries.
        /// </summary>
        Personality = 25,

        /// <summary>
        /// The return value indicates the printer's print rate. The value returned for PrintRateUnit indicates the units of the PrintRate value. The pOutput parameter is not used.
        /// </summary>
        PrintRate = 26,

        /// <summary>
        /// The return value is one of the following values that indicate the print rate units for the value returned for the PrintRate flag. The pOutput parameter is not used.
        ///PrintRateUnit_CPS    Characters per second.
        ///PrintRateUnit_IPM    Inches per minute.
        ///PrintRateUnit_LPM    Lines per minute.
        ///PrintRateUnit_PPM    Pages per minute.
        /// </summary>
        PrintRateUnit = 27,

        /// <summary>
        /// The return value is the amount of available printer memory, in kilobytes. The pOutput parameter is not used.
        /// </summary>
        PrinterMem = 28,

        /// <summary>
        /// Retrieves the names of the paper forms that are currently available for use. The pOutput buffer receives an array of string buffers. Each string buffer is 64 characters long and contains the name of a paper form. The return value indicates the number of entries in the array. The name strings are null-terminated unless the name is 64 characters long. If pOutput is NULL, the return value is the number of paper forms.
        /// </summary>
        MediaReady = 29,

        /// <summary>
        /// If the printer supports stapling, the return value is a nonzero value; otherwise, the return value is zero. The pOutput parameter is not used.
        /// </summary>
        Staple = 30,

        /// <summary>
        /// The return value indicates the printer's print rate, in pages per minute. The pOutput parameter is not used.
        /// </summary>
        PrintRatePPM = 31,

        /// <summary>
        /// If the printer supports color printing, the return value is 1; otherwise, the return value is zero. The pOutput parameter is not used.
        /// </summary>
        ColorDevice = 32,

        /// <summary>
        /// Retrieves an array of integers that indicate that printer's ability to print multiple document pages per printed page. The pOutput buffer receives an array of DWORD values. Each value represents a supported number of document pages per printed page. The return value indicates the number of entries in the array. If pOutput is NULL, the return value indicates the required number of entries in the array.
        /// </summary>
        NUP = 33,

        /// <summary>
        /// Retrieves the names of the supported media types. The pOutput buffer receives an array of string buffers. Each string buffer is 64 characters long and contains the name of a supported media type. The return value indicates the number of entries in the array. The strings are null-terminated unless the name is 64 characters long. If pOutput is NULL, the return value is the number of media type names required. Windows 2000:  This flag is not supported.
        /// </summary>
        MediaTypeNames = 34,

        /// <summary>
        /// Retrieves a list of supported media types. The pOutput buffer receives an array of DWORD values that indicate the supported media types. The return value indicates the number of entries in the array. For a list of possible array values, see the description of the dmMediaType member of the DEVMODE structure. If pOutput is NULL, the return value indicates the required number of entries in the array. Windows 2000:  This flag is not supported.
        /// </summary>
        MediaTypes = 35
    }
}
