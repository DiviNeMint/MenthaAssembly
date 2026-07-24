namespace MenthaAssembly.Win32
{
    /// <summary>
    /// Identifies a Desktop Window Manager attribute used by
    /// <c>DwmGetWindowAttribute</c> or <c>DwmSetWindowAttribute</c>.
    /// </summary>
    internal enum DwmWindowAttribute
    {
        /// <summary>
        /// Retrieves a Boolean that indicates whether non-client rendering is enabled.
        /// This attribute is read-only.
        /// </summary>
        NonClientRenderingEnabled = 1,

        /// <summary>
        /// Sets the non-client rendering policy using a DWM non-client rendering policy value.
        /// </summary>
        NonClientRenderingPolicy = 2,

        /// <summary>
        /// Sets a Boolean that forcibly disables or enables DWM transitions.
        /// </summary>
        TransitionsForcedDisabled = 3,

        /// <summary>
        /// Sets a Boolean that controls whether content painted in the non-client area is visible
        /// over the DWM-drawn frame.
        /// </summary>
        AllowNonClientPaint = 4,

        /// <summary>
        /// Retrieves the window-relative bounds of the caption button area as a rectangle.
        /// This attribute is read-only.
        /// </summary>
        CaptionButtonBounds = 5,

        /// <summary>
        /// Sets a Boolean that controls right-to-left mirroring of non-client content.
        /// </summary>
        NonClientRightToLeftLayout = 6,

        /// <summary>
        /// Sets a Boolean that forces the window to use a static iconic thumbnail or Peek representation.
        /// </summary>
        ForceIconicRepresentation = 7,

        /// <summary>
        /// Sets the policy that determines how Flip3D treats the window.
        /// </summary>
        Flip3DPolicy = 8,

        /// <summary>
        /// Retrieves the extended frame bounds in screen coordinates as a rectangle.
        /// This attribute is read-only.
        /// </summary>
        ExtendedFrameBounds = 9,

        /// <summary>
        /// Sets a Boolean that indicates the window supplies its own iconic bitmap.
        /// </summary>
        HasIconicBitmap = 10,

        /// <summary>
        /// Sets a Boolean that prevents or permits Peek preview for the window.
        /// </summary>
        DisallowPeek = 11,

        /// <summary>
        /// Sets a Boolean that excludes the window from fading during Peek.
        /// </summary>
        ExcludedFromPeek = 12,

        /// <summary>
        /// Sets a Boolean that cloaks or uncloaks the window while it remains composed by DWM.
        /// </summary>
        Cloak = 13,

        /// <summary>
        /// Retrieves flags that describe whether and why the window is cloaked.
        /// This attribute is read-only.
        /// </summary>
        Cloaked = 14,

        /// <summary>
        /// Sets a Boolean that freezes the current thumbnail representation of the window.
        /// </summary>
        FreezeRepresentation = 15,

        /// <summary>
        /// Sets a Boolean that updates the window only when composition is already running for another reason.
        /// </summary>
        PassiveUpdateMode = 16,

        /// <summary>
        /// Sets a Boolean that enables host backdrop brushes for the window.
        /// Supported starting with Windows 11 build 22000.
        /// </summary>
        UseHostBackdropBrush = 17,

        /// <summary>
        /// Sets a Boolean that allows the window frame to follow the system dark mode preference.
        /// Supported starting with Windows 11 build 22000.
        /// </summary>
        UseImmersiveDarkMode = 20,

        /// <summary>
        /// Sets the rounded-corner preference using a <see cref="DwmWindowCornerPreference"/> value.
        /// Supported starting with Windows 11 build 22000.
        /// </summary>
        WindowCornerPreference = 33,

        /// <summary>
        /// Sets the top-level window border color using a COLORREF value.
        /// Supported starting with Windows 11 build 22000.
        /// </summary>
        BorderColor = 34,

        /// <summary>
        /// Sets the caption background color using a COLORREF value.
        /// Supported starting with Windows 11 build 22000.
        /// </summary>
        CaptionColor = 35,

        /// <summary>
        /// Sets the caption text color using a COLORREF value.
        /// Supported starting with Windows 11 build 22000.
        /// </summary>
        TextColor = 36,

        /// <summary>
        /// Retrieves the DPI-aware width of the visible outer frame border as an unsigned integer.
        /// Supported starting with Windows 11 build 22000.
        /// This attribute is read-only.
        /// </summary>
        VisibleFrameBorderThickness = 37,

        /// <summary>
        /// Gets or sets the system-drawn backdrop material for the window.
        /// Supported starting with Windows 11 build 22621.
        /// </summary>
        SystemBackdropType = 38,

        /// <summary>
        /// Sets a Boolean that controls whether DWM uses premultiplied alpha from the redirection bitmap.
        /// Supported starting with Windows 11 build 26100.
        /// </summary>
        RedirectionBitmapAlpha = 39,

        /// <summary>
        /// Sets the inward border override distances using a frame-margin structure.
        /// Supported by an update to Windows 11 build 26100.
        /// </summary>
        BorderMargins = 40,

        /// <summary>
        /// Marks one past the highest recognized attribute value and is used only for validation.
        /// </summary>
        Last = 41,

    }
}
