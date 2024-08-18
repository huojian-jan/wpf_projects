using System.ComponentModel;
using System.Runtime.InteropServices;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Dwm;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Huojian.LibraryManagement.Common.PInvoke
{
    public static class User32X
    {
        //
        // 摘要:
        //     Retrieves the name of the class to which the specified window belongs.
        //
        // 参数:
        //   hWnd:
        //     A handle to the window and, indirectly, the class to which the window belongs.
        //
        //   maxLength:
        //     The size of the string to return
        //
        // 返回结果:
        //     The class name string.
        //
        // 异常:
        //   T:PInvoke.Win32Exception:
        //     Thrown when an error occurs.
        //
        // 言论：
        //     The maximum length for lpszClassName is 256. See WNDCLASS structure documentation:
        //     https://msdn.microsoft.com/en-us/library/windows/desktop/ms633576(v=vs.85).aspx
        public unsafe static string GetClassName(IntPtr hWnd, int maxLength = 256)
        {
            char* ptr = stackalloc char[maxLength];
            int className = Windows.Win32.PInvoke.GetClassName((HWND)hWnd, ptr, maxLength);
            if (className == 0)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            return new string(ptr, 0, className);
        }

        // 此方法不如SwitchToThisWindow通用，但是比较适用于UIATools的场景，暂时统一使用SwitchToThisWindow
        public unsafe static void SetForegroundWindow(IntPtr _hWnd)
        {
            HWND m_hWnd =new HWND(_hWnd);
            var hCurWnd = Windows.Win32.PInvoke.GetForegroundWindow();
            var dwMyID = Windows.Win32.PInvoke.GetCurrentThreadId();
            uint dwProcessId;
            var dwCurID = Windows.Win32.PInvoke.GetWindowThreadProcessId(hCurWnd, &dwProcessId);
            Windows.Win32.PInvoke.AttachThreadInput(dwCurID, dwMyID, true);
            Windows.Win32.PInvoke.SetWindowPos(m_hWnd, HWND.HWND_TOPMOST, 0, 0, 0, 0, SET_WINDOW_POS_FLAGS.SWP_NOSIZE | SET_WINDOW_POS_FLAGS.SWP_NOMOVE);
            Windows.Win32.PInvoke.SetWindowPos(m_hWnd, HWND.HWND_NOTOPMOST, 0, 0, 0, 0, SET_WINDOW_POS_FLAGS.SWP_NOSIZE | SET_WINDOW_POS_FLAGS.SWP_NOMOVE);
            Windows.Win32.PInvoke.SetForegroundWindow(m_hWnd);
            Windows.Win32.PInvoke.AttachThreadInput(dwCurID, dwMyID, false);
            Windows.Win32.PInvoke.SetFocus(m_hWnd);
            Windows.Win32.PInvoke.SetActiveWindow(m_hWnd);
        }

        /// <summary>
        /// 最小化一个窗口
        /// </summary>
        /// <param name="hWnd"></param>
        public static void MinimizedWindowWithoutAnimation(IntPtr _hWnd)
        {
            HWND hWnd = new HWND(_hWnd);
            WINDOWPLACEMENT placement = new WINDOWPLACEMENT();
            Windows.Win32.PInvoke.GetWindowPlacement(hWnd, ref placement);
            Windows.Win32.PInvoke.SetWindowPlacement(hWnd, new WINDOWPLACEMENT()
            {
                flags = placement.flags,
                showCmd = SHOW_WINDOW_CMD.SW_MINIMIZE,         //使用SW_HIDE仍然会有淡出的效果
                length = (uint)Marshal.SizeOf(typeof(WINDOWPLACEMENT)),
                ptMaxPosition = placement.ptMaxPosition,
                ptMinPosition = placement.ptMinPosition,
                rcNormalPosition = placement.rcNormalPosition
            });
        }

        /// <summary>
        /// 将一个最小化窗口显示出来：
        ///     1、最小化之前如果是normal , ->normal
        ///     2、最小化之前如果是max , ->max
        /// </summary>
        /// <param name="hWnd"></param>
        public static void ShowWindowWithoutAnimation(IntPtr _hWnd)
        {
            HWND hWnd = new HWND(_hWnd);
            WINDOWPLACEMENT placement = new WINDOWPLACEMENT();
            Windows.Win32.PInvoke.GetWindowPlacement(hWnd, ref placement);

            if (placement.flags == WINDOWPLACEMENT_FLAGS.WPF_RESTORETOMAXIMIZED)
            {
                Windows.Win32.PInvoke.SetWindowPlacement(hWnd, new WINDOWPLACEMENT()
                {
                    flags = WINDOWPLACEMENT_FLAGS.WPF_RESTORETOMAXIMIZED,
                    showCmd = SHOW_WINDOW_CMD.SW_MAXIMIZE,
                    length = (uint)Marshal.SizeOf(typeof(WINDOWPLACEMENT)),
                    ptMaxPosition = placement.ptMaxPosition,
                    ptMinPosition = placement.ptMinPosition,
                    rcNormalPosition = placement.rcNormalPosition
                });
            }
            else
            {
                Windows.Win32.PInvoke.SetWindowPlacement(hWnd, new WINDOWPLACEMENT()
                {
                    showCmd = SHOW_WINDOW_CMD.SW_SHOWNORMAL,
                    length = (uint)Marshal.SizeOf(typeof(WINDOWPLACEMENT)),
                    ptMaxPosition = placement.ptMaxPosition,
                    ptMinPosition = placement.ptMinPosition,
                    rcNormalPosition = placement.rcNormalPosition
                });
            }
        }

        /// <summary>
        /// 获取出现在任务栏上的窗口列表
        /// </summary>
        /// <returns></returns>
        public static IntPtr[] GetWindowsOnTaskbar()
        {
            var hWnds = new List<IntPtr>();
            // https://stackoverflow.com/a/62126899
            BOOL IsWindowOnTaskbar(HWND hWnd, LPARAM lparam)
            {
                if (!Windows.Win32.PInvoke.IsWindowVisible(hWnd))
                    return true;
                var exStyle = Windows.Win32.PInvoke.GetWindowLong(hWnd, WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE);
                if ((exStyle & (int)WINDOW_EX_STYLE.WS_EX_TOOLWINDOW) != 0)
                    return true;
                if ((exStyle & (int)WINDOW_EX_STYLE.WS_EX_NOACTIVATE) != 0)
                    return true;
                if (IsInvisibleWin10Background(hWnd))
                    return true;
                if (Windows.Win32.PInvoke.GetWindow(hWnd, GET_WINDOW_CMD.GW_OWNER) != IntPtr.Zero)
                    return true;
                hWnds.Add(hWnd);
                return true;
            }
            Windows.Win32.PInvoke.EnumWindows(new WNDENUMPROC(IsWindowOnTaskbar), (LPARAM)(nint)Windows.Win32.PInvoke.GetDesktopWindow());
            return hWnds.ToArray();
        }

        public unsafe static bool IsInvisibleWin10Background(IntPtr _hWnd)
        {
            HWND hWnd = new HWND(_hWnd);
            // //DwmGetWindowAttribute does not work in XP, compatible only from Vista
            if (Environment.OSVersion.Version.Major < 6)
                return false;
            int CloakedVal;
            HRESULT hRes = Windows.Win32.PInvoke.DwmGetWindowAttribute(hWnd, DWMWINDOWATTRIBUTE.DWMWA_CLOAKED, &CloakedVal, (uint)Marshal.SizeOf(CloakedVal));
            if (!hRes.Succeeded)
                CloakedVal = 0;
            return CloakedVal != 0;
        }

        public unsafe static int GetProcessIdByHwnd(IntPtr _hWnd)
        {
            HWND hWnd = new HWND(_hWnd);
            uint pid;
            Windows.Win32.PInvoke.GetWindowThreadProcessId(hWnd, &pid);
            return (int)pid;
        }

    }


}
