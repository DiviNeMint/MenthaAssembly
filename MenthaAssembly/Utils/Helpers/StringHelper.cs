using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace System
{
    public static class StringHelper
    {
        public static object ParseStaticObject(this string Path)
        {
            string[] Paths = Path.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);
            if (Paths.Length > 1)
            {
                string SourcePath = Paths[Paths.Length - 1];
                bool IsMethod = SourcePath.Contains("()");
                if (IsMethod)
                    SourcePath = SourcePath.Replace("()", string.Empty);

                string Root = Paths[Paths.Length - 2];
                Type RootType = AppDomain.CurrentDomain.GetAssemblies().TrySelectMany(i => i.GetTypes())
                                                                       .Where(i => Root.Equals(i.Name) &&
                                                                                   i.IsClass &&
                                                                                   (!i.IsAbstract || i.IsSealed) &&
                                                                                   i.GetMember(SourcePath).Length > 0)
                                                                       .FirstOrDefault();
                return IsMethod ? RootType?.GetMethod(SourcePath)?.Invoke(null, null) :
                                  RootType?.GetProperty(SourcePath)?.GetValue(null);
            }
            return null;
        }

        /// <summary>
        /// Splits a string into substrings based on the strings in an array. 
        /// </summary>
        /// <param name="This">The string to be splitted.</param>
        /// <param name="Separator">A string array that delimits the substrings in this string, an empty array that contains no delimiters, or null.</param>
        public static string[] Split(this string This, params string[] Separator)
            => This.Split(Separator, StringSplitOptions.RemoveEmptyEntries);

        /// <summary>
        /// Splits a string into characters and substrings that are based on the characters in an array.
        /// </summary>
        /// <param name="This">The string to be splitted.</param>
        /// <param name="Separator">A character array that delimits the substrings in this string, an empty array that contains no delimiters, or null.</param>
        public static IEnumerable<string> SplitAndKeep(this string This, params char[] Separator)
        {
            if (Separator.Length > 0)
            {
                int start = 0, index;
                while ((index = This.IndexOfAny(Separator, start)) != -1)
                {
                    if (index - start > 0)
                        yield return This.Substring(start, index - start);

                    yield return This.Substring(index, 1);
                    start = index + 1;
                }

                if (start < This.Length)
                    yield return This.Substring(start);
            }
            else
            {
                yield return This;
            }
        }

        /// <summary>
        /// Splits a string into substrings based on the symbols of line.
        /// </summary>
        /// <param name="Content">The string to be splitted.</param>
        public static IEnumerable<string> SplitToLines(this string Content)
        {
            if (string.IsNullOrEmpty(Content))
                yield break;

            using StringReader reader = new StringReader(Content);
            string line;
            while ((line = reader.ReadLine()) != null)
                yield return line;
        }

        /// <summary>
        /// Returns a new string in which all the characters in the current instance, beginning at a specified position and continuing through the last position, have been deleted.
        /// </summary>
        /// <param name="This">The string to remove characters.</param>
        /// <param name="Chars">A character array to be removed.</param>
        public static string Remove(this string This, params char[] Chars)
        {
            StringBuilder Builder = new StringBuilder();
            try
            {
                foreach (char Element in This)
                    if (!Chars.Contains(Element))
                        Builder.Append(Element);

                return Builder.ToString();
            }
            finally
            {
                Builder.Clear();
            }
        }

        /// <summary>
        /// Returns an object of the specified type and whose value is equivalent to the specified object.
        /// if it can't convert to the specified type, return the default value of the specified type.
        /// </summary>
        /// <param name="This">The string to be converted.</param>
        /// <param name="ValueType">The specified type to convert.</param>
        public static object ToValueType(this string This, Type ValueType)
        {
            try
            {
                return Convert.ChangeType(This, ValueType);
            }
            catch
            {

            }
            return Activator.CreateInstance(ValueType);
        }

        public static string GetNextName(string Seed, Predicate<string> ContainPredicate)
            => GetNextName((i) => $"{Seed} {i}", ContainPredicate);
        public static string GetNextName(Func<int, string> Format, Predicate<string> ContainPredicate)
        {
            int Index = 1;
            string DefaultName;
            do
            {
                DefaultName = Format.Invoke(Index++);
            } while (ContainPredicate.Invoke(DefaultName));

            return DefaultName;
        }

        public static bool IsInteger(this string This)
        {
            foreach (char c in This)
                if (!char.IsNumber(c))
                    return false;

            return true;
        }

        /// <summary>
        /// Convert string to Int32<para/>
        /// It don't check whether all chars is number.
        /// </summary>
        public static int ToInt32Fast(this string This)
        {
            if (string.IsNullOrEmpty(This))
                return default;

            char c = This[0];
            bool IsNegative = c.Equals('-');
            int r = IsNegative ? 0 : c - '0';

            for (int i = 1; i < This.Length; i++)
                r = r * 10 + (This[i] - '0');

            return IsNegative ? -r : r;
        }
        /// <summary>
        /// Convert string to Int32<para/>
        /// It don't check whether all chars is number (including char '-').
        /// </summary>
        public static int ToPositiveInt32Fast(this string This)
        {
            int r = 0;

            char c = This[0];
            bool IsNegative = '-'.Equals(c);

            if (!IsNegative)
                r = c - '0';

            for (int i = 1; i < This.Length; i++)
                r = r * 10 + (This[i] - '0');

            return IsNegative ? -r : r;
        }
        /// <summary>
        /// Convert string to UInt32<para/>
        /// It don't check whether all chars is number.
        /// </summary>
        public static uint ToUInt32Fast(this string This)
        {
            uint r = 0;

            for (int i = 0; i < This.Length; i++)
                r = r * 10 + (uint)(This[i] - '0');

            return r;
        }

        /// <summary>
        /// Convert string to Double<para/>
        /// It don't check whether all chars is number.
        /// </summary>
        public static double ToDoubleFast(this string This)
        {
            if (string.IsNullOrEmpty(This))
                return default;

            double Integer = 0d;

            char c;
            int i = 0,
                Length = This.Length;

            bool IsNegative = false;

            // Integer
            for (; i < Length; i++)
            {
                c = This[i];

                if (c.Equals('-'))
                {
                    IsNegative = true;
                    continue;
                }

                if (c.Equals('.'))
                    break;

                Integer = Integer * 10d + (c - '0');
            }

            // Digital
            double Digital = 0d;
            for (int j = Length - 1; j > i; j--)
                Digital = Digital * 0.1d + (This[j] - '0');

            return IsNegative ? -Integer - Digital * 0.1d : Integer + Digital * 0.1d;
        }
        /// <summary>
        /// Convert string to Double<para/>
        /// It don't check whether all chars is number (including char '-').
        /// </summary>
        public static double ToPositiveDoubleFast(this string This)
        {
            double Integer = 0d;

            char c;
            int i = 0,
                Length = This.Length;

            // Integer
            for (; i < Length; i++)
            {
                c = This[i];
                if (c == '.')
                    break;

                Integer = Integer * 10d + (c - '0');
            }

            // Digital
            double Digital = 0d;
            for (int j = Length - 1; j > i; j--)
                Digital = Digital * 0.1d + (This[i] - '0');

            return Integer + Digital * 0.1d;
        }

        /// <summary>
        /// Convert string to Float<para/>
        /// It don't check whether all chars is number.
        /// </summary>
        public static float ToFloatFast(this string This)
        {
            float Integer = 0f;

            char c;
            int i = 0,
                Length = This.Length;

            bool IsNegative = false;

            // Integer
            for (; i < Length; i++)
            {
                c = This[i];

                if (c.Equals('-'))
                {
                    IsNegative = true;
                    continue;
                }

                if (c.Equals('.'))
                    break;

                Integer = Integer * 10f + (c - '0');
            }

            // Digital
            float Digital = 0f;
            for (int j = Length - 1; j > i; j--)
                Digital = Digital * 0.1f + (This[j] - '0');

            return IsNegative ? -Integer - Digital * 0.1f : Integer + Digital * 0.1f;
        }
        /// <summary>
        /// Convert string to Float<para/>
        /// It don't check whether all chars is number (including char '-').
        /// </summary>
        public static float ToPositiveFloatFast(this string This)
        {
            float Integer = 0f;

            char c;
            int i = 0,
                Length = This.Length;

            // Integer
            for (; i < Length; i++)
            {
                c = This[i];
                if (c == '.')
                    break;

                Integer = Integer * 10f + (c - '0');
            }

            // Digital
            float Digital = 0f;
            for (int j = Length - 1; j > i; j--)
                Digital = Digital * 0.1f + (This[j] - '0');

            return Integer + Digital * 0.1f;
        }

    }
}
