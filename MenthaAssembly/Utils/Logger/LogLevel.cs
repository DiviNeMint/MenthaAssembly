namespace MenthaAssembly
{
    /// <summary>
    /// Represents the severity level of a log entry.
    /// </summary>
    public enum LogLevel : byte
    {

        /// <summary>
        /// Indicates detailed diagnostic information intended for tracing execution flow.
        /// </summary>
        Trace = 0,

        /// <summary>
        /// Indicates diagnostic information useful during development and debugging.
        /// </summary>
        Debug = 1,

        /// <summary>
        /// Indicates a normal informational message.
        /// </summary>
        Info = 2,

        /// <summary>
        /// Indicates a warning about a potential issue or unexpected state.
        /// </summary>
        Warning = 3,

        /// <summary>
        /// Indicates an error or failure condition.
        /// </summary>
        Error = 4,
    }
}
