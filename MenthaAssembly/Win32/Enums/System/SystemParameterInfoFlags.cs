namespace MenthaAssembly.Win32
{
    public enum SystemParameterInfoFlags : uint
    {
        None = 0x00,

        /// <summary>
        /// Writes the new system-wide parameter setting to the user profile.
        /// </summary>
        UpdateIniFile = 0x01,

        /// <summary>
        /// Broadcasts the WM_SettingChange message after updating the user profile.
        /// </summary>
        SendChange = 0x02,

        /// <summary>
        /// Same as SPIF_SendChange.
        /// </summary>
        SendWinIniChange = 0x02
    }
}
