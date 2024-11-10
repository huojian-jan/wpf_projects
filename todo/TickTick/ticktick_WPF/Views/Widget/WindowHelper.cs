// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Widget.WindowHelper
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Widget
{
  public static class WindowHelper
  {
    public static void MoveToCenter(Window window)
    {
      WindowHelper.POINT lpPoint;
      if (!WindowHelper.GetCursorPos(out lpPoint))
        return;
      IntPtr hMonitor = WindowHelper.MonitorFromPoint(lpPoint, WindowHelper.MONITOR_DEFAULTTO.MONITOR_DEFAULTTONULL);
      WindowHelper.MONITORINFO monitorinfo = new WindowHelper.MONITORINFO()
      {
        cbSize = (uint) Marshal.SizeOf<WindowHelper.MONITORINFO>()
      };
      ref WindowHelper.MONITORINFO local = ref monitorinfo;
      WindowHelper.WINDOWPLACEMENT lpwndpl;
      if (!WindowHelper.GetMonitorInfo(hMonitor, ref local) || !WindowHelper.GetWindowPlacement(new WindowInteropHelper(window).EnsureHandle(), out lpwndpl))
        return;
      int num1 = monitorinfo.rcWork.left + Math.Max(0, (int) ((double) (monitorinfo.rcWork.Width - lpwndpl.rcNormalPosition.Width) / 2.0));
      int num2 = monitorinfo.rcWork.top + Math.Max(0, (int) ((double) (monitorinfo.rcWork.Height - lpwndpl.rcNormalPosition.Height) / 2.0));
      if (!Utils.IsWindows7())
      {
        foreach (Screen allScreen in Screen.AllScreens)
        {
          Rectangle workingArea = allScreen.WorkingArea;
          if (workingArea.Left == monitorinfo.rcWork.left)
          {
            workingArea = allScreen.WorkingArea;
            if (workingArea.Top == monitorinfo.rcWork.top)
            {
              uint dpiX;
              uint dpiY;
              allScreen.GetDpi(DpiType.Effective, out dpiX, out dpiY);
              workingArea = allScreen.WorkingArea;
              double num3 = (double) workingArea.Left / ((double) dpiX / 96.0);
              workingArea = allScreen.WorkingArea;
              double num4 = (double) workingArea.Top / ((double) dpiY / 96.0);
              workingArea = allScreen.WorkingArea;
              double num5 = (double) workingArea.Width / ((double) dpiX / 96.0);
              workingArea = allScreen.WorkingArea;
              double num6 = (double) workingArea.Height / ((double) dpiY / 96.0);
              double num7 = Math.Max(0.0, (num5 - window.ActualWidth) / 2.0);
              num1 = (int) (num3 + num7);
              num2 = (int) (num4 + Math.Max(0.0, (num6 - window.ActualHeight) / 2.0));
            }
          }
        }
      }
      window.Left = (double) num1;
      window.Top = (double) num2;
      try
      {
        window.Activate();
      }
      catch (Exception ex)
      {
      }
    }

    public static void MoveTo(Window window, int left, int top)
    {
      if ((double) left <= SystemParameters.PrimaryScreenWidth && left >= 0 && top >= 0 && (double) top <= SystemParameters.PrimaryScreenHeight)
        return;
      IntPtr hMonitor = WindowHelper.MonitorFromPoint(new WindowHelper.POINT()
      {
        x = left,
        y = top
      }, WindowHelper.MONITOR_DEFAULTTO.MONITOR_DEFAULTTONEAREST);
      WindowHelper.MONITORINFO monitorinfo = new WindowHelper.MONITORINFO()
      {
        cbSize = (uint) Marshal.SizeOf<WindowHelper.MONITORINFO>()
      };
      ref WindowHelper.MONITORINFO local = ref monitorinfo;
      WindowHelper.WINDOWPLACEMENT lpwndpl;
      if (!WindowHelper.GetMonitorInfo(hMonitor, ref local) || !WindowHelper.GetWindowPlacement(new WindowInteropHelper(window).EnsureHandle(), out lpwndpl))
        return;
      left = monitorinfo.rcWork.left + Math.Max(0, (int) ((double) (monitorinfo.rcWork.Width - lpwndpl.rcNormalPosition.Width) / 2.0));
      top = monitorinfo.rcWork.top + Math.Max(0, (int) ((double) (monitorinfo.rcWork.Height - lpwndpl.rcNormalPosition.Height) / 2.0));
      if (!Utils.IsWindows7())
      {
        foreach (Screen allScreen in Screen.AllScreens)
        {
          Rectangle workingArea = allScreen.WorkingArea;
          if (workingArea.Left == monitorinfo.rcWork.left)
          {
            workingArea = allScreen.WorkingArea;
            if (workingArea.Top == monitorinfo.rcWork.top)
            {
              uint dpiX;
              uint dpiY;
              allScreen.GetDpi(DpiType.Effective, out dpiX, out dpiY);
              workingArea = allScreen.WorkingArea;
              double num1 = (double) workingArea.Left / ((double) dpiX / 96.0);
              workingArea = allScreen.WorkingArea;
              double num2 = (double) workingArea.Top / ((double) dpiY / 96.0);
              workingArea = allScreen.WorkingArea;
              double num3 = (double) workingArea.Width / ((double) dpiX / 96.0);
              workingArea = allScreen.WorkingArea;
              double num4 = (double) workingArea.Height / ((double) dpiY / 96.0);
              double num5 = Math.Max(0.0, (num3 - window.ActualWidth) / 2.0);
              left = (int) (num1 + num5);
              top = (int) (num2 + Math.Max(0.0, (num4 - window.ActualHeight) / 2.0));
            }
          }
        }
      }
      window.Left = (double) left;
      window.Top = (double) top;
      try
      {
        window.Activate();
      }
      catch (Exception ex)
      {
      }
    }

    [DllImport("User32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetCursorPos(out WindowHelper.POINT lpPoint);

    [DllImport("User32.dll")]
    private static extern IntPtr MonitorFromPoint(
      WindowHelper.POINT pt,
      WindowHelper.MONITOR_DEFAULTTO dwFlags);

    [DllImport("User32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetMonitorInfo(IntPtr hMonitor, ref WindowHelper.MONITORINFO lpmi);

    [DllImport("User32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetWindowPlacement(
      IntPtr hWnd,
      out WindowHelper.WINDOWPLACEMENT lpwndpl);

    [DllImport("User32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetWindowPlacement(
      IntPtr hWnd,
      [In] ref WindowHelper.WINDOWPLACEMENT lpwndpl);

    private enum MONITOR_DEFAULTTO : uint
    {
      MONITOR_DEFAULTTONULL,
      MONITOR_DEFAULTTOPRIMARY,
      MONITOR_DEFAULTTONEAREST,
    }

    private struct MONITORINFO
    {
      public uint cbSize;
      public WindowHelper.RECT rcMonitor;
      public WindowHelper.RECT rcWork;
      public uint dwFlags;
    }

    private struct WINDOWPLACEMENT
    {
      public uint length;
      public uint flags;
      public uint showCmd;
      public WindowHelper.POINT ptMinPosition;
      public WindowHelper.POINT ptMaxPosition;
      public WindowHelper.RECT rcNormalPosition;
    }

    private struct POINT
    {
      public int x;
      public int y;
    }

    private struct RECT
    {
      public int left;
      public int top;
      public int right;
      public int bottom;

      public int Width => this.right - this.left;

      public int Height => this.bottom - this.top;

      public RECT(int x, int y, int width, int height)
      {
        this.left = x;
        this.top = y;
        this.right = x + width;
        this.bottom = y + height;
      }
    }
  }
}
