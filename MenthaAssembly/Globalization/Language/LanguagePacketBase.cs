using System;
using System.Buffers;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using static MenthaAssembly.LanguageManager;

namespace MenthaAssembly.Globalization
{
    public abstract class LanguagePacketBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected Dictionary<string, Func<string>> GetCaches;
        protected Dictionary<string, Action<string>> SetCaches;

        public string LanguageName { internal protected set; get; } = "Default";

        public virtual string this[string Name]
        {
            get
            {
                if (string.IsNullOrEmpty(Name))
                    return null;

                if (GetCaches is null)
                    CreateCache();

                if (GetCaches.TryGetValue(Name, out Func<string> GetValue))
                    return GetValue();

                Debug.WriteLine($"[{GetType().Name}]Not fount {Name}.");
                return null;
            }
            protected internal set
            {
                if (SetCaches is null)
                    CreateCache();

                if (SetCaches.TryGetValue(Name, out Action<string> SetValue))
                    SetValue(value);
            }
        }

        public void Load(string LanguageName)
        {
            string FilePath = Path.Combine(LanguagesFolder, $"{LanguageName}{ExtensionName}");
            if (File.Exists(FilePath))
            {
                FileStream Stream = new(FilePath, FileMode.Open, FileAccess.Read);
                try
                {
                    if (IsZipArchive(Stream))
                    {
                        ZipArchive Archive = new(Stream, ZipArchiveMode.Read);
                        foreach (ZipArchiveEntry Entry in Archive.Entries.OrderBy(i => i.LastWriteTime))
                        {
                            Stream Content = Entry.Open();
                            try
                            {
                                LoadPacketContent(Content);
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
                        LoadPacketContent(Stream);
                    }
                }
                finally
                {
                    Stream.Dispose();
                }

                this.LanguageName = LanguageName;
                OnPropertyChanged();
            }
            else
            {
                throw new ArgumentNullException(LanguageName, $"Can't find Language Packet at {FilePath}");
            }
        }

        public void Save(string Folder)
            => Save(LanguageName, Folder);
        public void Save(string LanguageName, string Folder)
        {
            if (!Directory.Exists(LanguagesFolder))
                Directory.CreateDirectory(LanguagesFolder);

            File.WriteAllLines(Path.Combine(Folder, $"{LanguageName}{ExtensionName}"),
                               GetPropertyNames().TrySelect(i => $"{i}={this[i]}"));
        }

        private bool IsZipArchive(Stream Stream)
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
        private void LoadPacketContent(Stream Stream)
        {
            StreamReader Reader = new(Stream);
            try
            {
                while (!Reader.EndOfStream)
                {
                    string Line = Reader.ReadLine();
                    if (Line.StartsWith("//"))
                    {
                        // Comment
                        continue;
                    }
                    else if (Line.StartsWith("%"))
                    {
                        // Command
                        continue;
                    }
                    else
                    {
                        string[] Contents = Line.Split('=');
                        this[Contents[0]] = Contents[1];
                    }
                }
            }
            finally
            {
                Reader.Dispose();
            }
        }

        protected internal virtual IEnumerable<string> GetPropertyNames()
        {
            if (GetCaches is null)
                CreateCache();

            return GetCaches.Keys;
        }

        protected void CreateCache()
        {
            GetCaches = new Dictionary<string, Func<string>>();
            SetCaches = new Dictionary<string, Action<string>>();

            ConstantExpression This = Expression.Constant(this);
            foreach (PropertyInfo Info in GetType().GetProperties()
                                                   .Where(i => !i.Name.Equals(nameof(LanguageName)) && !i.Name.Equals("Item")))
            {
                MemberExpression Property = Expression.Property(This, Info);
                ParameterExpression Parameter = Expression.Parameter(typeof(string), "Value");
                BinaryExpression Assign = Expression.Assign(Property, Parameter);

                GetCaches.Add(Info.Name, Expression.Lambda<Func<string>>(Property).Compile());
                SetCaches.Add(Info.Name, Expression.Lambda<Action<string>>(Assign, Parameter).Compile());
            }
        }

        protected virtual void OnPropertyChanged()
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(""));

    }
}