using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static ShadowBot.Common.Utilities.KeyboardHook;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using Windows.Win32.UI.Input.KeyboardAndMouse;

namespace ShadowBot.Common.Utilities
{
    public class KeyboardHelper
    {
        public static bool IsKeyPressed(int vKey)
        {
            return 0 != (PInvoke.GetAsyncKeyState(vKey) & 0x8000);
        }

        public static bool IsKeyLocked(int vKey)
        {
            return PInvoke.GetKeyState(vKey) == 1;
        }

        public static KeyboardModifierKeys GetModifierKeys()
        {
            var keys = KeyboardModifierKeys.None;
            if (IsKeyPressed((int)VKeys.MENU))
                keys |= KeyboardModifierKeys.Alt;
            if (IsKeyPressed((int)VKeys.CONTROL))
                keys |= KeyboardModifierKeys.Control;
            if (IsKeyPressed((int)VKeys.SHIFT))
                keys |= KeyboardModifierKeys.Shift;
            if (IsKeyPressed((int)VKeys.LWIN) || IsKeyPressed((int)VKeys.RWIN))
                keys |= KeyboardModifierKeys.Windows;
            return keys;
        }
    }

    [Flags]
    public enum KeyboardModifierKeys
    {
        None = 0,
        Alt = 1,
        Control = 2,
        Shift = 4,
        Windows = 8
    }
}
