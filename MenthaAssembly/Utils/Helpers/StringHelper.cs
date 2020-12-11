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

        public static int ToInt32Fast(this string This)
        {
            int Result = 0;

            for (int i = 0; i < This.Length; i++)
                Result = Result * 10 + (This[i] - '0');

            return Result;
        }

        public static double ToDoubleFast(this string This)
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
            }

            // Digital
            double Digital = 0d;
            for (int j = This.Length - 1; j > i; j--)
                Digital = Digital * 0.1d + (This[i] - '0');

            return Integer + Digital * 0.1d;
        }

    }
}
