using MenthaAssembly.Globalization;
using MenthaAssembly.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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
                _Current = value;
                OnStaticPropertyChanged();
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

        public static string GetWindowsBuildInString(string Key, string CultureCode)
        {
            string Folder = Path.Combine(Environment.SystemDirectory, CultureCode);
            if (!Directory.Exists(Folder))
                return null;

            if (!TryGetBuildInStringInfo(Key, out string Filename, out uint Uid))
                return null;

            string FilePath = Path.Combine(Folder, Filename);
            return File.Exists(FilePath) ? LoadStringFromMui(FilePath, Uid) : null;
        }
        private static bool TryGetBuildInStringInfo(string Key, out string Filename, out uint Uid)
        {
            const string User32 = "user32.dll.mui",
                         Acppage = "acppage.dll.mui",
                         ActionCenter = "ActionCenter.dll.mui",
                         ActionCenterCPL = "ActionCenterCPL.dll.mui",
                         Audiodev = "audiodev.dll.mui",
                         Autoplay = "autoplay.dll.mui",
                         Avifil32 = "avifil32.dll.mui",
                         Azroleui = "azroleui.dll.mui",
                         BWContextHandler = "BWContextHandler.dll.mui",
                         Cmmon32 = "cmmon32.exe.mui",
                         DeviceCenter = "DeviceCenter.dll.mui",
                         DevicePairing = "DevicePairing.dll.mui",
                         DevicePairingFolder = "DevicePairingFolder.dll.mui",
                         Dot3gpui = "dot3gpui.dll.mui",
                         Dot3svc = "dot3svc.dll.mui",
                         Els = "els.dll.mui",
                         Eudcedit = "eudcedit.exe.mui",
                         Explorer = "explorer.exe.mui",
                         FirewallControlPanel = "FirewallControlPanel.dll.mui",
                         Fontext = "fontext.dll.mui",
                         GamePanel = "GamePanel.exe.mui",
                         Gpedit = "gpedit.dll.mui",
                         Hgcpl = "hgcpl.dll.mui",
                         Hidphone = "hidphone.tsp.mui",
                         Iac25_32 = "iac25_32.ax.mui",
                         Iertutil = "iertutil.dll.mui",
                         Ifmon = "ifmon.dll.mui",
                         Inetres = "inetres.dll.mui",
                         InkObjCore = "InkObjCore.dll.mui",
                         InstallService = "InstallService.dll.mui",
                         Joy = "joy.cpl.mui",
                         Jscript = "jscript.dll.mui",
                         LockAppBroker = "LockAppBroker.dll.mui",
                         MbaeApi = "MbaeApi.dll.mui",
                         Mdminst = "mdminst.dll.mui",
                         MFC40 = "MFC40.dll.mui",
                         Miutils = "miutils.dll.mui",
                         Mmc = "mmc.exe.mui",
                         Mmcbase = "mmcbase.dll.mui",
                         Mcndmgr = "mmcndmgr.dll.mui",
                         Modemui = "modemui.dll.mui",
                         Mshtmler = "mshtmler.dll.mui",
                         Msjint40 = "msjint40.dll.mui",
                         Mstask = "mstask.dll.mui",
                         Msvfw32 = "msvfw32.dll.mui",
                         NetworkExplorer = "NetworkExplorer.dll.mui",
                         Occache = "occache.dll.mui",
                         Odbcji32 = "odbcji32.dll.mui",
                         P2pnetsh = "p2pnetsh.dll.mui",
                         PhotoScreensaver = "PhotoScreensaver.scr.mui",
                         Qdvd = "qdvd.dll.mui",
                         Regedit = "regedit.exe.mui",
                         Sndvolsso = "sndvolsso.dll.mui",
                         Systeminfo = "systeminfo.exe.mui",
                         Tapi3 = "tapi3.dll.mui",
                         Tasklist = "Tasklist.exe.mui",
                         Themecpl = "themecpl.dll.mui",
                         Themeui = "themeui.dll.mui",
                         UIRibbon = "UIRibbon.dll.mui",
                         Vfwwdm32 = "vfwwdm32.dll.mui";

            (string Key, uint UID)? Value = Key.ToLower().Replace(' ', '\0') switch
            {
                #region Acppage
                "application" => (Acppage, 6004U),
                "system" => (Acppage, 2032U),
                "fast" => (Acppage, 2034U),
                "default" => (Acppage, 2038U),
                #endregion
                #region ActionCenter
                "backup" => (ActionCenter, 1000U),
                "details" => (ActionCenter, 1087U),
                "on" => (ActionCenter, 1308U),
                "off" => (ActionCenter, 1109U),
                "view" => (ActionCenter, 1302U),
                "download" => (ActionCenter, 1345U),
                "running" => (ActionCenter, 1706U),
                "repair" => (ActionCenter, 1751U),
                "ok" => (ActionCenter, 1901U),
                "restart" => (ActionCenter, 1919U),
                "signin" => (ActionCenter, 2003U),
                "signout" => (ActionCenter, 2008U),
                "verify" => (ActionCenter, 2038U),
                "install" => (ActionCenter, 2107U),
                #endregion
                #region ActionCenterCPL
                "recovery" => (ActionCenterCPL, 562U),
                "maintenance" => (ActionCenterCPL, 565U),
                #endregion
                #region Audiodev
                "media" => (Audiodev, 601U),
                "modified" => (Audiodev, 8980U),
                "status" => (Audiodev, 8981U),
                "title" => (Audiodev, 12288U),
                "artist" => (Audiodev, 12289U),
                "album" => (Audiodev, 12290U),
                "year" => (Audiodev, 12291U),
                "genre" => (Audiodev, 12292U),
                "lyrics" => (Audiodev, 12293U),
                "duration" => (Audiodev, 12544U),
                "bitrate" => (Audiodev, 12545U),
                "format" => (Audiodev, 12549U),
                "protected" => (Audiodev, 12800U),
                #endregion
                #region Autoplay
                "devices" => (Autoplay, 1107U),
                "dvds" => (Autoplay, 1118U),
                "cds" => (Autoplay, 1120U),
                "software" => (Autoplay, 1121U),
                #endregion
                #region Avifil32
                "video" => (Avifil32, 189U),
                "uncompressed" => (Avifil32, 193U),
                #endregion
                #region Azroleui
                "general" => (Azroleui, 16U),
                "task" => (Azroleui, 24U),
                "container" => (Azroleui, 25U),
                "scope" => (Azroleui, 26U),
                "groups" => (Azroleui, 28U),
                "operation" => (Azroleui, 41U),
                "definitions" => (Azroleui, 42U),
                "administrator" => (Azroleui, 103U),
                "reader" => (Azroleui, 104U),
                "group" => (Azroleui, 133U),
                #endregion
                #region BWContextHandler
                "authenticated" => (BWContextHandler, 1007U),
                "encrypted" => (BWContextHandler, 1009U),
                "bluetooth" => (BWContextHandler, 1011U),
                "upnp" => (BWContextHandler, 1022U),
                "wired" => (BWContextHandler, 1023U),
                "network" => (BWContextHandler, 1024U),
                "wifi" => (BWContextHandler, 1029U),
                "wlan" => (BWContextHandler, 1030U),
                "secure" => (BWContextHandler, 1035U),
                "standard" => (BWContextHandler, 1036U),
                #endregion
                #region Cmmon32
                "Disconnect" => (Cmmon32, 11064U),
                #endregion
                #region DeviceCenter
                "printers" => (DeviceCenter, 611U),
                "multimediadevices" => (DeviceCenter, 613U),
                #endregion
                #region DevicePairing
                "passcode" => (DevicePairing, 101U),
                "yes" => (DevicePairing, 4223U),
                "no" => (DevicePairing, 4225U),
                "add" => (DevicePairing, 4227U),
                "finish" => (DevicePairing, 4228U),
                "continue" => (DevicePairing, 4259U),
                #endregion
                #region DevicePairingFolder
                "address" => (DevicePairingFolder, 1202U),
                "paired" => (DevicePairingFolder, 1207U),
                "manufacturer" => (DevicePairingFolder, 1208U),
                "version" => (DevicePairingFolder, 1209U),
                #endregion
                #region Dot3gpui
                "invalid" => (Dot3gpui, 86U),
                "precedence" => (Dot3gpui, 109U),
                "transmit" => (Dot3gpui, 5014U),
                #endregion
                #region Dot3svc
                "ethernet" => (Dot3svc, 1200U),
                #endregion
                #region Els
                "name" => (Els, 130U),
                "type" => (Els, 131U),
                "description" => (Els, 132U),
                "size" => (Els, 133U),
                "date" => (Els, 141U),
                "time" => (Els, 142U),
                "source" => (Els, 143U),
                "category" => (Els, 144U),
                "event" => (Els, 145U),
                "information" => (Els, 204U),
                "warning" => (Els, 205U),
                "none" => (Els, 207U),
                "log" => (Els, 215U),
                "fileName" => (Els, 348U),
                "data" => (Els, 350U),
                "security" => (Els, 351U),
                "user" => (Els, 146U),
                "computer" => (Els, 147U),
                #endregion
                #region Eudcedit
                "edit" => (Eudcedit, 61239U),
                "reference" => (Eudcedit, 61240U),
                "all" => (Eudcedit, 61250U),
                "memory" => (Eudcedit, 61291U),
                #endregion
                #region Explorer
                "taskbar" => (Explorer, 518U),
                "start" => (Explorer, 578U),
                "restrictions" => (Explorer, 580U),
                "volume" => (Explorer, 600U),
                "power" => (Explorer, 602U),
                "microphone" => (Explorer, 603U),
                "run" => (Explorer, 722U),
                "back" => (Explorer, 906U),
                "pen" => (Explorer, 909U),
                "touchpad" => (Explorer, 910U),
                "people" => (Explorer, 912U),
                "desktop" => (Explorer, 22000U),
                #endregion
                #region FirewallControlPanel
                "browse" => (FirewallControlPanel, 55U),
                "cancel" => (FirewallControlPanel, 544U),
                "active" => (FirewallControlPanel, 557U),
                "oK" => (FirewallControlPanel, 543U),
                "domain" => (FirewallControlPanel, 1336U),
                "private" => (FirewallControlPanel, 1337U),
                "public" => (FirewallControlPanel, 1339U),
                "accessories" => (FirewallControlPanel, 1354U),
                "startup" => (FirewallControlPanel, 1355U),
                "unknown" => (FirewallControlPanel, 1350U),
                #endregion
                #region Fontext
                "fonts" => (Fontext, 209U),
                "stop" => (Fontext, 213U),
                "preview" => (Fontext, 1288U),
                "installable" => (Fontext, 1301U),
                "editable" => (Fontext, 1304U),
                "raster" => (Fontext, 1501U),
                "show" => (Fontext, 7000U),
                "hide" => (Fontext, 7001U),
                "mixed" => (Fontext, 7002U),
                "personalization" => (Fontext, 8005U),
                #endregion
                #region GamePanel
                "broadcasting" => (GamePanel, 136U),
                "loading" => (GamePanel, 139U),
                "broadcast" => (GamePanel, 152U),
                "settings" => (GamePanel, 203U),
                "move" => (GamePanel, 206U),
                "clips" => (GamePanel, 217U),
                "seconds" => (GamePanel, 223U),
                "minutes" => (GamePanel, 224U),
                "hour" => (GamePanel, 225U),
                "minute" => (GamePanel, 226U),
                "hours" => (GamePanel, 227U),
                "save" => (GamePanel, 263U),
                "reset" => (GamePanel, 264U),
                "shortcuts" => (GamePanel, 271U),
                "topleft" => (GamePanel, 382U),
                "topmiddle" => (GamePanel, 383U),
                "topright" => (GamePanel, 384U),
                "middleleft" => (GamePanel, 385U),
                "middleright" => (GamePanel, 386U),
                "bottomleft" => (GamePanel, 387U),
                "bottommiddle" => (GamePanel, 388U),
                "bottomright" => (GamePanel, 389U),
                "game" => (GamePanel, 455U),
                "mixer" => (GamePanel, 513U),
                #endregion
                #region Gpedit
                "logging" => (Gpedit, 21U),
                "planning" => (Gpedit, 22U),
                "revision" => (Gpedit, 23U),
                "applied" => (Gpedit, 48U),
                "selection" => (Gpedit, 56U),
                "filtering" => (Gpedit, 60U),
                "mode" => (Gpedit, 65U),
                "username" => (Gpedit, 66U),
                "sites" => (Gpedit, 154U),
                "computers" => (Gpedit, 155U),
                "users" => (Gpedit, 158U),
                "forest" => (Gpedit, 171U),
                "success" => (Gpedit, 227U),
                "failed" => (Gpedit, 228U),
                "inprogress" => (Gpedit, 229U),
                "replace" => (Gpedit, 293U),
                "merge" => (Gpedit, 294U),
                "pending" => (Gpedit, 310U),
                #endregion
                #region Hgcpl
                "addmember" => (Hgcpl, 200U),
                "usericon" => (Hgcpl, 201U),
                "fullname" => (Hgcpl, 202U),
                "userid" => (Hgcpl, 203U),
                "progressbar" => (Hgcpl, 204U),
                "foldericon" => (Hgcpl, 205U),
                "viewpassword" => (Hgcpl, 228U),
                #endregion
                #region Hidphone
                "flash" => (Hidphone, 116U),
                "hold" => (Hidphone, 117U),
                "redial" => (Hidphone, 118U),
                "transfer" => (Hidphone, 119U),
                "park" => (Hidphone, 121U),
                "forwardcalls" => (Hidphone, 122U),
                "line" => (Hidphone, 123U),
                "conference" => (Hidphone, 124U),
                "phonemute" => (Hidphone, 126U),
                "send" => (Hidphone, 129U),
                "volumeup" => (Hidphone, 130U),
                "volumedown" => (Hidphone, 131U),
                #endregion
                #region Iac25_32
                "controls" => (Iac25_32, 1002U),
                "about" => (Iac25_32, 1020U),
                #endregion
                #region Iertutil
                "abort" => (Iertutil, 257U),
                "tryagain" => (Iertutil, 261U),
                "retry" => (Iertutil, 263U),
                "ignore" => (Iertutil, 264U),
                #endregion
                #region Ifmon
                "enabled" => (Ifmon, 9001U),
                "disabled" => (Ifmon, 9002U),
                "connected" => (Ifmon, 9003U),
                "disconnected" => (Ifmon, 9004U),
                "connecting" => (Ifmon, 9005U),
                "client" => (Ifmon, 9006U),
                "homerouter" => (Ifmon, 9007U),
                "fullrouter" => (Ifmon, 9008U),
                "dedicated" => (Ifmon, 9009U),
                "internal" => (Ifmon, 9010U),
                "loopback" => (Ifmon, 9011U),
                "constant" => (Ifmon, 31002U),
                "closed" => (Ifmon, 32001U),
                "listen" => (Ifmon, 32002U),
                "established" => (Ifmon, 32005U),
                "closing" => (Ifmon, 32009U),
                "wait" => (Ifmon, 32011U),
                "dynamic" => (Ifmon, 32013U),
                "static" => (Ifmon, 33020U),
                "testing" => (Ifmon, 36003U),
                "unreachable" => (Ifmon, 36052U),
                "operational" => (Ifmon, 36056U),
                #endregion
                #region Inetres
                "subject" => (Inetres, 106U),
                "font" => (Inetres, 1175U),
                "unblock" => (Inetres, 1338U),
                "alignleft" => (Inetres, 5232U),
                "center" => (Inetres, 5233U),
                "alignright" => (Inetres, 5234U),
                "justify" => (Inetres, 5235U),
                "fontcolor" => (Inetres, 5273U),
                #endregion
                #region InkObjCore
                "ink" => (InkObjCore, 503U),
                "textink" => (InkObjCore, 504U),
                "drawing" => (InkObjCore, 507U),
                #endregion
                #region InstallService
                "launch" => (InstallService, 13001U),
                #endregion
                #region Joy
                "id" => (Joy, 1042U),
                "controller" => (Joy, 1151U),
                "port" => (Joy, 1177U),
                "removecontroller" => (Joy, 1254U),
                "renamedevice" => (Joy, 40036U),
                "adddevice" => (Joy, 40038U),
                "changeid" => (Joy, 40040U),
                #endregion
                #region Jscript
                "overflow" => (Jscript, 6U),
                "outofmemory" => (Jscript, 7U),
                "errorinloadingdll" => (Jscript, 48U),
                "internalerror" => (Jscript, 51U),
                "filenotfound" => (Jscript, 53U),
                "filealreadyopen" => (Jscript, 55U),
                "filealreadyexists" => (Jscript, 58U),
                "diskfull" => (Jscript, 61U),
                "permissiondenied" => (Jscript, 70U),
                "pathnotfound" => (Jscript, 76U),
                "syntaxerror" => (Jscript, 1002U),
                "infinity" => (Jscript, 6000U),
                "-infinity" => (Jscript, 6001U),
                #endregion
                #region LockAppBroker
                "activity" => (LockAppBroker, 4800U),
                "alert" => (LockAppBroker, 4810U),
                "available" => (LockAppBroker, 4820U),
                "away" => (LockAppBroker, 4830U),
                "busy" => (LockAppBroker, 4840U),
                "newmessage" => (LockAppBroker, 4850U),
                "paused" => (LockAppBroker, 4860U),
                "playing" => (LockAppBroker, 4870U),
                "unavailable" => (LockAppBroker, 4880U),
                "attention" => (LockAppBroker, 4910U),
                "alarm" => (LockAppBroker, 4930U),
                #endregion
                #region MbaeApi
                "unnamed" => (MbaeApi, 300U),
                "cellular" => (MbaeApi, 500U),
                #endregion
                #region Mdminst
                "alldevices" => (Mdminst, 3108U),
                "modems" => (Mdminst, 14100U),
                #endregion
                #region MFC40
                "saveas" => (MFC40, 61441U),
                "untitled" => (MFC40, 61443U),
                "savecopyas" => (MFC40, 61444U),
                "pixels" => (MFC40, 61888U),
                "highlight" => (MFC40, 65047U),
                "highlightedtext" => (MFC40, 65048U),
                "bitmap" => (MFC40, 65060U),
                "metafile" => (MFC40, 65061U),
                "icon" => (MFC40, 65062U),
                "pictures" => (MFC40, 65069U),
                #endregion
                #region Miutils
                "communicationserror" => (Miutils, 3U),
                "softwareerror" => (Miutils, 5U),
                "hardwareerror" => (Miutils, 6U),
                "environmentalerror" => (Miutils, 7U),
                "securityerror" => (Miutils, 8U),
                "minor" => (Miutils, 15U),
                "major" => (Miutils, 16U),
                "critical" => (Miutils, 17U),
                "congestion" => (Miutils, 26U),
                "receivefailure" => (Miutils, 58U),
                "timeout" => (Miutils, 127U),
                #endregion
                #region Mmc
                "moreactions" => (Mmc, 130U),
                "actions" => (Mmc, 131U),
                "selecteditem" => (Mmc, 133U),
                "restore" => (Mmc, 209U),
                #endregion
                #region Mmcbase
                "question" => (Mmcbase, 5U),
                "body" => (Mmcbase, 105U),
                "copyhere" => (Mmcbase, 13417U),
                "movehere" => (Mmcbase, 13418U),
                "selecteditems" => (Mmcbase, 13433U),
                "copy" => (Mmcbase, 13438U),
                "paste" => (Mmcbase, 13439U),
                "delete" => (Mmcbase, 13440U),
                "properties" => (Mmcbase, 13441U),
                "rename" => (Mmcbase, 13442U),
                "refresh" => (Mmcbase, 13443U),
                "print" => (Mmcbase, 13444U),
                "cut" => (Mmcbase, 13445U),
                "folder" => (Mmcbase, 14008U),
                "shortcut" => (Mmcbase, 14011U),
                "enumerated" => (Mmcbase, 14012U),
                "vendor" => (Mmcbase, 14017U),
                "taskpads" => (Mmcbase, 14087U),
                "minimized" => (Mmcbase, 14112U),
                "maximized" => (Mmcbase, 14113U),
                "taskicon" => (Mmcbase, 14148U),
                "commandtype" => (Mmcbase, 14150U),
                "small" => (Mmcbase, 14152U),
                "medium" => (Mmcbase, 14153U),
                "large" => (Mmcbase, 14154U),
                "navigation" => (Mmcbase, 14157U),
                "extended" => (Mmcbase, 14175U),
                #endregion
                #region Mcndmgr
                "button" => (Mcndmgr, 30013U),
                "compress" => (Mcndmgr, 30022U),
                "database" => (Mcndmgr, 30028U),
                "clock" => (Mcndmgr, 30030U),
                "harddrive" => (Mcndmgr, 30033U),
                "email" => (Mcndmgr, 30036U),
                "fax" => (Mcndmgr, 30040U),
                "folders" => (Mcndmgr, 30048U),
                "hardware" => (Mcndmgr, 30054U),
                "laptop" => (Mcndmgr, 30056U),
                "Internet" => (Mcndmgr, 30058U),
                "modem" => (Mcndmgr, 30060U),
                "phone" => (Mcndmgr, 30061U),
                "play" => (Mcndmgr, 30066U),
                "password" => (Mcndmgr, 30068U),
                "pause" => (Mcndmgr, 30070U),
                "printer" => (Mcndmgr, 30074U),
                "publish" => (Mcndmgr, 30078U),
                "reports" => (Mcndmgr, 30082U),
                "lock" => (Mcndmgr, 30092U),
                "monitor" => (Mcndmgr, 30096U),
                "arrow" => (Mcndmgr, 30098U),
                "mouse" => (Mcndmgr, 30101U),
                "table" => (Mcndmgr, 30105U),
                "calendar" => (Mcndmgr, 30106U),
                "upload" => (Mcndmgr, 30114U),
                "person" => (Mcndmgr, 30117U),
                "setup" => (Mcndmgr, 30124U),
                "message" => (Mcndmgr, 30127U),
                "inventory" => (Mcndmgr, 30136U),
                "schedule" => (Mcndmgr, 30140U),
                "bell" => (Mcndmgr, 30148U),
                "grouped" => (Mcndmgr, 30150U),
                "server" => (Mcndmgr, 30154U),
                "services" => (Mcndmgr, 30166U),
                "code" => (Mcndmgr, 30174U),
                "import" => (Mcndmgr, 30178U),
                "export" => (Mcndmgr, 30180U),
                "searchdatabase" => (Mcndmgr, 30182U),
                "book" => (Mcndmgr, 30184U),
                "publication" => (Mcndmgr, 30185U),
                "databases" => (Mcndmgr, 30186U),
                "inbox" => (Mcndmgr, 30199U),
                "finished" => (Mcndmgr, 30201U),
                "switchoff" => (Mcndmgr, 30204U),
                "switchon" => (Mcndmgr, 30206U),
                "networkconnection" => (Mcndmgr, 30211U),
                "accessibility" => (Mcndmgr, 30213U),
                "briefcase" => (Mcndmgr, 30215U),
                "camera" => (Mcndmgr, 30217U),
                "certificate" => (Mcndmgr, 30219U),
                "license" => (Mcndmgr, 30220U),
                "component" => (Mcndmgr, 30221U),
                "puzzle" => (Mcndmgr, 30222U),
                "expenses" => (Mcndmgr, 30223U),
                "money" => (Mcndmgr, 30224U),
                "home" => (Mcndmgr, 30225U),
                "house" => (Mcndmgr, 30226U),
                "midi" => (Mcndmgr, 30227U),
                "networkaccess" => (Mcndmgr, 30229U),
                "new" => (Mcndmgr, 30231U),
                "news" => (Mcndmgr, 30233U),
                "site" => (Mcndmgr, 30236U),
                "building" => (Mcndmgr, 30237U),
                "mainframe" => (Mcndmgr, 30240U),
                "shakinghands" => (Mcndmgr, 30241U),
                "wizard" => (Mcndmgr, 30244U),
                "checkserver" => (Mcndmgr, 30250U),
                "cluster" => (Mcndmgr, 30254U),
                "cubes" => (Mcndmgr, 30256U),
                "cube" => (Mcndmgr, 30260U),
                "gear" => (Mcndmgr, 30262U),
                "register" => (Mcndmgr, 30264U),
                #endregion
                #region Modemui
                "even" => (Modemui, 250U),
                "odd" => (Modemui, 251U),
                "mark" => (Modemui, 253U),
                "low" => (Modemui, 264U),
                "unspecified" => (Modemui, 300U),
                "voice" => (Modemui, 303U),
                "noresponse" => (Modemui, 3207U),
                "command" => (Modemui, 3212U),
                "response" => (Modemui, 3213U),
                "updating" => (Modemui, 3222U),
                "field" => (Modemui, 3238U),
                "value" => (Modemui, 3239U),
                "japan" => (Modemui, 6144U),
                "albania" => (Modemui, 6145U),
                "algeria" => (Modemui, 6146U),
                "americansamoa" => (Modemui, 6147U),
                "anguilla" => (Modemui, 6149U),
                "argentina" => (Modemui, 6151U),
                "ascensionisland" => (Modemui, 6152U),
                "australia" => (Modemui, 6153U),
                "austria" => (Modemui, 6154U),
                "bahamas" => (Modemui, 6155U),
                "bahrain" => (Modemui, 6156U),
                "bangladesh" => (Modemui, 6157U),
                "barbados" => (Modemui, 6158U),
                "belgium" => (Modemui, 6159U),
                "belize" => (Modemui, 6160U),
                "benin" => (Modemui, 6161U),
                "bermuda" => (Modemui, 6162U),
                "bhutan" => (Modemui, 6163U),
                "bolivia" => (Modemui, 6164U),
                "botswana" => (Modemui, 6165U),
                "brazil" => (Modemui, 6166U),
                "brunei" => (Modemui, 6170U),
                "bulgaria" => (Modemui, 6171U),
                "myanmar" => (Modemui, 6172U),
                "burundi" => (Modemui, 6173U),
                "belarus" => (Modemui, 6174U),
                "cameroon" => (Modemui, 6175U),
                "northkorea" => (Modemui, 6192U),
                "denmark" => (Modemui, 6193U),
                "djibouti" => (Modemui, 6194U),
                "dominica" => (Modemui, 6196U),
                "ecuador" => (Modemui, 6197U),
                "egypt" => (Modemui, 6198U),
                "elsalvador" => (Modemui, 6199U),
                "equatorialguinea" => (Modemui, 6200U),
                "ethiopia" => (Modemui, 6201U),
                "fijiIslands" => (Modemui, 6203U),
                "finland" => (Modemui, 6204U),
                "france" => (Modemui, 6205U),
                "frenchpolynesia" => (Modemui, 6206U),
                "gabon" => (Modemui, 6208U),
                "gambia" => (Modemui, 6209U),
                "angola" => (Modemui, 6211U),
                "ghana" => (Modemui, 6212U),
                "gibraltar" => (Modemui, 6213U),
                "greece" => (Modemui, 6214U),
                "grenada" => (Modemui, 6215U),
                "guam" => (Modemui, 6216U),
                "guatemala" => (Modemui, 6217U),
                "guernsey" => (Modemui, 6218U),
                "guinea" => (Modemui, 6219U),
                "guyana" => (Modemui, 6221U),
                "haiti" => (Modemui, 6222U),
                "honduras" => (Modemui, 6223U),
                "hongKong" => (Modemui, 6224U),
                "hungary" => (Modemui, 6225U),
                "iceland" => (Modemui, 6226U),
                "india" => (Modemui, 6227U),
                "indonesia" => (Modemui, 6228U),
                "iran" => (Modemui, 6229U),
                "iraq" => (Modemui, 6230U),
                "ireland" => (Modemui, 6231U),
                "israel" => (Modemui, 6232U),
                "italy" => (Modemui, 6233U),
                "jamaica" => (Modemui, 6235U),
                "afghanistan" => (Modemui, 6236U),
                "jersey" => (Modemui, 6237U),
                "jordan" => (Modemui, 6238U),
                "kenya" => (Modemui, 6239U),
                "kiribati" => (Modemui, 6240U),
                "korea" => (Modemui, 6241U),
                "kuwait" => (Modemui, 6242U),
                "laos" => (Modemui, 6243U),
                "lebanon" => (Modemui, 6244U),
                "lesotho" => (Modemui, 6245U),
                "liberia" => (Modemui, 6246U),
                "libya" => (Modemui, 6247U),
                "liechtenstein" => (Modemui, 6248U),
                "luxembourg" => (Modemui, 6249U),
                "macao" => (Modemui, 6250U),
                "madagascar" => (Modemui, 6251U),
                "malaysia" => (Modemui, 6252U),
                "malawi" => (Modemui, 6253U),
                "maldives" => (Modemui, 6254U),
                "mali" => (Modemui, 6255U),
                "malta" => (Modemui, 6256U),
                "mauritania" => (Modemui, 6257U),
                "mauritius" => (Modemui, 6258U),
                "mexico" => (Modemui, 6259U),
                "monaco" => (Modemui, 6260U),
                "mongolia" => (Modemui, 6261U),
                "montserrat" => (Modemui, 6262U),
                "morocco" => (Modemui, 6263U),
                "mozambique" => (Modemui, 6264U),
                "nauru" => (Modemui, 6265U),
                "nepal" => (Modemui, 6266U),
                "netherlands" => (Modemui, 6267U),
                "newcaledonia" => (Modemui, 6269U),
                "newzealand" => (Modemui, 6270U),
                "nicaragua" => (Modemui, 6271U),
                "niger" => (Modemui, 6272U),
                "nigeria" => (Modemui, 6273U),
                "norway" => (Modemui, 6274U),
                "oman" => (Modemui, 6275U),
                "pakistan" => (Modemui, 6276U),
                "panama" => (Modemui, 6277U),
                "papuanewguinea" => (Modemui, 6278U),
                "paraguay" => (Modemui, 6279U),
                "peru" => (Modemui, 6280U),
                "philippines" => (Modemui, 6281U),
                "poland" => (Modemui, 6282U),
                "portugal" => (Modemui, 6283U),
                "puertorico" => (Modemui, 6284U),
                "qatar" => (Modemui, 6285U),
                "romania" => (Modemui, 6286U),
                "rwanda" => (Modemui, 6287U),
                "sanmarino" => (Modemui, 6292U),
                "saudiarabia" => (Modemui, 6296U),
                "senegal" => (Modemui, 6297U),
                "seychelles" => (Modemui, 6298U),
                "sierraleone" => (Modemui, 6299U),
                "singapore" => (Modemui, 6300U),
                "solomonislands" => (Modemui, 6301U),
                "somalia" => (Modemui, 6302U),
                "southafrica" => (Modemui, 6303U),
                "spain" => (Modemui, 6304U),
                "srilanka" => (Modemui, 6305U),
                "sudan" => (Modemui, 6306U),
                "suriname" => (Modemui, 6307U),
                "swaziland" => (Modemui, 6308U),
                "sweden" => (Modemui, 6309U),
                "switzerland" => (Modemui, 6310U),
                "syria" => (Modemui, 6311U),
                "tanzania" => (Modemui, 6312U),
                "thailand" => (Modemui, 6313U),
                "togo" => (Modemui, 6314U),
                "tonga" => (Modemui, 6315U),
                "tunisia" => (Modemui, 6317U),
                "turkey" => (Modemui, 6318U),
                "tuvalu" => (Modemui, 6320U),
                "uganda" => (Modemui, 6321U),
                "ukraine" => (Modemui, 6322U),
                "unitedarabemirates" => (Modemui, 6323U),
                "unitedkingdom" => (Modemui, 6324U),
                "unitedstates" => (Modemui, 6325U),
                "burkinafaso" => (Modemui, 6326U),
                "uruguay" => (Modemui, 6327U),
                "vanuatu" => (Modemui, 6329U),
                "vatican" => (Modemui, 6330U),
                "venezuela" => (Modemui, 6331U),
                "vietnam" => (Modemui, 6332U),
                "samoa" => (Modemui, 6334U),
                "yemen" => (Modemui, 6335U),
                "congo" => (Modemui, 6338U),
                "zambia" => (Modemui, 6339U),
                "zimbabwe" => (Modemui, 6340U),
                "Marshallislands" => (Modemui, 6355U),
                "Micronesia" => (Modemui, 6356U),
                "Tokelau" => (Modemui, 6357U),
                "niue" => (Modemui, 6358U),
                "palau" => (Modemui, 6359U),
                "norfolkisland" => (Modemui, 6360U),
                "christmasisland" => (Modemui, 6361U),
                "tinianisland" => (Modemui, 6362U),
                "rotaisland" => (Modemui, 6363U),
                "saipan" => (Modemui, 6364U),
                "cocosislands" => (Modemui, 6365U),
                "martinique" => (Modemui, 6366U),
                "frenchguiana" => (Modemui, 6367U),
                "frenchantilles" => (Modemui, 6368U),
                "guadeloupe" => (Modemui, 6369U),
                "guantanamobay" => (Modemui, 6370U),
                "easttimor" => (Modemui, 6374U),
                "andorra" => (Modemui, 6375U),
                "moldova" => (Modemui, 6376U),
                "montenegro" => (Modemui, 6378U),
                "uzbekistan" => (Modemui, 6379U),
                "greenland" => (Modemui, 6380U),
                "faroeislands" => (Modemui, 6381U),
                "aruba" => (Modemui, 6382U),
                "eritrea" => (Modemui, 6383U),
                "mayotte" => (Modemui, 6384U),
                "namibia" => (Modemui, 6385U),
                "reunionisland" => (Modemui, 6386U),
                "diegogarcia" => (Modemui, 6388U),
                "slovakia" => (Modemui, 6395U),
                "taiwan" => (Modemui, 6398U),
                "sintmaarten" => (Modemui, 6399U),
                "estonia" => (Modemui, 6502U),
                "lithuania" => (Modemui, 6503U),
                "armenia" => (Modemui, 6504U),
                "georgia" => (Modemui, 6505U),
                "azerbaijan" => (Modemui, 6506U),
                "turkmenistan" => (Modemui, 6507U),
                "kazakhstan" => (Modemui, 6509U),
                "tajikistan" => (Modemui, 6510U),
                "kyrgyzstan" => (Modemui, 6511U),
                "latvia" => (Modemui, 6512U),
                "russia" => (Modemui, 6513U),
                "croatia" => (Modemui, 6600U),
                "slovenia" => (Modemui, 6601U),
                "serbia" => (Modemui, 6604U),
                "hardwareid" => (Modemui, 20013U),
                #endregion
                #region Mshtmler
                "normal" => (Mshtmler, 1000U),
                "formatted" => (Mshtmler, 1001U),
                "drop" => (Mshtmler, 2009U),
                "definition" => (Mshtmler, 1014U),
                "paragraph" => (Mshtmler, 1016U),
                "resize" => (Mshtmler, 2008U),
                "alignment" => (Mshtmler, 2010U),
                "centering" => (Mshtmler, 2011U),
                "spacing" => (Mshtmler, 2013U),
                "arrange" => (Mshtmler, 2014U),
                "typing" => (Mshtmler, 2016U),
                "overwrite" => (Mshtmler, 2023U),
                #endregion
                #region Msjint40
                "failure" => (Msjint40, 6070U),
                "design" => (Msjint40, 6133U),
                "createtable" => (Msjint40, 6137U),
                "deletetable" => (Msjint40, 6138U),
                "renametable" => (Msjint40, 6139U),
                "addcolumn" => (Msjint40, 6140U),
                "setcolumninformation" => (Msjint40, 6141U),
                "deletecolumn" => (Msjint40, 6142U),
                "renamecolumn" => (Msjint40, 6143U),
                "createindex" => (Msjint40, 6144U),
                "setindexinformation" => (Msjint40, 6145U),
                "deleteindex" => (Msjint40, 6146U),
                "renameindex" => (Msjint40, 6147U),
                "setproperty" => (Msjint40, 6148U),
                "createquery" => (Msjint40, 6149U),
                "createobject" => (Msjint40, 6150U),
                "deleteobject" => (Msjint40, 6151U),
                "renameobject" => (Msjint40, 6152U),
                "setobjectcolumn" => (Msjint40, 6153U),
                "setowner" => (Msjint40, 6154U),
                "setaccess" => (Msjint40, 6155U),
                "createrelationship" => (Msjint40, 6156U),
                "deleterelationship" => (Msjint40, 6157U),
                "createreference" => (Msjint40, 6158U),
                "deletereference" => (Msjint40, 6159U),
                "renamereference" => (Msjint40, 6160U),
                "insert" => (Msjint40, 6164U),
                "update" => (Msjint40, 6165U),
                "synchronizer" => (Msjint40, 6169U),
                "extract" => (Msjint40, 10003U),
                "criteria" => (Msjint40, 10005U),
                "true" => (Msjint40, 10020U),
                "false" => (Msjint40, 10021U),
                "jan" => (Msjint40, 10022U),
                "feb" => (Msjint40, 10023U),
                "mar" => (Msjint40, 10024U),
                "apr" => (Msjint40, 10025U),
                "may" => (Msjint40, 10026U),
                "jun" => (Msjint40, 10027U),
                "jul" => (Msjint40, 10028U),
                "aug" => (Msjint40, 10029U),
                "sep" => (Msjint40, 10030U),
                "oct" => (Msjint40, 10031U),
                "nov" => (Msjint40, 10032U),
                "dec" => (Msjint40, 10033U),
                "january" => (Msjint40, 10034U),
                "february" => (Msjint40, 10035U),
                "march" => (Msjint40, 10036U),
                "april" => (Msjint40, 10037U),
                "june" => (Msjint40, 10039U),
                "july" => (Msjint40, 10040U),
                "august" => (Msjint40, 10041U),
                "september" => (Msjint40, 10042U),
                "october" => (Msjint40, 10043U),
                "november" => (Msjint40, 10044U),
                "december" => (Msjint40, 10045U),
                "sunday" => (Msjint40, 10046U),
                "monday" => (Msjint40, 10047U),
                "tuesday" => (Msjint40, 10048U),
                "wednesday" => (Msjint40, 10049U),
                "thursday" => (Msjint40, 10050U),
                "friday" => (Msjint40, 10051U),
                "saturday" => (Msjint40, 10052U),
                "sun" => (Msjint40, 10053U),
                "mon" => (Msjint40, 10054U),
                "tue" => (Msjint40, 10055U),
                "wed" => (Msjint40, 10056U),
                "ths" => (Msjint40, 10057U),
                "fri" => (Msjint40, 10058U),
                "sat" => (Msjint40, 10059U),
                "row" => (Msjint40, 10064U),
                "importance" => (Msjint40, 10105U),
                "priority" => (Msjint40, 10107U),
                "sensitivity" => (Msjint40, 10108U),
                "from" => (Msjint40, 10110U),
                "received" => (Msjint40, 10116U),
                "Body" => (Msjint40, 10120U),
                "creationtime" => (Msjint40, 10121U),
                "lastmodificationtime" => (Msjint40, 10122U),
                "access" => (Msjint40, 10127U),
                "depth" => (Msjint40, 10136U),
                "notes" => (Msjint40, 10142U),
                "alias" => (Msjint40, 10146U),
                "initials" => (Msjint40, 10150U),
                "company" => (Msjint40, 10153U),
                "department" => (Msjint40, 10155U),
                "office" => (Msjint40, 10156U),
                "usercertificate" => (Msjint40, 10161U),
                "faxnumber" => (Msjint40, 10162U),
                "country" => (Msjint40, 10163U),
                "city" => (Msjint40, 10164U),
                "state" => (Msjint40, 10165U),
                "zipcode" => (Msjint40, 10167U),
                "assistant" => (Msjint40, 10172U),
                "primarycapability" => (Msjint40, 10174U),
                "primary" => (Msjint40, 10175U),
                "journal" => (Msjint40, 10181U),
                "webpage" => (Msjint40, 10182U),
                "teamtask" => (Msjint40, 10195U),
                "startdate" => (Msjint40, 10196U),
                "duedate" => (Msjint40, 10197U),
                "datecompleted" => (Msjint40, 10198U),
                "actualwork" => (Msjint40, 10199U),
                "totalwork" => (Msjint40, 10200U),
                "complete" => (Msjint40, 10201U),
                "owner" => (Msjint40, 10202U),
                "recurring" => (Msjint40, 10205U),
                "role" => (Msjint40, 10206U),
                "salutation" => (Msjint40, 10213U),
                "created" => (Msjint40, 10216U),
                "contents" => (Msjint40, 10217U),
                "width" => (Msjint40, 10231U),
                "height" => (Msjint40, 10232U),
                "content" => (Msjint40, 10235U),
                "keywords" => (Msjint40, 10236U),
                "set" => (Msjint40, 10254U),
                "age" => (Msjint40, 10258U),
                "attachements" => (Msjint40, 10261U),
                "end" => (Msjint40, 10264U),
                "mileage" => (Msjint40, 10268U),
                "billing" => (Msjint40, 10269U),
                "companies" => (Msjint40, 10273U),
                "contacts" => (Msjint40, 10274U),
                "contact" => (Msjint40, 10276U),
                "recalltime" => (Msjint40, 10277U),
                "currentversion" => (Msjint40, 10280U),
                "nexttime" => (Msjint40, 10282U),
                "deleted" => (Msjint40, 10283U),
                "account" => (Msjint40, 10285U),
                "children" => (Msjint40, 10292U),
                "sent" => (Msjint40, 10331U),
                #endregion
                #region Mstask
                "suspended" => (Mstask, 2U),
                "creator" => (Mstask, 106U),
                "high" => (Mstask, 128U),
                "missed" => (Mstask, 166U),
                "programs" => (Mstask, 328U),
                "daily" => (Mstask, 1067U),
                "weekly" => (Mstask, 1068U),
                "monthly" => (Mstask, 1069U),
                "every" => (Mstask, 1070U),
                "everyother" => (Mstask, 1071U),
                "everythird" => (Mstask, 1072U),
                "everyfourth" => (Mstask, 1073U),
                "everyfifth" => (Mstask, 1074U),
                "everysixth" => (Mstask, 1075U),
                "everytwelfth" => (Mstask, 1076U),
                "first" => (Mstask, 1079U),
                "second" => (Mstask, 1080U),
                "third" => (Mstask, 1081U),
                "fourth" => (Mstask, 1082U),
                "last" => (Mstask, 1083U),
                "taskscheduler" => (Mstask, 1085U),
                "scheduledtasks" => (Mstask, 3408U),
                "once" => (Mstask, 4101U),
                "atsystemstartup" => (Mstask, 4113U),
                "atlogon" => (Mstask, 4114U),
                #endregion
                #region Msvfw32
                "commandmenu" => (Msvfw32, 107U),
                "eject" => (Msvfw32, 108U),
                "allfiles" => (Msvfw32, 334U),
                "nodevice" => (Msvfw32, 336U),
                "record" => (Msvfw32, 2063U),
                #endregion
                #region NetworkExplorer
                "notconnected" => (NetworkExplorer, 1009U),
                "datecreated" => (NetworkExplorer, 1012U),
                "dateconnected" => (NetworkExplorer, 1013U),
                "networklocation" => (NetworkExplorer, 1023U),
                "workgroup" => (NetworkExplorer, 1032U),
                "macaddress" => (NetworkExplorer, 1052U),
                "ipaddress" => (NetworkExplorer, 1053U),
                #endregion
                #region Occache
                "programfile" => (Occache, 0U),
                "totalsize" => (Occache, 2U),
                "creationdate" => (Occache, 3U),
                "lastaccessed" => (Occache, 4U),
                "installed" => (Occache, 9U),
                "shared" => (Occache, 10U),
                "damaged" => (Occache, 11U),
                "packagename" => (Occache, 40U),
                "unplugged" => (Occache, 43U),
                "namespace" => (Occache, 46U),
                #endregion
                #region Odbcji32
                "workbook" => (Odbcji32, 44076U),
                "directory" => (Odbcji32, 44077U),
                "international" => (Odbcji32, 44571U),
                "csvdelimited" => (Odbcji32, 44700U),
                "tabdelimited" => (Odbcji32, 44701U),
                "customdelimited" => (Odbcji32, 44702U),
                "fixedlength" => (Odbcji32, 44703U),
                #endregion
                #region P2pnetsh
                "suspicious" => (P2pnetsh, 7427U),
                "virtual" => (P2pnetsh, 7502U),
                "synchronizing" => (P2pnetsh, 7503U),
                "alone" => (P2pnetsh, 7508U),
                "notused" => (P2pnetsh, 7511U),
                "connectfailed" => (P2pnetsh, 7514U),
                "synchronized" => (P2pnetsh, 7515U),
                "used" => (P2pnetsh, 7517U),
                "valid" => (P2pnetsh, 7613U),
                "expired" => (P2pnetsh, 7615U),
                #endregion
                #region PhotoScreensaver
                "photos" => (PhotoScreensaver, 1U),
                "random" => (PhotoScreensaver, 126U),
                "next" => (PhotoScreensaver, 11035U),
                "previous" => (PhotoScreensaver, 11036U),
                "exit" => (PhotoScreensaver, 11024U),
                "shuffle" => (PhotoScreensaver, 11025U),
                "loop" => (PhotoScreensaver, 11026U),
                "slow" => (PhotoScreensaver, 11027U),
                "mute" => (PhotoScreensaver, 11048U),
                #endregion
                #region Qdvd
                "position" => (Qdvd, 3005U),
                "config" => (Qdvd, 3006U),
                "quality" => (Qdvd, 3008U),
                #endregion
                #region Regedit
                "registryeditor" => (Regedit, 16U),
                "createlink" => (Regedit, 7001U),
                "queryvalue" => (Regedit, 7002U),
                "setvalue" => (Regedit, 7003U),
                "notify" => (Regedit, 7005U),
                "read" => (Regedit, 7011U),
                "latched" => (Regedit, 7521U),
                "readonly" => (Regedit, 7531U),
                "writeonly" => (Regedit, 7532U),
                #endregion
                #region Sndvolsso
                "stereo" => (Sndvolsso, 401U),
                "allow" => (Sndvolsso, 2014U),
                #endregion
                #region Systeminfo
                "hostname" => (Systeminfo, 151U),
                "timezone" => (Systeminfo, 172U),
                #endregion
                #region Tapi3
                "terminal" => (Tapi3, 113U),
                "audioin" => (Tapi3, 117U),
                "audioout" => (Tapi3, 118U),
                #endregion
                #region Tasklist
                "modules" => (Tasklist, 112U),
                #endregion
                #region Themecpl
                "custom" => (Themecpl, 18U),
                "highcontrast" => (Themecpl, 20U),
                "slideshow" => (Themecpl, 21U),
                "solidcolor" => (Themecpl, 22U),
                "textures" => (Themecpl, 67U),
                "vistas" => (Themecpl, 69U),
                "lightauras" => (Themecpl, 71U),
                "solidcolors" => (Themecpl, 72U),
                "nature" => (Themecpl, 73U),
                "display" => (Themecpl, 81U),
                "span" => (Themecpl, 503U),
                "fit" => (Themecpl, 504U),
                "fill" => (Themecpl, 505U),
                "stretch" => (Themecpl, 506U),
                "tile" => (Themecpl, 507U),
                "bloom" => (Themecpl, 633U),
                "color" => (Themecpl, 1108U),
                "sounds" => (Themecpl, 1110U),
                "colorintensity" => (Themecpl, 1126U),
                "apply" => (Themecpl, 1190U),
                "foreground" => (Themecpl, 1192U),
                "background" => (Themecpl, 1193U),
                #endregion
                #region Themeui
                "window" => (Themeui, 1408U),
                "scrollbar" => (Themeui, 1409U),
                "3dobjects" => (Themeui, 1410U),
                "messagebox" => (Themeui, 1415U),
                "tooltip" => (Themeui, 1422U),
                "flowers" => (Themeui, 2112U),
                "sunset" => (Themeui, 2115U),
                "sunrise" => (Themeui, 2116U),
                "vibrant" => (Themeui, 2117U),
                "colorful" => (Themeui, 2119U),
                "flux" => (Themeui, 2121U),
                "glow" => (Themeui, 2122U),
                "energy" => (Themeui, 2123U),
                "create" => (Themeui, 2124U),
                "make" => (Themeui, 2125U),
                "organic" => (Themeui, 2127U),
                "bubbles" => (Themeui, 2201U),
                "mystify" => (Themeui, 2203U),
                "ribbons" => (Themeui, 2205U),
                "blank" => (Themeui, 2206U),
                #endregion
                #region UIRibbon
                "rowup" => (UIRibbon, 38U),
                "rowdown" => (UIRibbon, 39U),
                "auto" => (UIRibbon, 43U),
                "press" => (UIRibbon, 45U),
                "execute" => (UIRibbon, 46U),
                "click" => (UIRibbon, 47U),
                "doubleclick" => (UIRibbon, 48U),
                "open" => (UIRibbon, 49U),
                "select" => (UIRibbon, 51U),
                "expand" => (UIRibbon, 52U),
                "collapse" => (UIRibbon, 53U),
                "switch" => (UIRibbon, 54U),
                "check" => (UIRibbon, 55U),
                "uncheck" => (UIRibbon, 56U),
                "toggle" => (UIRibbon, 57U),
                "enter" => (UIRibbon, 60U),
                "space" => (UIRibbon, 61U),
                "more" => (UIRibbon, 64U),
                "less" => (UIRibbon, 65U),
                "separator" => (UIRibbon, 72U),
                "vertical" => (UIRibbon, 96U),
                "horizontal" => (UIRibbon, 97U),
                "zoomout" => (UIRibbon, 108U),
                "zoomin" => (UIRibbon, 109U),
                "categories" => (UIRibbon, 110U),
                "automatic" => (UIRibbon, 123U),
                "nofill" => (UIRibbon, 125U),
                "nocolor" => (UIRibbon, 126U),
                "noline" => (UIRibbon, 128U),
                "noshadow" => (UIRibbon, 129U),
                "usedefault" => (UIRibbon, 143U),
                "themecolors" => (UIRibbon, 146U),
                "customcolors" => (UIRibbon, 147U),
                "colors" => (UIRibbon, 152U),
                "standardcolors" => (UIRibbon, 153U),
                "hyperlink" => (UIRibbon, 174U),
                "brown" => (UIRibbon, 178U),
                "darkgreen" => (UIRibbon, 180U),
                "darkteal" => (UIRibbon, 181U),
                "indigo" => (UIRibbon, 183U),
                "grange" => (UIRibbon, 186U),
                "bluegray" => (UIRibbon, 191U),
                "lightorange" => (UIRibbon, 194U),
                "lime" => (UIRibbon, 195U),
                "seagreen" => (UIRibbon, 196U),
                "aqua" => (UIRibbon, 197U),
                "gold" => (UIRibbon, 202U),
                "skyblue" => (UIRibbon, 206U),
                "plum" => (UIRibbon, 207U),
                "rose" => (UIRibbon, 209U),
                "lightyellow" => (UIRibbon, 211U),
                "lightturquoise" => (UIRibbon, 213U),
                "pale blue" => (UIRibbon, 214U),
                "lavender" => (UIRibbon, 215U),
                "periwinkle" => (UIRibbon, 217U),
                "ivory" => (UIRibbon, 218U),
                "darkpurple" => (UIRibbon, 219U),
                "coral" => (UIRibbon, 220U),
                "oceanblue" => (UIRibbon, 221U),
                "iceblue" => (UIRibbon, 222U),
                "file" => (UIRibbon, 410U),
                "black" => (UIRibbon, 506U),
                "blue" => (UIRibbon, 512U),
                "brightgreen" => (UIRibbon, 518U),
                "charcoal" => (UIRibbon, 519U),
                "darkblue" => (UIRibbon, 520U),
                "darkgray" => (UIRibbon, 526U),
                "darkred" => (UIRibbon, 527U),
                "darkyellow" => (UIRibbon, 528U),
                "earthyblue" => (UIRibbon, 529U),
                "earthybrown" => (UIRibbon, 530U),
                "earthygreen" => (UIRibbon, 531U),
                "earthyorange" => (UIRibbon, 532U),
                "earthyred" => (UIRibbon, 533U),
                "earthyyellow" => (UIRibbon, 534U),
                "green" => (UIRibbon, 537U),
                "lightblue" => (UIRibbon, 538U),
                "lightgray" => (UIRibbon, 539U),
                "lightgreen" => (UIRibbon, 540U),
                "mediumgray" => (UIRibbon, 541U),
                "olive" => (UIRibbon, 542U),
                "olivegreen" => (UIRibbon, 543U),
                "orange" => (UIRibbon, 549U),
                "pastelblue" => (UIRibbon, 555U),
                "pastelgreen" => (UIRibbon, 556U),
                "pastelorange" => (UIRibbon, 557U),
                "pastelpurple" => (UIRibbon, 558U),
                "pastelred" => (UIRibbon, 559U),
                "pink" => (UIRibbon, 561U),
                "professionalaqua" => (UIRibbon, 562U),
                "professionalblue" => (UIRibbon, 563U),
                "professionalgreen" => (UIRibbon, 564U),
                "professionalorange" => (UIRibbon, 565U),
                "professionalpurple" => (UIRibbon, 566U),
                "professionalred" => (UIRibbon, 567U),
                "purple" => (UIRibbon, 568U),
                "red" => (UIRibbon, 574U),
                "tan" => (UIRibbon, 580U),
                "teal" => (UIRibbon, 586U),
                "turquoise" => (UIRibbon, 587U),
                "vibrantblue" => (UIRibbon, 588U),
                "vibrantgreen" => (UIRibbon, 589U),
                "vibrantorange" => (UIRibbon, 590U),
                "vibrantpurple" => (UIRibbon, 591U),
                "vibrantred" => (UIRibbon, 592U),
                "vibrantyellow" => (UIRibbon, 593U),
                "violet" => (UIRibbon, 594U),
                "white" => (UIRibbon, 595U),
                "yellow" => (UIRibbon, 601U),
                "bold" => (UIRibbon, 1182U),
                "italic" => (UIRibbon, 1183U),
                "underline" => (UIRibbon, 1184U),
                "strikethrough" => (UIRibbon, 1185U),
                "fontfamily" => (UIRibbon, 1186U),
                "fontsize" => (UIRibbon, 1187U),
                "superscript" => (UIRibbon, 1189U),
                "subscript" => (UIRibbon, 1190U),
                "growfont" => (UIRibbon, 1191U),
                "shrinkfont" => (UIRibbon, 1192U),
                "textcolor" => (UIRibbon, 1193U),
                "bolditalic" => (UIRibbon, 1242U),
                "regular" => (UIRibbon, 1243U),
                #endregion
                #region Vfwwdm32
                "videosource" => (Vfwwdm32, 17U),
                "videoformat" => (Vfwwdm32, 19U),
                "zoom" => (Vfwwdm32, 46U),
                "focus" => (Vfwwdm32, 47U),
                "tilt" => (Vfwwdm32, 48U),
                "exposure" => (Vfwwdm32, 49U),
                "iris" => (Vfwwdm32, 50U),
                "pan" => (Vfwwdm32, 51U),
                "roll" => (Vfwwdm32, 52U),
                "brightness" => (Vfwwdm32, 53U),
                "contrast" => (Vfwwdm32, 54U),
                "hue" => (Vfwwdm32, 55U),
                "saturation" => (Vfwwdm32, 56U),
                "sharpness" => (Vfwwdm32, 57U),
                "whitebalance" => (Vfwwdm32, 58U),
                "resolution" => (Vfwwdm32, 65U),
                "gamma" => (Vfwwdm32, 69U),
                "sourceundefined" => (Vfwwdm32, 77U),
                #endregion
                #region User32
                "error" => (User32, 2U),
                "minimize" => (User32, 900U),
                "maximize" => (User32, 901U),
                "help" => (User32, 904U),
                "close" => (User32, 905U),
                "image" => (User32, 1001U),
                "text" => (User32, 1002U),
                "audio" => (User32, 1003U),
                "other" => (User32, 1004U),
                #endregion
                _ => null
            };

            if (Value.HasValue)
            {
                Filename = Value.Value.Key;
                Uid = Value.Value.UID;
                return true;
            }

            Filename = string.Empty;
            Uid = 0;
            return false;
        }
        private static string LoadStringFromMui(string FilePath, uint resourceId)
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

                Debug.WriteLine($"[{nameof(LoadStringFromMui)}][Win32Exception]{Message}");
                return null;
            }

            StringBuilder Buffer = new(256);
            try
            {
                return Win32.System.LoadString(hModule, resourceId, Buffer, Buffer.Capacity) > 0 ? Buffer.ToString() : null;
            }
            finally
            {
                Win32.System.FreeLibrary(hModule);
                Buffer.Clear();
            }
        }

        private static void OnStaticPropertyChanged([CallerMemberName] string PropertyName = null)
            => StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(PropertyName));

    }
}