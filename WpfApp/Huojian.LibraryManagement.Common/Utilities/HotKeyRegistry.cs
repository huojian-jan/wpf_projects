using ShadowBot.Common.LocalizationResources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using Huojian.LibraryManagement.Common;
using Huojian.LibraryManagement.Common.PInvoke;
using Microsoft.VisualBasic;

namespace ShadowBot.Common.Utilities
{
    public class HotKeyRegistry
    {
        private static readonly HotKeyRegistry _current;
        private static readonly Dictionary<int, HotKey> _hotkeyDict = new Dictionary<int, HotKey>();
        private static IntPtr _hWnd;
        private static HwndSource _source;

        static HotKeyRegistry()
        {
            _current = new HotKeyRegistry();
        }

        public static ICollection<HotKey> RegisteredHotKeys => _hotkeyDict.Values;

        public static void Initialize(IntPtr hWnd)
        {
            _hWnd = hWnd;
            _source = HwndSource.FromHwnd(hWnd);
            _source.AddHook(HwndHook);
        }

        public static HotKey RegisterHotKey(ModifierKeys modifierKeys, VirtualKey virtualKey)
        {
            return RegisterHotKey(modifierKeys, virtualKey, null, null);
        }

        public static HotKey RegisterHotKey(ModifierKeys modifierKeys, VirtualKey virtualKey, string hotkeyName)
        {
            return RegisterHotKey(modifierKeys, virtualKey, hotkeyName, null);
        }

        public static HotKey RegisterHotKey(ModifierKeys modifierKeys, VirtualKey virtualKey, string hotkeyName, Action action)
        {
            if (_hWnd == IntPtr.Zero)
                throw new GlobalHotKeyException($"{Strings.HotKeyRegistry_GlobalHotkeySettingsHaveNotBeenInitialized}");

            var hotkeyId = GenerateHotkeyId(modifierKeys, virtualKey);
            var hotkey = new HotKey(hotkeyId, hotkeyName, modifierKeys, virtualKey, action);
            if (!PInvoke.RegisterHotKey((HWND)_hWnd, hotkeyId, (HOT_KEY_MODIFIERS)(int)modifierKeys, (uint)virtualKey))
                throw new GlobalHotKeyException($"{Strings.HotKeyRegistry_UnableToRegisterGlobalHotkey}");

            _hotkeyDict[hotkeyId] = hotkey;
            return hotkey;
        }

        public static void UnRegisterHotKey(int hotKeyId)
        {
            if (_hWnd == IntPtr.Zero)
                throw new GlobalHotKeyException($"{Strings.HotKeyRegistry_GlobalHotkeySettingsHaveNotBeenInitialized}");

            if (!_hotkeyDict.ContainsKey(hotKeyId))
                return;
            PInvoke.UnregisterHotKey((HWND)_hWnd, hotKeyId);
            _hotkeyDict.Remove(hotKeyId);
        }

        public static bool UnRegisterAllHotKeys()
        {
            if (_hWnd == IntPtr.Zero)
                return false;

            int count = _hotkeyDict.Count;
            foreach (var item in _hotkeyDict)
            {
                int hotKeyId = item.Key;

                PInvoke.UnregisterHotKey((HWND)_hWnd, hotKeyId);
                _hotkeyDict.Remove(hotKeyId);
            }
            return count > 0;
        }

        public static void SetListener(string hotkeyName, Action action)
        {
            var hotkey = RegisteredHotKeys.FirstOrDefault(m => m.Name == hotkeyName);
            if (hotkey != null)
                hotkey.Action = action;
        }

        public static void RemoveListener(string hotkeyName)
        {
            var hotkey = RegisteredHotKeys.FirstOrDefault(m => m.Name == hotkeyName);
            if (hotkey != null)
                hotkey.Action = null;
        }

        public static Action GetListener(string hotkeyName)
        {
            var hotkey = RegisteredHotKeys.FirstOrDefault(m => m.Name == hotkeyName);
            if (hotkey != null)
            {
                return hotkey.Action;
            }
            else
            {
                return null;
            }
        }

        public static bool HotKeyContains(ModifierKeys modifierKeys, VirtualKey virtualKey)
        {
            var key = GenerateHotkeyId(modifierKeys, virtualKey);
            return _hotkeyDict.ContainsKey(key);
        }

        private static IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch ((uint)msg)
            {
                case PInvoke.WM_HOTKEY:
                    var hotkeyId = wParam.ToInt32();
                    if (_hotkeyDict.TryGetValue(hotkeyId, out HotKey hotKey))
                    {
                        try
                        {
                            hotKey.Action?.Invoke();
                        }
                        catch (Exception ex)
                        {
                            Logging.Error($"HotKey action exception error, code is {ex.HResult}", ex);
                        }
                    }
                    break;
            }
            return IntPtr.Zero;
        }

        private static int GenerateHotkeyId(ModifierKeys modifierKeys, VirtualKey virtualKey)
        {
            return (int)modifierKeys ^ ((int)virtualKey << 4) ^ _hWnd.ToInt32();
        }
    }

    public class HotKey
    {
        internal HotKey(int id, string name, ModifierKeys modifierKeys, VirtualKey virtualKey, Action action)
        {
            Id = id;
            Name = name;
            ModifierKeys = modifierKeys;
            VirtualKey = virtualKey;
            Action = action;
            DisplayKeyString = FormatString(ModifierKeys, VirtualKey);
        }

        public int Id { get; }
        public string Name { get; }
        public ModifierKeys ModifierKeys { get; }
        public VirtualKey VirtualKey { get; }
        public Action Action { get; set; }

        public string DisplayKeyString { get; }

        public static string FormatString(ModifierKeys modifierKeys, VirtualKey virtualKey)
        {
            var keyString = new StringBuilder();
            if (modifierKeys.HasFlag(ModifierKeys.Win))
                keyString.Append("Win+");
            if (modifierKeys.HasFlag(ModifierKeys.Control))
                keyString.Append("Ctrl+");
            if (modifierKeys.HasFlag(ModifierKeys.Shift))
                keyString.Append("Shift+");
            if (modifierKeys.HasFlag(ModifierKeys.Alt))
                keyString.Append("Alt+");
            keyString.Append(virtualKey.ToString().Substring(3)); // remove "VK_"
            return keyString.ToString();
        }
    }

    [Serializable]
    public class GlobalHotKeyException : Exception
    {
        public GlobalHotKeyException() { }
        public GlobalHotKeyException(string message) : base(message) { }
        public GlobalHotKeyException(string message, Exception inner) : base(message, inner) { }
        protected GlobalHotKeyException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
