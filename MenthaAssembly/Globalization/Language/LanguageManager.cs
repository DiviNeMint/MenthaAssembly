using MenthaAssembly.Globalization;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace MenthaAssembly
{
    public static class LanguageManager
    {
        public static string LanguagesFolder { set; get; } = Path.Combine(Environment.CurrentDirectory, "Languages");

        public static string ExtensionName { set; get; } = ".lgp";

        public static LanguagePacket Current { get; } = new LanguagePacket();


        public static void Load(this ILanguageData Packet, string LanguageName)
        {
            string FilePath = Path.Combine(LanguagesFolder, $"{LanguageName}{ExtensionName}");
            if (File.Exists(FilePath))
            {
                PropertyInfo[] PropertyInfos = Packet.GetType().GetProperties();
                foreach (Match item in Regex.Matches(File.ReadAllText(FilePath), @"(?<Property>[^=\s]+)=(?<Value>[^\r\n]+)"))
                    if (PropertyInfos.FirstOrDefault(i => i.Name.Equals(item.Groups["Property"].Value)) is PropertyInfo PropertyInfo)
                        PropertyInfo.SetValue(Packet, item.Groups["Value"].Value);
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
                if (!_Languages.Contains(FileName))
                    _Languages.Add(FileName);

                File.CopyTo(Path.Combine(LanguagesFolder, File.Name), true);
            }
        }
        public static void Export(this ILanguageData Packet, string LanguageName, string DirectoryPath)
        {
            if (!Directory.Exists(LanguagesFolder))
                Directory.CreateDirectory(LanguagesFolder);

            File.WriteAllLines(
                Path.Combine(DirectoryPath, $"{LanguageName}{ExtensionName}"),
                Packet.GetType().GetProperties().Select(i => $"{i.Name}={i.GetValue(Packet)}"));
        }


        private static IList<string> _Languages;
        //public static IEnumerable<string> Languages => GetLanguageNames();
        public static IEnumerable<string> GetLanguageNames()
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
                Current.Name = null;
            }

            return _Languages;
        }

        public static LanguagePacket GetLanguagePacket(string Name)
        {
            return new LanguagePacket
            {
                Name = Name
            };
        }
    }
}
