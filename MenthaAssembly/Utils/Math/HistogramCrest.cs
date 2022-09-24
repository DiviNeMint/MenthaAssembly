using System;

namespace MenthaAssembly
{
    /// <summary>
    /// Represents a histogram of the specified type.
    /// </summary>
    [Serializable]
    public struct HistogramCrest<T>
        where T : IComparable<T>
    {
        public int StartIndex { get; set; }

        public int EndIndex { get; set; }

        public T Peak { get; set; }

        public override string ToString()
            => $"Start : {StartIndex}, End : {EndIndex}, Peak : {Peak}";

    }
}