namespace MenthaAssembly.Offices
{
    /// <summary>
    /// Header and footer text. 
    /// </summary>
    public sealed class ExcelHeaderFooter
    {
        internal bool _HasDifferentFirst,
                      _HasDifferentOddEven;

        internal string _FirstHeader,
                        _FirstFooter,
                        _OddHeader,
                        _OddFooter,
                        _EvenHeader,
                        _EvenFooter;

        /// <summary>
        /// Gets a value indicating whether the header and footer are different on the first page. 
        /// </summary>
        public bool HasDifferentFirst
            => _HasDifferentFirst;

        /// <summary>
        /// Gets a value indicating whether the header and footer are different on odd and even pages.
        /// </summary>
        public bool HasDifferentOddEven
            => _HasDifferentOddEven;

        /// <summary>
        /// Gets the header used for the first page if <see cref="HasDifferentFirst"/> is <see langword="true"/>.
        /// </summary>
        public string FirstHeader
            => _FirstHeader;

        /// <summary>
        /// Gets the footer used for the first page if <see cref="HasDifferentFirst"/> is <see langword="true"/>.
        /// </summary>
        public string FirstFooter
            => _FirstFooter;

        /// <summary>
        /// Gets the header used for odd pages -or- all pages if <see cref="HasDifferentOddEven"/> is <see langword="false"/>. 
        /// </summary>
        public string OddHeader
            => _OddHeader;

        /// <summary>
        /// Gets the footer used for odd pages -or- all pages if <see cref="HasDifferentOddEven"/> is <see langword="false"/>. 
        /// </summary>
        public string OddFooter
            => _OddFooter;

        /// <summary>
        /// Gets the header used for even pages if <see cref="HasDifferentOddEven"/> is <see langword="true"/>. 
        /// </summary>
        public string EvenHeader
            => _EvenHeader;

        /// <summary>
        /// Gets the footer used for even pages if <see cref="HasDifferentOddEven"/> is <see langword="true"/>. 
        /// </summary>
        public string EvenFooter
            => _EvenFooter;

        internal ExcelHeaderFooter()
        {

        }

    }
}
