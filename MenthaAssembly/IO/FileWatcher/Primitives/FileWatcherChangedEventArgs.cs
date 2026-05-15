using System;
using System.IO;

namespace MenthaAssembly.IO
{
    public sealed class FileWatcherChangedEventArgs(string FullPath, WatcherChangeTypes ChangeType, string OldFullPath = null) : EventArgs
    {
        public string FullPath { get; } = FullPath ?? throw new ArgumentNullException(nameof(FullPath));

        public string OldFullPath { get; } = OldFullPath;

        public WatcherChangeTypes ChangeType { get; } = ChangeType;

    }
}
