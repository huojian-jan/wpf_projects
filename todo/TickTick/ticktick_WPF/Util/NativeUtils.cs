// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.NativeUtils
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using ticktick_WPF.Resource;

#nullable disable
namespace ticktick_WPF.Util
{
  internal static class NativeUtils
  {
    public const int WS_MAXIMIZEBOX = 65536;
    public const int WS_MINIMIZEBOX = 131072;
    public const long WS_POPUP = 2147483648;
    public const long WS_VISIBLE = 268435456;
    public const long WS_CLIPSIBLINGS = 1073741824;
    public const long WS_CLIPCHILDREN = 536870912;
    public const long WS_EX_LEFT = 0;
    public const long WS_EX_LTRREADING = 0;
    public const long WS_EX_RIGHTSCROLLBAR = 0;
    public const long WS_EX_TOOLWINDOW = 128;
    private const int WS_EX_TRANSPARENT = 32;
    private const int GWL_EXSTYLE = -20;
    public const int WM_MOUSEHWHEEL = 526;

    [DllImport("dwmapi.dll")]
    public static extern int DwmSetWindowAttribute(
      IntPtr hwnd,
      NativeUtils.DwmWindowAttribute dwAttribute,
      ref int pvAttribute,
      int cbAttribute);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr FindWindow([MarshalAs(UnmanagedType.LPTStr)] string lpClassName, [MarshalAs(UnmanagedType.LPTStr)] string lpWindowName);

    [DllImport("user32")]
    private static extern IntPtr FindWindowEx(
      IntPtr hWnd1,
      IntPtr hWnd2,
      string lpsz1,
      string lpsz2);

    [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
    private static extern int SetWindowLong32(
      IntPtr windowHandle,
      WinParameter nIndex,
      int dwNewLong);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
    private static extern IntPtr SetWindowLongPtr64(
      IntPtr windowHandle,
      WinParameter nIndex,
      IntPtr dwNewLong);

    [DllImport("user32.dll")]
    public static extern bool SetWindowPos(
      IntPtr hWnd,
      IntPtr hWndInsertAfter,
      int X,
      int Y,
      int cx,
      int cy,
      uint uFlags);

    [DllImport("user32.dll")]
    public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    public static extern long SetWindowLong(IntPtr hWnd, int nIndex, long dwNewLong);

    [DllImport("user32.dll")]
    public static extern IntPtr SendMessageTimeout(
      IntPtr hwnd,
      uint msg,
      IntPtr wParam,
      IntPtr lParam,
      uint fuFlage,
      uint timeout,
      IntPtr result);

    [DllImport("user32.dll")]
    public static extern bool EnumWindows(NativeUtils.EnumWindowsProc proc, IntPtr lParam);

    [DllImport("user32.dll")]
    public static extern bool ShowWindow(IntPtr hwnd, int nCmdShow);

    [DllImport("user32.dll")]
    public static extern IntPtr SetParent(IntPtr hwnd, IntPtr parentHwnd);

    public static int GetWindowLong(IntPtr hwnd) => NativeUtils.GetWindowLong(hwnd, -8);

    public static int SetWindowLong(IntPtr windowHandle, WinParameter nIndex, int dwNewLong)
    {
      return IntPtr.Size == 8 ? (int) NativeUtils.SetWindowLongPtr64(windowHandle, nIndex, new IntPtr(dwNewLong)) : NativeUtils.SetWindowLong32(windowHandle, nIndex, dwNewLong);
    }

    public static IntPtr GetDesktopPtr()
    {
      IntPtr num = IntPtr.Zero;
      IntPtr zero = IntPtr.Zero;
      IntPtr desktopPtr = IntPtr.Zero;
      IntPtr window = NativeUtils.FindWindow("Progman", "Program Manager");
      if (window != IntPtr.Zero)
      {
        IntPtr windowEx = NativeUtils.FindWindowEx(window, IntPtr.Zero, "SHELLDLL_DefView", (string) null);
        if (windowEx != IntPtr.Zero)
          desktopPtr = NativeUtils.FindWindowEx(windowEx, IntPtr.Zero, "SysListView32", (string) null);
      }
      if (desktopPtr != IntPtr.Zero)
        return desktopPtr;
      while (desktopPtr == IntPtr.Zero)
      {
        num = NativeUtils.FindWindowEx(IntPtr.Zero, num, "WorkerW", (string) null);
        if (!(num == IntPtr.Zero))
        {
          IntPtr windowEx = NativeUtils.FindWindowEx(num, IntPtr.Zero, "SHELLDLL_DefView", (string) null);
          if (!(windowEx == IntPtr.Zero))
            desktopPtr = NativeUtils.FindWindowEx(windowEx, IntPtr.Zero, "SysListView32", (string) null);
        }
        else
          break;
      }
      return desktopPtr;
    }

    public static IntPtr SendMsgToProgman()
    {
      IntPtr window = NativeUtils.FindWindow("Progman", (string) null);
      IntPtr zero = IntPtr.Zero;
      NativeUtils.SendMessageTimeout(window, 1324U, IntPtr.Zero, IntPtr.Zero, 0U, 2U, zero);
      NativeUtils.EnumWindows((NativeUtils.EnumWindowsProc) ((hwnd, lParam) =>
      {
        if (NativeUtils.FindWindowEx(hwnd, IntPtr.Zero, "SHELLDLL_DefView", (string) null) != IntPtr.Zero)
          NativeUtils.ShowWindow(NativeUtils.FindWindowEx(IntPtr.Zero, hwnd, "WorkerW", (string) null), 0);
        return true;
      }), IntPtr.Zero);
      return window;
    }

    [DllImport("user32.dll")]
    private static extern int SetWindowCompositionAttribute(
      IntPtr hwnd,
      ref WindowCompositionAttributeData data);

    public static void EnableBlur(bool enable, Window window)
    {
      try
      {
        if (Utils.IsWindows7() || Utils.IsWindows11())
          return;
        WindowInteropHelper windowInteropHelper = new WindowInteropHelper(window);
        AccentPolicy structure = new AccentPolicy()
        {
          AccentState = enable ? AccentState.ACCENT_ENABLE_BLURBEHIND : AccentState.ACCENT_DISABLED
        };
        int cb = Marshal.SizeOf<AccentPolicy>(structure);
        IntPtr num = Marshal.AllocHGlobal(cb);
        Marshal.StructureToPtr<AccentPolicy>(structure, num, false);
        WindowCompositionAttributeData data = new WindowCompositionAttributeData()
        {
          Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY,
          SizeOfData = cb,
          Data = num
        };
        NativeUtils.SetWindowCompositionAttribute(windowInteropHelper.Handle, ref data);
        Marshal.FreeHGlobal(num);
      }
      catch (Exception ex)
      {
      }
    }

    public static void SetDarkTheme(HwndSource source, bool darkThemeEnabled)
    {
      int pvAttribute1 = 1;
      int pvAttribute2 = 0;
      if (darkThemeEnabled)
        NativeUtils.DwmSetWindowAttribute(source.Handle, NativeUtils.DwmWindowAttribute.DWMWA_USE_IMMERSIVE_DARK_MODE, ref pvAttribute1, Marshal.SizeOf(typeof (int)));
      else
        NativeUtils.DwmSetWindowAttribute(source.Handle, NativeUtils.DwmWindowAttribute.DWMWA_USE_IMMERSIVE_DARK_MODE, ref pvAttribute2, Marshal.SizeOf(typeof (int)));
      NativeUtils.DwmSetWindowAttribute(source.Handle, NativeUtils.DwmWindowAttribute.DWMWA_MICA_EFFECT, ref pvAttribute1, Marshal.SizeOf(typeof (int)));
    }

    public static void SetWindowExTransparent(IntPtr hwnd)
    {
      int windowLong = NativeUtils.GetWindowLong(hwnd, -20);
      NativeUtils.SetWindowLong(hwnd, -20, (long) (windowLong | 32));
    }

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetCursorPos(ref NativeUtils.InteropPoint lpPoint);

    public static Point GetCursorPosition()
    {
      NativeUtils.InteropPoint lpPoint = new NativeUtils.InteropPoint();
      NativeUtils.GetCursorPos(ref lpPoint);
      double pointDpi = ScreenPositionUtils.GetPointDpi(new Point((double) lpPoint.X, (double) lpPoint.Y));
      return new Point((double) lpPoint.X / pointDpi, (double) lpPoint.Y / pointDpi);
    }

    [Flags]
    public enum DwmWindowAttribute : uint
    {
      DWMWA_USE_IMMERSIVE_DARK_MODE = 20, // 0x00000014
      DWMWA_MICA_EFFECT = 1029, // 0x00000405
    }

    public delegate bool EnumWindowsProc(IntPtr hwnd, IntPtr lParam);

    private struct InteropPoint
    {
      public int X;
      public int Y;
    }
  }
}
