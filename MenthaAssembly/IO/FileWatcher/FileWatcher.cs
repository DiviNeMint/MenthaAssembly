using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace MenthaAssembly.IO
{
    /// <summary>
    /// Monitors files that match a configured watch path rule and raises notifications for file system changes.
    /// </summary>
    public sealed class FileWatcher : IDisposable, INotifyPropertyChanged
    {
        /// <summary>
        /// Occurs when a matched file is created, changed, deleted, or renamed.
        /// </summary>
        public event EventHandler<FileWatcherChangedEventArgs> Changed;

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        private WatchPathRule _Path;
        /// <summary>
        /// Gets or sets the rule that defines the file system path to monitor.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The current instance has been disposed.</exception>
        /// <exception cref="InvalidOperationException">The property is set while the watcher is running.</exception>
        public WatchPathRule Path
        {
            get => _Path;
            set
            {
                ThrowIfDisposed();

                if (IsRunning)
                    throw new InvalidOperationException("Cannot change path while the watcher is running.");

                if (ReferenceEquals(_Path, value))
                    return;

                Processor.Clear();
                Dispatcher.Clear();
                ActivePath = null;
                Root = null;

                _Path = value;
                OnPropertyChanged();
            }
        }

        private bool _IsRunning;
        /// <summary>
        /// Gets a value that indicates whether the watcher is currently running.
        /// </summary>
        public bool IsRunning
        {
            get => _IsRunning;
            private set
            {
                if (_IsRunning == value)
                    return;

                _IsRunning = value;
                OnPropertyChanged();
            }
        }

        private WatchNode Root;
        private WatchPathRule ActivePath;
        /// <summary>
        /// Initializes a new instance of the <see cref="FileWatcher"/> class.
        /// </summary>
        public FileWatcher()
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="FileWatcher"/> class with the specified watch path rule.
        /// </summary>
        /// <param name="Path">The rule that defines the file system path to monitor.</param>
        public FileWatcher(WatchPathRule Path)
        {
            this.Path = Path;
        }

        private readonly WatcherProcessor Processor = new();
        private readonly DispatchWorker Dispatcher = new();
        /// <summary>
        /// Starts monitoring the configured path.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The current instance has been disposed.</exception>
        /// <exception cref="InvalidOperationException"><see cref="Path"/> is <see langword="null"/>.</exception>
        /// <exception cref="DirectoryNotFoundException">The root directory specified by <see cref="Path"/> does not exist.</exception>
        public void Start()
        {
            ThrowIfDisposed();

            if (IsRunning)
                return;

            WatchPathRule SourceRule = Path ?? throw new InvalidOperationException("Path must be set before starting the watcher.");
            SourceRule.Validate();
            WatchPathRule Rule = SourceRule.Clone();

            Root = BuildRoot(Rule);
            ActivePath = Rule;
            IsRunning = true;

            Processor.Start();
            Dispatcher.Start(OnChanged);
        }
        /// <summary>
        /// Stops monitoring the configured path.
        /// </summary>
        public void Stop()
        {
            if (!IsRunning)
                return;

            IsRunning = false;
            Dispatcher.Stop();

            using ManualResetEventSlim Completed = new(false);
            Processor.EnqueueControl(() =>
            {
                try
                {
                    WatchNode OldRoot = Root;
                    Root = null;
                    ActivePath = null;

                    DisposeSubtree(OldRoot);
                    Processor.Stop();
                }
                finally
                {
                    Completed.Set();
                }
            });

            Completed.Wait();
        }

        private WatchNode BuildRoot(WatchPathRule Rule)
        {
            PathSegmentRule RootRule = Rule.Segments[0];
            string RootPath = System.IO.Path.GetFullPath(RootRule.Pattern);

            if (!Directory.Exists(RootPath))
                throw new DirectoryNotFoundException(RootPath);

            WatchNode Node = new(RootPath);
            BuildNode(Node, Rule, 1);

            return Node;
        }
        private void BuildNode(WatchNode Node, WatchPathRule Rule, int NextSegmentIndex)
        {
            if (NextSegmentIndex >= Rule.Segments.Count)
                return;

            PathSegmentRule Segment = Rule.Segments[NextSegmentIndex];
            if (Segment.Kind == PathSegmentKind.File)
            {
                string Pattern = Segment.MatchMode == SegmentMatchMode.Wildcard ? Segment.Pattern : CreateWildcardPattern(Segment.Pattern);
                Node.Watcher = new FileSystemWatcher(Node.PhysicalPath, Pattern)
                {
                    IncludeSubdirectories = false,
                    NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size
                };

                void OnChanged(object sender, FileSystemEventArgs e)
                {
                    string FullPath = e.FullPath;
                    WatcherChangeTypes ChangeType = e.ChangeType;
                    Processor.Enqueue(() =>
                    {
                        if (!ShouldIgnoreFileEvent(Node, Segment, FullPath))
                            Dispatcher.Enqueue(new FileWatcherChangedEventArgs(FullPath, ChangeType));
                    });
                }
                void OnDeleted(object sender, FileSystemEventArgs e)
                {
                    string FullPath = e.FullPath;
                    Processor.Enqueue(() =>
                    {
                        if (IsRunning && Node.Watcher is not null)
                            Dispatcher.Enqueue(new FileWatcherChangedEventArgs(FullPath, WatcherChangeTypes.Deleted));
                    });
                }
                void OnRenamed(object sender, RenamedEventArgs e)
                {
                    string FullPath = e.FullPath;
                    string OldFullPath = e.OldFullPath;
                    Processor.Enqueue(() =>
                    {
                        if (!ShouldIgnoreFileRenameEvent(Node, Segment, FullPath, OldFullPath))
                            Dispatcher.Enqueue(new FileWatcherChangedEventArgs(FullPath, WatcherChangeTypes.Renamed, OldFullPath));
                    });
                }

                Node.Watcher.Created += OnChanged;
                Node.Watcher.Changed += OnChanged;
                Node.Watcher.Deleted += OnDeleted;
                Node.Watcher.Renamed += OnRenamed;
                Node.Watcher.EnableRaisingEvents = true;
                return;
            }

            if (Segment.IsDynamic)
            {
                string Pattern = Segment.MatchMode == SegmentMatchMode.Wildcard ? Segment.Pattern : CreateWildcardPattern(Segment.Pattern);
                Node.Watcher = new FileSystemWatcher(Node.PhysicalPath, Pattern)
                {
                    IncludeSubdirectories = false,
                    NotifyFilter = NotifyFilters.DirectoryName | NotifyFilters.CreationTime
                };

                void OnCreated(object sender, FileSystemEventArgs e)
                {
                    string FullPath = e.FullPath;
                    string Name = e.Name;
                    int ChildNextSegmentIndex = NextSegmentIndex + 1;
                    Processor.Enqueue(() =>
                    {
                        if (!IsRunning || Node.Watcher is null || !Directory.Exists(FullPath))
                            return;

                        WatchPathRule Rule = ActivePath;
                        if (IsHidden(FullPath, Segment.IgnoreHidden) || !ShouldAcceptSegment(Segment, Name))
                            return;

                        if (Node.Children.Any(i => PathEquals(i.PhysicalPath, FullPath)))
                            return;

                        WatchNode Child = new(FullPath);
                        BuildNode(Child, Rule, ChildNextSegmentIndex);
                        Node.Children.Add(Child);
                    });
                }
                void OnDeleted(object sender, FileSystemEventArgs e)
                {
                    string FullPath = e.FullPath;
                    Processor.Enqueue(() =>
                    {
                        if (IsRunning && Node.Watcher is not null)
                            RemoveChildSubtree(Node, FullPath);
                    });
                }
                void OnDirectoryRenamed(object sender, RenamedEventArgs e)
                {
                    string FullPath = e.FullPath;
                    string OldFullPath = e.OldFullPath;
                    string Name = e.Name;
                    int ChildNextSegmentIndex = NextSegmentIndex + 1;
                    Processor.Enqueue(() =>
                    {
                        if (!IsRunning || Node.Watcher is null)
                            return;

                        WatchPathRule Rule = ActivePath;
                        RemoveChildSubtree(Node, OldFullPath);

                        if (!Directory.Exists(FullPath))
                            return;

                        if (IsHidden(FullPath, Segment.IgnoreHidden) || !ShouldAcceptSegment(Segment, Name))
                            return;

                        WatchNode Child = new(FullPath);
                        BuildNode(Child, Rule, ChildNextSegmentIndex);
                        Node.Children.Add(Child);
                    });
                }

                Node.Watcher.Created += OnCreated;
                Node.Watcher.Deleted += OnDeleted;
                Node.Watcher.Renamed += OnDirectoryRenamed;
                Node.Watcher.EnableRaisingEvents = true;

                foreach (string DirectoryPath in Directory.EnumerateDirectories(Node.PhysicalPath, Pattern))
                {
                    if (IsHidden(DirectoryPath, Segment.IgnoreHidden))
                        continue;

                    string Name = System.IO.Path.GetFileName(DirectoryPath);
                    if (!ShouldAcceptSegment(Segment, Name))
                        continue;

                    WatchNode Child = new(DirectoryPath);
                    Node.Children.Add(Child);
                    BuildNode(Child, Rule, NextSegmentIndex + 1);
                }
                return;
            }

            string ChildPath = System.IO.Path.Combine(Node.PhysicalPath, Segment.Pattern);
            if (Directory.Exists(ChildPath) && !IsHidden(ChildPath, Segment.IgnoreHidden))
            {
                WatchNode Child = new(ChildPath);
                Node.Children.Add(Child);
                BuildNode(Child, Rule, NextSegmentIndex + 1);
            }
        }
        private bool ShouldIgnoreFileEvent(WatchNode Node, PathSegmentRule Segment, string FullPath)
        {
            if (Node?.Watcher is null || Segment is null)
                return true;

            if (!IsRunning || Node.Watcher is null)
                return true;

            if (IsHidden(FullPath, Segment.IgnoreHidden))
                return true;

            return !ShouldAcceptSegment(Segment, System.IO.Path.GetFileName(FullPath));
        }
        private bool ShouldIgnoreFileRenameEvent(WatchNode Node, PathSegmentRule Segment, string FullPath, string OldFullPath)
        {
            if (Node?.Watcher is null || Segment is null)
                return true;

            if (!IsRunning || Node.Watcher is null)
                return true;

            if (Segment.MatchMode != SegmentMatchMode.Regex)
                return false;

            if (!IsHidden(FullPath, Segment.IgnoreHidden) &&
                ShouldAcceptSegment(Segment, System.IO.Path.GetFileName(FullPath)))
                return false;

            return !ShouldAcceptSegment(Segment, System.IO.Path.GetFileName(OldFullPath));
        }
        private static bool ShouldAcceptSegment(PathSegmentRule Segment, string Name)
        {
            if (Segment.MatchMode != SegmentMatchMode.Regex)
                return true;

            if (string.IsNullOrEmpty(Name))
                return false;

            Match Match = Regex.Match(Name, Segment.Pattern);
            return Match.Success && Match.Index == 0 && Match.Length == Name.Length;
        }
        private static bool IsHidden(string Path, bool IgnoreHidden)
        {
            try
            {
                return IgnoreHidden && (File.GetAttributes(Path) & FileAttributes.Hidden) != 0;
            }
            catch
            {
                return false;
            }
        }
        private static bool PathEquals(string X, string Y)
            => string.Equals(
                System.IO.Path.GetFullPath(X).TrimEnd(System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar),
                System.IO.Path.GetFullPath(Y).TrimEnd(System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar),
                StringComparison.Ordinal);

        private static string CreateWildcardPattern(string Pattern)
        {
            if (string.IsNullOrEmpty(Pattern))
                return "*";

            if (Pattern.StartsWith(@"\A", StringComparison.Ordinal))
                Pattern = Pattern.Substring(2);
            else if (Pattern.StartsWith("^", StringComparison.Ordinal))
                Pattern = Pattern.Substring(1);

            if (Pattern.EndsWith(@"\z", StringComparison.Ordinal) || Pattern.EndsWith(@"\Z", StringComparison.Ordinal))
                Pattern = Pattern.Substring(0, Pattern.Length - 2);
            else if (Pattern.EndsWith("$", StringComparison.Ordinal))
                Pattern = Pattern.Substring(0, Pattern.Length - 1);

            string[] Alternatives = SplitRegexAlternatives(Pattern);
            if (Alternatives.Length > 1)
                return "*";

            if (!TryCreateWildcardSequence(Pattern, out string WildcardPattern))
                return "*";

            return string.IsNullOrEmpty(WildcardPattern) ? "*" : WildcardPattern;
        }
        private static bool TryCreateWildcardSequence(string Pattern, out string WildcardPattern)
        {
            WildcardPattern = null;
            StringBuilder Builder = new();

            for (int i = 0; i < Pattern.Length; i++)
            {
                string Token;
                char c = Pattern[i];
                if (c == '\\')
                {
                    if (!TryReadEscapedRegexToken(Pattern, ref i, out Token))
                        return false;
                }
                else if (c == '[')
                {
                    if (!TrySkipCharacterClass(Pattern, ref i))
                        return false;

                    Token = "?";
                }
                else if (c == '(')
                {
                    int EndIndex = FindGroupEnd(Pattern, i);
                    if (EndIndex < 0)
                        return false;

                    string GroupPattern = Pattern.Substring(i + 1, EndIndex - i - 1);
                    if (GroupPattern.StartsWith("?:", StringComparison.Ordinal))
                        GroupPattern = GroupPattern.Substring(2);
                    else if (GroupPattern.StartsWith("?", StringComparison.Ordinal))
                        return false;

                    Token = CreateGroupWildcardPattern(GroupPattern);
                    i = EndIndex;
                }
                else if (c == '.')
                {
                    Token = "?";
                }
                else if (c is '^' or '$')
                {
                    continue;
                }
                else if ("|)]*+?{}".IndexOf(c) >= 0)
                {
                    return false;
                }
                else
                {
                    Token = c.ToString();
                }

                if (!TryApplyRegexQuantifier(Pattern, ref i, ref Token))
                    return false;

                Builder.Append(Token);
            }

            WildcardPattern = Builder.ToString();
            return true;
        }
        private static string CreateGroupWildcardPattern(string Pattern)
        {
            string[] Alternatives = SplitRegexAlternatives(Pattern);
            if (Alternatives.Length == 1)
                return TryCreateWildcardSequence(Alternatives[0], out string WildcardPattern) ? WildcardPattern : "*";

            int Length = -1;
            foreach (string Alternative in Alternatives)
            {
                if (!TryCreateWildcardSequence(Alternative, out string WildcardPattern) ||
                    WildcardPattern.IndexOf('*') >= 0)
                    return "*";

                if (Length < 0)
                    Length = WildcardPattern.Length;
                else if (Length != WildcardPattern.Length)
                    return "*";
            }

            return Length < 0 ? "*" : new string('?', Length);
        }
        private static string[] SplitRegexAlternatives(string Pattern)
        {
            List<string> Alternatives = [];
            int StartIndex = 0;
            int GroupDepth = 0;
            bool IsInCharacterClass = false;
            bool IsEscaped = false;

            for (int i = 0; i < Pattern.Length; i++)
            {
                char c = Pattern[i];
                if (IsEscaped)
                {
                    IsEscaped = false;
                    continue;
                }

                if (c == '\\')
                {
                    IsEscaped = true;
                    continue;
                }

                if (IsInCharacterClass)
                {
                    if (c == ']')
                        IsInCharacterClass = false;

                    continue;
                }

                if (c == '[')
                {
                    IsInCharacterClass = true;
                    continue;
                }

                if (c == '(')
                {
                    GroupDepth++;
                    continue;
                }

                if (c == ')')
                {
                    if (GroupDepth > 0)
                        GroupDepth--;

                    continue;
                }

                if (c == '|' && GroupDepth == 0)
                {
                    Alternatives.Add(Pattern.Substring(StartIndex, i - StartIndex));
                    StartIndex = i + 1;
                }
            }

            if (Alternatives.Count == 0)
                return [Pattern];

            Alternatives.Add(Pattern.Substring(StartIndex));
            return Alternatives.ToArray();
        }
        private static bool TryReadEscapedRegexToken(string Pattern, ref int Index, out string Token)
        {
            Token = null;

            if (Index + 1 >= Pattern.Length)
                return false;

            char c = Pattern[++Index];
            switch (c)
            {
                case 'A':
                case 'z':
                case 'Z':
                case 'b':
                case 'B':
                    Token = string.Empty;
                    return true;

                case 'd':
                case 'D':
                case 'w':
                case 'W':
                case 's':
                case 'S':
                    Token = "?";
                    return true;

                case 'p':
                case 'P':
                    if (!TrySkipUnicodeCategory(Pattern, ref Index))
                        return false;

                    Token = "?";
                    return true;

                case 'x':
                    Index = Math.Min(Index + 2, Pattern.Length - 1);
                    Token = "?";
                    return true;

                case 'u':
                    Index = Math.Min(Index + 4, Pattern.Length - 1);
                    Token = "?";
                    return true;

                case '*':
                    Token = "*";
                    return true;

                case '?':
                    Token = "?";
                    return true;

                default:
                    Token = c.ToString();
                    return true;
            }
        }
        private static bool TrySkipUnicodeCategory(string Pattern, ref int Index)
        {
            if (Index + 1 >= Pattern.Length || Pattern[Index + 1] != '{')
                return false;

            int EndIndex = Pattern.IndexOf('}', Index + 2);
            if (EndIndex < 0)
                return false;

            Index = EndIndex;
            return true;
        }
        private static bool TrySkipCharacterClass(string Pattern, ref int Index)
        {
            bool IsEscaped = false;
            for (int i = Index + 1; i < Pattern.Length; i++)
            {
                if (IsEscaped)
                {
                    IsEscaped = false;
                    continue;
                }

                if (Pattern[i] == '\\')
                {
                    IsEscaped = true;
                    continue;
                }

                if (Pattern[i] == ']')
                {
                    Index = i;
                    return true;
                }
            }

            return false;
        }
        private static int FindGroupEnd(string Pattern, int StartIndex)
        {
            int Depth = 0;
            bool IsEscaped = false;
            bool IsInCharacterClass = false;

            for (int i = StartIndex; i < Pattern.Length; i++)
            {
                char c = Pattern[i];
                if (IsEscaped)
                {
                    IsEscaped = false;
                    continue;
                }

                if (c == '\\')
                {
                    IsEscaped = true;
                    continue;
                }

                if (IsInCharacterClass)
                {
                    if (c == ']')
                        IsInCharacterClass = false;

                    continue;
                }

                if (c == '[')
                {
                    IsInCharacterClass = true;
                    continue;
                }

                if (c == '(')
                {
                    Depth++;
                    continue;
                }

                if (c == ')')
                {
                    Depth--;
                    if (Depth == 0)
                        return i;
                }
            }

            return -1;
        }
        private static bool TryApplyRegexQuantifier(string Pattern, ref int Index, ref string Token)
        {
            if (Index + 1 >= Pattern.Length)
                return true;

            char c = Pattern[Index + 1];
            if (c is '*' or '+' or '?')
            {
                Token = "*";
                Index++;
                return true;
            }

            if (c != '{')
                return true;

            int EndIndex = Pattern.IndexOf('}', Index + 2);
            if (EndIndex < 0)
                return false;

            string Quantifier = Pattern.Substring(Index + 2, EndIndex - Index - 2);
            if (int.TryParse(Quantifier, out int Count) && Count >= 0)
                Token = RepeatWildcardToken(Token, Count);
            else
                Token = "*";

            Index = EndIndex;
            return true;
        }
        private static string RepeatWildcardToken(string Token, int Count)
        {
            if (Count == 0)
                return string.Empty;

            StringBuilder Builder = new(Token.Length * Count);
            for (int i = 0; i < Count; i++)
                Builder.Append(Token);

            return Builder.ToString();
        }

        private static void RemoveChildSubtree(WatchNode Parent, string PhysicalPath)
        {
            for (int i = Parent.Children.Count - 1; 0 <= i; i--)
            {
                WatchNode Child = Parent.Children[i];
                if (!PathEquals(Child.PhysicalPath, PhysicalPath))
                    continue;

                Parent.Children.RemoveAt(i);
                DisposeSubtree(Child);
                return;
            }
        }
        private static void DisposeSubtree(WatchNode Node)
        {
            if (Node is null)
                return;

            foreach (WatchNode Child in Node.Children)
                DisposeSubtree(Child);

            Node.Children.Clear();

            if (Node.Watcher != null)
            {
                Node.Watcher.EnableRaisingEvents = false;
                Node.Watcher.Dispose();
                Node.Watcher = null;
            }
        }

        private void OnChanged(FileWatcherChangedEventArgs Item)
        {
            if (Changed?.GetInvocationList() is not Delegate[] callbacks)
                return;

            foreach (Delegate Callback in callbacks)
            {
                try
                {
                    Callback.DynamicInvoke(this, Item);
                }
                catch
                {
                }
            }
        }
        private void OnPropertyChanged([CallerMemberName] string PropertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));

        private void ThrowIfDisposed()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(FileWatcher));
        }

        private bool IsDisposed;
        private void Dispose(bool IsDisposing)
        {
            if (!IsDisposed)
            {
                if (IsDisposing)
                {
                    Stop();
                    Processor.Clear();
                    Dispatcher.Clear();
                    Processor.Dispose();
                    Dispatcher.Dispose();
                }

                IsDisposed = true;
            }
        }
        /// <summary>
        /// Releases all resources used by the <see cref="FileWatcher"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private sealed class WatchNode(string PhysicalPath)
        {
            public string PhysicalPath { get; } = PhysicalPath;

            public FileSystemWatcher Watcher { get; set; }

            public List<WatchNode> Children { get; } = [];

        }

        private sealed class WatcherProcessor : IDisposable
        {
            private const int ControlPriority = 0;
            private const int NormalPriority = 10;

            private readonly ConcurrentPriorityQueue<Action, int> Queue = new();
            private readonly SemaphoreSlim Signal = new(0, 1);

            private Task Task;
            private CancellationTokenSource Cts;

            public void Start()
            {
                if (Task != null && !Task.IsCompleted &&
                    Cts != null && !Cts.IsCancellationRequested)
                {
                    ReleaseSignal();
                    return;
                }

                if (Task is null || Task.IsCompleted)
                {
                    Cts?.Dispose();
                }
                else
                {
                    CancellationTokenSource OldCts = Cts;
                    Task.ContinueWith(i => OldCts.Dispose(), CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
                }

                Cts = new CancellationTokenSource();
                Task = Task.Run(() => LoopAsync(Cts.Token));
                ReleaseSignal();
            }
            public void Stop()
            {
                Cts?.Cancel();
                ReleaseSignal();
            }

            public void Enqueue(Action Action)
                => Enqueue(Action, NormalPriority);
            public void EnqueueControl(Action Action)
                => Enqueue(Action, ControlPriority);
            private void Enqueue(Action Action, int Priority)
            {
                Queue.Enqueue(Action, Priority);
                ReleaseSignal();
            }

            public void Clear()
                => Queue.Clear();

            private int IsSignaled;
            private async Task LoopAsync(CancellationToken Token)
            {
                try
                {
                    while (!Token.IsCancellationRequested)
                    {
                        await Signal.WaitAsync(Token).ConfigureAwait(false);
                        while (!Token.IsCancellationRequested && Queue.TryDequeue(out Action Action))
                        {
                            try
                            {
                                Action();
                            }
                            catch
                            {
                            }
                        }

                        Interlocked.Exchange(ref IsSignaled, 0);
                        if (!Queue.IsEmpty &&
                            Interlocked.Exchange(ref IsSignaled, 1) == 0)
                            Signal.Release();
                    }
                }
                catch (OperationCanceledException)
                {
                }
            }
            private void ReleaseSignal()
            {
                if (Interlocked.Exchange(ref IsSignaled, 1) == 0)
                    Signal.Release();
            }

            private bool IsDisposed;
            public void Dispose()
            {
                if (IsDisposed)
                    return;

                IsDisposed = true;

                try
                {
                    Cts?.Cancel();
                    ReleaseSignal();
                    Task?.Wait();
                }
                catch (AggregateException e) when (e.InnerExceptions.All(i => i is OperationCanceledException))
                {
                }
                finally
                {
                    Clear();
                    Cts?.Dispose();
                    Signal.Dispose();
                }
            }

        }
        private sealed class DispatchWorker : IDisposable
        {
            private readonly ConcurrentQueue<FileWatcherChangedEventArgs> Queue = new();
            private readonly SemaphoreSlim Signal = new(0, 1);

            private Task Task;
            private CancellationTokenSource Cts;
            private Action<FileWatcherChangedEventArgs> Handler;
            public void Start(Action<FileWatcherChangedEventArgs> Handler)
            {
                this.Handler = Handler;

                if (Task != null && !Task.IsCompleted &&
                    Cts != null && !Cts.IsCancellationRequested)
                {
                    ReleaseSignal();
                    return;
                }

                if (Task is null || Task.IsCompleted)
                {
                    Cts?.Dispose();
                }
                else
                {
                    CancellationTokenSource OldCts = Cts;
                    Task.ContinueWith(i => OldCts.Dispose(), CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
                }

                Cts = new CancellationTokenSource();
                Task = Task.Run(() => LoopAsync(Cts.Token));
                ReleaseSignal();
            }
            public void Stop()
            {
                Cts?.Cancel();
                ReleaseSignal();
            }

            public void Enqueue(FileWatcherChangedEventArgs Item)
            {
                Queue.Enqueue(Item);
                ReleaseSignal();
            }
            public void Clear()
                => Queue.Clear();

            private int IsSignaled;
            private async Task LoopAsync(CancellationToken Token)
            {
                try
                {
                    while (!Token.IsCancellationRequested)
                    {
                        await Signal.WaitAsync(Token).ConfigureAwait(false);
                        while (!Token.IsCancellationRequested && Queue.TryDequeue(out FileWatcherChangedEventArgs Item))
                        {
                            try
                            {
                                Handler?.Invoke(Item);
                            }
                            catch
                            {
                            }
                        }

                        Interlocked.Exchange(ref IsSignaled, 0);
                        if (!Queue.IsEmpty &&
                            Interlocked.Exchange(ref IsSignaled, 1) == 0)
                            Signal.Release();
                    }
                }
                catch (OperationCanceledException)
                {
                }
            }
            private void ReleaseSignal()
            {
                if (Interlocked.Exchange(ref IsSignaled, 1) == 0)
                    Signal.Release();
            }

            private bool IsDisposed;
            public void Dispose()
            {
                if (IsDisposed)
                    return;

                IsDisposed = true;

                Cts?.Cancel();
                ReleaseSignal();

                // A Changed handler may call FileWatcher.Dispose(), which re-enters Dispatcher.Dispose() from this worker task.
                // Waiting here would make the worker wait for itself and deadlock.
                Task OldTask = Task;
                if (OldTask != null && OldTask.Id == Task.CurrentId)
                {
                    OldTask.ContinueWith(i =>
                    {
                        Clear();
                        Cts?.Dispose();
                        Signal.Dispose();
                    }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
                    return;
                }

                try
                {
                    OldTask?.Wait();
                }
                catch (AggregateException e) when (e.InnerExceptions.All(i => i is OperationCanceledException))
                {
                }
                finally
                {
                    Clear();
                    Cts?.Dispose();
                    Signal.Dispose();
                }
            }

        }

    }
}
