using System;

namespace SE.Editor.GUI
{
    public enum GUIStyleVar
    {
        Alpha = 0,
        WindowPadding = 1,
        WindowRounding = 2,
        WindowBorderSize = 3,
        WindowMinSize = 4,
        WindowTitleAlign = 5,
        ChildRounding = 6,
        ChildBorderSize = 7,
        PopupRounding = 8,
        PopupBorderSize = 9,
        FramePadding = 10,
        FrameRounding = 11,
        FrameBorderSize = 12,
        ItemSpacing = 13,
        ItemInnerSpacing = 14,
        IndentSpacing = 15,
        ScrollbarSize = 16,
        ScrollbarRounding = 17,
        GrabMinSize = 18,
        GrabRounding = 19,
        TabRounding = 20,
        ButtonTextAlign = 21,
        SelectableTextAlign = 22,
        COUNT = 23,
    }

    public enum GUIColor
    {
        Text = 0,
        TextDisabled = 1,
        WindowBg = 2,
        ChildBg = 3,
        PopupBg = 4,
        Border = 5,
        BorderShadow = 6,
        FrameBg = 7,
        FrameBgHovered = 8,
        FrameBgActive = 9,
        TitleBg = 10,
        TitleBgActive = 11,
        TitleBgCollapsed = 12,
        MenuBarBg = 13,
        ScrollbarBg = 14,
        ScrollbarGrab = 15,
        ScrollbarGrabHovered = 16,
        ScrollbarGrabActive = 17,
        CheckMark = 18,
        SliderGrab = 19,
        SliderGrabActive = 20,
        Button = 21,
        ButtonHovered = 22,
        ButtonActive = 23,
        Header = 24,
        HeaderHovered = 25,
        HeaderActive = 26,
        Separator = 27,
        SeparatorHovered = 28,
        SeparatorActive = 29,
        ResizeGrip = 30,
        ResizeGripHovered = 31,
        ResizeGripActive = 32,
        Tab = 33,
        TabHovered = 34,
        TabActive = 35,
        TabUnfocused = 36,
        TabUnfocusedActive = 37,
        PlotLines = 38,
        PlotLinesHovered = 39,
        PlotHistogram = 40,
        PlotHistogramHovered = 41,
        TextSelectedBg = 42,
        DragDropTarget = 43,
        NavHighlight = 44,
        NavWindowingHighlight = 45,
        NavWindowingDimBg = 46,
        ModalWindowDimBg = 47,
        COUNT = 48,
    }

    [Flags]
    public enum GUIColorEditFlags 
    { 
        None = 0, 
        NoAlpha = 2,
        NoPicker = 4,
        NoOptions = 8,
        NoSmallPreview = 16, // 0x00000010
        NoInputs = 32, // 0x00000020
        NoTooltip = 64, // 0x00000040
        NoLabel = 128, // 0x00000080
        NoSidePreview = 256, // 0x00000100
        NoDragDrop = 512, // 0x00000200
        AlphaBar = 65536, // 0x00010000
        AlphaPreview = 131072, // 0x00020000
        AlphaPreviewHalf = 262144, // 0x00040000
        HDR = 524288, // 0x00080000
        DisplayRGB = 1048576, // 0x00100000
        DisplayHSV = 2097152, // 0x00200000
        DisplayHex = 4194304, // 0x00400000
        Uint8 = 8388608, // 0x00800000
        Float = 16777216, // 0x01000000
        PickerHueBar = 33554432, // 0x02000000
        PickerHueWheel = 67108864, // 0x04000000
        InputRGB = 134217728, // 0x08000000
        InputHSV = 268435456, // 0x10000000
        _OptionsDefault = InputRGB | PickerHueBar | Uint8 | DisplayRGB, // 0x0A900000
        _DisplayMask = DisplayHex | DisplayHSV | DisplayRGB, // 0x00700000
        _DataTypeMask = Float | Uint8, // 0x01800000
        _PickerMask = PickerHueWheel | PickerHueBar, // 0x06000000
        _InputMask = InputHSV | InputRGB, // 0x18000000
    }

    [Flags]
    public enum GUITreeNodeFlags
    {
        None = 0,
        Selected = 1 << 0,
        Framed = 1 << 1,
        AllowItemOverlap = 1 << 2,
        NoTreePushOnOpen = 1 << 3,
        NoAutoOpenOnLog = 1 << 4,
        DefaultOpen = 1 << 5,
        OpenOnDoubleClick = 1 << 6,
        OpenOnArrow = 1 << 7,
        Leaf = 1 << 8,
        Bullet = 1 << 9,
        FramePadding = 1 << 10,
        NavLeftJumpsBackHere = 1 << 13,
        CollapsingHeader = Framed | NoTreePushOnOpen | NoAutoOpenOnLog,
    }

    [Flags]
    public enum GUITabItemFlags
    {
        None = 0,
        UnsavedDocument = 1 << 0,
        SetSelected = 1 << 1,
        NoCloseWithMiddleMouseButton = 1 << 2,
        NoPushId = 1 << 3,
    }

    [Flags]
    public enum GUITabBarFlags
    {
        None = 0,
        Reorderable = 1 << 0,
        AutoSelectNewTabs = 1 << 1,
        TabListPopupButton = 1 << 2,
        NoCloseWithMiddleMouseButton = 1 << 3,
        NoTabListScrollingButtons = 1 << 4,
        NoTooltip = 1 << 5,
        FittingPolicyResizeDown = 1 << 6,
        FittingPolicyScroll = 1 << 7,
        FittingPolicyMask = FittingPolicyResizeDown | FittingPolicyScroll,
        FittingPolicyDefault = FittingPolicyResizeDown,
    }

    [Flags]
    public enum GUIDrawCornerFlags
    {
        None = 0,
        TopLeft = 1 << 0,
        TopRight = 1 << 1,
        BotLeft = 1 << 2,
        BotRight = 1 << 3,
        Top = TopLeft | TopRight,
        Bot = BotLeft | BotRight,
        Left = TopLeft | BotLeft,
        Right = TopRight | BotRight,
        All = 0xF,
    }

    [Flags]
    public enum GUIDrawListFlags
    {
        None = 0,
        AntiAliasedLines = 1 << 0,
        AntiAliasedFill = 1 << 1,
        AllowVtxOffset = 1 << 2,
    }

    [Flags]
    public enum GUIFontAtlasFlags
    {
        None = 0,
        NoPowerOfTwoHeight = 1 << 0,
        NoMouseCursors = 1 << 1,
    }

    [Flags]
    public enum GUIBackendFlags
    {
        None = 0,
        HasGamepad = 1 << 0,
        HasMouseCursors = 1 << 1,
        HasSetMousePos = 1 << 2,
        RendererHasVtxOffset = 1 << 3,
    }

    [Flags]
    public enum GUIComboFlags
    {
        None = 0,
        PopupAlignLeft = 1 << 0,
        HeightSmall = 1 << 1,
        HeightRegular = 1 << 2,
        HeightLarge = 1 << 3,
        HeightLargest = 1 << 4,
        NoArrowButton = 1 << 5,
        NoPreview = 1 << 6,
        HeightMask = HeightSmall | HeightRegular | HeightLarge | HeightLargest,
    }

    [Flags]
    public enum GUIConfigFlags
    {
        None = 0,
        NavEnableKeyboard = 1 << 0,
        NavEnableGamepad = 1 << 1,
        NavEnableSetMousePos = 1 << 2,
        NavNoCaptureKeyboard = 1 << 3,
        NoMouse = 1 << 4,
        NoMouseCursorChange = 1 << 5,
        IsSRGB = 1 << 20,
        IsTouchScreen = 1 << 21,
    }

    [Flags]
    public enum GUIDragDropFlags
    {
        None = 0,
        SourceNoPreviewTooltip = 1 << 0,
        SourceNoDisableHover = 1 << 1,
        SourceNoHoldToOpenOthers = 1 << 2,
        SourceAllowNullID = 1 << 3,
        SourceExtern = 1 << 4,
        SourceAutoExpirePayload = 1 << 5,
        AcceptBeforeDelivery = 1 << 10,
        AcceptNoDrawDefaultRect = 1 << 11,
        AcceptNoPreviewTooltip = 1 << 12,
        AcceptPeekOnly = AcceptBeforeDelivery | AcceptNoDrawDefaultRect,
    }

    [Flags]
    public enum GUIFocusedFlags
    {
        None = 0,
        ChildWindows = 1 << 0,
        RootWindow = 1 << 1,
        AnyWindow = 1 << 2,
        RootAndChildWindows = RootWindow | ChildWindows,
    }

    [Flags]
    public enum GUIHoveredFlags
    {
        None = 0,
        ChildWindows = 1 << 0,
        RootWindow = 1 << 1,
        AnyWindow = 1 << 2,
        AllowWhenBlockedByPopup = 1 << 3,
        AllowWhenBlockedByActiveItem = 1 << 5,
        AllowWhenOverlapped = 1 << 6,
        AllowWhenDisabled = 1 << 7,
        RectOnly = AllowWhenBlockedByPopup | AllowWhenBlockedByActiveItem | AllowWhenOverlapped,
        RootAndChildWindows = RootWindow | ChildWindows,
    }

    [Flags]
    public enum GUIInputTextFlags
    {
        None = 0,
        CharsDecimal = 1 << 0,
        CharsHexadecimal = 1 << 1,
        CharsUppercase = 1 << 2,
        CharsNoBlank = 1 << 3,
        AutoSelectAll = 1 << 4,
        EnterReturnsTrue = 1 << 5,
        CallbackCompletion = 1 << 6,
        CallbackHistory = 1 << 7,
        CallbackAlways = 1 << 8,
        CallbackCharFilter = 1 << 9,
        AllowTabInput = 1 << 10,
        CtrlEnterForNewLine = 1 << 11,
        NoHorizontalScroll = 1 << 12,
        AlwaysInsertMode = 1 << 13,
        ReadOnly = 1 << 14,
        Password = 1 << 15,
        NoUndoRedo = 1 << 16,
        CharsScientific = 1 << 17,
        CallbackResize = 1 << 18,
        Multiline = 1 << 20,
        NoMarkEdited = 1 << 21,
    }

    [Flags]
    public enum GUISelectableFlags
    {
        None = 0,
        DontClosePopups = 1 << 0,
        SpanAllColumns = 1 << 1,
        AllowDoubleClick = 1 << 2,
        Disabled = 1 << 3,
    }

    [Flags]
    public enum GUIWindowFlags
    {
        None = 0,
        NoTitleBar = 1 << 0,
        NoResize = 1 << 1,
        NoMove = 1 << 2,
        NoScrollbar = 1 << 3,
        NoScrollWithMouse = 1 << 4,
        NoCollapse = 1 << 5,
        AlwaysAutoResize = 1 << 6,
        NoBackground = 1 << 7,
        NoSavedSettings = 1 << 8,
        NoMouseInputs = 1 << 9,
        MenuBar = 1 << 10,
        HorizontalScrollbar = 1 << 11,
        NoFocusOnAppearing = 1 << 12,
        NoBringToFrontOnFocus = 1 << 13,
        AlwaysVerticalScrollbar = 1 << 14,
        AlwaysHorizontalScrollbar = 1 << 15,
        AlwaysUseWindowPadding = 1 << 16,
        NoNavInputs = 1 << 18,
        NoNavFocus = 1 << 19,
        UnsavedDocument = 1 << 20,
        NoNav = NoNavInputs | NoNavFocus,
        NoDecoration = NoTitleBar | NoResize | NoScrollbar | NoCollapse,
        NoInputs = NoMouseInputs | NoNavInputs | NoNavFocus,
        NavFlattened = 1 << 23,
        ChildWindow = 1 << 24,
        Tooltip = 1 << 25,
        Popup = 1 << 26,
        Modal = 1 << 27,
        ChildMenu = 1 << 28,
    }
}
