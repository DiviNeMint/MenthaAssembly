using System;

namespace MenthaAssembly.Win32
{
    [Flags]
    internal enum WindowPaintFlags
    {
        /// <summary>
        /// Draws the window only if it is visible.
        /// </summary>
        CheckVisible = 0x01,

        /// <summary>
        /// Draws the nonclient area of the window.
        /// </summary>
        NonClient = 0x02,

        /// <summary>
        /// Draws the client area of the window.
        /// </summary>
        Client = 0x04,

        /// <summary>
        /// Erases the background before drawing the window.
        /// </summary>
        EraseBackground = 0x08,

        /// <summary>
        /// Draws all visible children windows.
        /// </summary>
        Children = 0x10,

        /// <summary>
        /// Draws all owned windows.
        /// </summary>
        Owned = 0x20,
    }
}
