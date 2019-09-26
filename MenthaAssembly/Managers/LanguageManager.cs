using MenthaAssembly.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace MenthaAssembly
{
    public static class LanguageManager
    {
        private static ILanguagePacket _Current;
        public static ILanguagePacket Current
        {
            get => _Current;
            set
            {
                   _Current = value;
                if (value is ILanguagePacket Packet)
                    foreach (var Info in Packet.GetType().GetProperties())
                        Packet.OnPropertyChanged(Info.Name);
            }
        }

        public static string LanguagesFolder { set; get; } = Path.Combine(Environment.CurrentDirectory, "Languages");

        public static string ExtensionName { set; get; } = ".lgp";

        private static IList<string> _LanguageNames;
        public static IList<string> LanguageNames
        {
            get
            {
                if (_LanguageNames is null)
                    _LanguageNames = new ObservableCollection<string>();

                DirectoryInfo Directory = new DirectoryInfo(LanguagesFolder);
                if (Directory.Exists)
                {
                    IEnumerable<string> FilesNames = Directory.EnumerateFiles()
                        .Where(i => i.Extension?.Equals(ExtensionName) ?? false)
                        .Select(i => i.Name.Replace(ExtensionName, string.Empty));

                    IEnumerable<string> TempFilesNames = _LanguageNames.Except(FilesNames);
                    if (TempFilesNames.Count() > 0)
                        foreach (string FileName in TempFilesNames)
                            _LanguageNames.Remove(FileName);

                    foreach (string FileName in FilesNames)
                        if (!_LanguageNames.Contains(FileName))
                            _LanguageNames.Add(FileName);
                }
                else
                {
                    _LanguageNames.Clear();
                }

                return _LanguageNames;
            }
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
            }
            else
            {
                throw new ArgumentNullException(LanguageName, $"Don't find Language Packet at {FilePath}");
            }
        }


        public static void Import(string FilePath)
        {
            FileInfo File = new FileInfo(FilePath);
            if (File.Exists &&
                (File.Extension?.Equals(ExtensionName) ?? false))
            {
                string FileName = File.Name.Replace(ExtensionName, string.Empty);
                if (!_LanguageNames.Contains(FileName))
                    _LanguageNames.Add(FileName);

                File.CopyTo(Path.Combine(LanguagesFolder, File.Name), true);
            }
        }

        public static void Export(this ILanguagePacket LanguagePacket, string LanguageName, string DirectoryPath)
        {
            if (!Directory.Exists(LanguagesFolder))
                Directory.CreateDirectory(LanguagesFolder);

            File.WriteAllLines(
                Path.Combine(DirectoryPath, $"{LanguageName}{ExtensionName}"),
                LanguagePacket.GetType().GetProperties().Select(i => $"{i.Name}={i.GetValue(LanguagePacket)}"));
        }

    }
}
