using MenthaAssembly.Network.Primitives;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MenthaAssembly.Network
{
    public abstract class HttpHeader : IEnumerable<HttpHeaderData>
    {
        protected internal readonly List<HttpHeaderData> Datas = new List<HttpHeaderData>();

        public int Count => Datas.Count;

        public string Protocol { get; set; } = "HTTP/1.1";

        public string this[string Key]
        {
            get => GetValue(Key);
            set => SetValue(Key, value);
        }

        /// <summary>
        /// 用來指定在這次的請求/回應鏈中的所有快取機制都必須遵守的指令<para/>
        /// Cache-Control: no-cache<para/>
        /// 向從伺服器直到客戶端在內的所有快取機制告知，它們是否可以快取這個物件。其單位為秒<para/>
        /// Cache-Control: max-age=3600<para/>
        /// </summary>
        public string CacheControl
        {
            get => GetValue("Cache-Control");
            set => SetValue("Cache-Control", value);
        }

        /// <summary>
        /// 針對該連接所預期的選項<para/>
        /// Connection: close<para/>
        /// </summary>
        public string Connection
        {
            get => GetValue("Connection");
            set => SetValue("Connection", value);
        }

        /// <summary>
        /// 訊息體的長度，以 位元組 （8位元為一位元組）為單位<para/>
        /// Content-Length: 348<para/>
        /// </summary>
        public int ContentLength
        {
            get => TryGetValue("Content-Length", out string Value) ? Convert.ToInt32(Value) : 0;
            set => SetValue("Content-Length", value is 0 ? string.Empty : value.ToString());
        }

        /// <summary>
        /// 內容的二進位 MD5 雜湊，以 Base64 方式編碼<para/>
        /// Content-MD5: Q2hlY2sgSW50ZWdyaXR5IQ==<para/>
        /// </summary>
        public string ContentMD5
        {
            get => GetValue("Content-MD5");
            set => SetValue("Content-MD5", value);
        }

        /// <summary>
        /// MIME類型<para/>
        /// Content-Type: text/html; charset=utf-8<para/>
        /// </summary>
        public string ContentType
        {
            get => GetValue("Content-Type");
            set => SetValue("Content-Type", value);
        }

        /// <summary>
        /// 在資料上使用的編碼類型。參考 超文字傳輸協定壓縮 。<para/>
        /// Content-Encoding: gzip<para/>
        /// </summary>
        public string ContentEncoding
        {
            get => GetValue("Content-Encoding");
            set => SetValue("Content-Encoding", value);
        }

        /// <summary>
        /// 訊息被傳送時的日期和時間(按照 RFC 7231 中定義的「超文字傳輸協定日期」格式來表示)<para/>
        /// Date: Tue, 15 Nov 1994 08:12:31 GMT<para/>
        /// </summary>
        public string Date
        {
            get => GetValue("Date");
            set => SetValue("Date", value);
        }

        /// <summary>
        /// 與具體的實現相關，這些欄位可能在請求/回應鏈中的任何時候產生多種效果。<para/>
        /// Pragma: no-cache<para/>
        /// </summary>
        public string Pragma
        {
            get => GetValue("Pragma");
            set => SetValue("Pragma", value);
        }

        /// <summary>
        /// 要求對方升級到另一個協定。<para/>
        /// Upgrade: HTTP/2.0, SHTTP/1.3, IRC/6.9, RTA/x11<para/>
        /// </summary>
        public string Upgrade
        {
            get => GetValue("Upgrade");
            set => SetValue("Upgrade", value);
        }

        /// <summary>
        /// 告知對方，當前回應是通過什麼途徑傳送的。<para/>
        /// Via: 1.0 fred, 1.1 example.com (Apache/1.1)<para/>
        /// </summary>
        public string Via
        {
            get => GetValue("Via");
            set => SetValue("Via", value);
        }

        /// <summary>
        /// 一般性的警告，告知在實體內容體中可能存在錯誤。<para/>
        /// Warning: 199 Miscellaneous warning<para/>
        /// </summary>
        public string Warning
        {
            get => GetValue("Warning");
            set => SetValue("Warning", value);
        }

        internal HttpHeader()
        {
        }

        public string GetValue(string Key)
            => Datas.FirstOrDefault(i => i.Key.Equals(Key)) is HttpHeaderData Data ? Data.Value : string.Empty;
        public bool TryGetValue(string Key, out string Value)
        {
            if (Datas.FirstOrDefault(i => i.Key.Equals(Key)) is HttpHeaderData Data)
            {
                Value = Data.Value;
                return true;
            }

            Value = null;
            return false;
        }

        public void SetValue(string Key, string Value)
        {
            if (string.IsNullOrEmpty(Value))
                Datas.RemoveAll(i => i.Key.Equals(Key));
            else
            {
                if (Datas.FirstOrDefault(i => i.Key.Equals(Key)) is HttpHeaderData Data)
                    Data.Value = Value;
                else
                    AddValue(Key, Value);
            }
        }

        public void AddValue(string Key, string Value)
        {
            if (Datas.Count == 100)
                throw new ArgumentOutOfRangeException(nameof(Key), "The maximum size of header is 100.");

            Datas.Add(new HttpHeaderData(Key, Value));
        }

        public abstract byte[] GetBytes(IEnumerable<string> Cookies);

        public override string ToString()
            => string.Join("\r\n", Datas.Select(i => $"{i.Key}:{i.Value}"));

        public IEnumerator<HttpHeaderData> GetEnumerator()
            => Datas.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

    }

}