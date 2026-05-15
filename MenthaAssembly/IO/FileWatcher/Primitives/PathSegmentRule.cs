using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MenthaAssembly.IO
{
    /// <summary>
    /// Represents a rule for a single path segment in a file watcher path.
    /// </summary>
    public sealed class PathSegmentRule : INotifyPropertyChanged
    {
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        private string _Pattern = string.Empty;
        /// <summary>
        /// Gets or sets the pattern used to match the path segment.
        /// </summary>
        public string Pattern
        {
            get => _Pattern;
            set
            {
                if (_Pattern == value)
                    return;

                _Pattern = value;
                OnPropertyChanged();
            }
        }

        private PathSegmentKind _Kind = PathSegmentKind.Directory;
        /// <summary>
        /// Gets or sets the kind of path segment.
        /// </summary>
        public PathSegmentKind Kind
        {
            get => _Kind;
            set
            {
                if (_Kind == value)
                    return;

                _Kind = value;
                OnPropertyChanged();
            }
        }

        private SegmentMatchMode _MatchMode = SegmentMatchMode.Wildcard;
        /// <summary>
        /// Gets or sets the match mode used to interpret <see cref="Pattern"/>.
        /// </summary>
        public SegmentMatchMode MatchMode
        {
            get => _MatchMode;
            set
            {
                if (_MatchMode == value)
                    return;

                _MatchMode = value;
                OnPropertyChanged();
            }
        }

        private bool _IsDynamic;
        /// <summary>
        /// Gets or sets a value that indicates whether the segment can match multiple file system entries.
        /// </summary>
        public bool IsDynamic
        {
            get => _MatchMode == SegmentMatchMode.Regex || _IsDynamic;
            set
            {
                if (_IsDynamic == value)
                    return;

                _IsDynamic = value;
                OnPropertyChanged();
            }
        }

        private bool _IgnoreHidden = true;
        /// <summary>
        /// Gets or sets a value that indicates whether hidden file system entries are ignored.
        /// </summary>
        public bool IgnoreHidden
        {
            get => _IgnoreHidden;
            set
            {
                if (_IgnoreHidden == value)
                    return;

                _IgnoreHidden = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PathSegmentRule"/> class.
        /// </summary>
        public PathSegmentRule()
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="PathSegmentRule"/> class with the specified segment kind and pattern.
        /// </summary>
        /// <param name="Kind">The kind of path segment.</param>
        /// <param name="Pattern">The pattern used to match the path segment.</param>
        public PathSegmentRule(PathSegmentKind Kind, string Pattern) : this(Kind, Pattern, SegmentMatchMode.Wildcard)
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="PathSegmentRule"/> class with the specified segment kind, pattern, and match mode.
        /// </summary>
        /// <param name="Kind">The kind of path segment.</param>
        /// <param name="Pattern">The pattern used to match the path segment.</param>
        /// <param name="MatchMode">The match mode used to interpret <paramref name="Pattern"/>.</param>
        public PathSegmentRule(PathSegmentKind Kind, string Pattern, SegmentMatchMode MatchMode)
        {
            this.Kind = Kind;
            this.Pattern = Pattern;
            this.MatchMode = MatchMode;
        }

        private void OnPropertyChanged([CallerMemberName] string PropertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));

    }
}