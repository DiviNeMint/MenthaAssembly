using System;

namespace MenthaAssembly.Win32
{
    [Flags]
    internal enum SHGetFileInfoFlags : uint
    {
        /// <summary>
		/// Get large Icon
		/// </summary>
        LargeIcon = 0x00000,

        /// <summary>
		/// Get small Icon
		/// </summary>
        SmallIcon = 0x00001,

        /// <summary>
		/// Get open Icon
		/// </summary>
        OpenIcon = 0x00002,

        /// <summary>
		/// Get shell size Icon
		/// </summary>
        ShellIconSize = 0x00004,

        /// <summary>
		/// pszPath is a pidl
		/// </summary>
        PIDL = 0x00008,

        /// <summary>
		/// use passed dwFileAttribute
		/// </summary>
        UseFileAttributes = 0x00010,

        /// <summary>
		/// apply the appropriate overlays
		/// </summary>
        AddOverlays = 0x00020,

        /// <summary>
		/// Get the index of the overlay in the upper 8 bits of the iIcon
		/// </summary>
        OverlayIndex = 0x00040,

        /// <summary>
		/// Get Icon
		/// </summary>
        Icon = 0x00100,

        /// <summary>
		/// Get display name
		/// </summary>
        DisplayName = 0x00200,

        /// <summary>
		/// Get type name
		/// </summary>
        TypeName = 0x00400,

        /// <summary>
		/// Get attributes
		/// </summary>
        Attributes = 0x00800,

        /// <summary>
		/// Get Icon location
		/// </summary>
        IconLocation = 0x01000,

        /// <summary>
		/// return exe type
		/// </summary>
        ExeType = 0x02000,

        /// <summary>
		/// Get system Icon index
		/// </summary>
        SysIconIndex = 0x04000,

        /// <summary>
		/// put a link overlay on Icon
		/// </summary>
        LinkOverlay = 0x08000,

        /// <summary>
		/// show Icon in selected state
		/// </summary>
        Selected = 0x10000,

        /// <summary>
		/// Get only specified attributes
		/// </summary>
        Attr_Specified = 0x20000,

    }
}
