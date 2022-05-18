using MenthaAssembly.Network.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MenthaAssembly.Network
{
    public class HttpRequestHeader : HttpHeader
    {
        public string URL { get; set; } = "/";

        public HttpMethods Method { get; set; } = HttpMethods.GET;

        /// <summary>
        /// 能夠接受的回應內容類型（Content-Types）。<para/>
        /// Accept: text/plain
        /// </summary>
        public string Accept
        {
            get => GetValue("Accept");
            set => SetValue("Accept", value);
        }

        /// <summary>
        /// 能夠接受的字元集<para/>
        /// Accept-Charset: utf-8
        /// </summary>
        public string AcceptCharset
        {
            get => GetValue("Accept-Charset");
            set => SetValue("Accept-Charset", value);
        }

        /// <summary>
        /// 能夠接受的編碼方式列表。參考HTTP壓縮。<para/>
        /// Accept-Encoding: gzip, deflate
        /// </summary>
        public string AcceptEncoding
        {
            get => GetValue("Accept-Encoding");
            set => SetValue("Accept-Encoding", value);
        }

        /// <summary>
        /// 能夠接受的回應內容的自然語言列表。參考 內容協商 。<para/>
        /// Accept-Language: en-US
        /// </summary>
        public string AcceptLanguage
        {
            get => GetValue("Accept-Language");
            set => SetValue("Accept-Language", value);
        }

        /// <summary>
        /// 能夠接受的按照時間來表示的版本<para/>
        /// Accept-Datetime: Thu, 31 May 2007 20:35:00 GMT
        /// </summary>
        public string AcceptDatetime
        {
            get => GetValue("Accept-Datetime");
            set => SetValue("Accept-Datetime", value);
        }

        /// <summary>
        /// 用於超文字傳輸協定的認證的認證資訊<para/>
        /// Authorization: Basic QWxhZGRpbjpvcGVuIHNlc2FtZQ==
        /// </summary>
        public string Authorization
        {
            get => GetValue("Authorization");
            set => SetValue("Authorization", value);
        }

        /// <summary>
        /// 之前由伺服器通過 Set- Cookie （下文詳述）傳送的一個 超文字傳輸協定Cookie<para/>
        /// Cookie: $Version=1; Skin=new;
        /// </summary>
        public string Cookie
        {
            get => GetValue("Cookie");
            set => SetValue("Cookie", value);
        }

        /// <summary>
        /// 表明客戶端要求伺服器做出特定的行為<para/>
        /// Expect: 100-continue
        /// </summary>
        public string Expect
        {
            get => GetValue("Expect");
            set => SetValue("Expect", value);
        }

        /// <summary>
        /// 發起此請求的使用者的郵件位址<para/>
        /// From: user@example.com
        /// </summary>
        public string From
        {
            get => GetValue("From");
            set => SetValue("From", value);
        }

        /// <summary>
        /// 伺服器的域名(用於虛擬主機 )，以及伺服器所監聽的傳輸控制協定埠號。如果所請求的埠是對應的服務的標準埠，則埠號可被省略。自超檔案傳輸協定版本1.1（HTTP/1.1）開始便是必需欄位。<para/>
        /// Host: zh.wikipedia.org:80
        /// </summary>
        public string Host
        {
            get => GetValue("Host");
            set => SetValue("Host", value);
        }

        /// <summary>
        /// 僅當客戶端提供的實體與伺服器上對應的實體相匹配時，才進行對應的操作。主要作用時，用作像 PUT 這樣的方法中，僅當從使用者上次更新某個資源以來，該資源未被修改的情況下，才更新該資源。<para/>
        /// If-Match: "737060cd8c284d8af7ad3082f209582d"
        /// </summary>
        public string IfMatch
        {
            get => GetValue("If-Match");
            set => SetValue("If-Match", value);
        }

        /// <summary>
        /// 允許在對應的內容未被修改的情況下返回304未修改（ 304 Not Modified ）<para/>
        /// If-Modified-Since: Sat, 29 Oct 1994 19:43:31 GMT
        /// </summary>
        public string IfModifiedSince
        {
            get => GetValue("If-Modified-Since");
            set => SetValue("If-Modified-Since", value);
        }

        /// <summary>
        /// 允許在對應的內容未被修改的情況下返回304未修改（ 304 Not Modified ），參考 超文字傳輸協定 的實體標記<para/>
        /// If-None-Match: "737060cd8c284d8af7ad3082f209582d"
        /// </summary>
        public string IfNoneMatch
        {
            get => GetValue("If-None-Match");
            set => SetValue("If-None-Match", value);
        }

        /// <summary>
        /// 如果該實體未被修改過，則向我傳送我所缺少的那一個或多個部分；否則，傳送整個新的實體<para/>
        /// If-Range: "737060cd8c284d8af7ad3082f209582d"
        /// </summary>
        public string IfRange
        {
            get => GetValue("If-Range");
            set => SetValue("If-Range", value);
        }

        /// <summary>
        /// 僅當該實體自某個特定時間已來未被修改的情況下，才傳送回應。<para/>
        /// If-Unmodified-Since: Sat, 29 Oct 1994 19:43:31 GMT
        /// </summary>
        public string IfUnmodifiedSince
        {
            get => GetValue("If-Unmodified-Since");
            set => SetValue("If-Unmodified-Since", value);
        }

        /// <summary>
        /// 限制該訊息可被代理及閘道器轉發的次數。<para/>
        /// Max-Forwards: 10
        /// </summary>
        public string MaxForwards
        {
            get => GetValue("Max-Forwards");
            set => SetValue("Max-Forwards", value);
        }

        /// <summary>
        /// 發起一個針對 跨來源資源共享 的請求（要求伺服器在回應中加入一個『存取控制-允許來源』（'Access-Control-Allow-Origin'）欄位）。<para/>
        /// Origin: http://www.example-social-network.com
        /// </summary>
        public string Origin
        {
            get => GetValue("Origin");
            set => SetValue("Origin", value);
        }

        /// <summary>
        /// 用來向代理進行認證的認證資訊。<para/>
        /// Proxy-Authorization: Basic QWxhZGRpbjpvcGVuIHNlc2FtZQ==
        /// </summary>
        public string ProxyAuthorization
        {
            get => GetValue("Proxy-Authorization");
            set => SetValue("Proxy-Authorization", value);
        }

        /// <summary>
        /// 僅請求某個實體的一部分。位元組偏移以0開始。參見位元組服務。<para/>
        /// Range: bytes=500-999
        /// </summary>
        public string Range
        {
            get => GetValue("Range");
            set => SetValue("Range", value);
        }

        /// <summary>
        /// 表示瀏覽器所存取的前一個頁面，正是那個頁面上的某個連結將瀏覽器帶到了當前所請求的這個頁面。<para/>
        /// Referer: http://zh.wikipedia.org/wiki/Main_Page
        /// </summary>
        public string Referer
        {
            get => GetValue("Referer");
            set => SetValue("Referer", value);
        }

        /// <summary>
        /// 瀏覽器預期接受的傳輸編碼方式：可使用回應協定頭 Transfer-Encoding 欄位中的值；另外還可用"trailers"（與"分塊 "傳輸方式相關）這個值來表明瀏覽器希望在最後一個尺寸為0的塊之後還接收到一些額外的欄位。<para/>
        /// TE: trailers, deflate
        /// </summary>
        public string TE
        {
            get => GetValue("TE");
            set => SetValue("TE", value);
        }

        /// <summary>
        /// 瀏覽器的瀏覽器身分標識字串<para/>
        /// User-Agent: Mozilla/5.0 (X11; Linux x86_64; rv:12.0) Gecko/20100101 Firefox/21.0
        /// </summary>
        public string UserAgent
        {
            get => GetValue("User-Agent");
            set => SetValue("User-Agent", value);
        }

        /// <summary>
        /// 主要用於標識 Ajax 及可延伸標記式語言 請求。大部分的JavaScript框架會傳送這個欄位，且將其值設定為 XMLHttpRequest<para/>
        /// X-Requested-With: XMLHttpRequest
        /// </summary>
        public string XRequestedWith
        {
            get => GetValue("X-Requested-With");
            set => SetValue("X-Requested-With", value);
        }

        /// <summary>
        /// 請求某個網頁應用程式停止跟蹤某個使用者。在火狐瀏覽器中，相當於X-Do-Not-Track協定頭欄位（自 Firefox/4.0 Beta 11 版開始支援）。Safari 和 Internet Explorer 9 也支援這個欄位。2011年3月7日，草案提交IETF。 全球資訊網協會 的跟蹤保護工作群組正在就此製作一項規範。<para/>
        /// DNT: 1 (DNT启用); DNT: 0 (DNT被禁用)
        /// </summary>
        public string DNT
        {
            get => GetValue("DNT");
            set => SetValue("DNT", value);
        }

        /// <summary>
        /// 一個事實標準 ，用於標識某個通過超文字傳輸協定代理或負載均衡連接到某個網頁伺服器的客戶端的原始網際網路位址<para/>
        /// X-Forwarded-For: client1, proxy1, proxy2; X-Forwarded-For: 129.78.138.66, 129.78.64.103
        /// </summary>
        public string XForwardedFor
        {
            get => GetValue("X-Forwarded-For");
            set => SetValue("X-Forwarded-For", value);
        }

        /// <summary>
        /// 一個事實標準 ，用於辨識客戶端原本發出的 Host 請求頭部。<para/>
        /// X-Forwarded-Host: zh.wikipedia.org:80 ; X-Forwarded-Host: zh.wikipedia.org
        /// </summary>
        public string XForwardedHost
        {
            get => GetValue("X-Forwarded-Host");
            set => SetValue("X-Forwarded-Host", value);
        }

        /// <summary>
        /// 一個事實標準，用於標識某個超文字傳輸協定請求最初所使用的協定。<para/>
        /// X-Forwarded-Proto: https
        /// </summary>
        public string XForwardedProto
        {
            get => GetValue("X-Forwarded-Proto");
            set => SetValue("X-Forwarded-Proto", value);
        }

        /// <summary>
        /// 被微軟的伺服器和負載均衡器所使用的非標準頭部欄位。<para/>
        /// Front-End-Https: on
        /// </summary>
        public string FrontEndHttps
        {
            get => GetValue("Front-End-Https");
            set => SetValue("Front-End-Https", value);
        }

        /// <summary>
        /// 請求某個網頁應用程式使用該協定頭欄位中指定的方法（一般是PUT或DELETE）來覆蓋掉在請求中所指定的方法（一般是POST）。當某個瀏覽器或防火牆阻止直接傳送PUT 或DELETE 方法時（注意，這可能是因為軟體中的某個漏洞，因而需要修復，也可能是因為某個組態選項就是如此要求的，因而不應當設法繞過），可使用這種方式。<para/>
        /// X-HTTP-Method-Override: DELETE
        /// </summary>
        public string XHttpMethodOverride
        {
            get => GetValue("X-Http-Method-Override");
            set => SetValue("X-Http-Method-Override", value);
        }

        /// <summary>
        /// 使伺服器更容易解讀AT&T裝置User-Agent欄位中常見的裝置型號、韌體資訊。<para/>
        /// X-Att-Deviceid: GT-P7320/P7320XXLPG
        /// </summary>
        public string XATTDeviceId
        {
            get => GetValue("X-ATT-DeviceId");
            set => SetValue("X-ATT-DeviceId", value);
        }

        /// <summary>
        /// 連結到網際網路上的一個XML檔案，其完整、仔細地描述了正在連接的裝置。右側以為AT&T Samsung Galaxy S2提供的XML檔案為例。<para/>
        /// x-wap-profile: http://wap.samsungmobile.com/uaprof/SGH-I777.xml
        /// </summary>
        public string XWapProfile
        {
            get => GetValue("X-Wap-Profile");
            set => SetValue("X-Wap-Profile", value);
        }

        /// <summary>
        /// 該欄位源於早期超文字傳輸協定版本實現中的錯誤。與標準的連接（Connection）欄位的功能完全相同。<para/>
        /// Proxy-Connection: keep-alive
        /// </summary>
        public string ProxyConnection
        {
            get => GetValue("Proxy-Connection");
            set => SetValue("Proxy-Connection", value);
        }

        /// <summary>
        /// 用於防止 跨站請求偽造。 輔助用的頭部有 X-CSRFToken 或 X-XSRF-TOKEN<para/>
        /// X-Csrf-Token: i8XNjC4b8KVok4uw5RftR38Wgp2BFwql
        /// </summary>
        public string XCsrfToken
        {
            get => GetValue("X-Csrf-Token");
            set => SetValue("X-Csrf-Token", value);
        }

        public HttpRequestHeader()
        {
            Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9";
            AcceptEncoding = "gzip, deflate, br";
            AcceptLanguage = "zh-TW,zh;q=0.9,en;q=0.8,en-GB;q=0.7,en-US;q=0.6,zh-CN;q=0.5";

            Connection = "keep-alive";
            CacheControl = "no-cache";
            Pragma = "no-cache";

            UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/101.0.4951.41 Safari/537.36 Edg/101.0.1210.32";

        }

        public override byte[] GetBytes(IEnumerable<string> Cookies)
        {
            StringBuilder Builder = new StringBuilder();
            try
            {
                Builder.AppendLine($"{Method} {URL} {Protocol}");
                foreach (HttpHeaderData Data in Datas.Where(i => !string.IsNullOrEmpty(i.Value)))
                    Builder.AppendLine(Data.ToString());

                foreach (string Cookie in Cookies)
                    Builder.AppendLine($"Cookie: {Cookie}");

                Builder.AppendLine();

                return Encoding.ASCII.GetBytes(Builder.ToString());
            }
            finally
            {
                Builder.Clear();
                Builder = null;
            }
        }

        public override string ToString()
            => $"{Method} {URL} {Protocol}\r\n{base.ToString()}";

        public static HttpRequestHeader ParseHttpRequestHeader(Stream Stream)
        {
            HttpRequestHeader Header = new HttpRequestHeader();

            using StreamReader Reader = new StreamReader(Stream, Encoding.ASCII);
            string Line = Reader.ReadLine();

            string[] Arguments = Line.Split(' ');
            Header.Method = Enum.TryParse(Arguments[0], out HttpMethods Method) ? Method : HttpMethods.UNKNOWN;
            Header.URL = Arguments[1];
            Header.Protocol = Arguments[2];

            StringBuilder Builder = new StringBuilder(256);
            try
            {
                char c;
                int Length;
                Line = Reader.ReadLine();
                while (!string.IsNullOrEmpty(Line))
                {
                    int i = 0;
                    Length = Line.Length;
                    while (i < Length)
                    {
                        c = Line[i++];
                        if (c.Equals(':'))
                            break;

                        Builder.Append(c);
                    }

                    string Key = Builder.ToString();
                    Builder.Clear();

                    i++;    // Skip Space
                    for (; i < Length; i++)
                        Builder.Append(Line[i]);

                    Header.AddValue(Key, Builder.ToString());
                    Builder.Clear();
                }
            }
            finally
            {
                Builder.Clear();
            }

            return Header;
        }

    }
}
