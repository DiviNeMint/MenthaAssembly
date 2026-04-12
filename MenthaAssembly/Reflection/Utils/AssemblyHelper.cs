using MenthaAssembly.Win32;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
#if NET6_0_OR_GREATER
using System.Runtime.Loader;
#endif

namespace System.Reflection
{
    public static class AssemblyHelper
    {
        internal static readonly BindingFlags AllModifierWithStatic =
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        // System assemblies usually have a specific PublicKeyToken, such as
        // "b77a5c561934e089" (.NET Framework) or "7cec85d7bea7798e" (.NET Core/.NET 5+)
        private static readonly HashSet<string> SystemPublicKeyTokens =
        [
            "b03f5f7f11d50a3a", // Microsoft Common Libraries
            "b77a5c561934e089", // .NET Framework
            "7cec85d7bea7798e", // .NET Core/.NET 5+
            "31bf3856ad364e35", // .NET Common Libraries
            "cc7b13ffcd2ddd51", // .NET Foundation
        ];

        public static bool IsDotNetAssembly(this Assembly This)
            => IsDotNetAssembly(This.GetName());
        public static bool IsDotNetAssembly(this AssemblyName This)
        {
            byte[] token = This.GetPublicKeyToken();
            if (token is null || token.Length == 0)
                return false;

            string publicKeyToken = BitConverter.ToString(token)
                                                .Replace("-", "")
                                                .ToLowerInvariant();
            return SystemPublicKeyTokens.Contains(publicKeyToken);
        }

        public static TargetFrameworkAttribute GetFramework(this Assembly This)
            => This.GetCustomAttribute<TargetFrameworkAttribute>();
        public static Assembly GetFrameworkAssembly(Assembly Target)
        {
            TargetFrameworkAttribute Framework = Target.GetFramework();
            if (Framework is null)
                return null;

            if (Framework.FrameworkName.StartsWith(".NETCoreApp"))
            {
                if (Target.GetReferencedAssemblies()
                          .FirstOrDefault(i => i.Name == "System.Runtime") is AssemblyName FrameworkAssemblyName)
                    return Assembly.Load(FrameworkAssemblyName);
            }
            else if (Framework.FrameworkName.StartsWith(".NETStandard"))
            {
                if (Target.GetReferencedAssemblies()
                          .FirstOrDefault(i => i.Name == "netstandard") is AssemblyName FrameworkAssemblyName)
                    return Assembly.Load(FrameworkAssemblyName);
            }

            return null;
        }

        public static string[] GetAssemblyAttributeTypeFullNames(string Filename)
        {
            if (string.IsNullOrWhiteSpace(Filename))
                throw new ArgumentException("Filename cannot be null or empty.", nameof(Filename));

            if (!File.Exists(Filename))
                throw new FileNotFoundException(string.Empty, Filename);

            PEImage Image = new(File.ReadAllBytes(Filename));
            return Image.TryReadAssemblyAttributeTypeFullNames(out string[] FullNames) ? FullNames : [];
        }
        public static bool TryGetAssemblyAttributeTypes(string Filename, Func<string, Type> Resolver, out Type[] AttributeTypes)
        {
            string[] FullNames;
            try
            {
                FullNames = GetAssemblyAttributeTypeFullNames(Filename);
            }
            catch
            {
                AttributeTypes = [];
                return false;
            }

            List<Type> Results = new(FullNames.Length);
            foreach (string FullName in FullNames)
            {
                Type Type = Type.GetType(FullName, false);
                Type ??= AppDomain.CurrentDomain.GetAssemblies()
                                                  .Select(i => i.GetType(FullName, false))
                                                  .FirstOrDefault(i => i != null);

                Type ??= Resolver?.Invoke(FullName);
                if (Type is null)
                {
                    AttributeTypes = [];
                    return false;
                }

                Results.Add(Type);
            }

            AttributeTypes = Results.ToArray();
            return true;
        }

        public static string[] GetReferencedAssemblyNames(string Filename)
        {
            if (string.IsNullOrWhiteSpace(Filename))
                throw new ArgumentException("Filename cannot be null or empty.", nameof(Filename));

            if (!File.Exists(Filename))
                throw new FileNotFoundException(string.Empty, Filename);

            PEImage Image = new(File.ReadAllBytes(Filename));
            return Image.TryReadReferencedAssemblyNames(out string[] Names) ? Names : [];
        }
        public static bool TryGetReferencedAssemblyNames(string Filename, out string[] Names)
        {
            try
            {
                Names = GetReferencedAssemblyNames(Filename);
                return true;
            }
            catch
            {
                Names = [];
                return false;
            }
        }

        public static IEnumerable<string> EnumerateManagedAssemblyFiles(string RootDirectory, SearchOption Option = SearchOption.AllDirectories)
        {
            if (string.IsNullOrWhiteSpace(RootDirectory) || !Directory.Exists(RootDirectory))
                yield break;

            foreach (string FilePath in Directory.EnumerateFiles(RootDirectory, "*.dll", Option))
            {
                LibraryType Type;
                try
                {
                    Type = GetLibraryType(FilePath);
                }
                catch
                {
                    continue;
                }

                if ((Type & LibraryType.Managed) > 0)
                    yield return FilePath;
            }
        }

        public static string[] GetDependencyManagedAssemblyPaths(this Assembly This, string RootDirectory, Func<AssemblyName, bool> SkipFilter = null)
            => [.. GetDependencyManagedAssemblyPathTable(This, RootDirectory, SkipFilter).Values];
        public static Dictionary<string, string> GetDependencyManagedAssemblyPathTable(this Assembly This, string RootDirectory, Func<AssemblyName, bool> SkipFilter = null)
        {
            Dictionary<string, string> Dependency = new(StringComparer.OrdinalIgnoreCase);
            GetDependencyManagedAssemblyPathTable(This, RootDirectory, SkipFilter, Dependency);
            return Dependency;
        }
        private static void GetDependencyManagedAssemblyPathTable(Assembly This, string RootDirectory, Func<AssemblyName, bool> SkipFilter, Dictionary<string, string> Dependency)
        {
            foreach (AssemblyName AssemblyName in This.GetReferencedAssemblies())
            {
                string Name = AssemblyName.Name;
                if (string.IsNullOrWhiteSpace(Name))
                    continue;

                if (Dependency.ContainsKey(Name))
                    continue;

                if (AssemblyName.IsDotNetAssembly())
                    continue;

                if (SkipFilter?.Invoke(AssemblyName) == true)
                    continue;

                if (!TryGetAssemblyPath(RootDirectory, AssemblyName, out string Path))
                    continue;

                Dependency[Name] = Path;

                Assembly ChildAssembly;
                try
                {
                    ChildAssembly = Assembly.LoadFrom(Path);
                }
                catch
                {
                    continue;
                }

                GetDependencyManagedAssemblyPathTable(ChildAssembly, RootDirectory, SkipFilter, Dependency);
            }
        }

        public static bool TryGetAssemblyPath(string RootDirectory, AssemblyName AssemblyName, out string AssemblyPath)
        {
            AssemblyPath = null;

            if (string.IsNullOrWhiteSpace(RootDirectory) ||
                !Directory.Exists(RootDirectory) ||
                AssemblyName is null ||
                string.IsNullOrWhiteSpace(AssemblyName.Name))
                return false;

            string FileName = $"{AssemblyName.Name}.dll";

            string Candidate = Path.Combine(RootDirectory, FileName);
            if (File.Exists(Candidate))
            {
                AssemblyPath = Path.GetFullPath(Candidate);
                return true;
            }

            Candidate = Directory.EnumerateFiles(RootDirectory, FileName, SearchOption.AllDirectories)
                                 .FirstOrDefault();

            if (Candidate is null)
                return false;

            AssemblyPath = Path.GetFullPath(Candidate);
            return true;
        }

        public static Assembly[] GetDependencyManagedNonSystemAssemblies(this Assembly This)
            => [.. GetDependencyManagedNonSystemAssemblyTable(This).Values];
        internal static Dictionary<string, Assembly> GetDependencyManagedNonSystemAssemblyTable(Assembly This)
        {
            Dictionary<string, Assembly> Dependency = [];
            GetDependencyManagedNonSystemAssemblyTable(This, ref Dependency);
            return Dependency;
        }
        private static Dictionary<string, Assembly> GetDependencyManagedNonSystemAssemblyTable(Assembly This, ref Dictionary<string, Assembly> Dependency)
        {
            Dictionary<string, Assembly> New = [];
            foreach (AssemblyName AssemblyName in This.GetReferencedAssemblies())
            {
                string Name = AssemblyName.Name;
                if (Dependency.ContainsKey(Name))
                    continue;

                if (AssemblyName.IsDotNetAssembly())
                    continue;

                if (AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(i => i.GetName().Name == Name) is not Assembly Assembly)
                    Assembly = AppDomain.CurrentDomain.Load(AssemblyName);

                Dependency[Name] = Assembly;

                foreach (KeyValuePair<string, Assembly> Info in GetDependencyManagedNonSystemAssemblyTable(Assembly, ref Dependency))
                {
                    Dependency[Info.Key] = Info.Value;
                    New[Info.Key] = Info.Value;
                }
            }

            return New;
        }

#if NET6_0_OR_GREATER
        public static Assembly[] GetDependencyManagedNonSystemAssemblies(this Assembly This, AssemblyLoadContext Loader)
            => [.. GetDependencyManagedNonSystemAssemblyTable(This, Loader).Values];
        internal static Dictionary<string, Assembly> GetDependencyManagedNonSystemAssemblyTable(Assembly This, AssemblyLoadContext Loader)
        {
            Dictionary<string, Assembly> Dependency = [];
            GetDependencyManagedNonSystemAssemblyTable(This, Loader, ref Dependency);
            return Dependency;
        }
        private static Dictionary<string, Assembly> GetDependencyManagedNonSystemAssemblyTable(Assembly This, AssemblyLoadContext Loader, ref Dictionary<string, Assembly> Dependency)
        {
            Dictionary<string, Assembly> New = [];
            foreach (AssemblyName AssemblyName in This.GetReferencedAssemblies())
            {
                string Name = AssemblyName.Name;
                if (Dependency.ContainsKey(Name))
                    continue;

                if (AssemblyName.IsDotNetAssembly())
                    continue;

                if (AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(i => i.GetName().Name == Name) is not Assembly Assembly)
                    Assembly = Loader.LoadFromAssemblyName(AssemblyName);

                Dependency[Name] = Assembly;

                foreach (KeyValuePair<string, Assembly> Info in GetDependencyManagedNonSystemAssemblyTable(Assembly, Loader, ref Dependency))
                {
                    Dependency[Info.Key] = Info.Value;
                    New[Info.Key] = Info.Value;
                }
            }

            return New;
        }

        public static Assembly[] GetDependencyManagedAssemblies(this Assembly This, AssemblyLoadContext Loader, Func<AssemblyName, bool> SkipFilter = null)
            => [.. GetDependencyManagedAssemblyTable(This, Loader, SkipFilter).Values];
        public static Dictionary<string, Assembly> GetDependencyManagedAssemblyTable(this Assembly This, AssemblyLoadContext Loader, Func<AssemblyName, bool> SkipFilter = null)
        {
            Dictionary<string, Assembly> Dependency = new(StringComparer.OrdinalIgnoreCase);
            GetDependencyManagedAssemblyTable(This, Loader, SkipFilter, Dependency);
            return Dependency;
        }
        private static void GetDependencyManagedAssemblyTable(Assembly This, AssemblyLoadContext Loader, Func<AssemblyName, bool> SkipFilter, Dictionary<string, Assembly> Dependency)
        {
            foreach (AssemblyName AssemblyName in This.GetReferencedAssemblies())
            {
                string Name = AssemblyName.Name;
                if (string.IsNullOrWhiteSpace(Name))
                    continue;

                if (Dependency.ContainsKey(Name))
                    continue;

                if (AssemblyName.IsDotNetAssembly())
                    continue;

                if (SkipFilter?.Invoke(AssemblyName) == true)
                    continue;

                Assembly Assembly;
                try
                {
                    Assembly = Loader.LoadFromAssemblyName(AssemblyName);
                }
                catch
                {
                    continue;
                }

                Dependency[Name] = Assembly;
                GetDependencyManagedAssemblyTable(Assembly, Loader, SkipFilter, Dependency);
            }
        }

#else
        public static Assembly[] GetDependencyManagedNonSystemAssemblies(this Assembly This, AppDomain Loader)
            => [.. GetDependencyManagedNonSystemAssemblyTable(This, Loader).Values];

        internal static Dictionary<string, Assembly> GetDependencyManagedNonSystemAssemblyTable(Assembly This, AppDomain Loader)
        {
            Dictionary<string, Assembly> Dependency = [];
            GetDependencyManagedNonSystemAssemblyTable(This, Loader, ref Dependency);
            return Dependency;
        }
        private static Dictionary<string, Assembly> GetDependencyManagedNonSystemAssemblyTable(Assembly This, AppDomain Loader, ref Dictionary<string, Assembly> Dependency)
        {
            Dictionary<string, Assembly> New = [];
            foreach (AssemblyName AssemblyName in This.GetReferencedAssemblies())
            {
                string Name = AssemblyName.Name;
                if (Dependency.ContainsKey(Name))
                    continue;

                if (AssemblyName.IsDotNetAssembly())
                    continue;

                if (AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(i => i.GetName().Name == Name) is not Assembly Assembly)
                    Assembly = Loader.Load(AssemblyName);

                Dependency[Name] = Assembly;

                foreach (KeyValuePair<string, Assembly> Info in GetDependencyManagedNonSystemAssemblyTable(Assembly, Loader, ref Dependency))
                {
                    Dependency[Info.Key] = Info.Value;
                    New[Info.Key] = Info.Value;
                }
            }

            return New;
        }

        public static Assembly[] GetDependencyManagedAssemblies(this Assembly This, AppDomain Loader, Func<AssemblyName, bool> SkipFilter = null)
            => [.. GetDependencyManagedAssemblyTable(This, Loader, SkipFilter).Values];

        public static Dictionary<string, Assembly> GetDependencyManagedAssemblyTable(this Assembly This, AppDomain Loader, Func<AssemblyName, bool> SkipFilter = null)
        {
            Dictionary<string, Assembly> Dependency = new(StringComparer.OrdinalIgnoreCase);
            GetDependencyManagedAssemblyTable(This, Loader, SkipFilter, Dependency);
            return Dependency;
        }
        private static void GetDependencyManagedAssemblyTable(Assembly This, AppDomain Loader, Func<AssemblyName, bool> SkipFilter, Dictionary<string, Assembly> Dependency)
        {
            foreach (AssemblyName AssemblyName in This.GetReferencedAssemblies())
            {
                string Name = AssemblyName.Name;
                if (string.IsNullOrWhiteSpace(Name))
                    continue;

                if (Dependency.ContainsKey(Name))
                    continue;

                if (AssemblyName.IsDotNetAssembly())
                    continue;

                if (SkipFilter?.Invoke(AssemblyName) == true)
                    continue;

                Assembly Assembly;
                try
                {
                    Assembly = Loader.Load(AssemblyName);
                }
                catch
                {
                    continue;
                }

                Dependency[Name] = Assembly;
                GetDependencyManagedAssemblyTable(Assembly, Loader, SkipFilter, Dependency);
            }
        }

#endif

        public static LibraryType GetLibraryType(string Filename)
        {
            // PE Struct
            // https://web.archive.org/web/20160202125049/http://blogs.msdn.com/b/kstanton/archive/2004/03/31/105060.aspx

            using FileStream s = new(Filename, FileMode.Open, FileAccess.Read);

            ushort Magic = s.Read<ushort>();
            if (Magic != 0x5A4D)
                return LibraryType.Unknown;

            // Skip to DosHeader.e_lfanew
            s.Seek(60, SeekOrigin.Begin);

            // FileHeader Position
            int FileHeaderPosition = s.Read<int>() + sizeof(uint);

            // Skip to FileHeader.Characteristics
            s.Seek(FileHeaderPosition + 18, SeekOrigin.Begin);

            ImageFileCharFlags FileFlags = s.Read<ImageFileCharFlags>();
            if ((FileFlags & ImageFileCharFlags.Dll) == 0)
                return LibraryType.Unknown;

            ImageOptionalMagicType p = s.Read<ImageOptionalMagicType>();

            LibraryType r;
            switch (p)
            {
                case ImageOptionalMagicType.HDR32_MAGIC:
                    // Skip to OptionalHeader.DataDirectory.Size
                    s.Seek(206 + sizeof(uint), SeekOrigin.Current);
                    r = LibraryType.x86;
                    break;

                case ImageOptionalMagicType.HDR64_MAGIC:
                    // Skip to OptionalHeader.DataDirectory.Size
                    s.Seek(222 + sizeof(uint), SeekOrigin.Current);
                    r = LibraryType.x64;
                    break;

                case ImageOptionalMagicType.ROM_OPTIONAL_HDR_MAGIC:
                default:
                    return LibraryType.Unknown;
            }

            int DataDirectorySize = s.Read<int>();
            return r | (DataDirectorySize > 0 ? LibraryType.Managed : LibraryType.Unmanaged);
        }

        public static IEnumerable<string> GetUnmanagedDependencyAssemblyNames(this Assembly This)
            => This.GetTypes()
                   .TrySelectMany(i => i.GetMethods(AllModifierWithStatic))
                   .TrySelect(i => i.GetCustomAttribute<DllImportAttribute>(false))
                   .Where(i => i != null)
                   .Select(i => i.Value)
                   .Distinct();
        public static string GetUnmanagedLibraryFullName(string DllName)
        {
            // Absolute path
            if (Path.IsPathRooted(DllName))
                return File.Exists(DllName) ? DllName : null;

            // Application Directory
            string Fullname = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DllName);
            if (File.Exists(Fullname))
                return Fullname;

            // System Directory
            Fullname = Path.Combine(Environment.SystemDirectory, DllName);
            if (File.Exists(Fullname))
                return Fullname;

            // Windows Directory
            Fullname = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), DllName);
            if (File.Exists(Fullname))
                return Fullname;

            // Environment PATH
            string PathValue = Environment.GetEnvironmentVariable("PATH");
            if (string.IsNullOrWhiteSpace(PathValue))
                return null;

            foreach (string Folder in PathValue.Split(';').Where(i => !string.IsNullOrWhiteSpace(i)))
            {
                Fullname = Path.Combine(Folder, DllName);
                if (File.Exists(Fullname))
                    return Fullname;
            }

            return null;
        }

        public static IEnumerable<Assembly> GetDependencyAssemblies(object Object)
        {
            if (Object is null)
                yield break;

            List<Type> SearchedTypes = [];
            List<Assembly> Assemblies = [];
            GetDependencyAssemblies(Object, Object.GetType(), ref SearchedTypes, ref Assemblies);

            foreach (Assembly Assembly in Assemblies)
                yield return Assembly;
        }
        private static void GetDependencyAssemblies(object Object, Type ObjectType, ref List<Type> SearchedTypes, ref List<Assembly> Assemblies)
        {
            // Collection
            if (Object is IEnumerable Collection)
                foreach (object Item in Collection.Where(i => i != null))
                    GetDependencyAssemblies(Item, Item.GetType(), ref SearchedTypes, ref Assemblies);

            if (SearchedTypes.Contains(ObjectType))
                return;

            SearchedTypes.Add(ObjectType);
            Assembly a = ObjectType.Assembly;
            if (!Assemblies.Contains(a))
                Assemblies.Add(a);

            // Generic Type
            if (ObjectType.IsGenericType)
                foreach (Type t in ObjectType.GenericTypeArguments)
                    GetDependencyAssemblies(null, t, ref SearchedTypes, ref Assemblies);

            // DotNet Assembly
            if (a.IsDotNetAssembly())
                return;

            // Constructors
            foreach (ConstructorInfo Method in ObjectType.GetConstructors(AllModifierWithStatic))
                foreach (ParameterInfo Parameter in Method.GetParameters())
                    GetDependencyAssemblies(null, Parameter.ParameterType, ref SearchedTypes, ref Assemblies);

            // Methods
            foreach (MethodInfo Method in ObjectType.GetMethods(AllModifierWithStatic))
            {
                // Return Type
                GetDependencyAssemblies(null, Method.ReturnType, ref SearchedTypes, ref Assemblies);

                // Parameters
                foreach (ParameterInfo Parameter in Method.GetParameters())
                    GetDependencyAssemblies(null, Parameter.ParameterType, ref SearchedTypes, ref Assemblies);
            }

            // Fields
            foreach (FieldInfo Field in ObjectType.GetFields(AllModifierWithStatic)
                                                  .Where(i => !ReflectionHelper.IsBackingField(i)))
            {
                GetDependencyAssemblies(null, Field.FieldType, ref SearchedTypes, ref Assemblies);

                // Field Value
                if ((!Field.IsStatic && Object is null) ||
                    Field.GetValue(Object) is not object FieldValue)
                    continue;

                GetDependencyAssemblies(FieldValue, FieldValue.GetType(), ref SearchedTypes, ref Assemblies);
            }

            // Properties
            foreach (PropertyInfo Property in ObjectType.GetProperties(AllModifierWithStatic))
            {
                GetDependencyAssemblies(null, Property.PropertyType, ref SearchedTypes, ref Assemblies);

                if (Property.CanRead)
                {
                    object Value;
                    if (Property.GetMethod.IsStatic)
                    {
                        Value = Property.GetValue(null);
                    }
                    else
                    {
                        // Indexer
                        ParameterInfo[] Parameters = Property.GetIndexParameters();
                        foreach (ParameterInfo Parameter in Parameters)
                            GetDependencyAssemblies(null, Parameter.ParameterType, ref SearchedTypes, ref Assemblies);

                        if (Parameters.Length > 0)
                            continue;

                        if (Object is null)
                            continue;

                        Value = Property.GetValue(Object);
                    }

                    if (Value is null)
                        continue;

                    GetDependencyAssemblies(Value, Value.GetType(), ref SearchedTypes, ref Assemblies);
                }
            }
        }

    }
}