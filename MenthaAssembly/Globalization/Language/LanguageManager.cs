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

        private static LanguagePacket _Custom;
        public static LanguagePacket Custom
        {
            get => _Custom;
            set
            {
                // Reset current SystemLanguage
                if (_Custom?.CultureCode != value?.CultureCode)
                    LazySystem = null;

                _Custom = value;

                // Notify
                OnStaticPropertyChanged();
            }
        }

        private static readonly ConcurrentDictionary<string, Lazy<OSLanguagePacket>> SystemCache = new();
        internal static Lazy<OSLanguagePacket> LazySystem;
        public static OSLanguagePacket System
        {
            get
            {
                if (LazySystem is null)
                {
                    string Culture = _Custom?.CultureCode ?? CultureInfo.CurrentCulture.Name;
                    LazySystem = SystemCache.GetOrAdd(Culture, key => new Lazy<OSLanguagePacket>(() => OSLanguagePacket.Load(key)));
                }

                return LazySystem.Value;
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
                    Custom = null;
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
                Custom = Packets.FirstOrDefault(i => i.CultureCode?.ToLower() == CultureCode);
            }
        }

        /// <summary>
        /// Gets the language content from key.
        /// </summary>
        public static string Translate(string Text)
            => Translate(Text, Text);
        /// <summary>
        /// Gets the language content from key.
        /// </summary>
        public static string Translate(string Text, string Default)
        {
            if (string.IsNullOrEmpty(Text))
                return Default;

            // Current
            string Result = Custom?[Text];
            if (!string.IsNullOrEmpty(Result))
                return Result;

            // Windows System Build-in String
            Result = System[Text];
            if (!string.IsNullOrEmpty(Result))
                return Result;

            // GoogleTranslate
            string ToCulture = Custom?.CultureCode?.ToLower();
            if (!string.IsNullOrEmpty(ToCulture) &&
                EnableGoogleTranslate && CanGoogleTranslate)
            {
                (string FromCulture, string ToCulture) Key = ("en-us", ToCulture);
                if (!CacheTranslate.TryGetValue(Key, out ConcurrentDictionary<string, Lazy<string>> Caches))
                    Caches = CacheTranslate.GetOrAdd(Key, k => []);

                Lazy<string> LazyResult = Caches.GetOrAdd(Text, k => new Lazy<string>(() => InternalGoogleTranslate(k, "en-us", ToCulture)));
                Result = LazyResult.Value;
                if (!string.IsNullOrEmpty(Result))
                    return Result;
            }

            return Default ?? Text;
        }

        internal static readonly ConcurrentDictionary<(string, string), ConcurrentDictionary<string, Lazy<string>>> CacheTranslate = [];
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
            if (string.IsNullOrEmpty(Text))
                throw new ArgumentNullException(nameof(Text), "Text cannot be null or empty.");

            if (string.IsNullOrEmpty(FromCulture))
                throw new ArgumentNullException(nameof(FromCulture), "Text cannot be null or empty.");

            FromCulture = FromCulture.ToLower();
            if (!CultureHelper.ExistsCulture(FromCulture))
                throw new InvalidDataException($"[{nameof(GoogleTranslate)}] Invalid culture : {FromCulture}");

            if (string.IsNullOrEmpty(ToCulture))
                throw new ArgumentNullException(nameof(ToCulture), "Text cannot be null or empty.");

            ToCulture = ToCulture.ToLower();
            if (!CultureHelper.ExistsCulture(ToCulture))
                throw new InvalidDataException($"[{nameof(GoogleTranslate)}] Invalid culture : {ToCulture}");

            if (CanGoogleTranslate)
                throw new HttpRequestException($"[{nameof(GoogleTranslate)}] Google Translate is not available.");

            (string FromCulture, string ToCulture) Key = (FromCulture, ToCulture);
            if (!CacheTranslate.TryGetValue(Key, out ConcurrentDictionary<string, Lazy<string>> Caches))
                Caches = CacheTranslate.GetOrAdd(Key, k => []);

            Lazy<string> LazyResult = Caches.GetOrAdd(Text, k => new Lazy<string>(() => InternalGoogleTranslate(k, FromCulture, ToCulture)));
            return LazyResult.Value;
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
            if (string.IsNullOrEmpty(Text))
                throw new ArgumentNullException(nameof(Text), "Text cannot be null or empty.");

            if (string.IsNullOrEmpty(FromCulture))
                throw new ArgumentNullException(nameof(FromCulture), "Text cannot be null or empty.");

            FromCulture = FromCulture.ToLower();
            if (!CultureHelper.ExistsCulture(FromCulture))
                throw new InvalidDataException($"[{nameof(GoogleTranslate)}] Invalid culture : {FromCulture}");

            if (string.IsNullOrEmpty(ToCulture))
                throw new ArgumentNullException(nameof(ToCulture), "Text cannot be null or empty.");

            ToCulture = ToCulture.ToLower();
            if (!CultureHelper.ExistsCulture(ToCulture))
                throw new InvalidDataException($"[{nameof(GoogleTranslate)}] Invalid culture : {ToCulture}");

            if (CanGoogleTranslate)
                throw new HttpRequestException($"[{nameof(GoogleTranslate)}] Google Translate is not available.");

            (string FromCulture, string ToCulture) Key = (FromCulture, ToCulture);
            if (!CacheTranslate.TryGetValue(Key, out ConcurrentDictionary<string, Lazy<string>> Caches))
                Caches = CacheTranslate.GetOrAdd(Key, k => []);

            Lazy<string> LazyResult = Caches.GetOrAdd(Text, k => new Lazy<string>(() => InternalGoogleTranslate(k, FromCulture, ToCulture)));
            return LazyResult.IsValueCreated ? LazyResult.Value : await Task.Run(() => LazyResult.Value);
        }

        internal static string InternalGoogleTranslate(string Text, string FromCulture, string ToCulture)
        {
            string Url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl={FromCulture.ToLower()}&tl={ToCulture.ToLower()}&dt=t&q={Uri.EscapeDataString(Text)}";
            string Json = GetGoogleTranslateResult(Url).Result;
            if (string.IsNullOrEmpty(Json))
                return null;

            string Result = ParseGoogleTranslateResult(Json);
            if (string.IsNullOrEmpty(Result))
            {
                Debug.WriteLine($"[{nameof(InternalGoogleTranslate)}] Invalid search result : {Json}");
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