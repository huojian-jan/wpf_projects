// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.WindowSizing
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media;

#nullable disable
namespace ticktick_WPF.Util
{
  public static class WindowSizing
  {
    private const int MONITOR_DEFAULTTONEAREST = 2;
    private static Dictionary<Window, Action> _disposeHandlers = new Dictionary<Window, Action>();

    [DllImport("shell32", CallingConvention = CallingConvention.StdCall)]
    public static extern int SHAppBarMessage(int dwMessage, ref WindowSizing.APPBARDATA pData);

    [DllImport("user32", SetLastError = true)]
    private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32")]
    internal static extern bool GetMonitorInfo(IntPtr hMonitor, WindowSizing.MONITORINFO lpmi);

    [DllImport("user32")]
    internal static extern IntPtr MonitorFromWindow(IntPtr handle, int flags);

    private static WindowSizing.MINMAXINFO AdjustWorkingAreaForAutoHide(
      IntPtr monitorContainingApplication,
      WindowSizing.MINMAXINFO mmi)
    {
      IntPtr window = WindowSizing.FindWindow("Shell_TrayWnd", (string) null);
      IntPtr num = WindowSizing.MonitorFromWindow(window, 2);
      if (!monitorContainingApplication.Equals((object) num))
        return mmi;
      WindowSizing.APPBARDATA pData = new WindowSizing.APPBARDATA();
      pData.cbSize = Marshal.SizeOf<WindowSizing.APPBARDATA>(pData);
      pData.hWnd = window;
      WindowSizing.SHAppBarMessage(5, ref pData);
      WindowSizing.ABEdge edge = WindowSizing.GetEdge(pData.rc);
      if (((WindowSizing.ABState) WindowSizing.SHAppBarMessage(4, ref pData)).HasFlag((Enum) WindowSizing.ABState.ABS_AUTOHIDE))
        WindowSizing.AdjustSizeForAutohide(edge, ref mmi);
      return mmi;
    }

    private static void AdjustSizeForAutohide(
      WindowSizing.ABEdge uEdge,
      ref WindowSizing.MINMAXINFO mmi)
    {
      switch (uEdge)
      {
        case WindowSizing.ABEdge.ABE_LEFT:
          mmi.ptMaxPosition.x += 2;
          mmi.ptMaxTrackSize.x -= 2;
          mmi.ptMaxSize.x -= 2;
          break;
        case WindowSizing.ABEdge.ABE_TOP:
          mmi.ptMaxPosition.y += 2;
          mmi.ptMaxTrackSize.y -= 2;
          mmi.ptMaxSize.y -= 2;
          break;
        case WindowSizing.ABEdge.ABE_RIGHT:
          mmi.ptMaxSize.x -= 2;
          mmi.ptMaxTrackSize.x -= 2;
          break;
        case WindowSizing.ABEdge.ABE_BOTTOM:
          mmi.ptMaxSize.y -= 2;
          mmi.ptMaxTrackSize.y -= 2;
          break;
      }
    }

    private static WindowSizing.ABEdge GetEdge(WindowSizing.RECT rc)
    {
      if (rc.top == rc.left && rc.bottom > rc.right)
        return WindowSizing.ABEdge.ABE_LEFT;
      if (rc.top == rc.left && rc.bottom < rc.right)
        return WindowSizing.ABEdge.ABE_TOP;
      return rc.top > rc.left ? WindowSizing.ABEdge.ABE_BOTTOM : WindowSizing.ABEdge.ABE_RIGHT;
    }

    public static Rectangle GetMonitorFromWindowLocation(Window window)
    {
      return Screen.FromRectangle(new Rectangle((int) window.Left, (int) window.Top, (int) window.Width, (int) window.Height)).WorkingArea;
    }

    public static Rectangle GetMonitorFromWindow(Window window)
    {
      HwndSource hwndSource = (HwndSource) PresentationSource.FromVisual((Visual) window);
      return hwndSource != null && VirtualDesktopUtils.GetIsInCurrent(hwndSource.Handle) ? Screen.FromHandle(hwndSource.Handle).WorkingArea : new Rectangle();
    }

    public static bool WmGetMinMaxInfo(
      IntPtr hwnd,
      IntPtr lParam,
      double minWidth,
      double minHeight)
    {
      IntPtr hMonitor = WindowSizing.MonitorFromWindow(hwnd, 2);
      if (!(hMonitor != IntPtr.Zero))
        return false;
      WindowSizing.MONITORINFO lpmi = new WindowSizing.MONITORINFO();
      WindowSizing.GetMonitorInfo(hMonitor, lpmi);
      WindowSizing.RECT rcWork = lpmi.rcWork;
      WindowSizing.RECT rcMonitor = lpmi.rcMonitor;
      int num = 0;
      WindowSizing.MINMAXINFO structure = (WindowSizing.MINMAXINFO) Marshal.PtrToStructure(lParam, typeof (WindowSizing.MINMAXINFO));
      structure.ptMaxPosition.x = Math.Abs(rcWork.left - rcMonitor.left) - num;
      structure.ptMaxPosition.y = Math.Abs(rcWork.top - rcMonitor.top) - num;
      structure.ptMaxSize.x = Math.Abs(rcWork.right - rcWork.left) + 2 * num;
      structure.ptMaxSize.y = Math.Abs(rcWork.bottom - rcWork.top) + 2 * num;
      structure.ptMaxTrackSize.x = structure.ptMaxSize.x;
      structure.ptMaxTrackSize.y = structure.ptMaxSize.y;
      double scalingRatio = ScreenPositionUtils.GetScalingRatio();
      structure.ptMinTrackSize.x = (int) (minWidth * scalingRatio);
      structure.ptMinTrackSize.y = (int) (minHeight * scalingRatio);
      WindowSizing.APPBARDATA pData = new WindowSizing.APPBARDATA();
      pData.cbSize = Marshal.SizeOf<WindowSizing.APPBARDATA>(pData);
      IntPtr window = WindowSizing.FindWindow("Shell_TrayWnd", (string) null);
      pData.hWnd = window;
      WindowSizing.SHAppBarMessage(5, ref pData);
      WindowSizing.ABEdge edge = WindowSizing.GetEdge(pData.rc);
      if (((WindowSizing.ABState) WindowSizing.SHAppBarMessage(4, ref pData)).HasFlag((Enum) WindowSizing.ABState.ABS_AUTOHIDE))
        WindowSizing.AdjustSizeForAutohide(edge, ref structure);
      Marshal.StructureToPtr((object) structure, lParam, true);
      return true;
    }

    public enum ABEdge
    {
      ABE_LEFT,
      ABE_TOP,
      ABE_RIGHT,
      ABE_BOTTOM,
    }

    [Flags]
    public enum ABState
    {
      ABS_AUTOHIDE = 1,
    }

    public enum ABMsg
    {
      ABM_GETSTATE = 4,
      ABM_GETTASKBARPOS = 5,
    }

    public struct APPBARDATA
    {
      public int cbSize;
      public IntPtr hWnd;
      public int uCallbackMessage;
      public int uEdge;
      public WindowSizing.RECT rc;
      public bool lParam;
    }

    public struct MINMAXINFO
    {
      public WindowSizing.POINT ptReserved;
      public WindowSizing.POINT ptMaxSize;
      public WindowSizing.POINT ptMaxPosition;
      public WindowSizing.POINT ptMinTrackSize;
      public WindowSizing.POINT ptMaxTrackSize;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public class MONITORINFO
    {
      public int cbSize = Marshal.SizeOf(typeof (WindowSizing.MONITORINFO));
      public WindowSizing.RECT rcMonitor;
      public WindowSizing.RECT rcWork;
      public int dwFlags;
    }

    public struct POINT
    {
      public int x;
      public int y;

      public POINT(int x, int y)
      {
        this.x = x;
        this.y = y;
      }
    }

    public struct RECT
    {
      public int left;
      public int top;
      public int right;
      public int bottom;
    }
  }
}
