using System;

namespace MenthaAssembly.Win32
{
    /// <summary>
    /// Values to pass to the <see cref="Graphic.GetDCEx"/>.
    /// </summary>
    [Flags]
    internal enum DeviceContextFlags : uint
    {
        /// <summary>
        /// Returns a DC that corresponds to the window rectangle rather than the client rectangle.
        /// </summary>
        Window = 0x000001,

        /// <summary>
        /// Returns a DC from the cache, rather than the OWNDC or CLASSDC window. 
        /// Essentially overrides CS_OWNDC and CS_CLASSDC.
        /// </summary>
        Cache = 0x000002,

        /// <summary>
        /// Does not reset the attributes of this DC to the default attributes when this DC is released.
        /// </summary>
        NoResetAttrs = 0x000004,

        /// <summary>
        /// Excludes the visible regions of all child windows below the window identified by hWnd.
        /// </summary>
        ClipChildren = 0x000008,

        /// <summary>
        /// Excludes the visible regions of all sibling windows above the window identified by hWnd.
        /// </summary>
        ClipSiblings = 0x000010,

        /// <summary>
        /// Uses the visible region of the parent window. 
        /// The parent's WS_CLIPCHILDREN and CS_PARENTDC style bits are ignored. 
        /// The origin is set to the upper-left corner of the window identified by hWnd.
        /// </summary>
        ParentClip = 0x000020,

        /// <summary>
        /// The clipping region identified by hrgnClip is excluded from the visible region of the returned DC.
        /// </summary>
        ExcludeRgn = 0x000040,

        /// <summary>
        /// The clipping region identified by hrgnClip is intersected with the visible region of the returned DC.
        /// </summary>
        IntersectRgn = 0x000080,

        /// <summary>
        /// Unknown...Undocumented
        /// </summary>
        ExcludeUpdate = 0x000100,

        /// <summary>
        /// Unknown...Undocumented
        /// </summary>
        IntersectUpdate = 0x000200,

        /// <summary>
        /// Allows drawing even if there is a LockWindowUpdate in effect that would otherwise exclude this window. 
        /// Used for drawing during tracking.
        /// </summary>
        LockWindowUpdate = 0x000400,

        /// <summary>
        /// When specified with <see cref="IntersectUpdate"/>, causes the DC to be completely validated. 
        /// Using this function with both <see cref="IntersectUpdate"/> and <see cref="Validate"/> is identical to using the BeginPaint function.
        /// </summary>
        Validate = 0x200000,

    }
}
