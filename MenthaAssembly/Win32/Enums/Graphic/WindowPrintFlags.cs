using System;

namespace MenthaAssembly.Win32
{
    [Flags]
    internal enum WindowPrintFlags : uint
    {
        /// <summary>
        /// The entire window is copied to hdcBlt.
        /// </summary>
        EntireWindow = 0x00,

        /// <summary>
        /// Only the client area of the window is copied to hdcBlt.
        /// </summary>
        ClientOnly = 0x01,

        /// <summary>
        /// Render content.
        /// </summary>
        RenderFullContent = 0x02,
    }
}
