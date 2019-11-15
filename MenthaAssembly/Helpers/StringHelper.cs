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
                Convert.ChangeType(This, ValueType);
            }
            catch
            {

            }
            return Activator.CreateInstance(ValueType);
        }

    }
}
