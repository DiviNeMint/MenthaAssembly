using System.Globalization;
using System.Text.RegularExpressions;

namespace MenthaAssembly.Offices.Primitives
{
    public static class ExcelSheetHelper
    {
        private static readonly Regex EscapeRegex = new Regex("_x([0-9A-F]{4,4})_", RegexOptions.Compiled);

        /// <summary>
        /// Logic for the Excel dimensions. Ex: A15
        /// </summary>
        /// <param name="Value">The value.</param>
        /// <param name="Column">The column, 1-based.</param>
        /// <param name="Row">The row, 1-based.</param>
        public static bool TryParseReference(this string Value, out int Column, out int Row)
        {
            Column = 0;
            int Position = 0;
            const int Offset = 'A' - 1;

            if (Value != null)
            {
                while (Position < Value.Length)
                {
                    char c = Value[Position];
                    if (c >= 'A' && c <= 'Z')
                    {
                        Position++;
                        Column *= 26;
                        Column += c - Offset;
                        continue;
                    }

                    if (char.IsDigit(c))
                        break;

                    Position = 0;
                    break;
                }
            }

            if (Position == 0)
            {
                Column = 0;
                Row = 0;
                return false;
            }

            return int.TryParse(Value.Substring(Position), NumberStyles.None, CultureInfo.InvariantCulture, out Row);
        }

        public static string ConvertEscapeChars(string Input)
            => EscapeRegex.Replace(Input, m => ((char)uint.Parse(m.Groups[1].Value, NumberStyles.HexNumber)).ToString());

    }
}