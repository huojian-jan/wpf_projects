// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.WindowsMonitorAPI
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Runtime.InteropServices;
using System.Windows;

#nullable disable
namespace ticktick_WPF.Util
{
  public static class WindowsMonitorAPI
  {
    private const string User32 = "user32.dll";
    public static readonly HandleRef NullHandleRef = new HandleRef((object) null, IntPtr.Zero);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern bool GetMonitorInfo(
      HandleRef hmonitor,
      [In, Out] WindowsMonitorAPI.MONITORINFOEX info);

    [DllImport("user32.dll")]
    public static extern bool EnumDisplayMonitors(
      HandleRef hdc,
      WindowsMonitorAPI.COMRECT rcClip,
      WindowsMonitorAPI.MonitorEnumProc lpfnEnum,
      IntPtr dwData);

    public delegate bool MonitorEnumProc(
      IntPtr monitor,
      IntPtr hdc,
      IntPtr lprcMonitor,
      IntPtr lParam);

    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Auto)]
    public class MONITORINFOEX
    {
      internal int cbSize = Marshal.SizeOf(typeof (WindowsMonitorAPI.MONITORINFOEX));
      internal WindowsMonitorAPI.RECT rcMonitor;
      internal WindowsMonitorAPI.RECT rcWork;
      internal int dwFlags;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
      internal char[] szDevice = new char[32];
    }

    public struct RECT
    {
      public int left;
      public int top;
      public int right;
      public int bottom;

      public RECT(Rect r)
      {
        this.left = (int) r.Left;
        this.top = (int) r.Top;
        this.right = (int) r.Right;
        this.bottom = (int) r.Bottom;
      }
    }

    [StructLayout(LayoutKind.Sequential)]
    public class COMRECT
    {
      public int left;
      public int top;
      public int right;
      public int bottom;
    }
  }
}
