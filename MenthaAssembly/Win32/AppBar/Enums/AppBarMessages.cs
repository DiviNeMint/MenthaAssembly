namespace MenthaAssembly.Win32
{
    internal enum AppBarMessages : uint
    {
        /// <summary>
		/// Registers a new appbar and specifies the message identifier that the system should use to send notification messages to the appbar.
		/// </summary>
        New = 0x00,

        /// <summary>
		/// Unregisters an appbar, removing the bar from the system's internal list.
		/// </summary>
        Remove = 0x01,

        /// <summary>
		/// Requests a size and screen position for an appbar.
		/// </summary>
        QueryPos = 0x02,

        /// <summary>
		/// Sets the size and screen position of an appbar.
		/// </summary>
        SetPos = 0x03,

        /// <summary>
		/// Retrieves the autohide and always-on-top states of the Windows taskbar.
		/// </summary>
        GetState = 0x04,

        /// <summary>
        /// Retrieves the bounding rectangle of the Windows taskbar.
        /// Note that this applies only to the system taskbar. 
        /// Other objects, particularly toolbars supplied with third-party software, also can be present. 
        /// As a result, some of the screen area not covered by the Windows taskbar might not be visible to the user. 
        /// To retrieve the area of the screen not covered by both the taskbar and other app bars—the working area available to your application—, use the GetMonitorInfo function.
        /// </summary>
        GetTaskBarPos = 0x05,

        /// <summary>
		/// Notifies the system to activate or deactivate an appbar.The lParam member of the APPBARDATA pointed to by pData is set to TRUE to activate or FALSE to deactivate.
		/// </summary>
        Activate = 0x06,

        /// <summary>
		/// Retrieves the handle to the autohide appbar associated with a particular edge of the screen.
		/// </summary>
        GetAutoHideBar = 0x07,

        /// <summary>
		/// Registers or unregisters an autohide appbar for an edge of the screen.
		/// </summary>
        SetAutoHideBar = 0x08,

        /// <summary>
		/// Notifies the system when an appbar's position has changed.
		/// </summary>
        WindowPosChanged = 0x09,

        /// <summary>
		/// Windows XP and later: Sets the state of the appbar's autohide and always-on-top attributes.
		/// </summary>
        SetState = 0x0A,

        /// <summary>
		/// Windows XP and later: Retrieves the handle to the autohide appbar associated with a particular edge of a particular monitor.
		/// </summary>
        GetAutoHideBarEx = 0x0B,

        /// <summary>
		/// Windows XP and later: Registers or unregisters an autohide appbar for an edge of a particular monitor.
		/// </summary>
        SetAutoHideBarEx = 0x0C
    }
}
