using MenthaAssembly.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace MenthaAssembly.Globalization
{
    public sealed class WindowsSystemLanguagePacket(string CultureCode) : IReadOnlyDictionary<string, string>
    {
        private readonly Dictionary<string, string> Single = [],
                                                    Complex = [],
                                                    ComplexWithoutSpace = [];

        public string CultureCode { get; } = CultureCode;

        public IEnumerable<string> Keys
            => Single.Keys.Concat(Complex.Keys);

        public IEnumerable<string> Values
            => Single.Values.Concat(Complex.Values);

        public string this[string Key]
            => TryGetValue(Key, out string Value) ? Value : null;

        public int Count
            => Single.Count + Complex.Count;

        public bool ContainsKey(string Key)
        {
            Key = Key.ToLower();
            return Key.Contains(' ') ? Complex.ContainsKey(Key) : Single.ContainsKey(Key) || ComplexWithoutSpace.ContainsKey(Key);
        }

        public bool TryGetValue(string Key, out string Value)
        {
            Key = Key.ToLower();
            return Key.Contains(' ') ? Complex.TryGetValue(Key, out Value) : Single.TryGetValue(Key, out Value) || ComplexWithoutSpace.TryGetValue(Key, out Value);
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
            => new Enumerator(this);
        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        public override string ToString()
            => $"Culture = {CultureCode} ,Count = {Count}";

        public static WindowsSystemLanguagePacket Load(string CultureCode)
        {
            string KeyFolder = Path.Combine(Environment.SystemDirectory, "en-US");
            if (!Directory.Exists(KeyFolder))
                return null;

            string Folder = Path.Combine(Environment.SystemDirectory, CultureCode);
            if (!Directory.Exists(Folder))
                return null;

            WindowsSystemLanguagePacket Packet = new(CultureCode);
            foreach (string FilePath in Directory.EnumerateFiles(Folder, "*.mui"))
            {
                string FileName = Path.GetFileName(FilePath);

                string KeyFile = Path.Combine(KeyFolder, FileName);
                if (!File.Exists(KeyFile))
                    continue;

                Dictionary<string, uint> KeyTable = CultureHelper.LoadStringTableFromMui(KeyFile);
                IEnumerable<(bool Valid, string Key, int Space)> Datas = KeyTable.Keys.Select(i => (VerifyWord(i, out int Space), i, Space))
                                                                                      .Where(i => i.Item1);
                if (Datas.Any())
                {
                    IntPtr hModule = Win32.System.LoadLibraryEx(FilePath, IntPtr.Zero, LoadLibraryFlag.LOAD_LIBRARY_AS_DATAFILE);
                    if (hModule == IntPtr.Zero)
                    {
                        int ErrorCode = Marshal.GetLastWin32Error();
                        string Message;

#if NET7_0_OR_GREATER
                        Message = Marshal.GetPInvokeErrorMessage(ErrorCode);
#else
                        Win32Exception Ex = new(Marshal.GetLastWin32Error());
                        Message = Ex.Message;
#endif

                        Debug.WriteLine($"[{nameof(WindowsSystemLanguagePacket)}][{nameof(Load)}][Win32Exception]{Message}");
                        continue;
                    }

                    try
                    {
                        StringBuilder Buffer = new(256);
                        foreach ((bool Valid, string Key, int Space) in Datas)
                        {
                            string Text = Win32.System.LoadString(hModule, KeyTable[Key], Buffer, Buffer.Capacity) > 0 ? Buffer.ToString() : null;
                            Buffer.Clear();

                            if (Key != Text)
                            {
                                string LowerKey = Key.ToLower();

                                // Single word
                                if (Space == 0)
                                {
                                    Packet.Single[LowerKey] = Text;
                                    continue;
                                }

                                // Complex words
                                Packet.Complex[LowerKey] = Text;
                                Packet.ComplexWithoutSpace[RemoveSpace(LowerKey)] = Text;
                            }
                        }
                    }
                    finally
                    {
                        Win32.System.FreeLibrary(hModule);
                    }
                }
            }

            return Packet;
        }

        private static bool VerifyWord(string Key, out int Space)
        {
            Space = 0;
            int Length = Key.Length;
            if (Length < 2)
                return false;

            char c = Key[0];
            if (!char.IsLetter(c))
                return false;

            for (int i = 1; i < Length; i++)
            {
                c = Key[i];
                if (c == ' ')
                {
                    if (++Space > 1)
                        return false;

                    continue;
                }

                if (!char.IsLetter(c))
                    return false;
            }

            return c != ' ';    // Checks last char
        }
        private static string RemoveSpace(string ComplexWords)
        {
            StringBuilder Builder = new();
            try
            {
                foreach (char c in ComplexWords)
                {
                    if (c == ' ')
                        continue;

                    Builder.Append(c);
                }

                return Builder.ToString();
            }
            finally
            {
                Builder.Clear();
            }
        }

        private class Enumerator(WindowsSystemLanguagePacket Parent) : IEnumerator<KeyValuePair<string, string>>
        {
            public KeyValuePair<string, string> Current
                => Enumerators[Index].Current;
            object IEnumerator.Current
                => Enumerators[Index].Current;

            private readonly IEnumerator<KeyValuePair<string, string>>[] Enumerators = [Parent.Single.GetEnumerator(), Parent.Complex.GetEnumerator()];
            private int Index = 0;

            public bool MoveNext()
            {
                for (; Index < Enumerators.Length; Index++)
                {
                    if (Enumerators[Index].MoveNext())
                        return true;
                }

                return false;
            }

            public void Reset()
            {
                Index = 0;

                foreach (IEnumerator<KeyValuePair<string, string>> Enumerator in Enumerators)
                    Enumerator.Reset();
            }

            public void Dispose()
            {
                foreach (IEnumerator<KeyValuePair<string, string>> Enumerator in Enumerators)
                    Enumerator.Dispose();
            }

        }

    }
}