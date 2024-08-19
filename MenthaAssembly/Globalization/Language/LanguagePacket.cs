using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace MenthaAssembly.Globalization
{
    public sealed class LanguagePacket : IEquatable<LanguagePacket>, ICloneable
    {
        public event EventHandler<string> LanguageContextChanged;

        public string LanguageName { get; }

        public string PacketPath { get; }

        private string _CultureCode;
        public string CultureCode
        {
            get
            {
                Load();
                return _CultureCode;
            }
        }

        private readonly Dictionary<string, string> Contexts;
        public string this[string Name]
        {
            get
            {
                if (string.IsNullOrEmpty(Name))
                    return null;

                Load();
                return Contexts.TryGetValue(Name, out string Value) ? Value : null;
            }
            set
            {
                if (Contexts.TryGetValue(Name, out string OldValue))
                {
                    if (string.IsNullOrEmpty(value))
                    {
                        Contexts.Remove(Name);
                        OnLanguageContextChanged(Name);
                    }
                    else if (!value.Equals(OldValue))
                    {
                        Contexts[Name] = value;
                        OnLanguageContextChanged(Name);
                    }
                }
                else if (!string.IsNullOrEmpty(value))
                {
                    Contexts[Name] = value;
                    OnLanguageContextChanged(Name);
                }
            }
        }

        private LanguagePacket(LanguagePacket Packet)
        {
            PacketPath = Packet.PacketPath;
            LanguageName = Packet.LanguageName;
            _CultureCode = Packet._CultureCode;
            IsLoaded = Packet.IsLoaded;
            Contexts = new Dictionary<string, string>(Packet.Contexts);
        }
        public LanguagePacket(string PacketPath)
        {
            this.PacketPath = PacketPath;
            LanguageName = Path.GetFileNameWithoutExtension(PacketPath);
            Contexts = [];
        }

        private bool IsLoaded = false;
        private void Load()
        {
            if (IsLoaded)
                return;

            if (File.Exists(PacketPath))
            {
                FileStream Stream = new(PacketPath, FileMode.Open, FileAccess.Read);
                try
                {
                    Parse(Stream);
                }
                finally
                {
                    Stream.Dispose();
                }
            }

            IsLoaded = true;
        }

        private void Parse(Stream Stream)
        {
            if (IsZipArchive(Stream))
            {
                ZipArchive Archive = new(Stream, ZipArchiveMode.Read);
                foreach (ZipArchiveEntry Entry in Archive.Entries.OrderBy(i => i.LastWriteTime))
                {
                    Stream Content = Entry.Open();
                    try
                    {
                        Parse(Content);
                    }
                    finally
                    {
                        Content.Dispose();
                    }
                }
            }
            else
            {
                Stream.Seek(0L, SeekOrigin.Begin);
                ParsePacket(Stream);
            }

        }
        private static bool IsZipArchive(Stream Stream)
        {
            const int IdentifierSize = 4;
            byte[] Identifier = ArrayPool<byte>.Shared.Rent(IdentifierSize);
            try
            {
                return Stream.ReadBuffer(Identifier, IdentifierSize) &&
                       ArchiveHelper.IsZipArchive(Identifier);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(Identifier);
            }
        }
        private void ParsePacket(Stream Stream)
        {
            StreamReader Reader = new(Stream);
            StringBuilder Builder = new();

            try
            {
                while (!Reader.EndOfStream)
                {
                    string Line = Reader.ReadLine();

                    // Comment
                    if (Line.StartsWith("//"))
                        continue;

                    // Command
#if NETSTANDARD2_1_OR_GREATER || NET7_0_OR_GREATER
                    if (Line.StartsWith('%'))
#else
                    if (Line.StartsWith("%"))
#endif
                    {
                        ParseContent(Line, 1, ref Builder, out string Command, out string Content);
                        switch (Command.ToLower())
                        {
                            case "culturecode":
                                {
                                    if (CultureHelper.ExistsCulture(Content))
                                        _CultureCode = Content;

                                    break;
                                }
                        }

                        continue;
                    }

                    // Content
                    ParseContent(Line, 0, ref Builder, out string Key, out string Value);
                    if (!string.IsNullOrEmpty(Key))
                        Contexts[Key] = Value;
                }
            }
            finally
            {
                Reader.Dispose();
            }
        }
        private static void ParseContent(string Line, int StartIndex, ref StringBuilder Builder, out string Key, out string Content)
        {
            int i = StartIndex,
                Length = Line.Length;

            char c;
            try
            {
                while (i < Length)
                {
                    c = Line[i++];
                    if (c == '=')
                        break;

                    Builder.Append(c);
                }

                Key = Builder.ToString();
            }
            finally
            {
                Builder.Clear();
            }

            try
            {
                while (i < Length)
                {
                    c = Line[i++];
                    Builder.Append(c);
                }

                Content = Builder.ToString();
            }
            finally
            {
                Builder.Clear();
            }
        }

        public bool TryMerge(LanguagePacket Packet)
            => TryMerge(Packet, true);
        public bool TryMerge(LanguagePacket Packet, bool Override)
        {
            if (!string.IsNullOrEmpty(Packet.CultureCode))
            {
                if (string.IsNullOrEmpty(CultureCode))
                    _CultureCode = Packet.CultureCode;

                else if (!CultureCode.Equals(Packet.CultureCode))
                    return false;
            }

            if (Override)
            {
                foreach (KeyValuePair<string, string> Context in Packet.Contexts)
                    Contexts[Context.Key] = Context.Value;
            }
            else
            {
                foreach (string Key in Packet.GetNames().Except(GetNames()))
                    Contexts[Key] = Packet.Contexts[Key];
            }

            return true;
        }

        public IEnumerable<string> GetNames()
        {
            Load();
            return Contexts.Keys;
        }

        public void Save(string Folder)
            => Save(Folder, LanguageName);
        public void Save(string Folder, string LanguageName)
        {
            if (!Directory.Exists(Folder))
                Directory.CreateDirectory(Folder);

            using FileStream Stream = new(Path.Combine(Folder, $"{LanguageName}{LanguageManager.ExtensionName}"), FileMode.Create, FileAccess.Write);
            using StreamWriter Writer = new(Stream);

            if (!string.IsNullOrEmpty(_CultureCode))
            {
                Writer.Write("%CultrueCode=");
                Writer.WriteLine(_CultureCode);
            }

            foreach (KeyValuePair<string, string> Context in Contexts)
            {
                Writer.Write(Context.Key);
                Writer.Write('=');
                Writer.WriteLine(Context.Value);
            }
        }

        public override bool Equals(object obj)
            => Equals(obj as LanguagePacket);
        public bool Equals(LanguagePacket other)
            => other is not null && LanguageName == other.LanguageName && PacketPath == other.PacketPath;

        object ICloneable.Clone()
            => Clone();
        public LanguagePacket Clone()
            => new(this);

        public override int GetHashCode()
        {
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
            return HashCode.Combine(LanguageName, PacketPath);
#else
            int hashCode = -821502486;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(LanguageName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(PacketPath);
            return hashCode;
#endif
        }

        public override string ToString()
            => LanguageName;

        private void OnLanguageContextChanged(string Name)
            => LanguageContextChanged?.Invoke(this, Name);

    }
}