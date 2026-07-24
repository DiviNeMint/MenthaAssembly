namespace MenthaAssembly.Win32
{
    /// <summary>
    /// Specifies the Desktop Window Manager preference for rounding a top-level window's corners.
    /// </summary>
    internal enum DwmWindowCornerPreference
    {
        /// <summary>
        /// Lets the system decide whether the window corners should be rounded.
        /// </summary>
        Default = 0,

        /// <summary>
        /// Prevents the window corners from being rounded.
        /// </summary>
        DoNotRound = 1,

        /// <summary>
        /// Rounds the window corners when the system considers rounding appropriate.
        /// </summary>
        Round = 2,

        /// <summary>
        /// Rounds the window corners with a small radius when the system considers rounding appropriate.
        /// </summary>
        RoundSmall = 3,

    }
}
