using System;

namespace MenthaAssembly
{
    /// <summary>
    /// Represents a histogram of the specified type.
    /// </summary>
    [Serializable]
    public struct HistogramCrest
    {
        public int Start { get; set; }

        public int End { get; set; }

        public int Peak { get; set; }

        public override string ToString()
            => $"Start : {Start}, Peak : {Peak}, End : {End}";

    }
}