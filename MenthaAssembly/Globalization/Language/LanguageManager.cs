using MenthaAssembly.Globalization;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace MenthaAssembly
{
    public static class LanguageManager
    {
        public static event PropertyChangedEventHandler StaticPropertyChanged;

        private static string _LanguagesFolder = Path.Combine(Environment.CurrentDirectory, "Languages");
        public static string LanguagesFolder
        {
            get => _LanguagesFolder;
            set
            {
                _LanguagesFolder = value;
                OnStaticPropertyChanged();
            }
        }

        private static string _ExtensionName = ".lgp";
        public static string ExtensionName
        {
            get => _ExtensionName;
            set
            {
                _ExtensionName = value;
                OnStaticPropertyChanged();
            }
        }

        private static ILanguagePacket _Current;
        public static ILanguagePacket Current
        {
            get => _Current;
            set
            {
                _Current = value;
                OnStaticPropertyChanged();
            }
        }

        private static IList<string> _Languages;
        public static IList<string> Languages
        {
            get
            {
                if (_Languages is null)
                    _Languages = new ObservableCollection<string>();

                DirectoryInfo Directory = new DirectoryInfo(LanguagesFolder);
                if (Directory.Exists)
                {
                    IEnumerable<string> FilesNames = Directory.EnumerateFiles()
                                                              .Where(i => i.Extension?.Equals(ExtensionName) ?? false)
                                                              .Select(i => i.Name.Replace(ExtensionName, string.Empty));

                    IEnumerable<string> TempFilesNames = _Languages.Except(FilesNames);
                    if (TempFilesNames.Count() > 0)
                        foreach (string FileName in TempFilesNames)
                            _Languages.Remove(FileName);

                    foreach (string FileName in FilesNames)
                        if (!_Languages.Contains(FileName))
                            _Languages.Add(FileName);
                }
                else
                {
                    _Languages.Clear();
                    Current = null;
                }

                return _Languages;
            }
        }


        public static void Load(string LanguageName)
        {
            if (Current is null &&
                AppDomain.CurrentDomain.GetAssemblies()
                                       .TrySelectMany(i => i.GetTypes())
                                       .FirstOrDefault(i => i.IsClass &&
                                                            !i.IsAbstract &&
                                                            i.BaseType != null &&
                                                            i.BaseType.Name.Equals(nameof(ILanguagePacket))) is Type DataType)
                Current = Activator.CreateInstance(DataType) as ILanguagePacket;

            Current.Load(LanguageName);
        }
        public static void Load(this ILanguagePacket LanguagePacket, string LanguageName)
        {
            string FilePath = Path.Combine(LanguagesFolder, $"{LanguageName}{ExtensionName}");
            if (File.Exists(FilePath))
            {
                PropertyInfo[] PropertyInfos = LanguagePacket.GetType().GetProperties();
                foreach (Match item in Regex.Matches(File.ReadAllText(FilePath), @"(?<Property>[^=\s]+)=(?<Value>[^\r\n]+)"))
                    if (PropertyInfos.FirstOrDefault(i => i.Name.Equals(item.Groups["Property"].Value)) is PropertyInfo PropertyInfo)
                        PropertyInfo.SetValue(LanguagePacket, item.Groups["Value"].Value);

                LanguagePacket.LanguageName = LanguageName;
                LanguagePacket.OnPropertyChanged();
            }
            else
            {
                throw new ArgumentNullException(LanguageName, $"Can't find Language Packet at {FilePath}");
            }
        }
        public static void Save(this ILanguagePacket LanguagePacket, string DirectoryPath)
        {
            if (!Directory.Exists(LanguagesFolder))
                Directory.CreateDirectory(LanguagesFolder);

            File.WriteAllLines(Path.Combine(DirectoryPath, $"{LanguagePacket.LanguageName}{ExtensionName}"),
                               LanguagePacket.GetType()
                                             .GetProperties()
                                             .Where(i => i.Name != nameof(LanguagePacket.LanguageName))
                                             .TrySelect(i => $"{i.Name}={i.GetValue(LanguagePacket)}"));
        }
        public static void Save(this ILanguagePacket LanguagePacket, string LanguageName, string DirectoryPath)
        {
            if (!Directory.Exists(LanguagesFolder))
                Directory.CreateDirectory(LanguagesFolder);

            File.WriteAllLines(Path.Combine(DirectoryPath, $"{LanguageName}{ExtensionName}"),
                               LanguagePacket.GetType()
                                             .GetProperties()
                                             .Where(i => !i.Name.Equals(nameof(LanguageName)) && !i.Name.Equals("Item"))
                                             .TrySelect(i => $"{i.Name}={i.GetValue(LanguagePacket)}"));
        }

        public static void Import(string FilePath)
        {
            FileInfo File = new FileInfo(FilePath);
            if (File.Exists &&
                (File.Extension?.Equals(ExtensionName) ?? false))
            {
                string FileName = File.Name.Replace(ExtensionName, string.Empty);
                if (!_Languages.Contains(FileName))
                    _Languages.Add(FileName);

                File.CopyTo(Path.Combine(LanguagesFolder, File.Name), true);
                OnStaticPropertyChanged(nameof(Languages));
            }
        }
        public static void Export(string DirectoryPath) 
            => Current?.Save(DirectoryPath);

        private static void OnStaticPropertyChanged([CallerMemberName]string PropertyName = null) 
            => StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(PropertyName));

    }
}
