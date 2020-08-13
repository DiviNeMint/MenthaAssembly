using System;

namespace MenthaAssembly.Win32
{
    [Flags]
    public enum WindowPosFlags : uint
    {
        /// <summary>
        /// Retains the current size (ignores the cx and cy members). 
        /// </summary>
        NoSize = 0x0001,

        /// <summary>
        /// Retains the current position (ignores the x and y members). 
        /// </summary>
        NoMove = 0x0002,

        /// <summary>
        /// Retains the current Z order (ignores the hwndInsertAfter member). 
        /// </summary>
        NoZOrder = 0x0004,

        /// <summary>
        /// Does not redraw changes. If this flag is set, no repainting of any kind occurs. This applies to the client area, the nonclient area (including the title bar and scroll bars), and any part of the parent window uncovered as a result of the window being moved. When this flag is set, the application must explicitly invalidate or redraw any parts of the window and parent window that need redrawing. 
        /// </summary>
        NoRedraw = 0x0008,


        /// <summary>
        /// Does not activate the window. If this flag is not set, the window is activated and moved to the top of either the topmost or non-topmost group (depending on the setting of the hwndInsertAfter member). 
        /// </summary>
        NoActivate = 0x0010,

        /// <summary>
        /// Draws a frame (defined in the window's class description) around the window. Same as the SWP_FrameChanged flag. 
        /// </summary>
        DrawFrame = 0x0020,

        /// <summary>
        /// Sends a WM_NCCALCSize message to the window, even if the window's size is not being changed. If this flag is not specified, WM_NCCALCSize is sent only when the window's size is being changed. 
        /// </summary>
        FrameChanged = 0x0020,

        /// <summary>
        /// Displays the window. 
        /// </summary>
        ShowWindow = 0x0040,

        /// <summary>
        /// Hides the window. 
        /// </summary>
        HideWindow = 0x0080,


        /// <summary>
        /// Discards the entire contents of the client area. If this flag is not specified, the valid contents of the client area are saved and copied back into the client area after the window is sized or repositioned. 
        /// </summary>
        NoCopyBits = 0x0100,

        /// <summary>
        /// Does not change the owner window's position in the Z order. 
        /// </summary>
        NoOwnerZOrder = 0x0200,

        /// <summary>
        /// Does not change the owner window's position in the Z order. Same as the SWP_NoOwnerZOrder flag. 
        /// </summary>
        NoReposition = 0x0200,

        /// <summary>
        /// Prevents the window from receiving the WM_WindowPOSChanging message. 
        /// </summary>
        NoSendChanging = 0x0400,

    }
}
