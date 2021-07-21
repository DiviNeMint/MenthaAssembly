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
                AppDomain.CurrentDomain.AssemblyLoad -= OnAssemblyLoad;

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
                                                              .Where(i => ExtensionName.Equals(i.Extension))
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
                                       .Where(i => i.IsClass &&
                                                   !i.IsAbstract &&
                                                   !i.Name.Equals(nameof(MultiLanguagePacket)) &&
                                                   i.IsBaseOn<ILanguagePacket>()) is IEnumerable<Type> DataTypes)
            {
                Current = new MultiLanguagePacket(DataTypes.Select(t => Activator.CreateInstance(t) as ILanguagePacket)
                                                           .Where(i => i != null));
                AppDomain.CurrentDomain.AssemblyLoad += OnAssemblyLoad;
            }

            Current.Load(LanguageName);
        }
        public static void Load(this ILanguagePacket Packet, string LanguageName)
        {
            string FilePath = Path.Combine(LanguagesFolder, $"{LanguageName}{ExtensionName}");
            if (File.Exists(FilePath))
            {
                foreach (Match item in Regex.Matches(File.ReadAllText(FilePath), @"(?<Property>[^=\s]+)=(?<Value>[^\r\n]+)"))
                    Packet[item.Groups["Property"].Value] = item.Groups["Value"].Value;

                Packet.LanguageName = LanguageName;
                Packet.OnPropertyChanged();
            }
            else
            {
                throw new ArgumentNullException(LanguageName, $"Can't find Language Packet at {FilePath}");
            }
        }
        public static void Save(this ILanguagePacket Packet, string DirectoryPath)
            => Save(Packet, Packet.LanguageName, DirectoryPath);
        public static void Save(this ILanguagePacket Packet, string LanguageName, string DirectoryPath)
        {
            if (!Directory.Exists(LanguagesFolder))
                Directory.CreateDirectory(LanguagesFolder);

            File.WriteAllLines(Path.Combine(DirectoryPath, $"{LanguageName}{ExtensionName}"),
                               Packet.GetPropertyNames()
                                     .TrySelect(i => $"{i}={Packet[i]}"));
        }

        public static void Import(string FilePath)
        {
            FileInfo File = new FileInfo(FilePath);
            if (File.Exists &&
                ExtensionName.Equals(File.Extension))
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

        private static void OnAssemblyLoad(object sender, AssemblyLoadEventArgs e)
        {
            try
            {
                if (_Current is MultiLanguagePacket Packet)
                {
                    ILanguagePacket[] Packets = e.LoadedAssembly.GetTypes()
                                                                .Where(i => i.IsClass &&
                                                                            !i.IsAbstract &&
                                                                            !i.Name.Equals(nameof(MultiLanguagePacket)) &&
                                                                            i.IsBaseOn<ILanguagePacket>())
                                                                .Select(t => Activator.CreateInstance(t) as ILanguagePacket)
                                                                .Where(i => i != null)
                                                                .ToArray();
                    if (Packets.Length > 0)
                    {
                        Packet.Add(Packets);
                        Packet.Load(_Current.LanguageName);
                    }
                }
            }
            catch
            {
            }
        }

        private static void OnStaticPropertyChanged([CallerMemberName] string PropertyName = null)
            => StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(PropertyName));

    }
}
