using MenthaAssembly.Globalization;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

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

        private static LanguagePacketBase _Current;
        public static LanguagePacketBase Current
        {
            get => _Current;
            set
            {
                AppDomain.CurrentDomain.AssemblyLoad -= OnAssemblyLoad;

                _Current = value;
                OnStaticPropertyChanged();
            }
        }

        private static ObservableCollection<string> _Languages;
        public static IReadOnlyList<string> Languages
        {
            get
            {
                _Languages ??= new ObservableCollection<string>();

                DirectoryInfo Directory = new(LanguagesFolder);
                if (Directory.Exists)
                {
                    IEnumerable<string> FilesNames = Directory.EnumerateFiles($"*{ExtensionName}")
                                                              .Select(i => Path.GetFileNameWithoutExtension(i.Name));

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
                                                   i.IsBaseOn<LanguagePacketBase>()) is IEnumerable<Type> DataTypes)
            {
                Current = new MultiLanguagePacket(DataTypes.Select(t => Activator.CreateInstance(t) as LanguagePacketBase)
                                                           .Where(i => i != null));
                AppDomain.CurrentDomain.AssemblyLoad += OnAssemblyLoad;
            }

            Current.Load(LanguageName);
        }

        public static void Import(string File)
        {
            FileInfo Info = new(File);
            if (Info.Exists &&
                ExtensionName.Equals(Info.Extension))
            {
                string FileName = Path.GetFileNameWithoutExtension(Info.Name);
                if (!_Languages.Contains(FileName))
                    _Languages.Add(FileName);

                Info.CopyTo(Path.Combine(LanguagesFolder, Info.Name), true);
                OnStaticPropertyChanged(nameof(Languages));
            }
        }
        public static void Export(string Folder)
            => Current?.Save(Folder);

        private static void OnAssemblyLoad(object sender, AssemblyLoadEventArgs e)
        {
            try
            {
                if (_Current is MultiLanguagePacket Packet)
                {
                    LanguagePacketBase[] Packets = e.LoadedAssembly.GetTypes()
                                                                .Where(i => i.IsClass &&
                                                                            !i.IsAbstract &&
                                                                            !i.Name.Equals(nameof(MultiLanguagePacket)) &&
                                                                            i.IsBaseOn<LanguagePacketBase>())
                                                                .Select(t => Activator.CreateInstance(t) as LanguagePacketBase)
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