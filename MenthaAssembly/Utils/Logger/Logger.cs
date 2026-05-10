using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace MenthaAssembly
{
    public sealed class Logger
    {
        private static readonly ConcurrentDictionary<string, Logger> Loggers = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Gets the shared general-purpose logger.
        /// </summary>
        public static Logger General
            => Register(nameof(General), GetDefaultRootFolder());

        /// <summary>
        /// Gets the console output logger, primarily for development errors and notifications.
        /// </summary>
        public static Logger Console
            => Register(nameof(Console),
                        GetDefaultConsoleRootFolder(),
                        new LoggerOptions
                        {
                            FolderLayout = LogFolderLayout.Category,
                            FileRotation = LogFileRotation.Daily,
                        });

        /// <summary>
        /// Occurs when a log entry is published.
        /// Set <see cref="LogEventArgs.CancelWriting"/> to <see langword="true"/> to skip file output.
        /// </summary>
        public event EventHandler<LogEventArgs> Logged;

        /// <summary>
        /// Gets the unique logger name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets or sets the root folder used for file output.
        /// </summary>
        public string RootFolder { get; }

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
        /// Gets the folder layout used for file output.
        /// </summary>
        public LogFolderLayout FolderLayout { get; }

        /// <summary>
        /// Gets when file output rotates to a new file.
        /// </summary>
        public LogFileRotation FileRotation { get; }

        private Logger(string name, string rootFolder, LoggerOptions options)
        {
            if (string.IsNullOrWhiteSpace(rootFolder))
                throw new ArgumentException("Root folder cannot be null or empty.", nameof(rootFolder));

            Name = name;
            RootFolder = rootFolder.Trim();
            EnableFileWriting = options?.EnableFileWriting ?? true;
            RetentionDays = options?.RetentionDays ?? 30;
            FolderLayout = options?.FolderLayout ?? LogFolderLayout.NameDate;
            FileRotation = options?.FileRotation ?? LogFileRotation.Hourly;
            WriteTimer = new Timer(OnWriteTimerTick, null, Timeout.Infinite, Timeout.Infinite);
        }

        /// <summary>
        /// Publishes a log entry.
        /// </summary>
        /// <param name="entry">The log entry to publish.</param>
        public void Log(LogEntry entry)
        {
            if (entry == null)
                throw new ArgumentNullException(nameof(entry));

            LogEventArgs args = new LogEventArgs(entry);
            Logged?.Invoke(this, args);

            if (args.CancelWriting || !EnableFileWriting)
                return;

            EnqueueWriting(entry);
        }
        /// <summary>
        /// Publishes a log entry with the specified level.
        /// </summary>
        public void Log(LogLevel level, object content)
            => Log(new LogEntry(level, content));
        /// <summary>
        /// Publishes a log entry with the specified level and category.
        /// </summary>
        public void Log(LogLevel level, string category, object content)
            => Log(new LogEntry(level, content) { Category = category });

        /// <summary>
        /// Publishes a trace log entry.
        /// </summary>
        public void Trace(object content)
            => Log(LogLevel.Trace, content);
        /// <summary>
        /// Publishes a debug log entry.
        /// </summary>
        public void Debug(object content)
            => Log(LogLevel.Debug, content);
        /// <summary>
        /// Publishes an information log entry.
        /// </summary>
        public void Info(object content)
            => Log(LogLevel.Info, content);
        /// <summary>
        /// Publishes a warning log entry.
        /// </summary>
        public void Warning(object content)
            => Log(LogLevel.Warning, content);
        /// <summary>
        /// Publishes an error log entry.
        /// </summary>
        public void Error(object content)
            => Log(LogLevel.Error, content);

        private int IsWriting;
        private readonly Timer WriteTimer;
        private readonly ConcurrentQueue<LogEntry> WriteQueue = new();
        private void EnqueueWriting(LogEntry entry)
        {
            WriteQueue.Enqueue(entry);
            WriteTimer.Change(0, Timeout.Infinite);
        }
        private void OnWriteTimerTick(object state)
        {
            if (Interlocked.Exchange(ref IsWriting, 1) == 1)
                return;

            bool canContinueWriting = true;
            try
            {
                while (WriteQueue.TryPeek(out LogEntry entry))
                {
                    if (!TryWriteToFile(entry))
                    {
                        canContinueWriting = false;
                        break;
                    }

                    WriteQueue.TryDequeue(out _);
                }
            }
            finally
            {
                Interlocked.Exchange(ref IsWriting, 0);

                if (canContinueWriting && !WriteQueue.IsEmpty)
                    WriteTimer.Change(0, Timeout.Infinite);
            }
        }
        private bool TryWriteToFile(LogEntry entry)
        {
            try
            {
                if (!EnableFileWriting)
                    return true;

                string filePath = GetFilePath(entry);
                if (string.IsNullOrWhiteSpace(filePath))
                    return true;

                string logFolder = Path.GetDirectoryName(filePath);
                if (string.IsNullOrWhiteSpace(logFolder))
                    return true;

                DeleteExpiredFiles();
                Directory.CreateDirectory(logFolder);
                using StreamWriter writer = File.AppendText(filePath);
                writer.WriteLine(entry);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private readonly object ExpiredFilesSyncRoot = new();
        private void DeleteExpiredFiles()
        {
            if (RetentionDays <= 0 || string.IsNullOrWhiteSpace(RootFolder) || !Directory.Exists(RootFolder))
                return;

            foreach (string loggerFolder in GetRetentionFolders())
            {
                if (!Directory.Exists(loggerFolder))
                    continue;

                lock (ExpiredFilesSyncRoot)
                {
                    DeleteExpiredDateFolders(loggerFolder);
                    DeleteExpiredDailyFiles(loggerFolder);
                }
            }
        }
        private void DeleteExpiredDateFolders(string folder)
        {
            foreach (string dateFolder in Directory.EnumerateDirectories(folder))
            {
                string folderName = Path.GetFileName(dateFolder);
                if (!DateTime.TryParseExact(folderName, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime folderDate))
                    continue;

                if ((DateTime.Today - folderDate).TotalDays <= RetentionDays)
                    continue;

                try
                {
                    Directory.Delete(dateFolder, true);
                }
                catch
                {
                }
            }
        }
        private void DeleteExpiredDailyFiles(string folder)
        {
            foreach (string file in Directory.EnumerateFiles(folder, "*.txt"))
            {
                string fileName = Path.GetFileNameWithoutExtension(file);
                if (!DateTime.TryParseExact(fileName, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fileDate))
                    continue;

                if ((DateTime.Today - fileDate).TotalDays <= RetentionDays)
                    continue;

                try
                {
                    File.Delete(file);
                }
                catch
                {
                }
            }
        }

        private string GetFilePath(LogEntry entry)
        {
            string[] folders = GetFolders(entry);
            string[] segments = new string[folders.Length + 2];
            segments[0] = RootFolder;
            for (int i = 0; i < folders.Length; i++)
                segments[i + 1] = folders[i];

            segments[segments.Length - 1] = GetFileName(entry);
            return Path.Combine(segments);
        }
        private string GetFileName(LogEntry entry)
            => FileRotation switch
            {
                LogFileRotation.Daily => $"{entry.Timestamp:yyyyMMdd}.txt",
                _ => $"{entry.Timestamp:HH}.txt",
            };
        private string[] GetFolders(LogEntry entry)
        {
            string dateFolder = entry.Timestamp.ToString("yyyyMMdd");
            string category = entry.Category?.Trim();

            return FolderLayout switch
            {
                LogFolderLayout.Root => Array.Empty<string>(),
                LogFolderLayout.Category => FilterFolders(category),
                LogFolderLayout.Date => new[] { dateFolder },
                LogFolderLayout.CategoryDate => FilterFolders(category, dateFolder),
                LogFolderLayout.DateCategory => FilterFolders(dateFolder, category),
                LogFolderLayout.Name => new[] { Name },
                LogFolderLayout.NameCategory => FilterFolders(Name, category),
                LogFolderLayout.NameDate => new[] { Name, dateFolder },
                LogFolderLayout.NameCategoryDate => FilterFolders(Name, category, dateFolder),
                LogFolderLayout.NameDateCategory => FilterFolders(Name, dateFolder, category),
                _ => new[] { Name, dateFolder },
            };
        }
        private string[] GetRetentionFolders()
        {
            return FolderLayout switch
            {
                LogFolderLayout.Root => new[] { RootFolder },
                LogFolderLayout.Category => GetRootAndChildFolders(RootFolder),
                LogFolderLayout.Date => new[] { RootFolder },
                LogFolderLayout.CategoryDate => GetRootAndChildFolders(RootFolder),
                LogFolderLayout.DateCategory => new[] { RootFolder },
                LogFolderLayout.Name => new[] { Path.Combine(RootFolder, Name) },
                LogFolderLayout.NameCategory => GetRootAndChildFolders(Path.Combine(RootFolder, Name)),
                LogFolderLayout.NameDate => new[] { Path.Combine(RootFolder, Name) },
                LogFolderLayout.NameCategoryDate => GetRootAndChildFolders(Path.Combine(RootFolder, Name)),
                LogFolderLayout.NameDateCategory => new[] { Path.Combine(RootFolder, Name) },
                _ => new[] { Path.Combine(RootFolder, Name) },
            };
        }
        private static string[] GetRootAndChildFolders(string rootFolder)
            => Directory.Exists(rootFolder) ?
               new[] { rootFolder }.Concat(Directory.EnumerateDirectories(rootFolder)).ToArray() :
               new[] { rootFolder };

        private static string[] FilterFolders(params string[] folders)
            => folders.Where(i => !string.IsNullOrWhiteSpace(i))
                      .ToArray();

        /// <summary>
        /// Gets a registered logger by name.
        /// </summary>
        public static Logger Get(string name)
            => Loggers[NormalizeName(name)];
        /// <summary>
        /// Attempts to get a registered logger by name.
        /// </summary>
        public static bool TryGet(string name, out Logger logger)
            => Loggers.TryGetValue(NormalizeName(name), out logger);

        /// <summary>
        /// Registers a logger using the specified name and root folder.
        /// </summary>
        public static Logger Register(string name, string rootFolder)
            => Register(name, rootFolder, null);
        /// <summary>
        /// Registers a logger using the specified name, root folder, and options.
        /// If the logger already exists, the existing instance is returned.
        /// </summary>
        public static Logger Register(string name, string rootFolder, LoggerOptions options)
            => Loggers.GetOrAdd(NormalizeName(name), loggerName => new Logger(loggerName, rootFolder, options ?? new LoggerOptions()));

        /// <summary>
        /// Unregisters a logger by name.
        /// </summary>
        public static bool Unregister(string name)
            => Loggers.TryRemove(NormalizeName(name), out _);

        /// <summary>
        /// Gets the default root folder used by <see cref="General"/>.
        /// </summary>
        public static string GetDefaultRootFolder()
        {
            string[] preferredDrives = { @"D:\", @"C:\" };
            foreach (string drive in preferredDrives)
            {
                if (Directory.Exists(drive))
                    return Path.Combine(drive, "Logs");
            }

            DriveInfo availableDrive = DriveInfo.GetDrives()
                                               .Where(d => d.IsReady)
                                               .OrderBy(d => d.Name)
                                               .FirstOrDefault();

            return availableDrive == null ? null : Path.Combine(availableDrive.RootDirectory.FullName, "Logs");
        }
        /// <summary>
        /// Gets the default root folder used by <see cref="Console"/>.
        /// </summary>
        public static string GetDefaultConsoleRootFolder()
        {
            Assembly assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
            AssemblyName assemblyName = assembly.GetName();
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string applicationName = SanitizePathSegment(assemblyName.Name);
            string version = SanitizePathSegment(assemblyName.Version?.ToString() ?? "0.0.0.0");

            return Path.Combine(appData, applicationName, nameof(Console), version);
        }

        private static string NormalizeName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Logger name cannot be null or empty.", nameof(name));

            return name.Trim();
        }
        private static string SanitizePathSegment(string segment)
        {
            if (string.IsNullOrWhiteSpace(segment))
                return "_";

            char[] invalidChars = Path.GetInvalidFileNameChars();
            char[] chars = segment.Trim().ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                if (invalidChars.Contains(chars[i]))
                    chars[i] = '_';
            }

            return new string(chars);
        }

    }
}