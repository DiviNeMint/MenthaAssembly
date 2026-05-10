using System;

namespace MenthaAssembly
{
    public sealed class LogEntry : EventArgs
    {
        /// <summary>
        /// Gets or sets the timestamp of this log entry.
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.Now;

        /// <summary>
        /// Gets or sets the severity level of this log entry.
        /// </summary>
        public LogLevel Level { get; set; } = LogLevel.Info;

        /// <summary>
        /// Gets or sets the optional category used for log grouping.
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Gets or sets the source object associated with this log entry.
        /// </summary>
        public object Source { get; set; }

        /// <summary>
        /// Gets or sets the log content.
        /// </summary>
        public object Content { get; set; }

        public LogEntry()
        {
        }
        public LogEntry(LogLevel level, object content)
        {
            Level = level;
            Content = content;
        }

        public override string ToString()
            => string.IsNullOrWhiteSpace(Category) ?
               $"[{Timestamp:yyyy/MM/dd HH:mm:ss}][{Level}]{Content}" :
               $"[{Timestamp:yyyy/MM/dd HH:mm:ss}][{Level}][{Category}]{Content}";
    }
}