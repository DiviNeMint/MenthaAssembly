using MenthaAssembly.Offices.Primitives;

namespace MenthaAssembly.Offices
{
    /// <summary>
    /// A range for cells using 0 index positions. 
    /// </summary>
    public sealed class ExcelCellRange
    {
        internal ExcelCellRange(string Range)
        {
            string[] FromTo = Range.Split(':');
            if (FromTo.Length == 2)
            {
                FromTo[0].TryParseReference(out int Column, out int Row);
                FromColumn = Column - 1;
                FromRow = Row - 1;

                FromTo[1].TryParseReference(out Column, out Row);
                ToColumn = Column - 1;
                ToRow = Row - 1;
            }
        }
        internal ExcelCellRange(int FromColumn, int FromRow, int ToColumn, int ToRow)
        {
            this.FromColumn = FromColumn;
            this.FromRow = FromRow;
            this.ToColumn = ToColumn;
            this.ToRow = ToRow;
        }

        /// <summary>
        /// Gets the column the range starts in
        /// </summary>
        public int FromColumn { get; }

        /// <summary>
        /// Gets the row the range starts in
        /// </summary>
        public int FromRow { get; }

        /// <summary>
        /// Gets the column the range ends in
        /// </summary>
        public int ToColumn { get; }

        /// <summary>
        /// Gets the row the range ends in
        /// </summary>
        public int ToRow { get; }

        public override string ToString()
            => $"{FromRow}, {ToRow}, {FromColumn}, {ToColumn}";

    }
}