namespace MenthaAssembly.Win32
{
    public enum WindowHitTests
    {
        /// <summary>
        /// 在螢幕背景上或 windows (與 NOWHERE 相同的分隔線上，不同之處在于 DefWindowProc 函式會產生系統嗶聲，指出) 錯誤。
        /// </summary>
        Error = -2,

        /// <summary>
        /// 在目前由相同執行緒中的另一個視窗所涵蓋的視窗中 (會將訊息傳送至相同執行緒中的基礎視窗，直到其中一個傳回未 TRANSPARENT) 的程式碼為止。
        /// </summary>
        Transparent = -1,

        /// <summary>
        /// 在螢幕背景或視窗之間的分隔線上。
        /// </summary>
        NowHere = 0,

        /// <summary>
        /// 在工作區中。
        /// </summary>
        Client = 1,

        /// <summary>
        /// 在標題列中。
        /// </summary>
        Caption = 2,

        /// <summary>
        /// 在 [視窗] 功能表或子視窗的 [ 關閉 ] 按鈕中。
        /// </summary>
        SysMenu = 3,

        /// <summary>
        /// 在 [大小] 方塊中。
        /// </summary>
        Size = 4,

        /// <summary>
        /// 在功能表中。
        /// </summary>
        Menu = 5,

        /// <summary>
        /// 水準捲軸。
        /// </summary>
        HScroll = 6,

        /// <summary>
        /// 在垂直捲動條中。
        /// </summary>
        VScroll = 7,

        /// <summary>
        /// 在 [ 最小化 ] 按鈕中。
        /// </summary>
        MinButton = 8,

        /// <summary>
        /// 在 [ 最大化 ] 按鈕中。
        /// </summary>
        MaxButton = 9,

        /// <summary>
        /// 在可調整大小之視窗的左邊框線中 (使用者可以按一下滑鼠，以水平方式調整視窗的大小) 。
        /// </summary>
        Left = 10,

        /// <summary>
        /// 在可調整大小之視窗的右框線中 (使用者可以按一下滑鼠，以水平方式調整視窗的大小) 。
        /// </summary>
        Right = 11,

        /// <summary>
        /// 在視窗的上水準框線中。
        /// </summary>
        Top = 12,

        /// <summary>
        /// 在視窗框線的左上角。
        /// </summary>
        TopLeft = 13,

        /// <summary>
        /// 在視窗框線的右上角。
        /// </summary>
        TopRight = 14,

        /// <summary>
        /// 在可調整大小之視窗的水準框線 (使用者可以按一下滑鼠，以垂直方式調整視窗的大小) 。
        /// </summary>
        Bottom = 15,

        /// <summary>
        /// 在可調整大小之視窗框線的左下角 (使用者可以按一下滑鼠，以對角線方式調整視窗的大小) 。
        /// </summary>
        BottomLeft = 16,

        /// <summary>
        /// 在可調整大小之視窗框線的右下角 (使用者可以按一下滑鼠，以對角線方式調整視窗的大小) 。
        /// </summary>
        BottomRight = 17,

        /// <summary>
        /// 在沒有調整大小框線的視窗框線中。
        /// </summary>
        Border = 18,

        /// <summary>
        /// 在 [ 關閉 ] 按鈕中。
        /// </summary>
        Close = 20,

        /// <summary>
        /// 在 [說明] 按鈕中。
        /// </summary>
        Help = 21,
    }
}
