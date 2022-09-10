namespace MenthaAssembly.Devices
{
    public enum PaperTypes : ushort
    {
        /// <summary>
        ///紙張大小由使用者定義。
        /// </summary>
        Custom = 0,

        /// <summary>
        ///Letter 紙張 (8.5x11 英吋)。
        /// </summary>
        Letter = 1,

        /// <summary>
        ///Letter Small 紙張 (8.5x11 英吋)。
        /// </summary>
        LetterSmall = 2,

        /// <summary>
        ///Tabloid 紙張 (11x17 英吋)。
        /// </summary>
        Tabloid = 3,

        /// <summary>
        ///Ledger 紙張 (17x11 英吋)。
        /// </summary>
        Ledger = 4,

        /// <summary>
        ///Legal 紙張 (8.5x14 英吋)。
        /// </summary>
        Legal = 5,

        /// <summary>
        ///Statement 紙張 (5.5x8.5 英吋)。
        /// </summary>
        Statement = 6,

        /// <summary>
        ///Executive 紙張 (7.25x10.5 英吋)。
        /// </summary>
        Executive = 7,

        /// <summary>
        ///A3 紙張 (297x420 公釐)。
        /// </summary>
        A3 = 8,

        /// <summary>
        ///A4 紙張 (210x297 公釐)。
        /// </summary>
        A4 = 9,

        /// <summary>
        ///A4 Small 紙張 (210x297 公釐)。
        /// </summary>
        A4Small = 10,

        /// <summary>
        ///A5 紙張 (148x210 公釐)。
        /// </summary>
        A5 = 11,

        /// <summary>
        ///B4 紙張 (250x353 公釐)。
        /// </summary>
        B4 = 12,

        /// <summary>
        ///B5 紙張 (176x250 公釐)。
        /// </summary>
        B5 = 13,

        /// <summary>
        ///Folio 紙張 (8.5x13 英吋)。
        /// </summary>
        Folio = 14,

        /// <summary>
        ///Quarto 紙張 (215x275 公釐)。
        /// </summary>
        Quarto = 15,

        /// <summary>
        ///標準紙張 (10x14 英吋)。
        /// </summary>
        Standard10x14 = 16,

        /// <summary>
        ///標準紙張 (11x17 英吋)。
        /// </summary>
        Standard11x17 = 17,

        /// <summary>
        ///便條紙 (8.5x11 英吋)。
        /// </summary>
        Note = 18,

        /// <summary>
        ///#9 Envelope (3.875x8.875 英吋)。
        /// </summary>
        Number9Envelope = 19,

        /// <summary>
        ///#10 Envelope (4.125x9.5 英吋)。
        /// </summary>
        Number10Envelope = 20,

        /// <summary>
        ///#11 Envelope (4.5x 10.375 英吋)。
        /// </summary>
        Number11Envelope = 21,

        /// <summary>
        ///#12 Envelope (4.75x11 英吋)。
        /// </summary>
        Number12Envelope = 22,

        /// <summary>
        ///#14 Envelope (5x11.5 英吋)。
        /// </summary>
        Number14Envelope = 23,

        /// <summary>
        ///C 紙張 (17x22 英吋)。
        /// </summary>
        CSheet = 24,

        /// <summary>
        ///D 紙張 (22x34 英吋)。
        /// </summary>
        DSheet = 25,

        /// <summary>
        ///E 紙張 (34x44 英吋)。
        /// </summary>
        ESheet = 26,

        /// <summary>
        ///DL Envelope (110x220 公釐)。
        /// </summary>
        DLEnvelope = 27,

        /// <summary>
        ///C5 Envelope (162x229 公釐)。
        /// </summary>
        C5Envelope = 28,

        /// <summary>
        ///C3 Envelope (324x458 公釐)。
        /// </summary>
        C3Envelope = 29,

        /// <summary>
        ///C4 Envelope (229x324 公釐)。
        /// </summary>
        C4Envelope = 30,

        /// <summary>
        ///C6 Envelope (114x162 公釐)。
        /// </summary>
        C6Envelope = 31,

        /// <summary>
        ///C65 Envelope (114x229 公釐)。
        /// </summary>
        C65Envelope = 32,

        /// <summary>
        ///B4 Envelope (250x353 公釐)。
        /// </summary>
        B4Envelope = 33,

        /// <summary>
        ///B5 Envelope (176x250 公釐)。
        /// </summary>
        B5Envelope = 34,

        /// <summary>
        ///B6 Envelope (176x125 公釐)。
        /// </summary>
        B6Envelope = 35,

        /// <summary>
        ///Italy Envelope (110x230 公釐)。
        /// </summary>
        ItalyEnvelope = 36,

        /// <summary>
        ///Monarch Envelope (3.875x7.5 英吋)。
        /// </summary>
        MonarchEnvelope = 37,

        /// <summary>
        ///6 3/4 Envelope (3.625x6.5 英吋)。
        /// </summary>
        PersonalEnvelope = 38,

        /// <summary>
        ///US Standard Fanfold (14.875x11 英吋)。
        /// </summary>
        USStandardFanfold = 39,

        /// <summary>
        ///German Standard Fanfold (9.275x12 英吋)。
        /// </summary>
        GermanStandardFanfold = 40,

        /// <summary>
        ///German Legal Fanfold (8.5x13 英吋)。
        /// </summary>
        GermanLegalFanfold = 41,

        /// <summary>
        ///ISO B4 (250x353 公釐)。
        /// </summary>
        IsoB4 = 42,

        /// <summary>
        ///Japanese Postcard (100x148 公釐)。
        /// </summary>
        JapanesePostcard = 43,

        /// <summary>
        ///標準紙張 (9x11 英吋)。
        /// </summary>
        Standard9x11 = 44,

        /// <summary>
        ///標準紙張 (10x11 英吋)。
        /// </summary>
        Standard10x11 = 45,

        /// <summary>
        ///標準紙張 (15x11 英吋)。
        /// </summary>
        Standard15x11 = 46,

        /// <summary>
        ///Invitation Envelope (220x220 公釐)。
        /// </summary>
        InviteEnvelope = 47,

        /// <summary>
        ///Letter Extra 紙張 (9.275x12 英吋)。 這是 PostScript 驅動程式的特定值，僅供 Linotronic 印表機使用以節省紙張。
        /// </summary>
        LetterExtra = 50,

        /// <summary>
        ///Legal Extra 紙張 (9.275x15 英吋)。 這是 PostScript 驅動程式的特定值，僅供 Linotronic 印表機使用以節省紙張。
        /// </summary>
        LegalExtra = 51,

        /// <summary>
        ///Tabloid Extra 紙張 (11.69x18 英吋)。 這是 PostScript 驅動程式的特定值，僅供 Linotronic 印表機使用以節省紙張。
        /// </summary>
        TabloidExtra = 52,

        /// <summary>
        ///A4 Extra 紙張 (236x322 公釐)。 這是 PostScript 驅動程式的特定值，僅供 Linotronic 印表機使用以協助節省紙張。
        /// </summary>
        A4Extra = 53,

        /// <summary>
        ///Letter Transverse 紙張 (8.275x11 英吋)。
        /// </summary>
        LetterTransverse = 54,

        /// <summary>
        ///A4 Transverse 紙張 (210x297 公釐)。
        /// </summary>
        A4Transverse = 55,

        /// <summary>
        ///Letter Extra Transverse 紙張 (9.275x12 英吋)。
        /// </summary>
        LetterExtraTransverse = 56,

        /// <summary>
        ///SuperA/SuperA/A4 紙張 (227x356 公釐)。
        /// </summary>
        APlus = 57,

        /// <summary>
        ///SuperB/SuperB/A3 紙張 (305x487 公釐)。
        /// </summary>
        BPlus = 58,

        /// <summary>
        ///Letter Plus 紙張 (8.5x12.69 英吋)。
        /// </summary>
        LetterPlus = 59,

        /// <summary>
        ///A4 Plus 紙張 (210x330 公釐)。
        /// </summary>
        A4Plus = 60,

        /// <summary>
        ///A5 Transverse 紙張 (148x210 公釐)。
        /// </summary>
        A5Transverse = 61,

        /// <summary>
        ///JIS B5 Transverse 紙張 (182x257 公釐)。
        /// </summary>
        B5Transverse = 62,

        /// <summary>
        ///A3 Extra 紙張 (322x445 公釐)。
        /// </summary>
        A3Extra = 63,

        /// <summary>
        ///A5 Extra 紙張 (174x235 公釐)。
        /// </summary>
        A5Extra = 64,

        /// <summary>
        ///ISO B5 Extra 紙張 (201x276 公釐)。
        /// </summary>
        B5Extra = 65,

        /// <summary>
        ///A2 紙張 (420x594 公釐)。
        /// </summary>
        A2 = 66,

        /// <summary>
        ///A3 Transverse 紙張 (297x420 公釐)。
        /// </summary>
        A3Transverse = 67,

        /// <summary>
        ///A3 Extra Transverse 紙張 (322x445 公釐)。
        /// </summary>
        A3ExtraTransverse = 68,

        /// <summary>
        ///Japanese Double Postcard (200x148 公釐)。 需要 Windows 98、Windows NT 4.0 或更新版本。
        /// </summary>
        JapaneseDoublePostcard = 69,

        /// <summary>
        ///A6 紙張 (105x148 公釐)。 需要 Windows 98、Windows NT 4.0 或更新版本。
        /// </summary>
        A6 = 70,

        /// <summary>
        ///Japanese Kaku #2 Envelope。 需要 Windows 98、Windows NT 4.0 或更新版本。
        /// </summary>
        JapaneseEnvelopeKakuNumber2 = 71,

        /// <summary>
        ///Japanese Kaku #3 Envelope。 需要 Windows 98、Windows NT 4.0 或更新版本。
        /// </summary>
        JapaneseEnvelopeKakuNumber3 = 72,

        /// <summary>
        ///Japanese Chou #3 Envelope。 需要 Windows 98、Windows NT 4.0 或更新版本。
        /// </summary>
        JapaneseEnvelopeChouNumber3 = 73,

        /// <summary>
        ///Japanese Chou #4 Envelope。 需要 Windows 98、Windows NT 4.0 或更新版本。
        /// </summary>
        JapaneseEnvelopeChouNumber4 = 74,

        /// <summary>
        ///Letter Rotated 紙張 (11x8.5 英吋)。
        /// </summary>
        LetterRotated = 75,

        /// <summary>
        ///A3 Rotated 紙張 (420x297 公釐)。
        /// </summary>
        A3Rotated = 76,

        /// <summary>
        ///A4 Rotated 紙張 (297x210 公釐)。 需要 Windows 98、Windows NT 4.0 或更新版本。
        /// </summary>
        A4Rotated = 77,

        /// <summary>
        ///A5 Rotated 紙張 (210x148 公釐)。 需要 Windows 98、Windows NT 4.0 或更新版本。
        /// </summary>
        A5Rotated = 78,

        /// <summary>
        ///JIS B4 Rotated 紙張 (364x257 公釐)。 需要 Windows 98、Windows NT 4.0 或更新版本。
        /// </summary>
        B4JisRotated = 79,

        /// <summary>
        ///JIS B5 Rotated 紙張 (257x182 公釐)。 需要 Windows 98、Windows NT 4.0 或更新版本。
        /// </summary>
        B5JisRotated = 80,

        /// <summary>
        ///Japanese Rotated Postcard (148x100 公釐)。 需要 Windows 98、Windows NT 4.0 或更新版本。
        /// </summary>
        JapanesePostcardRotated = 81,

        /// <summary>
        ///Japanese Rotated Double Postcard (148x200 公釐)。 需要 Windows 98、Windows NT 4.0 或更新版本。
        /// </summary>
        JapaneseDoublePostcardRotated = 82,

        /// <summary>
        ///A6 Rotated 紙張 (148x105 公釐)。 需要 Windows 98、Windows NT 4.0 或更新版本。
        /// </summary>
        A6Rotated = 83,

        /// <summary>
        ///Japanese Rotated Kaku #2 Envelope。 需要 Windows 98、Windows NT 4.0 或更新版本。
        /// </summary>
        JapaneseEnvelopeKakuNumber2Rotated = 84,

        /// <summary>
        ///Japanese Rotated Kaku #3 Envelope。 需要 Windows 98、Windows NT 4.0 或更新版本。
        /// </summary>
        JapaneseEnvelopeKakuNumber3Rotated = 85,

        /// <summary>
        ///Japanese Rotated Chou #3 Envelope。 需要 Windows 98、Windows NT 4.0 或更新版本。
        /// </summary>
        JapaneseEnvelopeChouNumber3Rotated = 86,

        /// <summary>
        ///Japanese Rotated Chou #4 Envelope。 需要 Windows 98、Windows NT 4.0 或更新版本。
        /// </summary>
        JapaneseEnvelopeChouNumber4Rotated = 87,

        /// <summary>
        ///JIS B6 紙張 (128x182 公釐)。 需要 Windows 98、Windows NT 4.0 或更新版本。
        /// </summary>
        B6Jis = 88,

        /// <summary>
        ///JIS B6 Rotated 紙張 (182x128 公釐)。 需要 Windows 98、Windows NT 4.0 或更新版本。
        /// </summary>
        B6JisRotated = 89,

        /// <summary>
        ///標準紙張 (14.875x11 英吋)。 需要 Windows 98、Windows NT 4.0 或更新版本。
        /// </summary>
        Standard12x11 = 90,

        /// <summary>
        ///Japanese You #4 Envelope。 需要 Windows 98、Windows NT 4.0 或更新版本。
        /// </summary>
        JapaneseEnvelopeYouNumber4 = 91,

        /// <summary>
        ///Japanese You #4 Rotated Envelope。 需要 Windows 98、Windows NT 4.0 或更新版本。
        /// </summary>
        JapaneseEnvelopeYouNumber4Rotated = 92,

        /// <summary>
        ///16K 紙張 (146x215 公釐)。 需要 Windows 98、Windows NT 4.0 或更新版本。
        /// </summary>
        Prc16K = 93,

        /// <summary>
        ///32K 紙張 (97x151 公釐)。 需要 Windows 98、Windows NT 4.0 或更新版本。
        /// </summary>
        Prc32K = 94,

        /// <summary>
        ///32K Big 紙張 (97x151 公釐)。 需要 Windows 98、Windows NT 4.0 或更新版本。
        /// </summary>
        Prc32KBig = 95,

        /// <summary>
        ///#1 Envelope (102x165 公釐)。 需要 Windows 98、Windows NT 4.0 或更新版本。
        /// </summary>
        PrcEnvelopeNumber1 = 96,

        /// <summary>
        ///#2 Envelope (102x176 公釐)。 需要 Windows 98、Windows NT 4.0 或更新版本。
        /// </summary>
        PrcEnvelopeNumber2 = 97,

        /// <summary>
        ///#3 Envelope (125x176 公釐)。 需要 Windows 98、Windows NT 4.0 或更新版本。
        /// </summary>
        PrcEnvelopeNumber3 = 98,

        /// <summary>
        ///#4 Envelope (110x208 公釐)。 需要 Windows 98、Windows NT 4.0 或更新版本。
        /// </summary>
        PrcEnvelopeNumber4 = 99,

        /// <summary>
        ///#5 Envelope (110x220 公釐)。 需要 Windows 98、Windows NT 4.0 或更新版本。
        /// </summary>
        PrcEnvelopeNumber5 = 100,

        /// <summary>
        ///#6 Envelope (120x230 公釐)。 需要 Windows 98、Windows NT 4.0 或更新版本。
        /// </summary>
        PrcEnvelopeNumber6 = 101,

        /// <summary>
        ///#7 Envelope (160x230 公釐)。 需要 Windows 98、Windows NT 4.0 或更新版本。
        /// </summary>
        PrcEnvelopeNumber7 = 102,

        /// <summary>
        ///#8 Envelope (120x309 公釐)。 需要 Windows 98、Windows NT 4.0 或更新版本。
        /// </summary>
        PrcEnvelopeNumber8 = 103,

        /// <summary>
        ///#9 Envelope (229x324 公釐)。 需要 Windows 98、Windows NT 4.0 或更新版本。
        /// </summary>
        PrcEnvelopeNumber9 = 104,

        /// <summary>
        ///#10 Envelope (324x458 公釐)。 需要 Windows 98、Windows NT 4.0 或更新版本。
        /// </summary>
        PrcEnvelopeNumber10 = 105,

        /// <summary>
        ///16K Rotated 紙張 (146x215 公釐)。 需要 Windows 98、Windows NT 4.0 或更新版本。
        /// </summary>
        Prc16KRotated = 106,

        /// <summary>
        ///32K Rotated 紙張 (97x151 公釐)。 需要 Windows 98、Windows NT 4.0 或更新版本。
        /// </summary>
        Prc32KRotated = 107,

        /// <summary>
        ///32K Big Rotated 紙張 (97x151 公釐)。 需要 Windows 98、Windows NT 4.0 或更新版本。
        /// </summary>
        Prc32KBigRotated = 108,

        /// <summary>
        ///#1 Rotated Envelope (165x102 公釐)。 需要 Windows 98、Windows NT 4.0 或更新版本。
        /// </summary>
        PrcEnvelopeNumber1Rotated = 109,

        /// <summary>
        ///#2 Rotated Envelope (176x102 公釐)。 需要 Windows 98、Windows NT 4.0 或更新版本。
        /// </summary>
        PrcEnvelopeNumber2Rotated = 110,

        /// <summary>
        ///#3 Rotated Envelope (176x125 公釐)。 需要 Windows 98、Windows NT 4.0 或更新版本。
        /// </summary>
        PrcEnvelopeNumber3Rotated = 111,

        /// <summary>
        ///#4 Rotated Envelope (208x110 公釐)。 需要 Windows 98、Windows NT 4.0 或更新版本。
        /// </summary>
        PrcEnvelopeNumber4Rotated = 112,

        /// <summary>
        ///#5 Rotated Envelope (220x110 公釐)。 需要 Windows 98、Windows NT 4.0 或更新版本。
        /// </summary>
        PrcEnvelopeNumber5Rotated = 113,

        /// <summary>
        ///#6 Rotated Envelope (230x120 公釐)。 需要 Windows 98、Windows NT 4.0 或更新版本。
        /// </summary>
        PrcEnvelopeNumber6Rotated = 114,

        /// <summary>
        ///#7 Rotated Envelope (230x160 公釐)。 需要 Windows 98、Windows NT 4.0 或更新版本。
        /// </summary>
        PrcEnvelopeNumber7Rotated = 115,

        /// <summary>
        ///#8 Rotated Envelope (309x120 公釐)。 需要 Windows 98、Windows NT 4.0 或更新版本。
        /// </summary>
        PrcEnvelopeNumber8Rotated = 116,

        /// <summary>
        ///#9 Rotated Envelope (324x229 公釐)。 需要 Windows 98、Windows NT 4.0 或更新版本。
        /// </summary>
        PrcEnvelopeNumber9Rotated = 117,

        /// <summary>
        ///#10 Rotated Envelope (458x324 公釐)。 需要 Windows 98、Windows NT 4.0 或更新版本。
        /// </summary>
        PrcEnvelopeNumber10Rotated = 118

    }
}