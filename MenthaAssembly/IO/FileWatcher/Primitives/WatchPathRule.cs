using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MenthaAssembly.IO
{
    /// <summary>
    /// Represents a file watcher path rule that is composed of path segment rules.
    /// </summary>
    public sealed class WatchPathRule : ICloneable
    {
        /// <summary>
        /// Gets or sets the path segments that define the watch rule.
        /// </summary>
        public IReadOnlyList<PathSegmentRule> Segments { get; set; }

        /// <summary>
        /// Gets the path segment at the specified index.
        /// </summary>
        /// <param name="Index">The zero-based index of the segment to get.</param>
        /// <returns>The path segment at the specified index.</returns>
        public PathSegmentRule this[int Index]
            => Segments[Index];

        /// <summary>
        /// Initializes a new instance of the <see cref="WatchPathRule"/> class.
        /// </summary>
        public WatchPathRule()
        {
            Segments = Array.Empty<PathSegmentRule>();
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="WatchPathRule"/> class with the specified path segments.
        /// </summary>
        /// <param name="Segments">The path segments that define the watch rule.</param>
        public WatchPathRule(IEnumerable<PathSegmentRule> Segments)
        {
            this.Segments = Segments?.ToArray() ?? Array.Empty<PathSegmentRule>();
        }

        /// <summary>
        /// Validates the current watch path rule.
        /// </summary>
        /// <exception cref="InvalidOperationException">The rule does not describe a supported watch path.</exception>
        public void Validate()
        {
            if (Segments is null || Segments.Count < 2)
                throw new InvalidOperationException("Watch path rule must include a root directory segment and a file segment.");

            PathSegmentRule First = Segments[0];
            if (First.Kind != PathSegmentKind.Directory || First.IsDynamic ||
                string.IsNullOrWhiteSpace(First.Pattern))
                throw new InvalidOperationException("The first segment must be a static directory.");

            for (int i = 1; i < Segments.Count - 1; i++)
            {
                PathSegmentRule Segment = Segments[i];
                if (Segment.Kind != PathSegmentKind.Directory)
                    throw new InvalidOperationException("Middle segments must be directories.");
            }

            PathSegmentRule Last = Segments[Segments.Count - 1];
            if (Last.Kind != PathSegmentKind.File)
                throw new InvalidOperationException("The last segment must be a file.");

        }

        /// <summary>
        /// Creates a copy of the current watch path rule.
        /// </summary>
        /// <returns>A copy of the current watch path rule.</returns>
        public WatchPathRule Clone()
            => new()
            {
                Segments = Segments?.Select(i => new PathSegmentRule
                {
                    Pattern = i.Pattern,
                    Kind = i.Kind,
                    MatchMode = i.MatchMode,
                    IsDynamic = i.IsDynamic,
                    IgnoreHidden = i.IgnoreHidden
                }).ToArray()
            };

        /// <summary>
        /// Creates a copy of the current watch path rule.
        /// </summary>
        /// <returns>A copy of the current watch path rule.</returns>
        object ICloneable.Clone()
            => Clone();

        /// <summary>
        /// Converts a wildcard path pattern to a watch path rule.
        /// </summary>
        /// <param name="Pattern">The wildcard path pattern to convert.</param>
        /// <returns>A watch path rule that represents the specified wildcard path pattern.</returns>
        /// <exception cref="ArgumentException"><paramref name="Pattern"/> is <see langword="null"/>, empty, or contains only white-space characters.</exception>
        /// <remarks>
        /// This method parses wildcard path patterns only. Regex path segments must be created explicitly by using <see cref="PathSegmentRule"/>.
        /// </remarks>
        public static WatchPathRule Parse(string Pattern)
        {
            if (string.IsNullOrWhiteSpace(Pattern))
                throw new ArgumentException("Path pattern cannot be empty.", nameof(Pattern));

            string FullPattern = Path.GetFullPath(Pattern);
            string Root = Path.GetPathRoot(FullPattern);
            string Remainder = FullPattern.Substring(Root.Length).Trim(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            string[] Parts = Remainder.Length == 0 ? [] : Remainder.Split([Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar], StringSplitOptions.RemoveEmptyEntries);

            if (Parts.Length == 0)
                throw new ArgumentException("Path pattern must include a file segment.", nameof(Pattern));

            int FileSegmentIndex = Parts.Length - 1;
            int FirstDynamicDirectoryIndex = FileSegmentIndex;
            for (int i = 0; i < FileSegmentIndex; i++)
            {
                if (IsWildcardPattern(Parts[i]))
                {
                    FirstDynamicDirectoryIndex = i;
                    break;
                }
            }

            string RootPath = Root;
            for (int i = 0; i < FirstDynamicDirectoryIndex; i++)
                RootPath = Path.Combine(RootPath, Parts[i]);

            List<PathSegmentRule> Segments =
            [
                new PathSegmentRule
                {
                    Pattern = RootPath,
                    Kind = PathSegmentKind.Directory,
                    MatchMode = SegmentMatchMode.Wildcard,
                    IsDynamic = false
                }
            ];

            for (int i = FirstDynamicDirectoryIndex; i < Parts.Length; i++)
            {
                bool IsLast = i == FileSegmentIndex;
                string Part = Parts[i];
                bool IsWildcard = IsWildcardPattern(Part);

                Segments.Add(new PathSegmentRule
                {
                    Pattern = Part,
                    Kind = IsLast ? PathSegmentKind.File : PathSegmentKind.Directory,
                    MatchMode = SegmentMatchMode.Wildcard,
                    IsDynamic = IsWildcard
                });
            }

            WatchPathRule Rule = new()
            {
                Segments = Segments
            };

            Rule.Validate();
            return Rule;
        }
        private static bool IsWildcardPattern(string Pattern)
            => Pattern.IndexOf('*') >= 0 || Pattern.IndexOf('?') >= 0;

        /// <summary>
        /// Converts a wildcard path pattern to a watch path rule.
        /// </summary>
        /// <param name="Pattern">The wildcard path pattern to convert.</param>
        public static implicit operator WatchPathRule(string Pattern)
            => Parse(Pattern);

    }
}
