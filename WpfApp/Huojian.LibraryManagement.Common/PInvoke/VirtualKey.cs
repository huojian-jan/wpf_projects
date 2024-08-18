namespace Huojian.LibraryManagement.Common.PInvoke
{
    //
    // 摘要:
    //     Virtual-key codes
    //
    // 言论：
    //     Defined in winuser.h from Windows SDK v6.1
    public enum VirtualKey : ushort
    {
        //
        // 摘要:
        //     This is an addendum to use on functions in which you have to pass a zero value
        //     to represent no key code
        VK_NO_KEY = 0,
        //
        // 摘要:
        //     Left mouse button
        VK_LBUTTON = 1,
        //
        // 摘要:
        //     Right mouse button
        VK_RBUTTON = 2,
        //
        // 摘要:
        //     Control-break processing
        VK_CANCEL = 3,
        //
        // 摘要:
        //     Middle mouse button (three-button mouse)
        //
        // 言论：
        //     NOT contiguous with L and R buttons
        VK_MBUTTON = 4,
        //
        // 摘要:
        //     X1 mouse button
        //
        // 言论：
        //     NOT contiguous with L and R buttons
        VK_XBUTTON1 = 5,
        //
        // 摘要:
        //     X2 mouse button
        //
        // 言论：
        //     NOT contiguous with L and R buttons
        VK_XBUTTON2 = 6,
        //
        // 摘要:
        //     BACKSPACE key
        VK_BACK = 8,
        //
        // 摘要:
        //     TAB key
        VK_TAB = 9,
        //
        // 摘要:
        //     CLEAR key
        VK_CLEAR = 12,
        //
        // 摘要:
        //     RETURN key
        VK_RETURN = 13,
        //
        // 摘要:
        //     SHIFT key
        VK_SHIFT = 16,
        //
        // 摘要:
        //     CONTROL key
        VK_CONTROL = 17,
        //
        // 摘要:
        //     ALT key
        VK_MENU = 18,
        //
        // 摘要:
        //     PAUSE key
        VK_PAUSE = 19,
        //
        // 摘要:
        //     CAPS LOCK key
        VK_CAPITAL = 20,
        //
        // 摘要:
        //     IME Kana mode
        VK_KANA = 21,
        //
        // 摘要:
        //     IME Hanguel mode (maintained for compatibility; use PInvoke.VirtualKey.VK_HANGUL)
        VK_HANGEUL = 21,
        //
        // 摘要:
        //     IME Hangul mode
        VK_HANGUL = 21,
        //
        // 摘要:
        //     IME Junja mode
        VK_JUNJA = 23,
        //
        // 摘要:
        //     IME final mode
        VK_FINAL = 24,
        //
        // 摘要:
        //     IME Hanja mode
        VK_HANJA = 25,
        //
        // 摘要:
        //     IME Kanji mode
        VK_KANJI = 25,
        //
        // 摘要:
        //     ESC key
        VK_ESCAPE = 27,
        //
        // 摘要:
        //     IME convert
        VK_CONVERT = 28,
        //
        // 摘要:
        //     IME nonconvert
        VK_NONCONVERT = 29,
        //
        // 摘要:
        //     IME accept
        VK_ACCEPT = 30,
        //
        // 摘要:
        //     IME mode change request
        VK_MODECHANGE = 31,
        //
        // 摘要:
        //     SPACEBAR
        VK_SPACE = 32,
        //
        // 摘要:
        //     PAGE UP key
        VK_PRIOR = 33,
        //
        // 摘要:
        //     PAGE DOWN key
        VK_NEXT = 34,
        //
        // 摘要:
        //     END key
        VK_END = 35,
        //
        // 摘要:
        //     HOME key
        VK_HOME = 36,
        //
        // 摘要:
        //     LEFT ARROW key
        VK_LEFT = 37,
        //
        // 摘要:
        //     UP ARROW key
        VK_UP = 38,
        //
        // 摘要:
        //     RIGHT ARROW key
        VK_RIGHT = 39,
        //
        // 摘要:
        //     DOWN ARROW key
        VK_DOWN = 40,
        //
        // 摘要:
        //     SELECT key
        VK_SELECT = 41,
        //
        // 摘要:
        //     PRINT key
        VK_PRINT = 42,
        //
        // 摘要:
        //     EXECUTE key
        VK_EXECUTE = 43,
        //
        // 摘要:
        //     PRINT SCREEN key
        VK_SNAPSHOT = 44,
        //
        // 摘要:
        //     INS key
        VK_INSERT = 45,
        //
        // 摘要:
        //     DEL key
        VK_DELETE = 46,
        //
        // 摘要:
        //     HELP key
        VK_HELP = 47,
        //
        // 摘要:
        //     0 key
        VK_KEY_0 = 48,
        //
        // 摘要:
        //     1 key
        VK_KEY_1 = 49,
        //
        // 摘要:
        //     2 key
        VK_KEY_2 = 50,
        //
        // 摘要:
        //     3 key
        VK_KEY_3 = 51,
        //
        // 摘要:
        //     4 key
        VK_KEY_4 = 52,
        //
        // 摘要:
        //     5 key
        VK_KEY_5 = 53,
        //
        // 摘要:
        //     6 key
        VK_KEY_6 = 54,
        //
        // 摘要:
        //     7 key
        VK_KEY_7 = 55,
        //
        // 摘要:
        //     8 key
        VK_KEY_8 = 56,
        //
        // 摘要:
        //     9 key
        VK_KEY_9 = 57,
        //
        // 摘要:
        //     A key
        VK_A = 65,
        //
        // 摘要:
        //     B key
        VK_B = 66,
        //
        // 摘要:
        //     C key
        VK_C = 67,
        //
        // 摘要:
        //     D key
        VK_D = 68,
        //
        // 摘要:
        //     E key
        VK_E = 69,
        //
        // 摘要:
        //     F key
        VK_F = 70,
        //
        // 摘要:
        //     G key
        VK_G = 71,
        //
        // 摘要:
        //     H key
        VK_H = 72,
        //
        // 摘要:
        //     I key
        VK_I = 73,
        //
        // 摘要:
        //     J key
        VK_J = 74,
        //
        // 摘要:
        //     K key
        VK_K = 75,
        //
        // 摘要:
        //     L key
        VK_L = 76,
        //
        // 摘要:
        //     M key
        VK_M = 77,
        //
        // 摘要:
        //     N key
        VK_N = 78,
        //
        // 摘要:
        //     O key
        VK_O = 79,
        //
        // 摘要:
        //     P key
        VK_P = 80,
        //
        // 摘要:
        //     Q key
        VK_Q = 81,
        //
        // 摘要:
        //     R key
        VK_R = 82,
        //
        // 摘要:
        //     S key
        VK_S = 83,
        //
        // 摘要:
        //     T key
        VK_T = 84,
        //
        // 摘要:
        //     U key
        VK_U = 85,
        //
        // 摘要:
        //     V key
        VK_V = 86,
        //
        // 摘要:
        //     W key
        VK_W = 87,
        //
        // 摘要:
        //     X key
        VK_X = 88,
        //
        // 摘要:
        //     Y key
        VK_Y = 89,
        //
        // 摘要:
        //     Z key
        VK_Z = 90,
        //
        // 摘要:
        //     Left Windows key (Natural keyboard)
        VK_LWIN = 91,
        //
        // 摘要:
        //     Right Windows key (Natural keyboard)
        VK_RWIN = 92,
        //
        // 摘要:
        //     Applications key (Natural keyboard)
        VK_APPS = 93,
        //
        // 摘要:
        //     Computer Sleep key
        VK_SLEEP = 95,
        //
        // 摘要:
        //     Numeric keypad 0 key
        VK_NUMPAD0 = 96,
        //
        // 摘要:
        //     Numeric keypad 1 key
        VK_NUMPAD1 = 97,
        //
        // 摘要:
        //     Numeric keypad 2 key
        VK_NUMPAD2 = 98,
        //
        // 摘要:
        //     Numeric keypad 3 key
        VK_NUMPAD3 = 99,
        //
        // 摘要:
        //     Numeric keypad 4 key
        VK_NUMPAD4 = 100,
        //
        // 摘要:
        //     Numeric keypad 5 key
        VK_NUMPAD5 = 101,
        //
        // 摘要:
        //     Numeric keypad 6 key
        VK_NUMPAD6 = 102,
        //
        // 摘要:
        //     Numeric keypad 7 key
        VK_NUMPAD7 = 103,
        //
        // 摘要:
        //     Numeric keypad 8 key
        VK_NUMPAD8 = 104,
        //
        // 摘要:
        //     Numeric keypad 9 key
        VK_NUMPAD9 = 105,
        //
        // 摘要:
        //     Multiply key
        VK_MULTIPLY = 106,
        //
        // 摘要:
        //     Add key
        VK_ADD = 107,
        //
        // 摘要:
        //     Separator key
        VK_SEPARATOR = 108,
        //
        // 摘要:
        //     Subtract key
        VK_SUBTRACT = 109,
        //
        // 摘要:
        //     Decimal key
        VK_DECIMAL = 110,
        //
        // 摘要:
        //     Divide key
        VK_DIVIDE = 111,
        //
        // 摘要:
        //     F1 Key
        VK_F1 = 112,
        //
        // 摘要:
        //     F2 Key
        VK_F2 = 113,
        //
        // 摘要:
        //     F3 Key
        VK_F3 = 114,
        //
        // 摘要:
        //     F4 Key
        VK_F4 = 115,
        //
        // 摘要:
        //     F5 Key
        VK_F5 = 116,
        //
        // 摘要:
        //     F6 Key
        VK_F6 = 117,
        //
        // 摘要:
        //     F7 Key
        VK_F7 = 118,
        //
        // 摘要:
        //     F8 Key
        VK_F8 = 119,
        //
        // 摘要:
        //     F9 Key
        VK_F9 = 120,
        //
        // 摘要:
        //     F10 Key
        VK_F10 = 121,
        //
        // 摘要:
        //     F11 Key
        VK_F11 = 122,
        //
        // 摘要:
        //     F12 Key
        VK_F12 = 123,
        //
        // 摘要:
        //     F13 Key
        VK_F13 = 124,
        //
        // 摘要:
        //     F14 Key
        VK_F14 = 125,
        //
        // 摘要:
        //     F15 Key
        VK_F15 = 126,
        //
        // 摘要:
        //     F16 Key
        VK_F16 = 127,
        //
        // 摘要:
        //     F17 Key
        VK_F17 = 128,
        //
        // 摘要:
        //     F18 Key
        VK_F18 = 129,
        //
        // 摘要:
        //     F19 Key
        VK_F19 = 130,
        //
        // 摘要:
        //     F20 Key
        VK_F20 = 131,
        //
        // 摘要:
        //     F21 Key
        VK_F21 = 132,
        //
        // 摘要:
        //     F22 Key
        VK_F22 = 133,
        //
        // 摘要:
        //     F23 Key
        VK_F23 = 134,
        //
        // 摘要:
        //     F24 Key
        VK_F24 = 135,
        //
        // 摘要:
        //     NUM LOCK key
        VK_NUMLOCK = 144,
        //
        // 摘要:
        //     SCROLL LOCK key
        VK_SCROLL = 145,
        //
        // 摘要:
        //     '=' key on numpad (NEC PC-9800 kbd definitions)
        VK_OEM_NEC_EQUAL = 146,
        //
        // 摘要:
        //     'Dictionary' key (Fujitsu/OASYS kbd definitions)
        VK_OEM_FJ_JISHO = 146,
        //
        // 摘要:
        //     'Unregister word' key (Fujitsu/OASYS kbd definitions)
        VK_OEM_FJ_MASSHOU = 147,
        //
        // 摘要:
        //     'Register word' key (Fujitsu/OASYS kbd definitions)
        VK_OEM_FJ_TOUROKU = 148,
        //
        // 摘要:
        //     'Left OYAYUBI' key (Fujitsu/OASYS kbd definitions)
        VK_OEM_FJ_LOYA = 149,
        //
        // 摘要:
        //     'Right OYAYUBI' key (Fujitsu/OASYS kbd definitions)
        VK_OEM_FJ_ROYA = 150,
        //
        // 摘要:
        //     Left SHIFT key
        //
        // 言论：
        //     Used only as parameters to PInvoke.GetAsyncKeyState(System.Int32) and
        //     PInvoke.GetKeyState(System.Int32). * No other API or message will distinguish
        //     left and right keys in this way.
        VK_LSHIFT = 160,
        //
        // 摘要:
        //     Right SHIFT key
        VK_RSHIFT = 161,
        //
        // 摘要:
        //     Left CONTROL key
        VK_LCONTROL = 162,
        //
        // 摘要:
        //     Right CONTROL key
        VK_RCONTROL = 163,
        //
        // 摘要:
        //     Left MENU key
        VK_LMENU = 164,
        //
        // 摘要:
        //     Right MENU key
        VK_RMENU = 165,
        //
        // 摘要:
        //     Browser Back key
        VK_BROWSER_BACK = 166,
        //
        // 摘要:
        //     Browser Forward key
        VK_BROWSER_FORWARD = 167,
        //
        // 摘要:
        //     Browser Refresh key
        VK_BROWSER_REFRESH = 168,
        //
        // 摘要:
        //     Browser Stop key
        VK_BROWSER_STOP = 169,
        //
        // 摘要:
        //     Browser Search key
        VK_BROWSER_SEARCH = 170,
        //
        // 摘要:
        //     Browser Favorites key
        VK_BROWSER_FAVORITES = 171,
        //
        // 摘要:
        //     Browser Start and Home key
        VK_BROWSER_HOME = 172,
        //
        // 摘要:
        //     Volume Mute key
        VK_VOLUME_MUTE = 173,
        //
        // 摘要:
        //     Volume Down key
        VK_VOLUME_DOWN = 174,
        //
        // 摘要:
        //     Volume Up key
        VK_VOLUME_UP = 175,
        //
        // 摘要:
        //     Next Track key
        VK_MEDIA_NEXT_TRACK = 176,
        //
        // 摘要:
        //     Previous Track key
        VK_MEDIA_PREV_TRACK = 177,
        //
        // 摘要:
        //     Stop Media key
        VK_MEDIA_STOP = 178,
        //
        // 摘要:
        //     Play/Pause Media key
        VK_MEDIA_PLAY_PAUSE = 179,
        //
        // 摘要:
        //     Start Mail key
        VK_LAUNCH_MAIL = 180,
        //
        // 摘要:
        //     Select Media key
        VK_LAUNCH_MEDIA_SELECT = 181,
        //
        // 摘要:
        //     Start Application 1 key
        VK_LAUNCH_APP1 = 182,
        //
        // 摘要:
        //     Start Application 2 key
        VK_LAUNCH_APP2 = 183,
        //
        // 摘要:
        //     Used for miscellaneous characters; it can vary by keyboard.
        //
        // 言论：
        //     For the US standard keyboard, the ';:' key
        VK_OEM_1 = 186,
        //
        // 摘要:
        //     For any country/region, the '+' key.
        VK_OEM_PLUS = 187,
        //
        // 摘要:
        //     For any country/region, the ',' key.
        VK_OEM_COMMA = 188,
        //
        // 摘要:
        //     For any country/region, the '-' key.
        VK_OEM_MINUS = 189,
        //
        // 摘要:
        //     For any country/region, the '.' key.
        VK_OEM_PERIOD = 190,
        //
        // 摘要:
        //     Used for miscellaneous characters; it can vary by keyboard.
        //
        // 言论：
        //     For the US standard keyboard, the '/?' key
        VK_OEM_2 = 191,
        //
        // 摘要:
        //     Used for miscellaneous characters; it can vary by keyboard.
        //
        // 言论：
        //     For the US standard keyboard, the '`~' key
        VK_OEM_3 = 192,
        //
        // 摘要:
        //     Used for miscellaneous characters; it can vary by keyboard.
        //
        // 言论：
        //     For the US standard keyboard, the '[{' key
        VK_OEM_4 = 219,
        //
        // 摘要:
        //     Used for miscellaneous characters; it can vary by keyboard.
        //
        // 言论：
        //     For the US standard keyboard, the '\|' key
        VK_OEM_5 = 220,
        //
        // 摘要:
        //     Used for miscellaneous characters; it can vary by keyboard.
        //
        // 言论：
        //     For the US standard keyboard, the ']}' key
        VK_OEM_6 = 221,
        //
        // 摘要:
        //     Used for miscellaneous characters; it can vary by keyboard.
        //
        // 言论：
        //     For the US standard keyboard, the 'single-quote/double-quote' (''"') key
        VK_OEM_7 = 222,
        //
        // 摘要:
        //     Used for miscellaneous characters; it can vary by keyboard.
        VK_OEM_8 = 223,
        //
        // 摘要:
        //     OEM specific
        //
        // 言论：
        //     'AX' key on Japanese AX kbd
        VK_OEM_AX = 225,
        //
        // 摘要:
        //     Either the angle bracket ("") key or the backslash ("\|") key on the RT 102-key
        //     keyboard
        VK_OEM_102 = 226,
        //
        // 摘要:
        //     OEM specific
        //
        // 言论：
        //     Help key on ICO
        VK_ICO_HELP = 227,
        //
        // 摘要:
        //     OEM specific
        //
        // 言论：
        //     00 key on ICO
        VK_ICO_00 = 228,
        //
        // 摘要:
        //     IME PROCESS key
        VK_PROCESSKEY = 229,
        //
        // 摘要:
        //     OEM specific
        //
        // 言论：
        //     Clear key on ICO
        VK_ICO_CLEAR = 230,
        //
        // 摘要:
        //     Used to pass Unicode characters as if they were keystrokes. The VK_PACKET key
        //     is the low word of a 32-bit Virtual Key value used for non-keyboard input methods.
        //
        // 言论：
        //     For more information, see Remark in PInvoke.KEYBDINPUT, PInvoke.SendInput(System.Int32,PInvoke.INPUT*,System.Int32),
        //     PInvoke.WindowMessage.WM_KEYDOWN, and PInvoke.WindowMessage.WM_KEYUP
        VK_PACKET = 231,
        //
        // 摘要:
        //     Nokia/Ericsson definition
        VK_OEM_RESET = 233,
        //
        // 摘要:
        //     Nokia/Ericsson definition
        VK_OEM_JUMP = 234,
        //
        // 摘要:
        //     Nokia/Ericsson definition
        VK_OEM_PA1 = 235,
        //
        // 摘要:
        //     Nokia/Ericsson definition
        VK_OEM_PA2 = 236,
        //
        // 摘要:
        //     Nokia/Ericsson definition
        VK_OEM_PA3 = 237,
        //
        // 摘要:
        //     Nokia/Ericsson definition
        VK_OEM_WSCTRL = 238,
        //
        // 摘要:
        //     Nokia/Ericsson definition
        VK_OEM_CUSEL = 239,
        //
        // 摘要:
        //     Nokia/Ericsson definition
        VK_OEM_ATTN = 240,
        //
        // 摘要:
        //     Nokia/Ericsson definition
        VK_OEM_FINISH = 241,
        //
        // 摘要:
        //     Nokia/Ericsson definition
        VK_OEM_COPY = 242,
        //
        // 摘要:
        //     Nokia/Ericsson definition
        VK_OEM_AUTO = 243,
        //
        // 摘要:
        //     Nokia/Ericsson definition
        VK_OEM_ENLW = 244,
        //
        // 摘要:
        //     Nokia/Ericsson definition
        VK_OEM_BACKTAB = 245,
        //
        // 摘要:
        //     Attn key
        VK_ATTN = 246,
        //
        // 摘要:
        //     CrSel key
        VK_CRSEL = 247,
        //
        // 摘要:
        //     ExSel key
        VK_EXSEL = 248,
        //
        // 摘要:
        //     Erase EOF key
        VK_EREOF = 249,
        //
        // 摘要:
        //     Play key
        VK_PLAY = 250,
        //
        // 摘要:
        //     Zoom key
        VK_ZOOM = 251,
        //
        // 摘要:
        //     Reserved constant by Windows headers definition
        VK_NONAME = 252,
        //
        // 摘要:
        //     PA1 key
        VK_PA1 = 253,
        //
        // 摘要:
        //     Clear key
        VK_OEM_CLEAR = 254
    }
}
