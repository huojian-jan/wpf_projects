// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.ScreenExtensions
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

#nullable disable
namespace ticktick_WPF.Util
{
  public static class ScreenExtensions
  {
    public static void GetDpi(this Screen screen, DpiType dpiType, out uint dpiX, out uint dpiY)
    {
      Rectangle bounds = screen.Bounds;
      int x = bounds.Left + 1;
      bounds = screen.Bounds;
      int y = bounds.Top + 1;
      ScreenExtensions.GetDpiForMonitor(ScreenExtensions.MonitorFromPoint(new Point(x, y), 2U), dpiType, out dpiX, out dpiY);
    }

    [DllImport("User32.dll")]
    private static extern IntPtr MonitorFromPoint([In] Point pt, [In] uint dwFlags);

    [DllImport("Shcore.dll")]
    private static extern IntPtr GetDpiForMonitor(
      [In] IntPtr hmonitor,
      [In] DpiType dpiType,
      out uint dpiX,
      out uint dpiY);
  }
}
