namespace MenthaAssembly.Win32
{
    internal enum SystemParameterActionType : uint
    {
        /// <summary>
        /// Determines whether the warning beeper is on. 
        /// The pvParam parameter must point to a BOOL variable that receives TRUE if the beeper is on, or FALSE if it is off.
        /// </summary>
        GetBeep = 0x0001,

        /// <summary>
        /// Turns the warning beeper on or off. The uiParam parameter specifies TRUE for on, or FALSE for off.
        /// </summary>
        SetBeep = 0x0002,

        /// <summary>
        /// Retrieves the two mouse threshold values and the mouse speed.
        /// </summary>
        GetMouse = 0x0003,

        /// <summary>
        /// Sets the two mouse threshold values and the mouse speed.
        /// </summary>
        SetMouse = 0x0004,

        /// <summary>
        /// Retrieves the border multiplier factor that determines the width of a window's sizing border. 
        /// The pvParam parameter must point to an integer variable that receives this value.
        /// </summary>
        GetBorder = 0x0005,

        /// <summary>
        /// Sets the border multiplier factor that determines the width of a window's sizing border. 
        /// The uiParam parameter specifies the new value.
        /// </summary>
        SetBorder = 0x0006,

        /// <summary>
        /// Retrieves the keyboard repeat-speed setting, which is a value in the range from 0 (approximately 2.5 repetitions per second) 
        /// through 31 (approximately 30 repetitions per second). The actual repeat rates are hardware-dependent and may vary from 
        /// a linear scale by as much as 20%. The pvParam parameter must point to a DWORD variable that receives the setting
        /// </summary>
        GetKeyboardSpeed = 0x000A,

        /// <summary>
        /// Sets the keyboard repeat-speed setting. The uiParam parameter must specify a value in the range from 0 
        /// (approximately 2.5 repetitions per second) through 31 (approximately 30 repetitions per second). 
        /// The actual repeat rates are hardware-dependent and may vary from a linear scale by as much as 20%. 
        /// If uiParam is greater than 31, the parameter is set to 31.
        /// </summary>
        SetKeyboardSpeed = 0x000B,

        /// <summary>
        /// Not implemented.
        /// </summary>
        LangDriver = 0x000C,

        /// <summary>
        /// Sets or retrieves the width, in pixels, of an icon cell. The system uses this rectangle to arrange icons in large icon view. 
        /// To set this value, set uiParam to the new value and set pvParam to null. You cannot set this value to less than SM_CXIcon.
        /// To retrieve this value, pvParam must point to an integer that receives the current value.
        /// </summary>
        IconHorizontalSpacing = 0x000D,

        /// <summary>
        /// Retrieves the screen saver time-out value, in seconds. 
        /// The pvParam parameter must point to an integer variable that receives the value.
        /// </summary>
        GetScreenSaveTimeout = 0x000E,

        /// <summary>
        /// Sets the screen saver time-out value to the value of the uiParam parameter. This value is the amount of time, in seconds, 
        /// that the system must be idle before the screen saver activates.
        /// </summary>
        SetScreenSaveTimeout = 0x000F,

        /// <summary>
        /// Determines whether screen saving is enabled. The pvParam parameter must point to a bool variable that receives TRUE 
        /// if screen saving is enabled, or FALSE otherwise.
        /// </summary>
        GetScreenSaveActive = 0x0010,

        /// <summary>
        /// Sets the state of the screen saver. The uiParam parameter specifies TRUE to activate screen saving, or FALSE to deactivate it.
        /// </summary>
        SetScreenSaveActive = 0x0011,

        /// <summary>
        /// Retrieves the current granularity value of the desktop sizing grid. The pvParam parameter must point to an integer variable 
        /// that receives the granularity.
        /// </summary>
        GetGridGranularity = 0x0012,

        /// <summary>
        /// Sets the granularity of the desktop sizing grid to the value of the uiParam parameter.
        /// </summary>
        SetGridGranularity = 0x0013,

        /// <summary>
        /// Sets the desktop wallpaper. 
        /// The value of the pvParam parameter determines the new wallpaper. To specify a wallpaper bitmap, 
        /// set pvParam to point to a null-terminated string containing the name of a bitmap file. Setting pvParam to "" removes the wallpaper. 
        /// Setting pvParam to SetWallPaper_Default or null reverts to the default wallpaper.
        /// </summary>
        SetDeskWallPaper = 0x0014,

        /// <summary>
        /// Sets the current desktop pattern by causing Windows to read the Pattern= setting from the WIN.INI file.
        /// </summary>
        SetDeskPattern = 0x0015,

        /// <summary>
        /// Retrieves the keyboard repeat-delay setting, which is a value in the range from 0 (approximately 250 ms delay) through 3 
        /// (approximately 1 second delay). The actual delay associated with each value may vary depending on the hardware. The pvParam parameter must point to an integer variable that receives the setting.
        /// </summary>
        GetKeyboardDelay = 0x0016,

        /// <summary>
        /// Sets the keyboard repeat-delay setting. The uiParam parameter must specify 0, 1, 2, or 3, where zero sets the shortest delay 
        /// (approximately 250 ms) and 3 sets the longest delay (approximately 1 second). The actual delay associated with each value may 
        /// vary depending on the hardware.
        /// </summary>
        SetKeyboardDelay = 0x0017,

        /// <summary>
        /// Sets or retrieves the height, in pixels, of an icon cell. 
        /// To set this value, set uiParam to the new value and set pvParam to null. You cannot set this value to less than SM_CYIcon.
        /// To retrieve this value, pvParam must point to an integer that receives the current value.
        /// </summary>
        IconVerticalSpacing = 0x0018,

        /// <summary>
        /// Determines whether icon-title wrapping is enabled. The pvParam parameter must point to a bool variable that receives TRUE 
        /// if enabled, or FALSE otherwise.
        /// </summary>
        GetIconTitleWrap = 0x0019,

        /// <summary>
        /// Turns icon-title wrapping on or off. The uiParam parameter specifies TRUE for on, or FALSE for off.
        /// </summary>
        SetIconTitleWrap = 0x001A,

        /// <summary>
        /// Determines whether pop-up menus are left-aligned or right-aligned, relative to the corresponding menu-bar item. 
        /// The pvParam parameter must point to a bool variable that receives TRUE if left-aligned, or FALSE otherwise.
        /// </summary>
        GetMenuDropAlignment = 0x001B,

        /// <summary>
        /// Sets the alignment value of pop-up menus. The uiParam parameter specifies TRUE for right alignment, or FALSE for left alignment.
        /// </summary>
        SetMenuDropAlignment = 0x001C,

        /// <summary>
        /// Sets the width of the double-click rectangle to the value of the uiParam parameter. 
        /// The double-click rectangle is the rectangle within which the second click of a double-click must fall for it to be registered 
        /// as a double-click.
        /// To retrieve the width of the double-click rectangle, call GetSystemMetrics with the SM_CXDoubleCLK flag.
        /// </summary>
        SetDoubleCLKWidth = 0x001D,

        /// <summary>
        /// Sets the height of the double-click rectangle to the value of the uiParam parameter. 
        /// The double-click rectangle is the rectangle within which the second click of a double-click must fall for it to be registered 
        /// as a double-click.
        /// To retrieve the height of the double-click rectangle, call GetSystemMetrics with the SM_CYDoubleCLK flag.
        /// </summary>
        SetDoubleCLKHeight = 0x001E,

        /// <summary>
        /// Retrieves the logical font information for the current icon-title font. The uiParam parameter specifies the size of a LogFont structure, 
        /// and the pvParam parameter must point to the LogFont structure to fill in.
        /// </summary>
        GetIconTitleLogFont = 0x001F,

        /// <summary>
        /// Sets the double-click time for the mouse to the value of the uiParam parameter. The double-click time is the maximum number 
        /// of milliseconds that can occur between the first and second clicks of a double-click. You can also call the SetDoubleClickTime 
        /// function to set the double-click time. To get the current double-click time, call the GetDoubleClickTime function.
        /// </summary>
        SetDoubleClickTime = 0x0020,

        /// <summary>
        /// Swaps or restores the meaning of the left and right mouse buttons. The uiParam parameter specifies TRUE to swap the meanings 
        /// of the buttons, or FALSE to restore their original meanings.
        /// </summary>
        SetMouseButtonSwap = 0x0021,

        /// <summary>
        /// Sets the font that is used for icon titles. The uiParam parameter specifies the size of a LogFont structure, 
        /// and the pvParam parameter must point to a LogFont structure.
        /// </summary>
        SetIconTitleLogFont = 0x0022,

        /// <summary>
        /// This flag is obsolete. 
        /// Previous versions of the system use this flag to determine whether ALT+TAB fast task switching is enabled. 
        /// For Windows 95, Windows 98, and Windows NT version 4.0 and later, fast task switching is always enabled.
        /// </summary>
        GetFastTaskSwitch = 0x0023,

        /// <summary>
        /// This flag is obsolete. 
        /// Previous versions of the system use this flag to enable or disable ALT+TAB fast task switching. 
        /// For Windows 95, Windows 98, and Windows NT version 4.0 and later, fast task switching is always enabled.
        /// </summary>
        SetFastTaskSwitch = 0x0024,

        /// <summary>
        /// Sets dragging of full windows either on or off. The uiParam parameter specifies TRUE for on, or FALSE for off. 
        /// Windows 95:  This flag is supported only if Windows Plus! is installed. See GetWindowsExtension.
        /// </summary>
        SetDragFullWindows = 0x0025,

        /// <summary>
        /// Determines whether dragging of full windows is enabled. The pvParam parameter must point to a BOOL variable that receives TRUE 
        /// if enabled, or FALSE otherwise. 
        /// Windows 95:  This flag is supported only if Windows Plus! is installed. See GetWindowsExtension.
        /// </summary>
        GetDragFullWindows = 0x0026,

        /// <summary>
        /// Retrieves the metrics associated with the nonClient area of nonminimized windows. 
        /// The pvParam parameter must point to a NonClientMetrics structure that receives the information.
        /// Set the cbSize member of this structure and the uiParam parameter to sizeof(NonClientMetrics).
        /// </summary>
        GetNonClientMetrics = 0x0029,

        /// <summary>
        /// Sets the metrics associated with the nonClient area of nonminimized windows. 
        /// The pvParam parameter must point to a NonClientMetrics structure that contains the new parameters. 
        /// Set the cbSize member of this structure and the uiParam parameter to sizeof(NonClientMetrics). 
        /// Also, the lfHeight member of the LogFont structure must be a negative value.
        /// </summary>
        SetNonClientMetrics = 0x002A,

        /// <summary>
        /// Retrieves the metrics associated with minimized windows. The pvParam parameter must point to a MinimizedMetrics structure 
        /// that receives the information. Set the cbSize member of this structure and the uiParam parameter to sizeof(MinimizedMetrics).
        /// </summary>
        GetMinimizedMetrics = 0x002B,

        /// <summary>
        /// Sets the metrics associated with minimized windows. The pvParam parameter must point to a MinimizedMetrics structure 
        /// that contains the new parameters. Set the cbSize member of this structure and the uiParam parameter to sizeof(MinimizedMetrics).
        /// </summary>
        SetMinimizedMetrics = 0x002C,

        /// <summary>
        /// Retrieves the metrics associated with icons. 
        /// The pvParam parameter must point to an IconMetrics structure that receives the information. 
        /// Set the cbSize member of this structure and the uiParam parameter to sizeof(IconMetrics).
        /// </summary>
        GetIconMetrics = 0x002D,

        /// <summary>
        /// Sets the metrics associated with icons. 
        /// The pvParam parameter must point to an IconMetrics structure that contains the new parameters. 
        /// Set the cbSize member of this structure and the uiParam parameter to sizeof(IconMetrics).
        /// </summary>
        SetIconMetrics = 0x002E,

        /// <summary>
        /// Sets the size of the work area. 
        /// The work area is the portion of the screen not obscured by the system taskbar or by application desktop toolbars. 
        /// The pvParam parameter is a pointer to a RECT structure that specifies the new work area rectangle, expressed in virtual screen coordinates. 
        /// In a system with multiple display monitors, the function sets the work area of the monitor that contains the specified rectangle.
        /// </summary>
        SetWorkArea = 0x002F,

        /// <summary>
        /// Retrieves the size of the work area on the primary display monitor. The work area is the portion of the screen not obscured 
        /// by the system taskbar or by application desktop toolbars. The pvParam parameter must point to a RECT structure that receives 
        /// the coordinates of the work area, expressed in virtual screen coordinates. 
        /// To get the work area of a monitor other than the primary display monitor, call the GetMonitorInfo function.
        /// </summary>
        GetWorkArea = 0x0030,

        /// <summary>
        /// Windows Me/98/95:  Pen windows is being loaded or unloaded. The uiParam parameter is TRUE when loading and FALSE 
        /// when unloading pen windows. The pvParam parameter is null.
        /// </summary>
        SetPenWindows = 0x0031,

        /// <summary>
        /// Retrieves information about the HighContrast accessibility feature. The pvParam parameter must point to a HighContrast structure 
        /// that receives the information. Set the cbSize member of this structure and the uiParam parameter to sizeof(HighContrast). 
        /// For a general discussion, see remarks.
        /// Windows NT:  This value is not supported.
        /// </summary>
        /// <remarks>
        /// There is a difference between the High Contrast color scheme and the High Contrast Mode. The High Contrast color scheme changes 
        /// the system colors to colors that have obvious contrast; you switch to this color scheme by using the Display Options in the control panel. 
        /// The High Contrast Mode, which uses GetHighContrast and SetHighContrast, advises applications to modify their appearance 
        /// for visually-impaired users. It involves such things as audible warning to users and customized color scheme 
        /// (using the Accessibility Options in the control panel). For more information, see HighContrast on MSDN.
        /// For more information on general accessibility features, see Accessibility on MSDN.
        /// </remarks>
        GetHighContrast = 0x0042,

        /// <summary>
        /// Sets the parameters of the HighContrast accessibility feature. The pvParam parameter must point to a HighContrast structure 
        /// that contains the new parameters. Set the cbSize member of this structure and the uiParam parameter to sizeof(HighContrast).
        /// Windows NT:  This value is not supported.
        /// </summary>
        SetHighContrast = 0x0043,

        /// <summary>
        /// Determines whether the user relies on the keyboard instead of the mouse, and wants applications to display keyboard interfaces 
        /// that would otherwise be hidden. The pvParam parameter must point to a BOOL variable that receives TRUE 
        /// if the user relies on the keyboard; or FALSE otherwise.
        /// Windows NT:  This value is not supported.
        /// </summary>
        GetKeyboardPREF = 0x0044,

        /// <summary>
        /// Sets the keyboard preference. The uiParam parameter specifies TRUE if the user relies on the keyboard instead of the mouse, 
        /// and wants applications to display keyboard interfaces that would otherwise be hidden; uiParam is FALSE otherwise.
        /// Windows NT:  This value is not supported.
        /// </summary>
        SetKeyboardPREF = 0x0045,

        /// <summary>
        /// Determines whether a screen reviewer utility is running. A screen reviewer utility directs textual information to an output device, 
        /// such as a speech synthesizer or Braille display. When this flag is set, an application should provide textual information 
        /// in situations where it would otherwise present the information graphically.
        /// The pvParam parameter is a pointer to a BOOL variable that receives TRUE if a screen reviewer utility is running, or FALSE otherwise.
        /// Windows NT:  This value is not supported.
        /// </summary>
        GetScreenReader = 0x0046,

        /// <summary>
        /// Determines whether a screen review utility is running. The uiParam parameter specifies TRUE for on, or FALSE for off.
        /// Windows NT:  This value is not supported.
        /// </summary>
        SetScreenReader = 0x0047,

        /// <summary>
        /// Retrieves the animation effects associated with user actions. 
        /// The pvParam parameter must point to an AnimationInfo structure 
        /// that receives the information. Set the cbSize member of this structure and the uiParam parameter to sizeof(AnimationInfo).
        /// </summary>
        GetAnimation = 0x0048,

        /// <summary>
        /// Sets the animation effects associated with user actions. The pvParam parameter must point to an AnimationInfo structure 
        /// that contains the new parameters. Set the cbSize member of this structure and the uiParam parameter to sizeof(AnimationInfo).
        /// </summary>
        SetAnimation = 0x0049,

        /// <summary>
        /// Determines whether the font smoothing feature is enabled. This feature uses font antialiasing to make font curves appear smoother 
        /// by painting pixels at different gray levels. 
        /// The pvParam parameter must point to a BOOL variable that receives TRUE if the feature is enabled, or FALSE if it is not.
        /// Windows 95:  This flag is supported only if Windows Plus! is installed. See GetWindowsExtension.
        /// </summary>
        GetFontSmoothing = 0x004A,

        /// <summary>
        /// Enables or disables the font smoothing feature, which uses font antialiasing to make font curves appear smoother 
        /// by painting pixels at different gray levels. 
        /// To enable the feature, set the uiParam parameter to TRUE. To disable the feature, set uiParam to FALSE.
        /// Windows 95:  This flag is supported only if Windows Plus! is installed. See GetWindowsExtension.
        /// </summary>
        SetFontSmoothing = 0x004B,

        /// <summary>
        /// Sets the width, in pixels, of the rectangle used to detect the start of a drag operation. Set uiParam to the new value. 
        /// To retrieve the drag width, call GetSystemMetrics with the SM_CXDrag flag.
        /// </summary>
        SetDragWidth = 0x004C,

        /// <summary>
        /// Sets the height, in pixels, of the rectangle used to detect the start of a drag operation. Set uiParam to the new value. 
        /// To retrieve the drag height, call GetSystemMetrics with the SM_CYDrag flag.
        /// </summary>
        SetDragHeight = 0x004D,

        /// <summary>
        /// Used internally; applications should not use this value.
        /// </summary>
        SetHandHeld = 0x004E,

        /// <summary>
        /// Retrieves the time-out value for the low-power phase of screen saving. The pvParam parameter must point to an integer variable 
        /// that receives the value. This flag is supported for 32-bit applications only.
        /// Windows NT, Windows Me/98:  This flag is supported for 16-bit and 32-bit applications.
        /// Windows 95:  This flag is supported for 16-bit applications only.
        /// </summary>
        GetLowPowerTimeout = 0x004F,

        /// <summary>
        /// Retrieves the time-out value for the power-off phase of screen saving. The pvParam parameter must point to an integer variable 
        /// that receives the value. This flag is supported for 32-bit applications only.
        /// Windows NT, Windows Me/98:  This flag is supported for 16-bit and 32-bit applications.
        /// Windows 95:  This flag is supported for 16-bit applications only.
        /// </summary>
        GetPowerOffTimeout = 0x0050,

        /// <summary>
        /// Sets the time-out value, in seconds, for the low-power phase of screen saving. The uiParam parameter specifies the new value. 
        /// The pvParam parameter must be null. This flag is supported for 32-bit applications only.
        /// Windows NT, Windows Me/98:  This flag is supported for 16-bit and 32-bit applications.
        /// Windows 95:  This flag is supported for 16-bit applications only.
        /// </summary>
        SetLowPowerTimeout = 0x0051,

        /// <summary>
        /// Sets the time-out value, in seconds, for the power-off phase of screen saving. The uiParam parameter specifies the new value. 
        /// The pvParam parameter must be null. This flag is supported for 32-bit applications only.
        /// Windows NT, Windows Me/98:  This flag is supported for 16-bit and 32-bit applications.
        /// Windows 95:  This flag is supported for 16-bit applications only.
        /// </summary>
        SetPowerOffTimeout = 0x0052,

        /// <summary>
        /// Determines whether the low-power phase of screen saving is enabled. The pvParam parameter must point to a BOOL variable 
        /// that receives TRUE if enabled, or FALSE if disabled. This flag is supported for 32-bit applications only.
        /// Windows NT, Windows Me/98:  This flag is supported for 16-bit and 32-bit applications.
        /// Windows 95:  This flag is supported for 16-bit applications only.
        /// </summary>
        GetLowPowerActive = 0x0053,

        /// <summary>
        /// Determines whether the power-off phase of screen saving is enabled. The pvParam parameter must point to a BOOL variable 
        /// that receives TRUE if enabled, or FALSE if disabled. This flag is supported for 32-bit applications only.
        /// Windows NT, Windows Me/98:  This flag is supported for 16-bit and 32-bit applications.
        /// Windows 95:  This flag is supported for 16-bit applications only.
        /// </summary>
        GetPowerOffActive = 0x0054,

        /// <summary>
        /// Activates or deactivates the low-power phase of screen saving. Set uiParam to 1 to activate, or zero to deactivate. 
        /// The pvParam parameter must be null. This flag is supported for 32-bit applications only.
        /// Windows NT, Windows Me/98:  This flag is supported for 16-bit and 32-bit applications.
        /// Windows 95:  This flag is supported for 16-bit applications only.
        /// </summary>
        SetLowPowerActive = 0x0055,

        /// <summary>
        /// Activates or deactivates the power-off phase of screen saving. Set uiParam to 1 to activate, or zero to deactivate. 
        /// The pvParam parameter must be null. This flag is supported for 32-bit applications only.
        /// Windows NT, Windows Me/98:  This flag is supported for 16-bit and 32-bit applications.
        /// Windows 95:  This flag is supported for 16-bit applications only.
        /// </summary>
        SetPowerOffActive = 0x0056,

        /// <summary>
        /// Reloads the system cursors. Set the uiParam parameter to zero and the pvParam parameter to null.
        /// </summary>
        SetCursors = 0x0057,

        /// <summary>
        /// Reloads the system icons. Set the uiParam parameter to zero and the pvParam parameter to null.
        /// </summary>
        SetIcons = 0x0058,

        /// <summary>
        /// Retrieves the input locale identifier for the system default input language. The pvParam parameter must point 
        /// to an HKL variable that receives this value. For more information, see Languages, Locales, and Keyboard Layouts on MSDN.
        /// </summary>
        GetDefaultInputLang = 0x0059,

        /// <summary>
        /// Sets the default input language for the system shell and applications. The specified language must be displayable 
        /// using the current system character set. The pvParam parameter must point to an HKL variable that contains 
        /// the input locale identifier for the default language. For more information, see Languages, Locales, and Keyboard Layouts on MSDN.
        /// </summary>
        SetDefaultInputLang = 0x005A,

        /// <summary>
        /// Sets the hot key set for switching between input languages. The uiParam and pvParam parameters are not used. 
        /// The value sets the shortcut keys in the keyboard property sheets by reading the registry again. The registry must be set before this flag is used. the path in the registry is \HKEY_CURRENT_USER\keyboard layout\toggle. Valid values are "1" = ALT+SHIFT, "2" = CTRL+SHIFT, and "3" = none.
        /// </summary>
        SetLangToggle = 0x005B,

        /// <summary>
        /// Windows 95:  Determines whether the Windows extension, Windows Plus!, is installed. Set the uiParam parameter to 1. 
        /// The pvParam parameter is not used. The function returns TRUE if the extension is installed, or FALSE if it is not.
        /// </summary>
        GetWindowsExtension = 0x005C,

        /// <summary>
        /// Enables or disables the Mouse Trails feature, which improves the visibility of mouse cursor movements by briefly showing 
        /// a trail of cursors and quickly erasing them. 
        /// To disable the feature, set the uiParam parameter to zero or 1. To enable the feature, set uiParam to a value greater than 1 
        /// to indicate the number of cursors drawn in the trail.
        /// Windows 2000/NT:  This value is not supported.
        /// </summary>
        SetMouseTrails = 0x005D,

        /// <summary>
        /// Determines whether the Mouse Trails feature is enabled. This feature improves the visibility of mouse cursor movements 
        /// by briefly showing a trail of cursors and quickly erasing them. 
        /// The pvParam parameter must point to an integer variable that receives a value. If the value is zero or 1, the feature is disabled. 
        /// If the value is greater than 1, the feature is enabled and the value indicates the number of cursors drawn in the trail. 
        /// The uiParam parameter is not used.
        /// Windows 2000/NT:  This value is not supported.
        /// </summary>
        GetMouseTrails = 0x005E,

        /// <summary>
        /// Windows Me/98:  Used internally; applications should not use this flag.
        /// </summary>
        SetScreenSaverRunning = 0x0061,

        /// <summary>
        /// Retrieves information about the FilterKeys accessibility feature. The pvParam parameter must point to a FilterKeys structure 
        /// that receives the information. Set the cbSize member of this structure and the uiParam parameter to sizeof(FilterKeys).
        /// </summary>
        GetFilterKeys = 0x0032,

        /// <summary>
        /// Sets the parameters of the FilterKeys accessibility feature. The pvParam parameter must point to a FilterKeys structure 
        /// that contains the new parameters. Set the cbSize member of this structure and the uiParam parameter to sizeof(FilterKeys).
        /// </summary>
        SetFilterKeys = 0x0033,

        /// <summary>
        /// Retrieves information about the ToggleKeys accessibility feature. The pvParam parameter must point to a ToggleKeys structure 
        /// that receives the information. Set the cbSize member of this structure and the uiParam parameter to sizeof(ToggleKeys).
        /// </summary>
        GetToggleKeys = 0x0034,

        /// <summary>
        /// Sets the parameters of the ToggleKeys accessibility feature. The pvParam parameter must point to a ToggleKeys structure 
        /// that contains the new parameters. Set the cbSize member of this structure and the uiParam parameter to sizeof(ToggleKeys).
        /// </summary>
        SetToggleKeys = 0x0035,

        /// <summary>
        /// Retrieves information about the MouseKeys accessibility feature. The pvParam parameter must point to a MouseKeys structure 
        /// that receives the information. Set the cbSize member of this structure and the uiParam parameter to sizeof(MouseKeys).
        /// </summary>
        GetMouseKeys = 0x0036,

        /// <summary>
        /// Sets the parameters of the MouseKeys accessibility feature. The pvParam parameter must point to a MouseKeys structure 
        /// that contains the new parameters. Set the cbSize member of this structure and the uiParam parameter to sizeof(MouseKeys).
        /// </summary>
        SetMouseKeys = 0x0037,

        /// <summary>
        /// Determines whether the Show Sounds accessibility flag is on or off. If it is on, the user requires an application 
        /// to present information visually in situations where it would otherwise present the information only in audible form. 
        /// The pvParam parameter must point to a BOOL variable that receives TRUE if the feature is on, or FALSE if it is off. 
        /// Using this value is equivalent to calling GetSystemMetrics (SM_ShowSounds). That is the recommended call.
        /// </summary>
        GetShowSounds = 0x0038,

        /// <summary>
        /// Sets the parameters of the SoundSentry accessibility feature. The pvParam parameter must point to a SoundsEntry structure 
        /// that contains the new parameters. Set the cbSize member of this structure and the uiParam parameter to sizeof(SoundsEntry).
        /// </summary>
        SetShowSounds = 0x0039,

        /// <summary>
        /// Retrieves information about the StickyKeys accessibility feature. The pvParam parameter must point to a StickyKeys structure 
        /// that receives the information. Set the cbSize member of this structure and the uiParam parameter to sizeof(StickyKeys).
        /// </summary>
        GetStickyKeys = 0x003A,

        /// <summary>
        /// Sets the parameters of the StickyKeys accessibility feature. The pvParam parameter must point to a StickyKeys structure 
        /// that contains the new parameters. Set the cbSize member of this structure and the uiParam parameter to sizeof(StickyKeys).
        /// </summary>
        SetStickyKeys = 0x003B,

        /// <summary>
        /// Retrieves information about the time-out period associated with the accessibility features. The pvParam parameter must point 
        /// to an AccessTimeout structure that receives the information. Set the cbSize member of this structure and the uiParam parameter 
        /// to sizeof(AccessTimeout).
        /// </summary>
        GetAccessTimeout = 0x003C,

        /// <summary>
        /// Sets the time-out period associated with the accessibility features. The pvParam parameter must point to an AccessTimeout 
        /// structure that contains the new parameters. Set the cbSize member of this structure and the uiParam parameter to sizeof(AccessTimeout).
        /// </summary>
        SetAccessTimeout = 0x003D,

        /// <summary>
        /// Windows Me/98/95:  Retrieves information about the SerialKeys accessibility feature. The pvParam parameter must point 
        /// to a SerialKeys structure that receives the information. Set the cbSize member of this structure and the uiParam parameter 
        /// to sizeof(SerialKeys).
        /// Windows Server 2003, Windows XP/2000/NT:  Not supported. The user controls this feature through the control panel.
        /// </summary>
        GetSerialKeys = 0x003E,

        /// <summary>
        /// Windows Me/98/95:  Sets the parameters of the SerialKeys accessibility feature. The pvParam parameter must point 
        /// to a SerialKeys structure that contains the new parameters. Set the cbSize member of this structure and the uiParam parameter 
        /// to sizeof(SerialKeys). 
        /// Windows Server 2003, Windows XP/2000/NT:  Not supported. The user controls this feature through the control panel.
        /// </summary>
        SetSerialKeys = 0x003F,

        /// <summary>
        /// Retrieves information about the SoundSentry accessibility feature. The pvParam parameter must point to a SoundsEntry structure 
        /// that receives the information. Set the cbSize member of this structure and the uiParam parameter to sizeof(SoundsEntry).
        /// </summary>
        GetSoundsEntry = 0x0040,

        /// <summary>
        /// Sets the parameters of the SoundSentry accessibility feature. The pvParam parameter must point to a SoundsEntry structure 
        /// that contains the new parameters. Set the cbSize member of this structure and the uiParam parameter to sizeof(SoundsEntry).
        /// </summary>
        SetSoundsEntry = 0x0041,

        /// <summary>
        /// Determines whether the snap-to-default-button feature is enabled. If enabled, the mouse cursor automatically moves 
        /// to the default button, such as OK or Apply, of a dialog box. The pvParam parameter must point to a BOOL variable 
        /// that receives TRUE if the feature is on, or FALSE if it is off. 
        /// Windows 95:  Not supported.
        /// </summary>
        GetSnapToDefButton = 0x005F,

        /// <summary>
        /// Enables or disables the snap-to-default-button feature. If enabled, the mouse cursor automatically moves to the default button, 
        /// such as OK or Apply, of a dialog box. Set the uiParam parameter to TRUE to enable the feature, or FALSE to disable it. 
        /// Applications should use the ShowWindow function when displaying a dialog box so the dialog manager can position the mouse cursor. 
        /// Windows 95:  Not supported.
        /// </summary>
        SetSnapToDefButton = 0x0060,

        /// <summary>
        /// Retrieves the width, in pixels, of the rectangle within which the mouse pointer has to stay for TrackMouseEvent 
        /// to generate a WM_MouseHover message. The pvParam parameter must point to a UINT variable that receives the width. 
        /// Windows 95:  Not supported.
        /// </summary>
        GetMouseHoverWidth = 0x0062,

        /// <summary>
        /// Retrieves the width, in pixels, of the rectangle within which the mouse pointer has to stay for TrackMouseEvent 
        /// to generate a WM_MouseHover message. The pvParam parameter must point to a UINT variable that receives the width. 
        /// Windows 95:  Not supported.
        /// </summary>
        SetMouseHoverWidth = 0x0063,

        /// <summary>
        /// Retrieves the height, in pixels, of the rectangle within which the mouse pointer has to stay for TrackMouseEvent 
        /// to generate a WM_MouseHover message. The pvParam parameter must point to a UINT variable that receives the height. 
        /// Windows 95:  Not supported.
        /// </summary>
        GetMouseHoverHeight = 0x0064,

        /// <summary>
        /// Sets the height, in pixels, of the rectangle within which the mouse pointer has to stay for TrackMouseEvent 
        /// to generate a WM_MouseHover message. Set the uiParam parameter to the new height.
        /// Windows 95:  Not supported.
        /// </summary>
        SetMouseHoverHeight = 0x0065,

        /// <summary>
        /// Retrieves the time, in milliseconds, that the mouse pointer has to stay in the hover rectangle for TrackMouseEvent 
        /// to generate a WM_MouseHover message. The pvParam parameter must point to a UINT variable that receives the time. 
        /// Windows 95:  Not supported.
        /// </summary>
        GetMouseHoverTime = 0x0066,

        /// <summary>
        /// Sets the time, in milliseconds, that the mouse pointer has to stay in the hover rectangle for TrackMouseEvent 
        /// to generate a WM_MouseHover message. This is used only if you pass Hover_Default in the dwHoverTime parameter in the call to TrackMouseEvent. Set the uiParam parameter to the new time. 
        /// Windows 95:  Not supported.
        /// </summary>
        SetMouseHoverTime = 0x0067,

        /// <summary>
        /// Retrieves the number of lines to scroll when the mouse wheel is rotated. The pvParam parameter must point 
        /// to a UINT variable that receives the number of lines. The default value is 3. 
        /// Windows 95:  Not supported.
        /// </summary>
        GetWheelScrollLines = 0x0068,

        /// <summary>
        /// Sets the number of lines to scroll when the mouse wheel is rotated. The number of lines is set from the uiParam parameter. 
        /// The number of lines is the suggested number of lines to scroll when the mouse wheel is rolled without using modifier keys. 
        /// If the number is 0, then no scrolling should occur. If the number of lines to scroll is greater than the number of lines viewable, 
        /// and in particular if it is Wheel_PAGEScroll (#defined as UINT_MAX), the scroll operation should be interpreted 
        /// as clicking once in the page down or page up regions of the scroll bar.
        /// Windows 95:  Not supported.
        /// </summary>
        SetWheelScrollLines = 0x0069,

        /// <summary>
        /// Retrieves the time, in milliseconds, that the system waits before displaying a shortcut menu when the mouse cursor is 
        /// over a submenu item. The pvParam parameter must point to a DWORD variable that receives the time of the delay. 
        /// Windows 95:  Not supported.
        /// </summary>
        GetMenuShowDelay = 0x006A,

        /// <summary>
        /// Sets uiParam to the time, in milliseconds, that the system waits before displaying a shortcut menu when the mouse cursor is 
        /// over a submenu item. 
        /// Windows 95:  Not supported.
        /// </summary>
        SetMenuShowDelay = 0x006B,

        /// <summary>
        /// Determines whether the IME status window is visible (on a per-user basis). The pvParam parameter must point to a BOOL variable 
        /// that receives TRUE if the status window is visible, or FALSE if it is not.
        /// Windows NT, Windows 95:  This value is not supported.
        /// </summary>
        GetShowIMEUI = 0x006E,

        /// <summary>
        /// Sets whether the IME status window is visible or not on a per-user basis. The uiParam parameter specifies TRUE for on or FALSE for off.
        /// Windows NT, Windows 95:  This value is not supported.
        /// </summary>
        SetShowIMEUI = 0x006F,

        /// <summary>
        /// Retrieves the current mouse speed. The mouse speed determines how far the pointer will move based on the distance the mouse moves. 
        /// The pvParam parameter must point to an integer that receives a value which ranges between 1 (slowest) and 20 (fastest). 
        /// A value of 10 is the default. The value can be set by an end user using the mouse control panel application or 
        /// by an application using SetMouseSpeed.
        /// Windows NT, Windows 95:  This value is not supported.
        /// </summary>
        GetMouseSpeed = 0x0070,

        /// <summary>
        /// Sets the current mouse speed. The pvParam parameter is an integer between 1 (slowest) and 20 (fastest). A value of 10 is the default. 
        /// This value is typically set using the mouse control panel application.
        /// Windows NT, Windows 95:  This value is not supported.
        /// </summary>
        SetMouseSpeed = 0x0071,

        /// <summary>
        /// Determines whether a screen saver is currently running on the window station of the calling process. 
        /// The pvParam parameter must point to a BOOL variable that receives TRUE if a screen saver is currently running, or FALSE otherwise.
        /// Note that only the interactive window station, "WinSta0", can have a screen saver running.
        /// Windows NT, Windows 95:  This value is not supported.
        /// </summary>
        GetScreenSaverRunning = 0x0072,

        /// <summary>
        /// Retrieves the full path of the bitmap file for the desktop wallpaper. The pvParam parameter must point to a buffer 
        /// that receives a null-terminated path string. Set the uiParam parameter to the size, in characters, of the pvParam buffer. The returned string will not exceed MAX_PATH characters. If there is no desktop wallpaper, the returned string is empty.
        /// Windows NT, Windows Me/98/95:  This value is not supported.
        /// </summary>
        GetDeskWallPaper = 0x0073,

        /// <summary>
        /// Determines whether active window tracking (activating the window the mouse is on) is on or off. The pvParam parameter must point 
        /// to a BOOL variable that receives TRUE for on, or FALSE for off.
        /// Windows NT, Windows 95:  This value is not supported.
        /// </summary>
        GetActiveWindowTracking = 0x1000,

        /// <summary>
        /// Sets active window tracking (activating the window the mouse is on) either on or off. Set pvParam to TRUE for on or FALSE for off.
        /// Windows NT, Windows 95:  This value is not supported.
        /// </summary>
        SetActiveWindowTracking = 0x1001,

        /// <summary>
        /// Determines whether the menu animation feature is enabled. This master switch must be on to enable menu animation effects. 
        /// The pvParam parameter must point to a BOOL variable that receives TRUE if animation is enabled and FALSE if it is disabled. 
        /// If animation is enabled, GetMenuFade indicates whether menus use fade or slide animation.
        /// Windows NT, Windows 95:  This value is not supported.
        /// </summary>
        GetMenuAnimation = 0x1002,

        /// <summary>
        /// Enables or disables menu animation. This master switch must be on for any menu animation to occur. 
        /// The pvParam parameter is a BOOL variable; set pvParam to TRUE to enable animation and FALSE to disable animation.
        /// If animation is enabled, GetMenuFade indicates whether menus use fade or slide animation.
        /// Windows NT, Windows 95:  This value is not supported.
        /// </summary>
        SetMenuAnimation = 0x1003,

        /// <summary>
        /// Determines whether the slide-open effect for combo boxes is enabled. The pvParam parameter must point to a BOOL variable 
        /// that receives TRUE for enabled, or FALSE for disabled.
        /// Windows NT, Windows 95:  This value is not supported.
        /// </summary>
        GetComboBoxAnimation = 0x1004,

        /// <summary>
        /// Enables or disables the slide-open effect for combo boxes. Set the pvParam parameter to TRUE to enable the gradient effect, 
        /// or FALSE to disable it.
        /// Windows NT, Windows 95:  This value is not supported.
        /// </summary>
        SetComboBoxAnimation = 0x1005,

        /// <summary>
        /// Determines whether the smooth-scrolling effect for list boxes is enabled. The pvParam parameter must point to a BOOL variable 
        /// that receives TRUE for enabled, or FALSE for disabled.
        /// Windows NT, Windows 95:  This value is not supported.
        /// </summary>
        GetListBoxSmoothScrolling = 0x1006,

        /// <summary>
        /// Enables or disables the smooth-scrolling effect for list boxes. Set the pvParam parameter to TRUE to enable the smooth-scrolling effect,
        /// or FALSE to disable it.
        /// Windows NT, Windows 95:  This value is not supported.
        /// </summary>
        SetListBoxSmoothScrolling = 0x1007,

        /// <summary>
        /// Determines whether the gradient effect for window title bars is enabled. The pvParam parameter must point to a BOOL variable 
        /// that receives TRUE for enabled, or FALSE for disabled. For more information about the gradient effect, see the GetSysColor function.
        /// Windows NT, Windows 95:  This value is not supported.
        /// </summary>
        GetGradientCaptions = 0x1008,

        /// <summary>
        /// Enables or disables the gradient effect for window title bars. Set the pvParam parameter to TRUE to enable it, or FALSE to disable it. 
        /// The gradient effect is possible only if the system has a color depth of more than 256 colors. For more information about 
        /// the gradient effect, see the GetSysColor function.
        /// Windows NT, Windows 95:  This value is not supported.
        /// </summary>
        SetGradientCaptions = 0x1009,

        /// <summary>
        /// Determines whether menu access keys are always underlined. The pvParam parameter must point to a BOOL variable that receives TRUE 
        /// if menu access keys are always underlined, and FALSE if they are underlined only when the menu is activated by the keyboard.
        /// Windows NT, Windows 95:  This value is not supported.
        /// </summary>
        GetKeyboardCues = 0x100A,

        /// <summary>
        /// Sets the underlining of menu access key letters. The pvParam parameter is a BOOL variable. Set pvParam to TRUE to always underline menu 
        /// access keys, or FALSE to underline menu access keys only when the menu is activated from the keyboard.
        /// Windows NT, Windows 95:  This value is not supported.
        /// </summary>
        SetKeyboardCues = 0x100B,

        /// <summary>
        /// Same as GetKeyboardCues.
        /// </summary>
        GetMenuUnderLines = GetKeyboardCues,

        /// <summary>
        /// Same as SetKeyboardCues.
        /// </summary>
        SetMenuUnderLines = SetKeyboardCues,

        /// <summary>
        /// Determines whether windows activated through active window tracking will be brought to the top. The pvParam parameter must point 
        /// to a BOOL variable that receives TRUE for on, or FALSE for off.
        /// Windows NT, Windows 95:  This value is not supported.
        /// </summary>
        GetActiveWNDTRKZOrder = 0x100C,

        /// <summary>
        /// Determines whether or not windows activated through active window tracking should be brought to the top. Set pvParam to TRUE 
        /// for on or FALSE for off.
        /// Windows NT, Windows 95:  This value is not supported.
        /// </summary>
        SetActiveWNDTRKZOrder = 0x100D,

        /// <summary>
        /// Determines whether hot tracking of user-interface elements, such as menu names on menu bars, is enabled. The pvParam parameter 
        /// must point to a BOOL variable that receives TRUE for enabled, or FALSE for disabled. 
        /// Hot tracking means that when the cursor moves over an item, it is highlighted but not selected. You can query this value to decide 
        /// whether to use hot tracking in the user interface of your application.
        /// Windows NT, Windows 95:  This value is not supported.
        /// </summary>
        GetHotTracking = 0x100E,

        /// <summary>
        /// Enables or disables hot tracking of user-interface elements such as menu names on menu bars. Set the pvParam parameter to TRUE 
        /// to enable it, or FALSE to disable it.
        /// Hot-tracking means that when the cursor moves over an item, it is highlighted but not selected.
        /// Windows NT, Windows 95:  This value is not supported.
        /// </summary>
        SetHotTracking = 0x100F,

        /// <summary>
        /// Determines whether menu fade animation is enabled. The pvParam parameter must point to a BOOL variable that receives TRUE 
        /// when fade animation is enabled and FALSE when it is disabled. If fade animation is disabled, menus use slide animation. 
        /// This flag is ignored unless menu animation is enabled, which you can do using the SetMenuAnimation flag. 
        /// For more information, see AnimateWindow.
        /// Windows NT, Windows Me/98/95:  This value is not supported.
        /// </summary>
        GetMenuFade = 0x1012,

        /// <summary>
        /// Enables or disables menu fade animation. Set pvParam to TRUE to enable the menu fade effect or FALSE to disable it. 
        /// If fade animation is disabled, menus use slide animation. he The menu fade effect is possible only if the system 
        /// has a color depth of more than 256 colors. This flag is ignored unless MenuAnimation is also set. For more information, 
        /// see AnimateWindow.
        /// Windows NT, Windows Me/98/95:  This value is not supported.
        /// </summary>
        SetMenuFade = 0x1013,

        /// <summary>
        /// Determines whether the selection fade effect is enabled. The pvParam parameter must point to a BOOL variable that receives TRUE 
        /// if enabled or FALSE if disabled. 
        /// The selection fade effect causes the menu item selected by the user to remain on the screen briefly while fading out 
        /// after the menu is dismissed.
        /// Windows NT, Windows Me/98/95:  This value is not supported.
        /// </summary>
        GetSelectionFade = 0x1014,

        /// <summary>
        /// Set pvParam to TRUE to enable the selection fade effect or FALSE to disable it.
        /// The selection fade effect causes the menu item selected by the user to remain on the screen briefly while fading out 
        /// after the menu is dismissed. The selection fade effect is possible only if the system has a color depth of more than 256 colors.
        /// Windows NT, Windows Me/98/95:  This value is not supported.
        /// </summary>
        SetSelectionFade = 0x1015,

        /// <summary>
        /// Determines whether ToolTip animation is enabled. The pvParam parameter must point to a BOOL variable that receives TRUE 
        /// if enabled or FALSE if disabled. If ToolTip animation is enabled, GetToolTipFade indicates whether ToolTips use fade or slide animation.
        /// Windows NT, Windows Me/98/95:  This value is not supported.
        /// </summary>
        GetToolTipAnimation = 0x1016,

        /// <summary>
        /// Set pvParam to TRUE to enable ToolTip animation or FALSE to disable it. If enabled, you can use SetToolTipFade 
        /// to specify fade or slide animation.
        /// Windows NT, Windows Me/98/95:  This value is not supported.
        /// </summary>
        SetToolTipAnimation = 0x1017,

        /// <summary>
        /// If SetToolTipAnimation is enabled, GetToolTipFade indicates whether ToolTip animation uses a fade effect or a slide effect.
        ///  The pvParam parameter must point to a BOOL variable that receives TRUE for fade animation or FALSE for slide animation. 
        ///  For more information on slide and fade effects, see AnimateWindow.
        /// Windows NT, Windows Me/98/95:  This value is not supported.
        /// </summary>
        GetToolTipFade = 0x1018,

        /// <summary>
        /// If the SetToolTipAnimation flag is enabled, use SetToolTipFade to indicate whether ToolTip animation uses a fade effect 
        /// or a slide effect. Set pvParam to TRUE for fade animation or FALSE for slide animation. The tooltip fade effect is possible only 
        /// if the system has a color depth of more than 256 colors. For more information on the slide and fade effects, 
        /// see the AnimateWindow function.
        /// Windows NT, Windows Me/98/95:  This value is not supported.
        /// </summary>
        SetToolTipFade = 0x1019,

        /// <summary>
        /// Determines whether the cursor has a shadow around it. The pvParam parameter must point to a BOOL variable that receives TRUE 
        /// if the shadow is enabled, FALSE if it is disabled. This effect appears only if the system has a color depth of more than 256 colors.
        /// Windows NT, Windows Me/98/95:  This value is not supported.
        /// </summary>
        GetCursorShadow = 0x101A,

        /// <summary>
        /// Enables or disables a shadow around the cursor. The pvParam parameter is a BOOL variable. Set pvParam to TRUE to enable the shadow 
        /// or FALSE to disable the shadow. This effect appears only if the system has a color depth of more than 256 colors.
        /// Windows NT, Windows Me/98/95:  This value is not supported.
        /// </summary>
        SetCursorShadow = 0x101B,

        //#if(_WIN32_WINNT >= 0x0501)
        /// <summary>
        /// Retrieves the state of the Mouse Sonar feature. The pvParam parameter must point to a BOOL variable that receives TRUE 
        /// if enabled or FALSE otherwise. For more information, see About Mouse Input on MSDN.
        /// Windows 2000/NT, Windows 98/95:  This value is not supported.
        /// </summary>
        GetMouseSonar = 0x101C,

        /// <summary>
        /// Turns the Sonar accessibility feature on or off. This feature briefly shows several concentric circles around the mouse pointer 
        /// when the user presses and releases the CTRL key. The pvParam parameter specifies TRUE for on and FALSE for off. The default is off. 
        /// For more information, see About Mouse Input.
        /// Windows 2000/NT, Windows 98/95:  This value is not supported.
        /// </summary>
        SetMouseSonar = 0x101D,

        /// <summary>
        /// Retrieves the state of the Mouse ClickLock feature. The pvParam parameter must point to a BOOL variable that receives TRUE 
        /// if enabled, or FALSE otherwise. For more information, see About Mouse Input.
        /// Windows 2000/NT, Windows 98/95:  This value is not supported.
        /// </summary>
        GetMouseClickLock = 0x101E,

        /// <summary>
        /// Turns the Mouse ClickLock accessibility feature on or off. This feature temporarily locks down the primary mouse button 
        /// when that button is clicked and held down for the time specified by SetMouseClickLockTime. The uiParam parameter specifies 
        /// TRUE for on, 
        /// or FALSE for off. The default is off. For more information, see Remarks and About Mouse Input on MSDN.
        /// Windows 2000/NT, Windows 98/95:  This value is not supported.
        /// </summary>
        SetMouseClickLock = 0x101F,

        /// <summary>
        /// Retrieves the state of the Mouse Vanish feature. The pvParam parameter must point to a BOOL variable that receives TRUE 
        /// if enabled or FALSE otherwise. For more information, see About Mouse Input on MSDN.
        /// Windows 2000/NT, Windows 98/95:  This value is not supported.
        /// </summary>
        GetMouseVanish = 0x1020,

        /// <summary>
        /// Turns the Vanish feature on or off. This feature hides the mouse pointer when the user types; the pointer reappears 
        /// when the user moves the mouse. The pvParam parameter specifies TRUE for on and FALSE for off. The default is off. 
        /// For more information, see About Mouse Input on MSDN.
        /// Windows 2000/NT, Windows 98/95:  This value is not supported.
        /// </summary>
        SetMouseVanish = 0x1021,

        /// <summary>
        /// Determines whether native User menus have flat menu appearance. The pvParam parameter must point to a BOOL variable 
        /// that returns TRUE if the flat menu appearance is set, or FALSE otherwise.
        /// Windows 2000/NT, Windows Me/98/95:  This value is not supported.
        /// </summary>
        GetFlatMenu = 0x1022,

        /// <summary>
        /// Enables or disables flat menu appearance for native User menus. Set pvParam to TRUE to enable flat menu appearance 
        /// or FALSE to disable it. 
        /// When enabled, the menu bar uses COLOR_MenuBAR for the menubar background, COLOR_Menu for the menu-popup background, COLOR_MenuHILIGHT 
        /// for the fill of the current menu selection, and COLOR_HILIGHT for the outline of the current menu selection. 
        /// If disabled, menus are drawn using the same metrics and colors as in Windows 2000 and earlier.
        /// Windows 2000/NT, Windows Me/98/95:  This value is not supported.
        /// </summary>
        SetFlatMenu = 0x1023,

        /// <summary>
        /// Determines whether the drop shadow effect is enabled. The pvParam parameter must point to a BOOL variable that returns TRUE 
        /// if enabled or FALSE if disabled.
        /// Windows 2000/NT, Windows Me/98/95:  This value is not supported.
        /// </summary>
        GetDropShadow = 0x1024,

        /// <summary>
        /// Enables or disables the drop shadow effect. Set pvParam to TRUE to enable the drop shadow effect or FALSE to disable it. 
        /// You must also have CS_DropShadow in the window class style.
        /// Windows 2000/NT, Windows Me/98/95:  This value is not supported.
        /// </summary>
        SetDropShadow = 0x1025,

        /// <summary>
        /// Retrieves a BOOL indicating whether an application can reset the screensaver's timer by calling the SendInput function 
        /// to simulate keyboard or mouse input. The pvParam parameter must point to a BOOL variable that receives TRUE 
        /// if the simulated input will be blocked, or FALSE otherwise. 
        /// </summary>
        GetBlockSendInputResets = 0x1026,

        /// <summary>
        /// Determines whether an application can reset the screensaver's timer by calling the SendInput function to simulate keyboard 
        /// or mouse input. The uiParam parameter specifies TRUE if the screensaver will not be deactivated by simulated input, 
        /// or FALSE if the screensaver will be deactivated by simulated input.
        /// </summary>
        SetBlockSendInputResets = 0x1027,

        /// <summary>
        /// Determines whether UI effects are enabled or disabled. The pvParam parameter must point to a BOOL variable that receives TRUE 
        /// if all UI effects are enabled, or FALSE if they are disabled.
        /// Windows NT, Windows Me/98/95:  This value is not supported.
        /// </summary>
        GetUIEffects = 0x103E,

        /// <summary>
        /// Enables or disables UI effects. Set the pvParam parameter to TRUE to enable all UI effects or FALSE to disable all UI effects.
        /// Windows NT, Windows Me/98/95:  This value is not supported.
        /// </summary>
        SetUIEffects = 0x103F,

        /// <summary>
        /// Retrieves the amount of time following user input, in milliseconds, during which the system will not allow applications 
        /// to force themselves into the foreground. The pvParam parameter must point to a DWORD variable that receives the time.
        /// Windows NT, Windows 95:  This value is not supported.
        /// </summary>
        GetForegroundLockTimeout = 0x2000,

        /// <summary>
        /// Sets the amount of time following user input, in milliseconds, during which the system does not allow applications 
        /// to force themselves into the foreground. Set pvParam to the new timeout value.
        /// The calling thread must be able to change the foreground window, otherwise the call fails.
        /// Windows NT, Windows 95:  This value is not supported.
        /// </summary>
        SetForegroundLockTimeout = 0x2001,

        /// <summary>
        /// Retrieves the active window tracking delay, in milliseconds. The pvParam parameter must point to a DWORD variable 
        /// that receives the time.
        /// Windows NT, Windows 95:  This value is not supported.
        /// </summary>
        GetActiveWNDTRKTimeout = 0x2002,

        /// <summary>
        /// Sets the active window tracking delay. Set pvParam to the number of milliseconds to delay before activating the window 
        /// under the mouse pointer.
        /// Windows NT, Windows 95:  This value is not supported.
        /// </summary>
        SetActiveWNDTRKTimeout = 0x2003,

        /// <summary>
        /// Retrieves the number of times SetForegroundWindow will flash the taskbar button when rejecting a foreground switch request. 
        /// The pvParam parameter must point to a DWORD variable that receives the value.
        /// Windows NT, Windows 95:  This value is not supported.
        /// </summary>
        GetForegroundFlashCount = 0x2004,

        /// <summary>
        /// Sets the number of times SetForegroundWindow will flash the taskbar button when rejecting a foreground switch request. 
        /// Set pvParam to the number of times to flash.
        /// Windows NT, Windows 95:  This value is not supported.
        /// </summary>
        SetForegroundFlashCount = 0x2005,

        /// <summary>
        /// Retrieves the caret width in edit controls, in pixels. The pvParam parameter must point to a DWORD that receives this value.
        /// Windows NT, Windows Me/98/95:  This value is not supported.
        /// </summary>
        GetCaretWidth = 0x2006,

        /// <summary>
        /// Sets the caret width in edit controls. Set pvParam to the desired width, in pixels. The default and minimum value is 1.
        /// Windows NT, Windows Me/98/95:  This value is not supported.
        /// </summary>
        SetCaretWidth = 0x2007,

        /// <summary>
        /// Retrieves the time delay before the primary mouse button is locked. The pvParam parameter must point to DWORD that receives 
        /// the time delay. This is only enabled if SetMouseClickLock is set to TRUE. For more information, see About Mouse Input on MSDN.
        /// Windows 2000/NT, Windows 98/95:  This value is not supported.
        /// </summary>
        GetMouseClickLockTime = 0x2008,

        /// <summary>
        /// Turns the Mouse ClickLock accessibility feature on or off. This feature temporarily locks down the primary mouse button 
        /// when that button is clicked and held down for the time specified by SetMouseClickLockTime. The uiParam parameter 
        /// specifies TRUE for on, or FALSE for off. The default is off. For more information, see Remarks and About Mouse Input on MSDN.
        /// Windows 2000/NT, Windows 98/95:  This value is not supported.
        /// </summary>
        SetMouseClickLockTime = 0x2009,

        /// <summary>
        /// Retrieves the type of font smoothing. The pvParam parameter must point to a UINT that receives the information.
        /// Windows 2000/NT, Windows Me/98/95:  This value is not supported.
        /// </summary>
        GetFontSmoothingType = 0x200A,

        /// <summary>
        /// Sets the font smoothing type. The pvParam parameter points to a UINT that contains either FE_FontSmoothingSTANDARD, 
        /// if standard anti-aliasing is used, or FE_FontSmoothingCLEARType, if ClearType is used. The default is FE_FontSmoothingSTANDARD. 
        /// When using this option, the fWinIni parameter must be set to SPIF_SendWININICHANGE | SPIF_UPDATEINIFILE; otherwise, 
        /// SystemParametersInfo fails.
        /// </summary>
        SetFontSmoothingType = 0x200B,

        /// <summary>
        /// Retrieves a contrast value that is used in ClearType™ smoothing. The pvParam parameter must point to a UINT 
        /// that receives the information.
        /// Windows 2000/NT, Windows Me/98/95:  This value is not supported.
        /// </summary>
        GetFontSmoothingContrast = 0x200C,

        /// <summary>
        /// Sets the contrast value used in ClearType smoothing. The pvParam parameter points to a UINT that holds the contrast value. 
        /// Valid contrast values are from 1000 to 2200. The default value is 1400.
        /// When using this option, the fWinIni parameter must be set to SPIF_SendWININICHANGE | SPIF_UPDATEINIFILE; otherwise, 
        /// SystemParametersInfo fails.
        /// SetFontSmoothingType must also be set to FE_FontSmoothingCLEARType.
        /// Windows 2000/NT, Windows Me/98/95:  This value is not supported.
        /// </summary>
        SetFontSmoothingContrast = 0x200D,

        /// <summary>
        /// Retrieves the width, in pixels, of the left and right edges of the focus rectangle drawn with DrawFocusRect. 
        /// The pvParam parameter must point to a UINT.
        /// Windows 2000/NT, Windows Me/98/95:  This value is not supported.
        /// </summary>
        GetFocusBorderWidth = 0x200E,

        /// <summary>
        /// Sets the height of the left and right edges of the focus rectangle drawn with DrawFocusRect to the value of the pvParam parameter.
        /// Windows 2000/NT, Windows Me/98/95:  This value is not supported.
        /// </summary>
        SetFocusBorderWidth = 0x200F,

        /// <summary>
        /// Retrieves the height, in pixels, of the top and bottom edges of the focus rectangle drawn with DrawFocusRect. 
        /// The pvParam parameter must point to a UINT.
        /// Windows 2000/NT, Windows Me/98/95:  This value is not supported.
        /// </summary>
        GetFocusBorderHeight = 0x2010,

        /// <summary>
        /// Sets the height of the top and bottom edges of the focus rectangle drawn with DrawFocusRect to the value of the pvParam parameter.
        /// Windows 2000/NT, Windows Me/98/95:  This value is not supported.
        /// </summary>
        SetFocusBorderHeight = 0x2011,

        /// <summary>
        /// Not implemented.
        /// </summary>
        GetFontSmoothingOrientation = 0x2012,

        /// <summary>
        /// Not implemented.
        /// </summary>
        SetFontSmoothingOrientation = 0x2013,
    }
}
