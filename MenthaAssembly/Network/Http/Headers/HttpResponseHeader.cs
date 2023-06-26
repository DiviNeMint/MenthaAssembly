using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace MenthaAssembly.Network
{
    public class HttpResponseHeader : HttpHeader
    {
        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;

        /// <summary>
        /// 指定哪些網站可參與到跨來源資源共享過程中<para/>
        /// Access-Control-Allow-Origin: *<para/>
        /// </summary>
        public string AccessControlAllowOrigin
        {
            get => GetValue("Access-Control-Allow-Origin");
            set => SetValue("Access-Control-Allow-Origin", value);
        }

        /// <summary>
        /// 指定伺服器支援的檔案格式類型。<para/>
        /// Accept-Patch: text/example;charset=utf-8<para/>
        /// </summary>
        public string AcceptPatch
        {
            get => GetValue("Accept-Patch");
            set => SetValue("Accept-Patch", value);
        }

        /// <summary>
        /// 這個伺服器支援哪些種類的部分內容範圍<para/>
        /// Accept-Ranges: bytes<para/>
        /// </summary>
        public string AcceptRanges
        {
            get => GetValue("Accept-Ranges");
            set => SetValue("Accept-Ranges", value);
        }

        /// <summary>
        /// 這個物件在代理快取中存在的時間，以秒為單位<para/>
        /// Age: 12<para/>
        /// </summary>
        public string Age
        {
            get => GetValue("Age");
            set => SetValue("Age", value);
        }

        /// <summary>
        /// 對於特定資源有效的動作。針對HTTP/405這一錯誤代碼而使用<para/>
        /// Allow: GET, HEAD<para/>
        /// </summary>
        public string Allow
        {
            get => GetValue("Allow");
            set => SetValue("Allow", value);
        }

        /// <summary>
        /// 一個可以讓客戶端下載檔案並建議檔名的頭部。檔名需要用雙引號包裹。<para/>
        /// Content-Disposition: attachment; filename="fname.ext"<para/>
        /// </summary>
        public string ContentDisposition
        {
            get => GetValue("Content-Disposition");
            set => SetValue("Content-Disposition", value);
        }

        /// <summary>
        /// 內容所使用的語言<para/>
        /// Content-Language: da<para/>
        /// </summary>
        public string ContentLanguage
        {
            get => GetValue("Content-Language");
            set => SetValue("Content-Language", value);
        }

        /// <summary>
        /// 所返回的資料的一個候選位置<para/>
        /// Content-Location: /index.htm<para/>
        /// </summary>
        public string ContentLocation
        {
            get => GetValue("Content-Location");
            set => SetValue("Content-Location", value);
        }

        /// <summary>
        /// 這條部分訊息是屬於某條完整訊息的哪個部分<para/>
        /// Content-Range: bytes 21010-47021/47022<para/>
        /// </summary>
        public string ContentRange
        {
            get => GetValue("Content-Range");
            set => SetValue("Content-Range", value);
        }

        /// <summary>
        /// 對於某個資源的某個特定版本的一個識別碼，通常是一個 訊息雜湊<para/>
        /// ETag: "737060cd8c284d8af7ad3082f209582d"<para/>
        /// </summary>
        public string ETag
        {
            get => GetValue("ETag");
            set => SetValue("ETag", value);
        }

        /// <summary>
        /// 指定一個日期/時間，超過該時間則認為此回應已經過期<para/>
        /// Expires: Thu, 01 Dec 1994 16:00:00 GMT<para/>
        /// </summary>
        public string Expires
        {
            get => GetValue("Expires");
            set => SetValue("Expires", value);
        }

        /// <summary>
        /// 所請求的物件的最後修改日期(按照 RFC 7231 中定義的「超文字傳輸協定日期」格式來表示)<para/>
        /// Last-Modified: Tue, 15 Nov 1994 12:45:26 GMT<para/>
        /// </summary>
        public string LastModified
        {
            get => GetValue("Last-Modified");
            set => SetValue("Last-Modified", value);
        }

        /// <summary>
        /// 用來表達與另一個資源之間的類型關係，此處所說的類型關係是在 RFC 5988 中定義的<para/>
        /// Link: <para/></feed>; rel="alternate"
        /// </summary>
        public string Link
        {
            get => GetValue("Link");
            set => SetValue("Link", value);
        }

        /// <summary>
        /// 用來 進行重新導向，或者在建立了某個新資源時使用。<para/>
        /// Location: http://www.w3.org/pub/WWW/People.html<para/>
        /// </summary>
        public string Location
        {
            get => GetValue("Location");
            set => SetValue("Location", value);
        }

        /// <summary>
        /// 用於支援設定P3P策略，標準格式為「P3P:CP="your_compact_policy"」。然而P3P規範並不成功，大部分現代瀏覽器沒有完整實現該功能，而大量網站也將該值設為假值，從而足以用來欺騙瀏覽器的P3P外掛程式功能並授權給第三方Cookies。<para/>
        /// P3P: CP="This is not a P3P policy! See http://www.google.com/support/accounts/bin/answer.py?hl=en&answer=151657 for more info."<para/>
        /// </summary>
        public string P3P
        {
            get => GetValue("P3P");
            set => SetValue("P3P", value);
        }

        /// <summary>
        /// 要求在存取代理時提供身分認證資訊。<para/>
        /// Proxy-Authenticate: Basic<para/>
        /// </summary>
        public string ProxyAuthenticate
        {
            get => GetValue("Proxy-Authenticate");
            set => SetValue("Proxy-Authenticate", value);
        }

        /// <summary>
        /// 用於緩解中間人攻擊，聲明網站認證使用的傳輸層安全協定憑證的雜湊值<para/>
        /// Public-Key-Pins: max-age=2592000; pin-sha256="E9CZ9INDbd+2eRQozYqqbQ2yXLVKB9+xcprMF+44U1g=";<para/>
        /// </summary>
        public string PublicKeyPins
        {
            get => GetValue("Public-Key-Pins");
            set => SetValue("Public-Key-Pins", value);
        }

        /// <summary>
        /// 用於設定可定時的重新導向跳轉。右邊例子設定了5秒後跳轉至「http://www.w3.org/pub/WWW/People.html」。<para/>
        /// Refresh: 5; url=http://www.w3.org/pub/WWW/People.html<para/>
        /// </summary>
        public string Refresh
        {
            get => GetValue("Refresh");
            set => SetValue("Refresh", value);
        }

        /// <summary>
        /// 如果某個實體臨時不可用，則，此協定頭用來告知客戶端日後重試。其值可以是一個特定的時間段(以秒為單位)或一個超文字傳輸協定日期。 <para/>
        /// Example 1: Retry-After: 120<para/>
        /// </summary>
        public string RetryAfter
        {
            get => GetValue("Retry-After");
            set => SetValue("Retry-After", value);
        }

        /// <summary>
        /// 伺服器的名字<para/>
        /// Server: Apache/2.4.1 (Unix)<para/>
        /// </summary>
        public string Server
        {
            get => GetValue("Server");
            set => SetValue("Server", value);
        }

        /// <summary>
        /// HTTP cookie<para/>
        /// Set-Cookie: UserID=JohnDoe; Max-Age=3600; Version=1<para/>
        /// </summary>
        public IEnumerable<string> SetCookie
            => Datas.Where(i => i.Key.Equals("Set-Cookie")).Select(i => i.Value);

        /// <summary>
        /// 通用閘道器介面 協定頭欄位，用來說明當前這個超文字傳輸協定回應的 狀態 。普通的超文字傳輸協定回應，會使用單獨的「狀態行」（"Status-Line"）作為替代，這一點是在 RFC 7230 中定義的。 <para/>
        /// Status: 200 OK<para/>
        /// </summary>
        public string Status
        {
            get => GetValue("Status");
            set => SetValue("Status", value);
        }

        /// <summary>
        /// HTTP 嚴格傳輸安全這一頭部告知客戶端快取這一強制 HTTPS 策略的時間，以及這一策略是否適用於其子域名。<para/>
        /// Strict-Transport-Security: max-age=16070400; includeSubDomains<para/>
        /// </summary>
        public string StrictTransportSecurity
        {
            get => GetValue("Strict-Transport-Security");
            set => SetValue("Strict-Transport-Security", value);
        }

        /// <summary>
        /// 這個頭部數值指示了在這一系列頭部資訊由由分塊傳輸編碼編碼。<para/>
        /// Trailer: Max-Forwards<para/>
        /// </summary>
        public string Trailer
        {
            get => GetValue("Trailer");
            set => SetValue("Trailer", value);
        }

        /// <summary>
        /// 用來將實體安全地傳輸給使用者的編碼形式。當前定義的方法包括：分塊（chunked）、compress、deflate、gzip和identity。<para/>
        /// Transfer-Encoding: chunked<para/>
        /// </summary>
        public string TransferEncoding
        {
            get => GetValue("Transfer-Encoding");
            set => SetValue("Transfer-Encoding", value);
        }

        /// <summary>
        /// 告知下游的代理伺服器，應當如何對未來的請求協定頭進行匹配，以決定是否可使用已快取的回應內容而不是重新從原始伺服器請求新的內容。<para/>
        /// Vary: *<para/>
        /// </summary>
        public string Vary
        {
            get => GetValue("Vary");
            set => SetValue("Vary", value);
        }

        /// <summary>
        /// 表明在請求取得這個實體時應當使用的認證模式。<para/>
        /// WWW-Authenticate: Basic<para/>
        /// </summary>
        public string WWWAuthenticate
        {
            get => GetValue("WWW-Authenticate");
            set => SetValue("WWW-Authenticate", value);
        }

        /// <summary>
        /// 點擊劫持保護<para/>
        /// deny：該頁面不允許在 frame 中展示，即使是同域名內。<para/>
        /// sameorigin：該頁面允許同域名內在 frame 中展示。<para/>
        /// allow-from uri：該頁面允許在指定uri的 frame 中展示。<para/>
        /// allowall：允許任意位置的frame顯示，非標準值。<para/>
        /// X-Frame-Options: deny<para/>
        /// </summary>
        public string XFrameOptions
        {
            get => GetValue("X-Frame-Options");
            set => SetValue("X-Frame-Options", value);
        }

        /// <summary>
        /// 跨站指令碼攻擊 （XSS）過濾器<para/>
        /// X-XSS-Protection: 1; mode=block
        /// </summary>
        public string XXSSProtection
        {
            get => GetValue("X-XSS-Protection");
            set => SetValue("X-XSS-Protection", value);
        }

        /// <summary>
        /// 內容安全策略定義。<para/>
        /// X-WebKit-CSP: default-src 'self'
        /// </summary>
        public string ContentSecurityPolicy
        {
            get => GetValue("Content-Security-Policy");
            set => SetValue("Content-Security-Policy", value);
        }

        /// <summary>
        /// 內容安全策略定義。<para/>
        /// X-WebKit-CSP: default-src 'self'
        /// </summary>
        public string XContentSecurityPolicy
        {
            get => GetValue("X-Content-Security-Policy");
            set => SetValue("X-Content-Security-Policy", value);
        }

        /// <summary>
        /// 內容安全策略定義。<para/>
        /// X-WebKit-CSP: default-src 'self'
        /// </summary>
        public string XWebKitCSP
        {
            get => GetValue("X-WebKit-CSP");
            set => SetValue("X-WebKit-CSP", value);
        }

        /// <summary>
        /// 唯一允許的數值為"nosniff"，防止 Internet Explorer 對檔案進行MIME類型嗅探。這也對 Google Chrome 下載擴充時適用。<para/>
        /// X-Content-Type-Options: nosniff
        /// </summary>
        public string XContentTypeOptions
        {
            get => GetValue("X-Content-Type-Options");
            set => SetValue("X-Content-Type-Options", value);
        }

        /// <summary>
        /// 表明用於支援當前網頁應用程式的技術（例如：PHP）（版本號細節通常放置在 X-Runtime 或 X-Version 中）<para/>
        /// X-Powered-By: PHP/5.4.0
        /// </summary>
        public string XPoweredBy
        {
            get => GetValue("X-Powered-By");
            set => SetValue("X-Powered-By", value);
        }

        /// <summary>
        /// 推薦指定的彩現引擎（通常是向下相容模式）來顯示內容。也用於啟用 Internet Explorer 中的 Chrome Frame。<para/>
        /// X-UA-Compatible: IE=edge
        /// </summary>
        public string XUACompatible
        {
            get => GetValue("X-UA-Compatible");
            set => SetValue("X-UA-Compatible", value);
        }

        /// <summary>
        /// 指出音影片的長度，單位為秒。只受Gecko核心瀏覽器支援。<para/>
        /// X-Content-Duration: 42.666
        /// </summary>
        public string XContentDuration
        {
            get => GetValue("X-Content-Duration");
            set => SetValue("X-Content-Duration", value);
        }

        /// <summary>
        /// 管控特定應用程式介面<para/>
        /// Feature-Policy: vibrate 'none'; geolocation 'none'
        /// </summary>
        public string FeaturePolicy
        {
            get => GetValue("Feature-Policy");
            set => SetValue("Feature-Policy", value);
        }

        /// <summary>
        /// 管控特定應用程式介面為W3C標準 替代Feature-Policy<para/>
        /// Permissions-Policy: microphone=(),geolocation=(),camera=()
        /// </summary>
        public string PermissionsPolicy
        {
            get => GetValue("Permissions-Policy");
            set => SetValue("Permissions-Policy", value);
        }

        /// <summary>
        /// Flash的跨網站攻擊防禦<para/>
        /// X-Permitted-Cross-Domain-Policies: none
        /// </summary>
        public string XPermittedCrossDomainPolicies
        {
            get => GetValue("X-Permitted-Cross-Domain-Policies");
            set => SetValue("X-Permitted-Cross-Domain-Policies", value);
        }

        /// <summary>
        /// 保護資訊洩漏<para/>
        /// Referrer-Policy: origin-when-cross-origin
        /// </summary>
        public string ReferrerPolicy
        {
            get => GetValue("Referrer-Policy");
            set => SetValue("Referrer-Policy", value);
        }

        /// <summary>
        /// 防止欺騙 SSL，單位為秒<para/>
        /// Expect-CT: max-age=31536000, enforce
        /// </summary>
        public string ExpectCT
        {
            get => GetValue("Expect-CT");
            set => SetValue("Expect-CT", value);
        }

        public override byte[] GetBytes(IEnumerable<string> Cookies)
            => Encoding.ASCII.GetBytes($"{Protocol} {StatusCode:d} {StatusCode}\r\n{string.Join("\r\n", Datas.Where(i => !string.IsNullOrEmpty(i.Value)).Select(i => i.ToString()))}\r\n\r\n");

        public override string ToString()
            => $"{Protocol} {StatusCode:d} {StatusCode}\r\n{base.ToString()}";

        public static HttpResponseHeader Parse(Stream Stream)
        {
            HttpResponseHeader Header = new HttpResponseHeader();

            StringBuilder LineBuilder = new StringBuilder();
            string ReadLine()
            {
                try
                {
                    while (Stream.TryReadByte(out int c))
                    {
                        if (c == 13)
                        {
                            c = Stream.ReadByte();
                            if (c == 10)
                                break;

                            LineBuilder.Append('\r');
                            LineBuilder.Append((char)c);
                            continue;
                        }

                        LineBuilder.Append((char)c);
                    }

                    return LineBuilder.ToString();
                }
                finally
                {
                    LineBuilder.Clear();
                }
            }

            string Line = ReadLine();

            string[] Arguments = Line.Split(' ');
            Header.Protocol = Arguments[0];
            Header.StatusCode = !Enum.TryParse(Arguments[2], out HttpStatusCode StatusCode) || StatusCode.ToString("d") != Arguments[1] ? HttpStatusCode.BadRequest : StatusCode;

            StringBuilder Builder = new StringBuilder(256);

            char c;
            int Length;
            Line = ReadLine();
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

                Line = ReadLine();
            }

            return Header;
        }

    }
}