namespace MenthaAssembly
{
    /// <summary>
    /// Specifies when <see cref="Logger"/> starts writing to a new log file.
    /// </summary>
    public enum LogFileRotation
    {
        /// <summary>
        /// Writes one file per hour: HH.txt
        /// </summary>
        Hourly = 0,

        /// <summary>
        /// Writes one file per day: yyyyMMdd.txt
        /// </summary>
        Daily = 1,
    }
}
