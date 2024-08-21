using MenthaAssembly.Globalization;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

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
                OnStaticPropertyChanged(nameof(Languages));
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

        private static LanguagePacket _Current;
        public static LanguagePacket Current
        {
            get => _Current;
            set
            {
                // Reset current WindowsSystemLanguagePacket
                if (_Current?.CultureCode != value?.CultureCode)
                    CurrentWindowsSystemCultureCode = null;

                _Current = value;

                // Notify
                OnStaticPropertyChanged();
            }
        }

        private static readonly Dictionary<string, WindowsSystemLanguagePacket> WindowsSystemCache = [];
        private static string CurrentWindowsSystemCultureCode;
        private static WindowsSystemLanguagePacket _CurrentWindowsSystem;
        public static WindowsSystemLanguagePacket CurrentWindowsSystem
        {
            get
            {
                if (string.IsNullOrEmpty(CurrentWindowsSystemCultureCode))
                {
                    CurrentWindowsSystemCultureCode = _Current?.CultureCode?.ToLower() ?? CultureInfo.CurrentCulture.Name.ToLower();
                    if (!WindowsSystemCache.TryGetValue(CurrentWindowsSystemCultureCode, out _CurrentWindowsSystem) &&
                        WindowsSystemLanguagePacket.Load(CurrentWindowsSystemCultureCode) is WindowsSystemLanguagePacket LoadPacket)
                    {
                        WindowsSystemCache[CurrentWindowsSystemCultureCode] = LoadPacket;
                        _CurrentWindowsSystem = LoadPacket;
                    }
                }

                return _CurrentWindowsSystem;
            }
        }

        private static ObservableCollection<LanguagePacket> _Languages;
        public static IReadOnlyList<LanguagePacket> Languages
        {
            get
            {
                _Languages ??= [];

                DirectoryInfo Directory = new(LanguagesFolder);
                if (Directory.Exists)
                {
                    IEnumerable<LanguagePacket> Packets = Directory.EnumerateFiles($"*{ExtensionName}")
                                                                   .Select(i => new LanguagePacket(i.FullName));

                    IEnumerable<LanguagePacket> TempPackets = _Languages.Except(Packets);
                    if (TempPackets.Any())
                        foreach (LanguagePacket Packet in TempPackets)
                            _Languages.Remove(Packet);

                    foreach (LanguagePacket Packet in Packets)
                        if (!_Languages.Contains(Packet))
                            _Languages.Add(Packet);
                }
                else
                {
                    _Languages.Clear();
                    Current = null;
                }

                return _Languages;
            }
        }

        private static DateTime NextUpdateTime = DateTime.Now;
        private static bool _CanGoogleTranslate;
        public static bool CanGoogleTranslate
        {
            get
            {
                if (DateTime.Now < NextUpdateTime)
                    return _CanGoogleTranslate;

                NextUpdateTime = DateTime.Now.AddMinutes(10);

                try
                {
                    IPHostEntry host = Dns.GetHostEntry("translate.googleapis.com");
                    IPAddress[] addresses = host.AddressList;
                    if (addresses.Length == 0)
                    {
                        _CanGoogleTranslate = false;
                        return _CanGoogleTranslate;
                    }

                    using Ping Ping = new();
                    PingReply Reply = Ping.Send(addresses[0], 1000);
                    _CanGoogleTranslate = Reply.Status == IPStatus.Success;
                }
                catch
                {
                    _CanGoogleTranslate = false;
                }

                return _CanGoogleTranslate;
            }
        }

        private static bool _EnableGoogleTranslate;
        public static bool EnableGoogleTranslate
        {
            get => _EnableGoogleTranslate;
            set
            {
                _EnableGoogleTranslate = value;
                OnStaticPropertyChanged();
            }
        }

        static LanguageManager()
        {
            IReadOnlyList<LanguagePacket> Packets = Languages;
            if (Packets.Count > 0)
            {
                string CultureCode = CultureInfo.CurrentCulture.Name.ToLower();
                Current = Packets.FirstOrDefault(i => i.CultureCode?.ToLower() == CultureCode);
            }
        }

        internal static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, string>> CacheTranslate = [];
        /// <summary>
        /// Gets the language content from key.
        /// </summary>
        public static string Get(string Key)
            => Get(Key, Key);
        /// <summary>
        /// Gets the language content from key.
        /// </summary>
        public static string Get(string Key, string Default)
        {
            if (string.IsNullOrEmpty(Key))
                return Default;

            // Current
            string Result = Current?[Key];
            if (!string.IsNullOrEmpty(Result))
                return Result;

            // Windows System Build-in String
            Result = CurrentWindowsSystem[Key];
            if (!string.IsNullOrEmpty(Result))
                return Result;

            // GoogleTranslate
            string ToCulture = Current?.CultureCode;
            if (!string.IsNullOrEmpty(ToCulture) &&
                CanGoogleTranslate &&
                EnableGoogleTranslate)
            {
                if (CacheTranslate.TryGetValue(ToCulture, out ConcurrentDictionary<string, string> Caches))
                {
                    if (Caches.TryGetValue(Key, out Result))
                        return Result;
                }
                else
                {
                    Caches = [];
                    CacheTranslate.AddOrUpdate(ToCulture, Caches, (k, v) => Caches);
                }

                Result = GoogleTranslate(Key, "en-US", ToCulture);
                if (!string.IsNullOrEmpty(Result))
                {
                    Caches.AddOrUpdate(Key, Result, (k, v) => Result);
                    return Result;
                }
            }

            Debug.WriteLine($"[Language] Not fount {Key}.");
            return Default ?? Key;
        }

        /// <summary>
        /// Translates a string into another language using Google's translate API JSON calls.
        /// </summary>
        /// <param name="Text">Text to translate. Should be a single word or sentence.</param>
        /// <param name="FromCulture">
        /// Two letter culture (en of en-us, fr of fr-ca, de of de-ch)
        /// </param>
        /// <param name="ToCulture">
        /// Two letter culture (as for FromCulture)
        /// </param>
        public static string GoogleTranslate(string Text, string FromCulture, string ToCulture)
        {
            FromCulture = FromCulture.ToLower();
            if (!CultureHelper.ExistsCulture(FromCulture))
            {
                Debug.WriteLine($"[{nameof(GoogleTranslate)}] Invalid culture : {FromCulture}");
                return null;
            }

            ToCulture = ToCulture.ToLower();
            if (!CultureHelper.ExistsCulture(ToCulture))
            {
                Debug.WriteLine($"[{nameof(GoogleTranslate)}] Invalid culture : {ToCulture}");
                return null;
            }

            string Url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl={FromCulture}&tl={ToCulture}&dt=t&q={Uri.EscapeDataString(Text)}";

            string Json = GetGoogleTranslateResult(Url).Result;
            if (string.IsNullOrEmpty(Json))
                return null;

            string Result = ParseGoogleTranslateResult(Json);
            if (string.IsNullOrEmpty(Result))
            {
                Debug.WriteLine($"[{nameof(GoogleTranslate)}] Invalid search result : {Json}");
                return null;
            }

            return Result;
        }
        /// <summary>
        /// Translates a string into another language using Google's translate API JSON calls.
        /// </summary>
        /// <param name="Text">Text to translate. Should be a single word or sentence.</param>
        /// <param name="FromCulture">
        /// Two letter culture (en of en-us, fr of fr-ca, de of de-ch)
        /// </param>
        /// <param name="ToCulture">
        /// Two letter culture (as for FromCulture)
        /// </param>
        public static async Task<string> GoogleTranslateAsync(string Text, string FromCulture, string ToCulture)
        {
            FromCulture = FromCulture.ToLower();
            if (CultureHelper.ExistsCulture(FromCulture))
            {
                Debug.WriteLine($"[{nameof(GoogleTranslate)}] Invalid culture : {FromCulture}");
                return null;
            }

            ToCulture = ToCulture.ToLower();
            if (CultureHelper.ExistsCulture(ToCulture))
            {
                Debug.WriteLine($"[{nameof(GoogleTranslate)}] Invalid culture : {ToCulture}");
                return null;
            }

            string Url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl={FromCulture}&tl={ToCulture}&dt=t&q={Uri.EscapeDataString(Text)}";

            string Json = await GetGoogleTranslateResult(Url);
            if (string.IsNullOrEmpty(Json))
                return null;

            string Result = ParseGoogleTranslateResult(Json);
            if (string.IsNullOrEmpty(Result))
            {
                Debug.WriteLine($"[{nameof(GoogleTranslate)}] InvalidSearchResult : {Json}");
                return null;
            }

            return Result;
        }

        private static Task<string> GetGoogleTranslateResult(string Url)
        {
            try
            {
                HttpClient Client = new();
                return Client.GetStringAsync(Url);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[GoogleTranslate] {ex.GetBaseException().Message}");
                return null;
            }
        }
        private static string ParseGoogleTranslateResult(string Json)
        {
            char c;
            int i, Counter = 0;
            for (i = 0; i < Json.Length;)
            {
                c = Json[i++];
                if (Counter < 3)
                {
                    if (c == '[')
                        Counter++;

                    continue;
                }

                if (c == '"')
                    break;
            }

            StringBuilder Builder = new();
            try
            {
                while (i < Json.Length)
                {
                    c = Json[i++];
                    if (c == '"')
                        break;

                    Builder.Append(c);
                }

                return Builder.ToString();
            }
            finally
            {
                Builder.Clear();
            }
        }

        private static void OnStaticPropertyChanged([CallerMemberName] string PropertyName = null)
            => StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(PropertyName));

    }
}