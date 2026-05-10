using System;

namespace MenthaAssembly
{
    public sealed class LogEventArgs(LogEntry entry) : EventArgs
    {
        /// <summary>
        /// Gets the log entry being published.
        /// </summary>
        public LogEntry Entry { get; } = entry ?? throw new ArgumentNullException(nameof(entry));

        /// <summary>
        /// Gets or sets a value indicating whether file writing should be skipped.
        /// </summary>
        public bool CancelWriting { get; set; }

    }
}