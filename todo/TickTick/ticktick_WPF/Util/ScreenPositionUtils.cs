// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.ScreenPositionUtils
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;

#nullable disable
namespace ticktick_WPF.Util
{
  public class ScreenPositionUtils
  {
    public static void GetScreens()
    {
      System.Drawing.Point mousePosition = Control.MousePosition;
      foreach (Screen allScreen in Screen.AllScreens)
      {
        double virtualScreenHeight = SystemParameters.VirtualScreenHeight;
        double primaryScreenHeight = SystemParameters.PrimaryScreenHeight;
        Rectangle bounds = allScreen.Bounds;
      }
    }

    public static Rectangle GetPointLocationScreen(System.Windows.Point point1, System.Windows.Point? point2)
    {
      foreach (Screen allScreen in Screen.AllScreens)
      {
        Rectangle workingArea = allScreen.WorkingArea;
        if (point1.X <= (double) (workingArea.X + workingArea.Width) && point1.X >= (double) workingArea.X && point1.Y <= (double) (workingArea.Y + workingArea.Height) && point1.Y >= (double) workingArea.Y)
          return workingArea;
      }
      if (point2.HasValue)
      {
        System.Windows.Point point = point2.Value;
        foreach (Screen allScreen in Screen.AllScreens)
        {
          Rectangle bounds = allScreen.Bounds;
          if (point.X <= (double) (bounds.X + bounds.Width) && point.X >= (double) bounds.X && point.Y <= (double) (bounds.Y + bounds.Height) && point.Y >= (double) bounds.Y)
            return bounds;
        }
      }
      return new Rectangle(0, 0, (int) SystemParameters.PrimaryScreenWidth, (int) SystemParameters.PrimaryScreenHeight);
    }

    public static double GetScalingRatio()
    {
      try
      {
        double logicalHeight = ScreenPositionUtils.GetLogicalHeight();
        double actualHeight = ScreenPositionUtils.GetActualHeight();
        if (logicalHeight > 0.0)
        {
          if (actualHeight > 0.0)
            return logicalHeight / actualHeight;
        }
      }
      catch (Exception ex)
      {
      }
      return 1.0;
    }

    private static double GetActualHeight() => SystemParameters.PrimaryScreenHeight;

    private static double GetLogicalHeight()
    {
      double logicalHeight = 0.0;
      WindowsMonitorAPI.MonitorEnumProc lpfnEnum = (WindowsMonitorAPI.MonitorEnumProc) ((m, h, lm, lp) =>
      {
        WindowsMonitorAPI.MONITORINFOEX info = new WindowsMonitorAPI.MONITORINFOEX();
        WindowsMonitorAPI.GetMonitorInfo(new HandleRef((object) null, m), info);
        if ((info.dwFlags & 1) != 0)
          logicalHeight = (double) (info.rcMonitor.bottom - info.rcMonitor.top);
        return true;
      });
      WindowsMonitorAPI.EnumDisplayMonitors(WindowsMonitorAPI.NullHandleRef, (WindowsMonitorAPI.COMRECT) null, lpfnEnum, IntPtr.Zero);
      return logicalHeight;
    }

    public static Rectangle GetMousePositionRect()
    {
      System.Drawing.Point mousePosition = Control.MousePosition;
      Rectangle mousePositionRect = ScreenPositionUtils.GetPointLocationScreen(new System.Windows.Point((double) mousePosition.X, (double) mousePosition.Y), new System.Windows.Point?());
      double scalingRatio = ScreenPositionUtils.GetScalingRatio();
      mousePositionRect = new Rectangle((int) ((double) mousePositionRect.Left / scalingRatio), (int) ((double) mousePositionRect.Top / scalingRatio), (int) ((double) mousePositionRect.Width / scalingRatio), (int) ((double) mousePositionRect.Height / scalingRatio));
      return mousePositionRect;
    }

    public static Rectangle GetWindowPositionRect(Window window)
    {
      double num = PresentationSource.FromVisual((Visual) window)?.CompositionTarget?.TransformFromDevice.M22 ?? ScreenPositionUtils.GetScalingRatio();
      System.Windows.Point point = new System.Windows.Point(window.Left * num, window.Top * num);
      Rectangle windowPositionRect = ScreenPositionUtils.GetPointLocationScreen(new System.Windows.Point(point.X, point.Y), new System.Windows.Point?());
      windowPositionRect = new Rectangle((int) ((double) windowPositionRect.Left / num), (int) ((double) windowPositionRect.Top / num), (int) ((double) windowPositionRect.Width / num), (int) ((double) windowPositionRect.Height / num));
      return windowPositionRect;
    }

    public static Screen GetPointScreen(System.Windows.Point point)
    {
      foreach (Screen allScreen in Screen.AllScreens)
      {
        if (allScreen.WorkingArea.Contains(new System.Drawing.Point((int) point.X, (int) point.Y)))
          return allScreen;
      }
      return (Screen) null;
    }

    public static double GetPointDpi(System.Windows.Point point)
    {
      if (Utils.IsWindows7())
      {
        foreach (Screen allScreen in Screen.AllScreens)
        {
          Rectangle bounds = allScreen.Bounds;
          if (bounds.Left == 0)
          {
            bounds = allScreen.Bounds;
            if (bounds.Top == 0)
            {
              bounds = allScreen.Bounds;
              return (double) bounds.Width / SystemParameters.PrimaryScreenWidth;
            }
          }
        }
        return 1.0;
      }
      Screen pointScreen = ScreenPositionUtils.GetPointScreen(point);
      if (pointScreen == null)
        return 1.0;
      uint dpiX;
      pointScreen.GetDpi(DpiType.Effective, out dpiX, out uint _);
      return (double) dpiX / 96.0;
    }
  }
}
