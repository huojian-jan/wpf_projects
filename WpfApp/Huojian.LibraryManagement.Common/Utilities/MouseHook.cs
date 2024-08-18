using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using Huojian.LibraryManagement.Common;

namespace ShadowBot.Common.Utilities
{
    /// <summary>
    /// Class for intercepting low level Windows mouse hooks.
    /// </summary>
    public class MouseHook
    {
        //模拟鼠标发送的dwExtraInfo
        public static readonly UIntPtr MagicUNum = new UIntPtr(29384756);
        public static readonly IntPtr MagicNum = new IntPtr(29384756);

        private static ConcurrentDictionary<MouseHook, HHOOK> s_allHookInfos = new ConcurrentDictionary<MouseHook, HHOOK>();
        public static List<MouseHook> UninstallAll()
        {
            var UninstalledAll = new List<MouseHook>();
            foreach (var item in s_allHookInfos)
            {
                var pThis = item.Key;
                pThis.Uninstall();
                UninstalledAll.Add(pThis);
            }
            return UninstalledAll;
        }
        public static void ReinstallAll(List<MouseHook> UninstalledAll)
        {
            foreach (var item in UninstalledAll)
            {
                var pThis = item;
                pThis.Install();
            }
            UninstalledAll.Clear();
        }
        /// <summary>
        /// Internal callback processing function
        /// </summary>
        private HOOKPROC hookHandler;

        /// <summary>
        /// Function to be called when defined even occurs
        /// </summary>
        /// <param name="mouseStruct">MSLLHOOKSTRUCT mouse structure</param>
        public delegate void MouseHookCallback(MouseHookEventArgs args);

        #region Events
        public event MouseHookCallback MouseEvent;
        public event MouseHookCallback LeftButtonDown;
        public event MouseHookCallback LeftButtonUp;
        public event MouseHookCallback RightButtonDown;
        public event MouseHookCallback RightButtonUp;
        public event MouseHookCallback MouseMove;
        public event MouseHookCallback MouseWheel;
        public event MouseHookCallback DoubleClick;
        public event MouseHookCallback MiddleButtonDown;
        public event MouseHookCallback MiddleButtonUp;
        #endregion

        /// <summary>
        /// Low level mouse hook's ID
        /// </summary>
        private HHOOK hookID;

        /// <summary>
        /// Install low level mouse hook
        /// </summary>
        /// <param name="mouseHookCallbackFunc">Callback function</param>
        public void Install()
        {
            Debug.Assert(hookID == default(HHOOK));

            hookHandler = HookFunc;
            hookID = SetHook(hookHandler);
            if (hookID == default(HHOOK))
            {
                Logging.Warn("xx");
                return;
            }
            s_allHookInfos.TryAdd(this, hookID);
        }

        /// <summary>
        /// Remove low level mouse hook
        /// </summary>
        public void Uninstall()
        {
            if (hookID == default(HHOOK))
                return;

            PInvoke.UnhookWindowsHookEx(hookID);
            s_allHookInfos.TryRemove(this, out _);
            hookID = default(HHOOK);
        }

        /// <summary>
        /// Destructor. Unhook current hook
        /// </summary>
        ~MouseHook()
        {
            Uninstall();
        }

        /// <summary>
        /// Sets hook and assigns its ID for tracking
        /// </summary>
        /// <param name="proc">Internal callback function</param>
        /// <returns>Hook ID</returns>
        private unsafe HHOOK SetHook(HOOKPROC proc)
        {
            using (ProcessModule module = Process.GetCurrentProcess().MainModule)
            {
                fixed (char* lpModuleNameLocal = module.ModuleName)
                {
                    HINSTANCE hmod = PInvoke.GetModuleHandle(lpModuleNameLocal);
                    return PInvoke.SetWindowsHookEx(WINDOWS_HOOK_ID.WH_MOUSE_LL, proc, hmod, 0);
                }
            }
        }

        /// <summary>
        /// Callback function
        /// </summary>
        private LRESULT HookFunc(int nCode, WPARAM wParam, LPARAM lParam)
        {
            // parse system messages
            if (nCode >= 0)
            {
                var kind = (uint)(nuint)wParam;
                var args = new MouseHookEventArgs(kind, (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT)));
                MouseEvent?.Invoke(args);
                switch (kind)
                {
                    case PInvoke.WM_LBUTTONDOWN:
                        LeftButtonDown?.Invoke(args);
                        break;
                    case PInvoke.WM_LBUTTONUP:
                        LeftButtonUp?.Invoke(args);
                        break;
                    case PInvoke.WM_MOUSEMOVE:
                        MouseMove?.Invoke(args);
                        break;
                    case PInvoke.WM_MOUSEWHEEL:
                        MouseWheel?.Invoke(args);
                        break;
                    case PInvoke.WM_RBUTTONDOWN:
                        RightButtonDown?.Invoke(args);
                        break;
                    case PInvoke.WM_RBUTTONUP:
                        RightButtonUp?.Invoke(args);
                        break;
                    case PInvoke.WM_LBUTTONDBLCLK:
                        DoubleClick?.Invoke(args);
                        break;
                    case PInvoke.WM_MBUTTONDOWN:
                        MiddleButtonDown?.Invoke(args);
                        break;
                    case PInvoke.WM_MBUTTONUP:
                        MiddleButtonUp?.Invoke(args);
                        break;
                    default:
                        break;
                }
                if (args.Handled)
                    return (LRESULT)(IntPtr)1;
            }
            return PInvoke.CallNextHookEx(hookID, nCode, wParam, lParam);
        }
    }

    public class MouseHookEventArgs : EventArgs
    {
        internal MouseHookEventArgs(uint kind, MSLLHOOKSTRUCT mouseStruct)
        {
            Kind = kind;
            MouseStruct = mouseStruct;
        }

        public MSLLHOOKSTRUCT MouseStruct { get; }

        public uint Kind { get; }

        public bool Handled { get; set; } = false;
    }
}