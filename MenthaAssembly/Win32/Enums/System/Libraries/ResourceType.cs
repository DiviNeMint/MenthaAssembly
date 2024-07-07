namespace MenthaAssembly.Win32
{
    public enum ResourceType : uint
    {
        /// <summary>
        /// 硬體相依的資料指標資源。
        /// </summary>
        Cursor = 1,

        /// <summary>
        /// 點陣圖資源。
        /// </summary>
        Bitmap = 2,

        /// <summary>
        /// 硬體相依圖示資源。
        /// </summary>
        Icon = 3,

        /// <summary>
        /// 功能表資源。
        /// </summary>
        Menu = 4,

        /// <summary>
        /// 對話方塊。
        /// </summary>
        Dialog = 5,

        /// <summary>
        /// 字串資料表專案。
        /// </summary>
        String = 6,

        /// <summary>
        /// 字型目錄資源。
        /// </summary>
        FontDir = 7,

        /// <summary>
        /// 字型資源。
        /// </summary>
        Font = 8,

        /// <summary>
        /// 快速鍵資料表。
        /// </summary>
        Accelerator = 9,

        /// <summary>
        /// 應用程式定義的資源 (原始資料) 。
        /// </summary>
        RCData = 10,

        /// <summary>
        /// 消息表專案。
        /// </summary>
        MessageTable = 11,

        /// <summary>
        /// 版本資源。
        /// </summary>
        Version = 16,

        /// <summary>
        /// 允許資源編輯工具將字串與 .rc 檔案產生關聯。 一般而言，字串是提供符號名稱的標頭檔名稱。 資源編譯器會剖析字串，否則會忽略值。 例如，1 DLGINCLUDE "MyFile.h"
        /// </summary>
        DLGInclude = 17,

        /// <summary>
        /// 隨插即用資源。
        /// </summary>
        PlugPlay = 19,

        /// <summary>
        /// Vxd。
        /// </summary>
        VXD = 20,

        /// <summary>
        /// 動畫游標。
        /// </summary>
        AniCursor = 21,

        /// <summary>
        /// 動畫圖標。
        /// </summary>
        AniIcon = 22,

        /// <summary>
        /// HTML 資源。
        /// </summary>
        HTML = 23,

        /// <summary>
        /// 並存組件資訊清單。
        /// </summary>
        Manifest = 24,

    }
}