namespace MenthaAssembly
{
    public sealed class LoggerOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether this logger writes entries to files.
        /// </summary>
        public bool EnableFileWriting { get; set; } = true;

        /// <summary>
        /// Gets or sets the number of days to keep log files and folders.
        /// Values less than or equal to zero disable automatic cleanup.
        /// </summary>
        public int RetentionDays { get; set; } = 30;

        /// <summary>
        /// Gets or sets the folder layout used for file output.
        /// </summary>
        public LogFolderLayout FolderLayout { get; set; } = LogFolderLayout.NameDate;

        /// <summary>
        /// Gets or sets when file output rotates to a new file.
        /// </summary>
        public LogFileRotation FileRotation { get; set; } = LogFileRotation.Hourly;

    }
}
