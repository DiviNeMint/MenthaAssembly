using System;

namespace MenthaAssembly.Devices
{
    [Flags]
    public enum PrinterFlags
    {
        Default = 0x00000001,

        /// <summary>
        /// If the <see cref="Name"/> is not also passed, the function ignores the Name parameter, and enumerates the locally installed printers. <para/>
        /// If <see cref="Name"/> is also passed, the function enumerates the local printers on Name.
        /// </summary>
        Local = 0x00000002,

        /// <summary>
        /// The function enumerates the list of printers to which the user has made previous connections.
        /// </summary>
        Connections = 0x00000004,

        //Favorite = 0x00000004,

        /// <summary>
        /// The function enumerates the printer identified by Name.<para/>
        /// This can be a server, a domain, or a print provider.<para/>
        /// If Name is NULL, the function enumerates available print providers.
        /// </summary>
        Name = 0x00000008,

        /// <summary>
        /// The function enumerates network printers and print servers in the computer's domain.<para/>
        /// </summary>
        Remote = 0x00000010,

        /// <summary>
        /// The function enumerates printers that have the shared attribute.
        /// Cannot be used in isolation; use an OR operation to combine with another PRINTER_ENUM type.
        /// </summary>
        Shared = 0x00000020,

        /// <summary>
        /// The function enumerates network printers in the computer's domain. 
        /// </summary>
        Network = 0x00000040,

        //Expand = 0x00004000,
        //Container = 0x00008000,
        //IconMask = 0x00ff0000,
        //Icon1 = 0x00010000,
        //Icon2 = 0x00020000,
        //Icon3 = 0x00040000,
        //Icon4 = 0x00080000,
        //Icon5 = 0x00100000,
        //Icon6 = 0x00200000,
        //Icon7 = 0x00400000,
        //Icon8 = 0x00800000,
        //Hide = 0x01000000,

        /// <summary>
        /// The function enumerates all print devices, including 3D printers.
        /// </summary>
        Category_ALL = 0x02000000,

        /// <summary>
        /// The function enumerates only 3D printers.
        /// </summary>
        Category_3D = 0x04000000
    }
}