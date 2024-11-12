using MenthaAssembly.Win32;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
#if NET6_0_OR_GREATER
using System.Runtime.Loader;
#endif

namespace System.Reflection
{
    public static class AssemblyHelper
    {
        internal static readonly BindingFlags AllModifierWithStatic = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        // System assemblies usually have a specific PublicKeyToken, such as "b77a5c561934e089" (.NET Framework) or "7cec85d7bea7798e" (.NET Core/.NET 5+)
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
            // Use the PublicKeyToken of the assembly as the judgment criterion.
            string publicKeyToken = BitConverter.ToString(This.GetPublicKeyToken()).Replace("-", "").ToLower();
            return SystemPublicKeyTokens.Contains(publicKeyToken);
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

                // Sub Dependency
                foreach (KeyValuePair<string, Assembly> Info in GetDependencyManagedNonSystemAssemblyTable(Assembly, Loader, ref Dependency))
                {
                    Dependency[Info.Key] = Info.Value;
                    New[Info.Key] = Info.Value;
                }
            }

            return New;
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

                // Sub Dependency
                foreach (KeyValuePair<string, Assembly> Info in GetDependencyManagedNonSystemAssemblyTable(Assembly, Loader, ref Dependency))
                {
                    Dependency[Info.Key] = Info.Value;
                    New[Info.Key] = Info.Value;
                }
            }

            return New;
        }
#endif

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
            foreach (string Folder in Environment.GetEnvironmentVariable("PATH").Split(';'))
            {
                Fullname = Path.Combine(Folder, DllName);
                if (File.Exists(Fullname))
                    return Fullname;
            }

            return null;
        }

        public static LibraryType GetLibraryType(string Filename)
        {
            // PE Struct
            //https://web.archive.org/web/20160202125049/http://blogs.msdn.com/b/kstanton/archive/2004/03/31/105060.aspx

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

        public static IEnumerable<Assembly> GetDependencyAssemblies(object Obj)
        {
            if (Obj is null)
                yield break;

            Dictionary<Type, Assembly> Datas = [];

            // Type Assembly
            Type t = Obj.GetType();
            Datas[t] = t.Assembly;

            // Content
            GetDependencyAssemblies(t, ref Datas);

            foreach (Assembly Assembly in Datas.Values.Distinct())
                yield return Assembly;
        }
        private static void GetDependencyAssemblies(Type ObjectType, ref Dictionary<Type, Assembly> Datas)
        {
            // Constructors
            foreach (ConstructorInfo Method in ObjectType.GetConstructors(AllModifierWithStatic))
            {
                foreach (ParameterInfo Parameter in Method.GetParameters())
                {
                    // Parameter Type Assembly
                    Type t = Parameter.ParameterType;
                    if (Datas.ContainsKey(t))
                        continue;

                    Datas[t] = t.Assembly;

                    // Nested Type Assembly
                    GetDependencyAssemblies(t, ref Datas);
                }
            }

            // Methods
            foreach (MethodInfo Method in ObjectType.GetMethods(AllModifierWithStatic))
            {
                // Return Type Assembly
                Type ReturnType = Method.ReturnType;
                if (!Datas.ContainsKey(ReturnType))
                    Datas[ReturnType] = ReturnType.Assembly;

                // Parameters
                foreach (ParameterInfo Parameter in Method.GetParameters())
                {
                    // Parameter Type Assembly
                    Type t = Parameter.ParameterType;
                    if (Datas.ContainsKey(t))
                        continue;

                    Datas[t] = t.Assembly;

                    // Nested Type Assembly
                    GetDependencyAssemblies(t, ref Datas);
                }
            }

            // Fields
            foreach (FieldInfo Field in ObjectType.GetFields(AllModifierWithStatic)
                                                  .Where(i => !ReflectionHelper.IsBackingField(i)))
            {
                // Field Type Assembly
                Type FieldType = Field.FieldType;
                if (!Datas.ContainsKey(FieldType))
                    Datas[FieldType] = FieldType.Assembly;
            }

            // Properties
            foreach (PropertyInfo Property in ObjectType.GetProperties(AllModifierWithStatic))
            {
                // Property Type Assembly
                Type PropertyType = Property.PropertyType;
                if (!Datas.ContainsKey(PropertyType))
                    Datas[PropertyType] = PropertyType.Assembly;
            }
        }

    }
}