namespace MenthaAssembly
{
    /// <summary>
    /// Specifies the folder layout used by <see cref="Logger"/> when writing log files.
    /// </summary>
    public enum LogFolderLayout
    {
        /// <summary>
        /// Writes files to: RootFolder / FileName
        /// </summary>
        Root = 0,

        /// <summary>
        /// Writes files to: RootFolder / Category / FileName
        /// When <see cref="LogEntry.Category"/> is empty, the Category folder is skipped.
        /// </summary>
        Category = 1,

        /// <summary>
        /// Writes files to: RootFolder / yyyyMMdd / FileName
        /// </summary>
        Date = 2,

        /// <summary>
        /// Writes files to: RootFolder / Category / yyyyMMdd / FileName
        /// When <see cref="LogEntry.Category"/> is empty, the Category folder is skipped.
        /// </summary>
        CategoryDate = 3,

        /// <summary>
        /// Writes files to: RootFolder / yyyyMMdd / Category / FileName
        /// When <see cref="LogEntry.Category"/> is empty, the Category folder is skipped.
        /// </summary>
        DateCategory = 4,

        /// <summary>
        /// Writes files to: RootFolder / Logger.Name / FileName
        /// </summary>
        Name = 5,

        /// <summary>
        /// Writes files to: RootFolder / Logger.Name / Category / FileName
        /// When <see cref="LogEntry.Category"/> is empty, the Category folder is skipped.
        /// </summary>
        NameCategory = 6,

        /// <summary>
        /// Writes files to: RootFolder / Logger.Name / yyyyMMdd / FileName
        /// </summary>
        NameDate = 7,

        /// <summary>
        /// Writes files to: RootFolder / Logger.Name / Category / yyyyMMdd / FileName
        /// When <see cref="LogEntry.Category"/> is empty, the Category folder is skipped.
        /// </summary>
        NameCategoryDate = 8,

        /// <summary>
        /// Writes files to: RootFolder / Logger.Name / yyyyMMdd / Category / FileName
        /// When <see cref="LogEntry.Category"/> is empty, the Category folder is skipped.
        /// </summary>
        NameDateCategory = 9,
    }
}
