namespace MenthaAssembly.Win32
{
    public enum WindowLongType
    {
        /// <summary>
        /// Set/Get a new address for the window procedure. 
        /// You cannot change this attribute if the window does not belong to the same process as the calling thread.
        /// </summary>
        WndProc = -4,

        /// <summary>
        /// Set/Get a new application instance handle. 
        /// </summary>
        HInstance = -6,

        HwndParent = -8,

        /// <summary>
        /// Set/Get a new identifier of the child window.The window cannot be a top -level window.
        /// </summary>
        ID = -12,

        /// <summary>
        /// Set/Get a new window style. 
        /// </summary>
        Style = -16,

        /// <summary>
        /// Set/Get a new extended window style. 
        /// </summary>
        ExStyle = -20,

        /// <summary>
        /// Set/Get the user data associated with the window. 
        /// This data is intended for use by the application that created the window. 
        /// Its value is initially zero.  
        /// </summary>
        UserData = -21,
    }
}
