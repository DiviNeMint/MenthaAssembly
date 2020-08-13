using System;

namespace MenthaAssembly.Win32
{
    [Flags]
    public enum WindowExStyles : uint
    {
        /// <summary>
        /// Specifies a window that accepts drag-drop files.
        /// </summary>
        AcceptFiles = 0x00000010,

        /// <summary>
        /// Forces a top-level window onto the taskbar when the window is visible.
        /// </summary>
        AppWindow = 0x00040000,

        /// <summary>
        /// Specifies a window that has a border with a sunken edge.
        /// </summary>
        ClientEdge = 0x00000200,

        /// <summary>
        /// Specifies a window that paints all descendants in bottom-to-top painting order using double-buffering.
        /// This cannot be used if the window has a class style of either CS_OWNDC or CS_CLASSDC. This style is not supported in Windows 2000.
        /// </summary>
        /// <remarks>
        /// With WS_EX_Composited set, all descendants of a window get bottom-to-top painting order using double-buffering.
        /// Bottom-to-top painting order allows a descendent window to have translucency (alpha) and transparency (color-key) effects,
        /// but only if the descendent window also has the WS_EX_Transparent bit set.
        /// Double-buffering allows the window and its descendents to be painted without flicker.
        /// </remarks>
        Composited = 0x02000000,

        /// <summary>
        /// Specifies a window that includes a question mark in the title bar. When the user clicks the question mark,
        /// the cursor changes to a question mark with a pointer. If the user then clicks a child window, the child receives a WM_Help message.
        /// The child window should pass the message to the parent window procedure, which should call the WinHelp function using the Help_WM_Help command.
        /// The Help application displays a pop-up window that typically contains help for the child window.
        /// WS_EX_ContextHelp cannot be used with the WS_MAXIMIZEBOX or WS_MINIMIZEBOX styles.
        /// </summary>
        ContextHelp = 0x00000400,

        /// <summary>
        /// Specifies a window which contains child windows that should take part in dialog box navigation.
        /// If this style is specified, the dialog manager recurses into children of this window when performing navigation operations
        /// such as handling the TAB key, an arrow key, or a keyboard mnemonic.
        /// </summary>
        ControlParent = 0x00010000,

        /// <summary>
        /// Specifies a window that has a double border.
        /// </summary>
        DLGMODALFrame = 0x00000001,

        /// <summary>
        /// Specifies a window that is a layered window.
        /// This cannot be used for child windows or if the window has a class style of either CS_OWNDC or CS_CLASSDC.
        /// </summary>
        Layered = 0x00080000,

        /// <summary>
        /// Specifies a window with the horizontal origin on the right edge. Increasing horizontal values advance to the left.
        /// The shell language must support reading-order alignment for this to take effect.
        /// </summary>
        LayoutRTL = 0x00400000,

        /// <summary>
        /// Specifies a window that has generic left-aligned properties. This is the default.
        /// </summary>
        Left = 0x00000000,

        /// <summary>
        /// Specifies a window with the vertical scroll bar (if present) to the left of the client area.
        /// The shell language must support reading-order alignment for this to take effect.
        /// </summary>
        LeftScrollBar = 0x00004000,

        /// <summary>
        /// Specifies a window that displays text using left-to-right reading-order properties. 
        /// This is the default.
        /// </summary>
        LTRReading = 0x00000000,

        /// <summary>
        /// Specifies a multiple-document interface (MDI) child window.
        /// </summary>
        MDIChild = 0x00000040,

        /// <summary>
        /// Specifies a top-level window created with this style does not become the foreground window when the user clicks it.
        /// The system does not bring this window to the foreground when the user minimizes or closes the foreground window.
        /// The window does not appear on the taskbar by default. To force the window to appear on the taskbar, use the WS_EX_AppWindow style.
        /// To activate the window, use the SetActiveWindow or SetForegroundWindow function.
        /// </summary>
        NoActivate = 0x08000000,

        /// <summary>
        /// Specifies a window which does not pass its window layout to its child windows.
        /// </summary>
        NoInheritLayout = 0x00100000,

        /// <summary>
        /// Specifies that a child window created with this style does not send the WM_ParentNotify message to its parent window when it is created or destroyed.
        /// </summary>
        NoParentNotify = 0x00000004,

        /// <summary>
        /// The window does not render to a redirection surface.
        /// This is for windows that do not have visible content or that use mechanisms other than surfaces to provide their visual.
        /// </summary>
        NoRedirectionBitmap = 0x00200000,

        /// <summary>
        /// Specifies an overlapped window.
        /// </summary>
        OverLappedWindow = WindowEdge | ClientEdge,

        /// <summary>
        /// Specifies a palette window, which is a modeless dialog box that presents an array of commands.</summary>
        PaletteWindow = WindowEdge | ToolWindow | TopMost,

        /// <summary>
        /// Specifies a window that has generic "right-aligned" properties. This depends on the window class.
        /// The shell language must support reading-order alignment for this to take effect.
        /// Using the WS_EX_Right style has the same effect as using the SS_Right (static), ES_Right (edit), and BS_Right/BS_RightBUTTON (button) control styles.
        /// </summary>
        Right = 0x00001000,

        /// <summary>
        /// Specifies a window with the vertical scroll bar (if present) to the right of the client area. This is the default.</summary>
        RightScrollBar = 0x00000000,

        /// <summary>
        /// Specifies a window that displays text using right-to-left reading-order properties.
        /// The shell language must support reading-order alignment for this to take effect.
        /// </summary>
        RTLReading = 0x00002000,

        /// <summary>
        /// Specifies a window with a three-dimensional border style intended to be used for items that do not accept user input.
        /// </summary>
        StaticEdge = 0x00020000,

        /// <summary>
        /// Specifies a window that is intended to be used as a floating toolbar.
        /// A tool window has a title bar that is shorter than a normal title bar, and the window title is drawn using a smaller font.
        /// A tool window does not appear in the taskbar or in the dialog that appears when the user presses ALT+TAB.
        /// If a tool window has a system menu, its icon is not displayed on the title bar.
        /// However, you can display the system menu by right-clicking or by typing ALT+SPACE. 
        /// </summary>
        ToolWindow = 0x00000080,

        /// <summary>
        /// Specifies a window that should be placed above all non-topmost windows and should stay above them, even when the window is deactivated.
        /// To add or remove this style, use the SetWindowPos function.
        /// </summary>
        TopMost = 0x00000008,

        /// <summary>
        /// Specifies a window that should not be painted until siblings beneath the window (that were created by the same thread) have been painted.
        /// The window appears transparent because the bits of underlying sibling windows have already been painted.
        /// To achieve transparency without these restrictions, use the SetWindowRgn function.
        /// </summary>
        Transparent = 0x00000020,

        /// <summary>
        /// Specifies a window that has a border with a raised edge.
        /// </summary>
        WindowEdge = 0x00000100
    }
}
