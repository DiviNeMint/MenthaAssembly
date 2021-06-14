using System.Collections.Generic;
using System.IO;
using System.Linq;

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

        public static IEnumerable<string> Split(this string This, params string[] Separator)
            => This.Split(Separator, StringSplitOptions.RemoveEmptyEntries);

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

        public static IEnumerable<string> SplitToLines(this string Content)
        {
            if (string.IsNullOrEmpty(Content))
                yield break;

            using StringReader reader = new StringReader(Content);
            string line;
            while ((line = reader.ReadLine()) != null)
                yield return line;

        }

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
            int i = 0;

            bool IsNegative = false;

            // Integer
            for (; i < This.Length; i++)
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
            for (int j = This.Length - 1; j > i; j--)
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

            int i = 0;
            char c;
            
            // Integer
            for (; i < This.Length; i++)
            {
                c = This[i];
                if (c == '.')
                    break;
                
                Integer = Integer * 10d + (c - '0');
                HandleInteger();
            }

            // Digital
            double Digital = 0d;
            for (int j = This.Length - 1; j > i; j--)
                Digital = Digital * 0.1d + (This[i] - '0');

            return IsNegative ? -Integer - Digital * 0.1d : Integer + Digital * 0.1d;
        }
        /// <summary>
        /// Convert string to Float<para/>
        /// It don't check whether all chars is number.
        /// </summary>
        public static float ToFloatFast(this string This)
        {
            if (string.IsNullOrEmpty(This))
                return default;

            float Integer = 0f;

            int i = 1;
            char c = This[0];
            bool IsNegative = '-'.Equals(c);

            void HandleInteger()
            {
                for (; i < This.Length; i++)
                {
                    c = This[i];

                    if ('.'.Equals(c))
                        break;

                    Integer = Integer * 10f + (c - '0');
                }
            }

            if (IsNegative)
            {
                HandleInteger();
            }
            else if (!'.'.Equals(c))
            {
                Integer = Integer * 10f + (c - '0');
                HandleInteger();
            }

            // Digital
            float Digital = 0f;
            for (int j = This.Length - 1; j > i; j--)
                Digital = Digital * 0.1f + (This[i] - '0');

            return IsNegative ? -Integer - Digital * 0.1f : Integer + Digital * 0.1f;
        }

        /// <summary>
        /// Convert string to Float<para/>
        /// It don't check whether all chars is number.
        /// </summary>
        public static float ToFloatFast(this string This)
        {
            float Integer = 0f;

            char c;
            int i = 0;

            bool IsNegative = false;

            // Integer
            for (; i < This.Length; i++)
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
            for (int j = This.Length - 1; j > i; j--)
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

            int i = 0;
            char c;

            // Integer
            for (; i < This.Length; i++)
            {
                c = This[i];
                if (c == '.')
                    break;

                Integer = Integer * 10f + (c - '0');
            }

            // Digital
            float Digital = 0f;
            for (int j = This.Length - 1; j > i; j--)
                Digital = Digital * 0.1f + (This[j] - '0');

            return Integer + Digital * 0.1f;
        }

    }
}
