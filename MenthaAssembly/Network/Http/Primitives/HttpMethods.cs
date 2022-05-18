namespace MenthaAssembly.Network
{
    public enum HttpMethods : byte
    {
        /// <summary>
        /// 未知方法
        /// </summary>
        UNKNOWN = 0,

        /// <summary>
        /// GET 方法請求展示指定資源。使用 GET 的請求只應用於取得資料。
        /// </summary>
        GET = 1,

        /// <summary>
        /// POST 方法用於提交指定資源的實體，通常會改變伺服器的狀態或副作用（side effect）。
        /// </summary>
        POST = 2,

        /// <summary>
        /// HEAD 方法請求與 GET 方法相同的回應，但它沒有回應主體（response body）。
        /// </summary>
        HEAD = 3,

        /// <summary>
        /// PUT 方法會取代指定資源所酬載請求（request payload）的所有表現。
        /// </summary>
        PUT = 4,

        /// <summary>
        /// DELETE 方法會刪除指定資源.
        /// </summary>
        DELETE = 5,

        /// <summary>
        /// CONNECT 方法會和指定資源標明的伺服器之間，建立隧道（tunnel）。
        /// </summary>
        CONNECT = 6,

        /// <summary>
        /// OPTIONS 方法描述指定資源的溝通方法（communication option）。
        /// </summary>
        OPTIONS = 7,

        /// <summary>
        /// TRACE 方法會與指定資源標明的伺服器之間，執行迴路返回測試（loop-back test）。
        /// </summary>
        TRACE = 8,

        /// <summary>
        /// PATCH 方法套用指定資源的部份修改。
        /// </summary>
        PATCH = 9,
    }
}
