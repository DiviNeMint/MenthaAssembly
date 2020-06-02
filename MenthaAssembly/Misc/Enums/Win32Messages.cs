﻿namespace System.ComponentModel
{
    public enum Win32Messages
    {
        WM_Null = 0,
        WM_Create = 1,
        WM_Destroy = 2,
        WM_Move = 3,
        WM_Size = 5,
        WM_Activate = 6,
        WM_SetFocus = 7,
        WM_KillFocus = 8,
        WM_Enable = 10,
        WM_SetRedraw = 11,
        WM_SetText = 12,
        WM_GetText = 13,
        WM_GetTextLength = 14,
        WM_Paint = 15,
        WM_Close = 16,
        WM_QueryEndSession = 17,
        WM_Quit = 18,
        WM_QueryOpen = 19,
        WM_ERASEBKGND = 20,
        WM_SYSColorChange = 21,
        WM_EndSession = 22,
        WM_ShowWindow = 24,
        WM_CTLColor = 25,
        WM_WinIniChange = 26,
        WM_DevModeChange = 27,
        WM_ActivateAPP = 28,
        WM_FontChange = 29,
        WM_TimeChange = 30,
        WM_CancelMode = 31,
        WM_SetCursor = 32,
        WM_MouseActivate = 33,
        WM_ChildActivate = 34,
        WM_QueueSYNC = 35,
        WM_GetMinMaxInfo = 36,
        WM_PaintIcon = 38,
        WM_IconERASEBKGND = 39,
        WM_NextDLGCTL = 40,
        WM_SPOOLERStatus = 42,
        WM_DrawItem = 43,
        WM_MeasureItem = 44,
        WM_DeleteItem = 45,
        WM_VKeyToItem = 46,
        WM_CharToItem = 47,
        WM_SetFont = 48,
        WM_GetFont = 49,
        WM_SetHotKey = 50,
        WM_GetHotKey = 51,
        WM_QueryDragIcon = 55,
        WM_CompareItem = 57,
        WM_GetObject = 61,
        WM_COMPACTING = 65,
        WM_COMMNotify = 68,
        WM_WindowPOSChanging = 70,
        WM_WindowPOSChanged = 71,
        WM_Power = 72,
        WM_CopyGlobalData = 73,
        WM_CopyData = 74,
        WM_CancelJOURNAL = 75,
        WM_Notify = 78,
        WM_InputLANGChangeRequest = 80,
        WM_InputLANGChange = 81,
        WM_TCARD = 82,
        WM_Help = 83,
        WM_UserChanged = 84,
        WM_NotifyFormat = 85,
        WM_ContextMenu = 123,
        WM_StyleChanging = 124,
        WM_StyleChanged = 125,
        WM_DisplayChange = 126,
        WM_GetIcon = 127,
        WM_SetIcon = 128,
        WM_NCCreate = 129,
        WM_NCDestroy = 130,
        WM_NCCALCSize = 131,
        WM_NCHitTest = 132,
        WM_NCPaint = 133,
        WM_NCActivate = 134,
        WM_GetDLGCode = 135,
        WM_SYNCPaint = 136,
        WM_NCMouseMove = 160,
        WM_NCLButtonDown = 161,
        WM_NCLButtonUp = 162,
        WM_NCLButtonDBLCLK = 163,
        WM_NCRButtonDown = 164,
        WM_NCRButtonUp = 165,
        WM_NCRButtonDBLCLK = 166,
        WM_NCMButtonDown = 167,
        WM_NCMButtonUp = 168,
        WM_NCMButtonDBLCLK = 169,
        WM_NCXButtonDown = 171,
        WM_NCXButtonUp = 172,
        WM_NCXButtonDBLCLK = 173,
        EM_GetSEL = 176,
        EM_SetSEL = 177,
        EM_GetRECT = 178,
        EM_SetRECT = 179,
        EM_SetRECTNP = 180,
        EM_Scroll = 181,
        EM_LineScroll = 182,
        EM_ScrollCaret = 183,
        EM_GetModify = 185,
        EM_SetModify = 187,
        EM_GetLineCount = 188,
        EM_LineIndex = 189,
        EM_SetHandle = 190,
        EM_GetHandle = 191,
        EM_GetThumb = 192,
        EM_LineLength = 193,
        EM_ReplaceSEL = 194,
        EM_SetFont = 195,
        EM_GetLine = 196,
        EM_LimitText = 197,
        EM_SetLimitText = 197,
        EM_CanUndo = 198,
        EM_Undo = 199,
        EM_FMTLineS = 200,
        EM_LineFromChar = 201,
        EM_SetWordBreak = 202,
        EM_SetTabStops = 203,
        EM_SetPassWordChar = 204,
        EM_EmptyUndoBuffer = 205,
        EM_GetFirstVisibleLine = 206,
        EM_SetReadOnly = 207,
        EM_SetWordBreakProc = 209,
        EM_GetWordBreakProc = 209,
        EM_GetPassWordChar = 210,
        EM_SetMargins = 211,
        EM_GetMargins = 212,
        EM_GetLimitText = 213,
        EM_POSFromChar = 214,
        EM_CharFromPOS = 215,
        EM_SetIMEStatus = 216,
        EM_GetIMEStatus = 217,
        SBM_SetPOS = 224,
        SBM_GetPOS = 225,
        SBM_SetRange = 226,
        SBM_GetRange = 227,
        SBM_Enable_Arrows = 228,
        SBM_SetRangeRedraw = 230,
        SBM_SetScrollInfo = 233,
        SBM_GetScrollInfo = 234,
        SBM_GetScrollBarInfo = 235,
        BM_GetCheck = 240,
        BM_SetCheck = 241,
        BM_GetState = 242,
        BM_SetState = 243,
        BM_SetStyle = 244,
        BM_Click = 245,
        BM_GetImage = 246,
        BM_SetImage = 247,
        BM_SetDontClick = 248,
        WM_Input = 255,
        WM_KeyDown = 256,
        WM_KeyFirst = 256,
        WM_KeyUp = 257,
        WM_Char = 258,
        WM_DEADChar = 259,
        WM_SYSKeyDown = 260,
        WM_SYSKeyUp = 261,
        WM_SYSChar = 262,
        WM_SYSDEADChar = 263,
        WM_UNIChar = 265,
        WM_KeyLAST = 265,
        WM_WNT_ConvertRequestEX = 265,
        WM_ConvertRequest = 266,
        WM_ConvertResult = 267,
        WM_INTERIM = 268,
        WM_IME_StartComposition = 269,
        WM_IME_EndComposition = 270,
        WM_IME_Composition = 271,
        WM_IME_KeyLAST = 271,
        WM_InitDIALOG = 272,
        WM_Command = 273,
        WM_SYSCommand = 274,
        WM_Timer = 275,
        WM_HScroll = 276,
        WM_VScroll = 277,
        WM_InitMenu = 278,
        WM_InitMenuPopup = 279,
        WM_SYSTimer = 280,
        WM_MenuSelect = 287,
        WM_MenuChar = 288,
        WM_ENTERIDLE = 289,
        WM_MenuRButtonUp = 290,
        WM_MenuDrag = 291,
        WM_MenuGetObject = 292,
        WM_UNInitMenupOPUp = 293,
        WM_MenuCommand = 294,
        WM_ChangeUIstate = 295,
        WM_UpdateUIstate = 296,
        WM_QueryUIstate = 297,
        WM_CTLColorMSGBox = 306,
        WM_CTLColorEdit = 307,
        WM_CTLColorListBox = 308,
        WM_CTLColorBTN = 309,
        WM_CTLColorDLG = 310,
        WM_CTLColorScrollBar = 311,
        WM_CTLColorStatic = 312,
        WM_MouseFirst = 512,
        WM_MouseMove = 512,
        WM_LButtonDown = 513,
        WM_LButtonUp = 514,
        WM_LButtonDBLCLK = 515,
        WM_RButtonDown = 516,
        WM_RButtonUp = 517,
        WM_RButtonDBLCLK = 518,
        WM_MButtonDown = 519,
        WM_MButtonUp = 520,
        WM_MButtonDBLCLK = 521,
        WM_MouseLAST = 521,
        WM_MouseWheel = 522,
        WM_XButtonDown = 523,
        WM_XButtonUp = 524,
        WM_XButtonDBLCLK = 525,
        WM_MouseHWheel = 526,
        WM_ParentNotify = 528,
        WM_ENTERMenuLoop = 529,
        WM_EXITMenuLoop = 530,
        WM_NextMenu = 531,
        WM_Sizing = 532,
        WM_CaptureChanged = 533,
        WM_Moving = 534,
        WM_PowerBROADCAST = 536,
        WM_DeviceChange = 537,
        WM_MDICreate = 544,
        WM_MDIDestroy = 545,
        WM_MDIActivate = 546,
        WM_MDIRestore = 547,
        WM_MDINext = 548,
        WM_MDIMaximize = 549,
        WM_MDITile = 550,
        WM_MDICASCADE = 551,
        WM_MDIIconArrange = 552,
        WM_MDIGetActive = 553,
        WM_MDIsetMenu = 560,
        WM_ENTERSizeMove = 561,
        WM_EXITSizeMove = 562,
        WM_DROPFileS = 563,
        WM_MDIRefreshMenu = 564,
        WM_IME_Report = 640,
        WM_IME_SetContext = 641,
        WM_IME_Notify = 642,
        WM_IME_Control = 643,
        WM_IME_CompositionFULL = 644,
        WM_IME_Select = 645,
        WM_IME_Char = 646,
        WM_IME_Request = 648,
        WM_IMEKeyDown = 656,
        WM_IME_KeyDown = 656,
        WM_IMEKeyUp = 657,
        WM_IME_KeyUp = 657,
        WM_NCMouseHover = 672,
        WM_MouseHover = 673,
        WM_NCMouseLeave = 674,
        WM_MouseLeave = 675,
        WM_CUT = 768,
        WM_Copy = 769,
        WM_Paste = 770,
        WM_Clear = 771,
        WM_Undo = 772,
        WM_RenderFormat = 773,
        WM_RenderAllFormats = 774,
        WM_DestroyClipboard = 775,
        WM_DrawClipboard = 776,
        WM_PaintClipboard = 777,
        WM_VScrollClipboard = 778,
        WM_SizeClipboard = 779,
        WM_ASKCBFormatName = 780,
        WM_ChangeCBChain = 781,
        WM_HScrollClipboard = 782,
        WM_QueryNewPalette = 783,
        WM_PaletteIsChanging = 784,
        WM_PaletteChanged = 785,
        WM_HotKey = 786,
        WM_Print = 791,
        WM_PrintClient = 792,
        WM_APPCommand = 793,
        WM_HANDHELDFirst = 856,
        WM_HANDHELDLAST = 863,
        WM_AFXFirst = 864,
        WM_AFXLAST = 895,
        WM_PenWINFirst = 896,
        WM_RCResult = 897,
        WM_HOOKRCResult = 898,
        WM_GlobalRCChange = 899,
        WM_PenMIsCInfo = 899,
        WM_SKB = 900,
        WM_HEditCTL = 901,
        WM_PenCTL = 901,
        WM_PenMIsC = 902,
        WM_CTLInit = 903,
        WM_PenEvent = 904,
        WM_PenWINLAST = 911,
        DDM_SetFMT = 1024,
        DM_GetDEFID = 1024,
        NIN_Select = 1024,
        TBM_GetPOS = 1024,
        WM_PSD_PageSetUpDLG = 1024,
        WM_User = 1024,
        CBEM_InsertItemA = 1025,
        DDM_Draw = 1025,
        DM_SetDEFID = 1025,
        HKM_SetHotKey = 1025,
        PBM_SetRange = 1025,
        RB_InsertBandA = 1025,
        SB_SetTextA = 1025,
        TB_EnableButton = 1025,
        TBM_GetRangeMin = 1025,
        TTM_Activate = 1025,
        WM_ChooseFont_GetLOGFont = 1025,
        WM_PSD_FULLPageRECT = 1025,
        CBEM_SetImageList = 1026,
        DDM_Close = 1026,
        DM_REPosition = 1026,
        HKM_GetHotKey = 1026,
        PBM_SetPOS = 1026,
        RB_DeleteBand = 1026,
        SB_GetTextA = 1026,
        TB_CheckButton = 1026,
        TBM_GetRangeMax = 1026,
        WM_PSD_MinMarginRECT = 1026,
        CBEM_GetImageList = 1027,
        DDM_Begin = 1027,
        HKM_SetRules = 1027,
        PBM_DELTAPOS = 1027,
        RB_GetBarInfo = 1027,
        SB_GetTextLengthA = 1027,
        TBM_GetTIC = 1027,
        TB_PressButton = 1027,
        TTM_SetDelayTime = 1027,
        WM_PSD_MarginRECT = 1027,
        CBEM_GetItemA = 1028,
        DDM_End = 1028,
        PBM_SetSTEP = 1028,
        RB_SetBarInfo = 1028,
        SB_SetPARTS = 1028,
        TB_HideButton = 1028,
        TBM_SetTIC = 1028,
        TTM_AddToolA = 1028,
        WM_PSD_GREEKTextRECT = 1028,
        CBEM_SetItemA = 1029,
        PBM_STEPIT = 1029,
        TB_Indeterminate = 1029,
        TBM_SetPOS = 1029,
        TTM_DELToolA = 1029,
        WM_PSD_ENVSTAMPRECT = 1029,
        CBEM_GetCOMBOControl = 1030,
        PBM_SetRange32 = 1030,
        RB_SetBandInfoA = 1030,
        SB_GetPARTS = 1030,
        TB_MarkButton = 1030,
        TBM_SetRange = 1030,
        TTM_NewToolRECTA = 1030,
        WM_PSD_YAFULLPageRECT = 1030,
        CBEM_GetEditControl = 1031,
        PBM_GetRange = 1031,
        RB_SetParent = 1031,
        SB_GetBorders = 1031,
        TBM_SetRangeMin = 1031,
        TTM_RELAYEvent = 1031,
        CBEM_SetEXStyle = 1032,
        PBM_GetPOS = 1032,
        RB_HitTest = 1032,
        SB_SetMinHeight = 1032,
        TBM_SetRangeMax = 1032,
        TTM_GetToolInfoA = 1032,
        CBEM_GetEXStyle = 1033,
        CBEM_GetExtendedStyle = 1033,
        PBM_SetBarColor = 1033,
        RB_GetRECT = 1033,
        SB_Simple = 1033,
        TB_IsButtonEnableD = 1033,
        TBM_ClearTICS = 1033,
        TTM_SetToolInfoA = 1033,
        CBEM_HasEditChanged = 1034,
        RB_InsertBandW = 1034,
        SB_GetRECT = 1034,
        TB_IsButtonCheckED = 1034,
        TBM_SetSEL = 1034,
        TTM_HitTestA = 1034,
        WIZ_QueryNUMPageS = 1034,
        CBEM_InsertItemW = 1035,
        RB_SetBandInfoW = 1035,
        SB_SetTextW = 1035,
        TB_IsButtonPressED = 1035,
        TBM_SetSELStart = 1035,
        TTM_GetTextA = 1035,
        WIZ_Next = 1035,
        CBEM_SetItemW = 1036,
        RB_GetBandCount = 1036,
        SB_GetTextLengthW = 1036,
        TB_IsButtonHidden = 1036,
        TBM_SetSELEnd = 1036,
        TTM_UpdateTipTextA = 1036,
        WIZ_Prev = 1036,
        CBEM_GetItemW = 1037,
        RB_GetRowCount = 1037,
        SB_GetTextW = 1037,
        TB_IsButtonIndeterminate = 1037,
        TTM_GetToolCount = 1037,
        CBEM_SetExtendedStyle = 1038,
        RB_GetRowHeight = 1038,
        SB_IsSimple = 1038,
        TB_IsButtonHighLighted = 1038,
        TBM_GetPTICS = 1038,
        TTM_EnumToolSA = 1038,
        SB_SetIcon = 1039,
        TBM_GetTICPOS = 1039,
        TTM_GetCurrentToolA = 1039,
        RB_IDToIndex = 1040,
        SB_SetTipTextA = 1040,
        TBM_GetNumatics = 1040,
        TTM_WindowFromPoint = 1040,
        RB_GetToolTips = 1041,
        SB_SetTipTextW = 1041,
        TBM_GetSELStart = 1041,
        TB_SetState = 1041,
        TTM_TrackActivate = 1041,
        RB_SetToolTips = 1042,
        SB_GetTipTextA = 1042,
        TB_GetState = 1042,
        TBM_GetSELEnd = 1042,
        TTM_TrackPosition = 1042,
        RB_SetBKColor = 1043,
        SB_GetTipTextW = 1043,
        TB_AddBitmap = 1043,
        TBM_ClearSEL = 1043,
        TTM_SetTipBKColor = 1043,
        RB_GetBKColor = 1044,
        SB_GetIcon = 1044,
        TB_AddButtonSA = 1044,
        TBM_SetTICFREQ = 1044,
        TTM_SetTipTextColor = 1044,
        RB_SetTextColor = 1045,
        TB_InsertButtonA = 1045,
        TBM_SetPageSize = 1045,
        TTM_GetDelayTime = 1045,
        RB_GetTextColor = 1046,
        TB_DeleteButton = 1046,
        TBM_GetPageSize = 1046,
        TTM_GetTipBKColor = 1046,
        RB_SizeToRECT = 1047,
        TB_GetButton = 1047,
        TBM_SetLineSize = 1047,
        TTM_GetTipTextColor = 1047,
        RB_BeginDrag = 1048,
        TB_ButtonCount = 1048,
        TBM_GetLineSize = 1048,
        TTM_SetMaxTipWidth = 1048,
        RB_EndDrag = 1049,
        TB_CommandToIndex = 1049,
        TBM_GetThumbRECT = 1049,
        TTM_GetMaxTipWidth = 1049,
        RB_DragMove = 1050,
        TBM_GetChannelRECT = 1050,
        TB_SaveRestoreA = 1050,
        TTM_SetMargin = 1050,
        RB_GetBarHeight = 1051,
        TB_Customize = 1051,
        TBM_SetThumbLength = 1051,
        TTM_GetMargin = 1051,
        RB_GetBandInfoW = 1052,
        TB_AddStringA = 1052,
        TBM_GetThumbLength = 1052,
        TTM_POP = 1052,
        RB_GetBandInfoA = 1053,
        TB_GetItemRECT = 1053,
        TBM_SetToolTips = 1053,
        TTM_Update = 1053,
        RB_MinimizeBand = 1054,
        TB_ButtonStructSize = 1054,
        TBM_GetToolTips = 1054,
        TTM_GetBubbleSize = 1054,
        RB_MaximizeBand = 1055,
        TBM_SetTipSIDE = 1055,
        TB_SetButtonSize = 1055,
        TTM_AdjustRECT = 1055,
        TBM_SetBuddy = 1056,
        TB_SetBitmapSize = 1056,
        TTM_SetTitleA = 1056,
        MSG_FTS_Jump_VA = 1057,
        TB_AutoSize = 1057,
        TBM_GetBuddy = 1057,
        TTM_SetTitleW = 1057,
        RB_GetBandBorders = 1058,
        MSG_FTS_Jump_QWord = 1059,
        RB_ShowBand = 1059,
        TB_GetToolTips = 1059,
        MSG_REIndex_Request = 1060,
        TB_SetToolTips = 1060,
        MSG_FTS_WHERE_Is_IT = 1061,
        RB_SetPalette = 1061,
        TB_SetParent = 1061,
        RB_GetPalette = 1062,
        RB_MoveBand = 1063,
        TB_SetRowS = 1063,
        TB_GetRowS = 1064,
        TB_GetBitmapFLAGS = 1065,
        TB_SetCMDID = 1066,
        RB_PUSHCHEVROn = 1067,
        TB_ChangeBitmap = 1067,
        TB_GetBitmap = 1068,
        MSG_Get_DEFFont = 1069,
        TB_GetButtonTextA = 1069,
        TB_ReplaceBitmap = 1070,
        TB_SetINDENT = 1071,
        TB_SetImageList = 1072,
        TB_GetImageList = 1073,
        TB_LoadImageS = 1074,
        EM_CanPaste = 1074,
        TTM_AddToolW = 1074,
        EM_DisplayBand = 1075,
        TB_GetRECT = 1075,
        TTM_DELToolW = 1075,
        EM_EXGetSEL = 1076,
        TB_SetHotImageList = 1076,
        TTM_NewToolRECTW = 1076,
        EM_EXLimitText = 1077,
        TB_GetHotImageList = 1077,
        TTM_GetToolInfoW = 1077,
        EM_EXLineFromChar = 1078,
        TB_SetDisabledImageList = 1078,
        TTM_SetToolInfoW = 1078,
        EM_EXSetSEL = 1079,
        TB_GetDisabledImageList = 1079,
        TTM_HitTestW = 1079,
        EM_FindText = 1080,
        TB_SetStyle = 1080,
        TTM_GetTextW = 1080,
        EM_FormatRange = 1081,
        TB_GetStyle = 1081,
        TTM_UpdateTipTextW = 1081,
        EM_GetCharFormat = 1082,
        TB_GetButtonSize = 1082,
        TTM_EnumToolSW = 1082,
        EM_GetEventMask = 1083,
        TB_SetButtonWidth = 1083,
        TTM_GetCurrentToolW = 1083,
        EM_GetOLEInterface = 1084,
        TB_SetMaxTextRowS = 1084,
        EM_GetPARAFormat = 1085,
        TB_GetTextRowS = 1085,
        EM_GetSELText = 1086,
        TB_GetObject = 1086,
        EM_HideSelection = 1087,
        TB_GetButtonInfoW = 1087,
        EM_PasteSpecial = 1088,
        TB_SetButtonInfoW = 1088,
        EM_RequestResize = 1089,
        TB_GetButtonInfoA = 1089,
        EM_SelectionType = 1090,
        TB_SetButtonInfoA = 1090,
        EM_SetBKGNDColor = 1091,
        TB_InsertButtonW = 1091,
        EM_SetCharFormat = 1092,
        TB_AddButtonSW = 1092,
        EM_SetEventMask = 1093,
        TB_HitTest = 1093,
        EM_SetOLECallback = 1094,
        TB_SetDrawTextFLAGS = 1094,
        EM_SetPARAFormat = 1095,
        TB_GetHotItem = 1095,
        EM_SetTARGetDevice = 1096,
        TB_SetHotItem = 1096,
        EM_STREAMin = 1097,
        TB_SetAnchorHighlight = 1097,
        EM_STREAMOut = 1098,
        TB_GetAnchorHighlight = 1098,
        EM_GetTextRange = 1099,
        TB_GetButtonTextW = 1099,
        EM_FindWordBreak = 1100,
        TB_SaveRestoreW = 1100,
        EM_SetOptionS = 1101,
        TB_AddStringW = 1101,
        EM_GetOptionS = 1102,
        TB_MapAcceleratorA = 1102,
        EM_FindTextEX = 1103,
        TB_GetInsertMark = 1103,
        EM_GetWordBreakProcEX = 1104,
        TB_SetInsertMark = 1104,
        EM_SetWordBreakProcEX = 1105,
        TB_InsertMarkHitTest = 1105,
        EM_SetUndoLimit = 1106,
        TB_MoveButton = 1106,
        TB_GetMaxSize = 1107,
        EM_Redo = 1108,
        TB_SetExtendedStyle = 1108,
        EM_CanRedo = 1109,
        TB_GetExtendedStyle = 1109,
        EM_GetUndoName = 1110,
        TB_GetPadding = 1110,
        EM_GetRedoName = 1111,
        TB_SetPadding = 1111,
        EM_StopGroupTyping = 1112,
        TB_SetInsertMarkColor = 1112,
        EM_SetTextMode = 1113,
        TB_GetInsertMarkColor = 1113,
        EM_GetTextMode = 1114,
        TB_MapAcceleratorW = 1114,
        EM_AutoURLDetect = 1115,
        TB_GetStringW = 1115,
        EM_GetAutoURLDetect = 1116,
        TB_GetStringA = 1116,
        EM_SetPalette = 1117,
        EM_GetTextEX = 1118,
        EM_GetTextLengthEX = 1119,
        EM_ShowScrollBar = 1120,
        EM_SetTextEX = 1121,
        TAPI_Reply = 1123,
        ACM_OpenA = 1124,
        BFFM_SetStatusTextA = 1124,
        CDM_First = 1124,
        CDM_GetSPEC = 1124,
        EM_SetPunctuation = 1124,
        IPM_ClearAddress = 1124,
        WM_CAP_Unicode_Start = 1124,
        ACM_Play = 1125,
        BFFM_EnableOK = 1125,
        CDM_GetFilePath = 1125,
        EM_GetPunctuation = 1125,
        IPM_SetAddress = 1125,
        PSM_SetCURSEL = 1125,
        UDM_SetRange = 1125,
        WM_ChooseFont_SetLOGFont = 1125,
        ACM_Stop = 1126,
        BFFM_SetSelectionA = 1126,
        CDM_GetFolderPath = 1126,
        EM_SetWordWrapMode = 1126,
        IPM_GetAddress = 1126,
        PSM_RemovePage = 1126,
        UDM_GetRange = 1126,
        WM_CAP_Set_Callback_ErrorW = 1126,
        WM_ChooseFont_SetFLAGS = 1126,
        ACM_OpenW = 1127,
        BFFM_SetSelectionW = 1127,
        CDM_GetFolderIDList = 1127,
        EM_GetWordWrapMode = 1127,
        IPM_SetRange = 1127,
        PSM_AddPage = 1127,
        UDM_SetPOS = 1127,
        WM_CAP_Set_Callback_StatusW = 1127,
        BFFM_SetStatusTextW = 1128,
        CDM_SetControlText = 1128,
        EM_SetIMEColor = 1128,
        IPM_SetFocus = 1128,
        PSM_Changed = 1128,
        UDM_GetPOS = 1128,
        CDM_HideControl = 1129,
        EM_GetIMEColor = 1129,
        IPM_IsBlank = 1129,
        PSM_REStartWindows = 1129,
        UDM_SetBuddy = 1129,
        CDM_SetDEFEXT = 1130,
        EM_SetIMEOptionS = 1130,
        PSM_REBOOTSYSTEM = 1130,
        UDM_GetBuddy = 1130,
        EM_GetIMEOptionS = 1131,
        PSM_CancelToClose = 1131,
        UDM_SetACCEL = 1131,
        EM_COnVPosition = 1132,
        PSM_QuerySiblings = 1132,
        UDM_GetACCEL = 1132,
        MCIWNDM_GetZoom = 1133,
        PSM_UNChanged = 1133,
        UDM_SetBase = 1133,
        PSM_APPLY = 1134,
        UDM_GetBase = 1134,
        PSM_SetTitleA = 1135,
        UDM_SetRange32 = 1135,
        PSM_SetWIZButtonS = 1136,
        UDM_GetRange32 = 1136,
        WM_CAP_Driver_Get_NameW = 1136,
        PSM_PressButton = 1137,
        UDM_SetPOS32 = 1137,
        WM_CAP_Driver_Get_VersionW = 1137,
        PSM_SetCURSELID = 1138,
        UDM_GetPOS32 = 1138,
        PSM_SetFinishTextA = 1139,
        PSM_GetTabControl = 1140,
        PSM_IsDIALOGMESSAGE = 1141,
        MCIWNDM_Realize = 1142,
        PSM_GetCurrentPageHWND = 1142,
        MCIWNDM_SetTimeFormatA = 1143,
        PSM_InsertPage = 1143,
        EM_SetLANGOptionS = 1144,
        MCIWNDM_GetTimeFormatA = 1144,
        PSM_SetTitleW = 1144,
        WM_CAP_File_Set_Capture_FileW = 1144,
        EM_GetLANGOptionS = 1145,
        MCIWNDM_VALIDATEMedia = 1145,
        PSM_SetFinishTextW = 1145,
        WM_CAP_File_Get_Capture_FileW = 1145,
        EM_GetIMECOMPMode = 1146,
        EM_FindTextW = 1147,
        MCIWNDM_PlayTo = 1147,
        WM_CAP_File_SaveASW = 1147,
        EM_FindTextEXW = 1148,
        MCIWNDM_GetFileNameA = 1148,
        EM_Reconversion = 1149,
        MCIWNDM_GetDeviceA = 1149,
        PSM_SetHeaderTitleA = 1149,
        WM_CAP_File_SaveDIBW = 1149,
        EM_SetIMEModeBIAS = 1150,
        MCIWNDM_GetPalette = 1150,
        PSM_SetHeaderTitleW = 1150,
        EM_GetIMEModeBIAS = 1151,
        MCIWNDM_SetPalette = 1151,
        PSM_SetHeaderSubTitleA = 1151,
        MCIWNDM_GetErrorA = 1152,
        PSM_SetHeaderSubTitleW = 1152,
        PSM_HWNDToIndex = 1153,
        PSM_IndexToHWND = 1154,
        MCIWNDM_SetInactiveTimes = 1155,
        PSM_PAGetOIndex = 1155,
        PSM_IndexToPage = 1156,
        DL_BeginDrag = 1157,
        MCIWNDM_GetInactiveTimes = 1157,
        PSM_IDToIndex = 1157,
        DL_Dragging = 1158,
        PSM_IndexToID = 1158,
        DL_Dropped = 1159,
        PSM_GetResult = 1159,
        DL_CancelDrag = 1160,
        PSM_RecalcPageSizes = 1160,
        MCIWNDM_Get_Source = 1164,
        MCIWNDM_PUT_Source = 1165,
        MCIWNDM_Get_Dest = 1166,
        MCIWNDM_PUT_Dest = 1167,
        MCIWNDM_Can_Play = 1168,
        MCIWNDM_Can_Window = 1169,
        MCIWNDM_Can_Record = 1170,
        MCIWNDM_Can_Save = 1171,
        MCIWNDM_Can_Eject = 1172,
        MCIWNDM_Can_Config = 1173,
        IE_GetINK = 1174,
        IE_MSGFirst = 1174,
        MCIWNDM_PaletteKick = 1174,
        IE_SetINK = 1175,
        IE_GetPenTip = 1176,
        IE_SetPenTip = 1177,
        IE_GetEraserTip = 1178,
        IE_SetEraserTip = 1179,
        IE_GetBKGND = 1180,
        IE_SetBKGND = 1181,
        IE_GetGridOrigin = 1182,
        IE_SetGridOrigin = 1183,
        IE_GetGridPen = 1184,
        IE_SetGridPen = 1185,
        IE_GetGridSize = 1186,
        IE_SetGridSize = 1187,
        IE_GetMode = 1188,
        IE_SetMode = 1189,
        IE_GetINKRECT = 1190,
        WM_CAP_Set_MCI_DeviceW = 1190,
        WM_CAP_Get_MCI_DeviceW = 1191,
        WM_CAP_PAL_OpenW = 1204,
        WM_CAP_PAL_SaveW = 1205,
        IE_GetAPPData = 1208,
        IE_SetAPPData = 1209,
        IE_GetDrawOPTS = 1210,
        IE_SetDrawOPTS = 1211,
        IE_GetFormat = 1212,
        IE_SetFormat = 1213,
        IE_GetINKInput = 1214,
        IE_SetINKInput = 1215,
        IE_GetNotify = 1216,
        IE_SetNotify = 1217,
        IE_GetRECOG = 1218,
        IE_SetRECOG = 1219,
        IE_GetSecurity = 1220,
        IE_SetSecurity = 1221,
        IE_GetSEL = 1222,
        IE_SetSEL = 1223,
        CDM_LAST = 1224,
        EM_SetBIDIOptionS = 1224,
        IE_DOCommand = 1224,
        MCIWNDM_NotifyMode = 1224,
        EM_GetBIDIOptionS = 1225,
        IE_GetCommand = 1225,
        EM_SetTypographyOptionS = 1226,
        IE_GetCount = 1226,
        EM_GetTypographyOptionS = 1227,
        IE_GetGesture = 1227,
        MCIWNDM_NotifyMedia = 1227,
        EM_SetEditStyle = 1228,
        IE_GetMenu = 1228,
        EM_GetEditStyle = 1229,
        IE_GetPaintDC = 1229,
        MCIWNDM_NotifyError = 1229,
        IE_GetPDEvent = 1230,
        IE_GetSELCount = 1231,
        IE_GetSELItems = 1232,
        IE_GetStyle = 1233,
        MCIWNDM_SetTimeFormatW = 1243,
        EM_OutLine = 1244,
        MCIWNDM_GetTimeFormatW = 1244,
        EM_GetScrollPOS = 1245,
        EM_SetScrollPOS = 1246,
        EM_SetFontSize = 1247,
        EM_GetZoom = 1248,
        MCIWNDM_GetFileNameW = 1248,
        EM_SetZoom = 1249,
        MCIWNDM_GetDeviceW = 1249,
        EM_GetViewKind = 1250,
        EM_SetViewKind = 1251,
        EM_GetPage = 1252,
        MCIWNDM_GetErrorW = 1252,
        EM_SetPage = 1253,
        EM_GetHyphenateInfo = 1254,
        EM_SetHyphenateInfo = 1255,
        EM_GetPageRotate = 1259,
        EM_SetPageRotate = 1260,
        EM_GetCTFModeBIAS = 1261,
        EM_SetCTFModeBIAS = 1262,
        EM_GetCTFOpenStatus = 1264,
        EM_SetCTFOpenStatus = 1265,
        EM_GetIMECOMPText = 1266,
        EM_IsIME = 1267,
        EM_GetIMEPROPERTY = 1268,
        EM_GetQueryRTFOBJ = 1293,
        EM_SetQueryRTFOBJ = 1294,
        FM_GetFocus = 1536,
        FM_GetDriveInfoA = 1537,
        FM_GetSELCount = 1538,
        FM_GetSELCountLFN = 1539,
        FM_GetFileSELA = 1540,
        FM_GetFileSELLFNA = 1541,
        FM_Refresh_Windows = 1542,
        FM_Reload_Extensions = 1543,
        FM_GetDriveInfoW = 1553,
        FM_GetFileSELW = 1556,
        FM_GetFileSELLFNW = 1557,
        WLX_WM_SAS = 1625,
        SM_GetSELCount = 2024,
        UM_GetSELCount = 2024,
        WM_CPL_Launch = 2024,
        SM_GetServerSELA = 2025,
        UM_GetUserSELA = 2025,
        WM_CPL_LaunchED = 2025,
        SM_GetServerSELW = 2026,
        UM_GetUserSELW = 2026,
        SM_GetCURFocusA = 2027,
        UM_GetGroupSELA = 2027,
        SM_GetCURFocusW = 2028,
        UM_GetGroupSELW = 2028,
        SM_GetOptionS = 2029,
        UM_GetCURFocusA = 2029,
        UM_GetCURFocusW = 2030,
        UM_GetOptionS = 2031,
        UM_GetOptionS2 = 2032,
        LVM_First = 4096,
        LVM_GetBKColor = 4096,
        LVM_SetBKColor = 4097,
        LVM_GetImageList = 4098,
        LVM_SetImageList = 4099,
        LVM_GetItemCount = 4100,
        LVM_GetItemA = 4101,
        LVM_SetItemA = 4102,
        LVM_InsertItemA = 4103,
        LVM_DeleteItem = 4104,
        LVM_DeleteAllItems = 4105,
        LVM_GetCallbackMask = 4106,
        LVM_SetCallbackMask = 4107,
        LVM_GetNextItem = 4108,
        LVM_FindItemA = 4109,
        LVM_GetItemRECT = 4110,
        LVM_SetItemPosition = 4111,
        LVM_GetItemPosition = 4112,
        LVM_GetStringWidthA = 4113,
        LVM_HitTest = 4114,
        LVM_EnsureVisible = 4115,
        LVM_Scroll = 4116,
        LVM_RedrawItems = 4117,
        LVM_Arrange = 4118,
        LVM_EditLabelA = 4119,
        LVM_GetEditControl = 4120,
        LVM_GetColumnA = 4121,
        LVM_SetColumnA = 4122,
        LVM_InsertColumnA = 4123,
        LVM_DeleteColumn = 4124,
        LVM_GetColumnWidth = 4125,
        LVM_SetColumnWidth = 4126,
        LVM_GetHeader = 4127,
        LVM_CreateDragImage = 4129,
        LVM_GetViewRECT = 4130,
        LVM_GetTextColor = 4131,
        LVM_SetTextColor = 4132,
        LVM_GetTextBKColor = 4133,
        LVM_SetTextBKColor = 4134,
        LVM_GetToPIndex = 4135,
        LVM_GetCountPerPage = 4136,
        LVM_GetOrigin = 4137,
        LVM_Update = 4138,
        LVM_SetItemstate = 4139,
        LVM_GetItemstate = 4140,
        LVM_GetItemTextA = 4141,
        LVM_SetItemTextA = 4142,
        LVM_SetItemCount = 4143,
        LVM_SortItems = 4144,
        LVM_SetItemPosition32 = 4145,
        LVM_GetSelectEDCount = 4146,
        LVM_GetItemsPACING = 4147,
        LVM_GetIsearchStringA = 4148,
        LVM_SetIconSPACING = 4149,
        LVM_SetExtendedListViewStyle = 4150,
        LVM_GetExtendedListViewStyle = 4151,
        LVM_GetSubItemRECT = 4152,
        LVM_SubItemHitTest = 4153,
        LVM_SetColumnOrderArray = 4154,
        LVM_GetColumnOrderArray = 4155,
        LVM_SetHotItem = 4156,
        LVM_GetHotItem = 4157,
        LVM_SetHotCursor = 4158,
        LVM_GetHotCursor = 4159,
        LVM_ApproximateViewRECT = 4160,
        LVM_SetWorkAreas = 4161,
        LVM_GetSelectionMark = 4162,
        LVM_SetSelectionMark = 4163,
        LVM_SetBKImageA = 4164,
        LVM_GetBKImageA = 4165,
        LVM_GetWorkAreas = 4166,
        LVM_SetHoverTime = 4167,
        LVM_GetHoverTime = 4168,
        LVM_GetNumberOfWorkAreas = 4169,
        LVM_SetToolTips = 4170,
        LVM_GetItemW = 4171,
        LVM_SetItemW = 4172,
        LVM_InsertItemW = 4173,
        LVM_GetToolTips = 4174,
        LVM_FindItemW = 4179,
        LVM_GetStringWidthW = 4183,
        LVM_GetColumnW = 4191,
        LVM_SetColumnW = 4192,
        LVM_InsertColumnW = 4193,
        LVM_GetItemTextW = 4211,
        LVM_SetItemTextW = 4212,
        LVM_GetIsearchStringW = 4213,
        LVM_EditLabelW = 4214,
        LVM_GetBKImageW = 4235,
        LVM_SetSelectEDColumn = 4236,
        LVM_SetTileWidth = 4237,
        LVM_SetView = 4238,
        LVM_GetView = 4239,
        LVM_InsertGroup = 4241,
        LVM_SetGroupInfo = 4243,
        LVM_GetGroupInfo = 4245,
        LVM_RemoveGroup = 4246,
        LVM_MoveGroup = 4247,
        LVM_MoveItemToGroup = 4250,
        LVM_SetGroupMetrics = 4251,
        LVM_GetGroupMetrics = 4252,
        LVM_EnableGroupView = 4253,
        LVM_SortGroups = 4254,
        LVM_InsertGroupSortED = 4255,
        LVM_RemoveAllGroups = 4256,
        LVM_HasGroup = 4257,
        LVM_SetTileViewInfo = 4258,
        LVM_GetTileViewInfo = 4259,
        LVM_SetTileInfo = 4260,
        LVM_GetTileInfo = 4261,
        LVM_SetInsertMark = 4262,
        LVM_GetInsertMark = 4263,
        LVM_InsertMarkHitTest = 4264,
        LVM_GetInsertMarkRECT = 4265,
        LVM_SetInsertMarkColor = 4266,
        LVM_GetInsertMarkColor = 4267,
        LVM_SetInfoTip = 4269,
        LVM_GetSelectEDColumn = 4270,
        LVM_IsGroupViewEnableD = 4271,
        LVM_GetOutLineColor = 4272,
        LVM_SetOutLineColor = 4273,
        LVM_CancelEditLabel = 4275,
        LVM_MapIndexToID = 4276,
        LVM_MapIDToIndex = 4277,
        LVM_IsItemVisible = 4278,
        OCM_Base = 8192,
        LVM_SetUnicodeFormat = 8197,
        LVM_GetUnicodeFormat = 8198,
        OCM_CTLColor = 8217,
        OCM_DrawItem = 8235,
        OCM_MeasureItem = 8236,
        OCM_DeleteItem = 8237,
        OCM_VKeyToItem = 8238,
        OCM_CharToItem = 8239,
        OCM_CompareItem = 8249,
        OCM_Notify = 8270,
        OCM_Command = 8465,
        OCM_HScroll = 8468,
        OCM_VScroll = 8469,
        OCM_CTLColorMSGBox = 8498,
        OCM_CTLColorEdit = 8499,
        OCM_CTLColorListBox = 8500,
        OCM_CTLColorBTN = 8501,
        OCM_CTLColorDLG = 8502,
        OCM_CTLColorScrollBar = 8503,
        OCM_CTLColorStatic = 8504,
        OCM_ParentNotify = 8720,
        WM_APP = 32768,
        WM_RASDIALEvent = 52429,


    }
}
