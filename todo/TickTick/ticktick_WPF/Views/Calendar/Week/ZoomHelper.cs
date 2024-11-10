// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Calendar.Week.ZoomHelper
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

#nullable disable
namespace ticktick_WPF.Views.Calendar.Week
{
  public static class ZoomHelper
  {
    public static void Zoom(double delta, double actualHeight, ScrollViewer scroller)
    {
      double hourHeight = CalendarGeoHelper.HourHeight;
      double height1 = (actualHeight * 1.0 - (CalendarGeoHelper.TopFolded ? 64.0 : 0.0)) / (double) (24 - CalendarGeoHelper.GetCollapsedHours());
      bool zoomIn = delta > 0.0;
      double height2 = hourHeight > height1 || zoomIn ? ZoomHelper.GetNextZoomHeight(zoomIn, hourHeight) : height1;
      if (height2 <= height1)
        height2 = (double) ZoomHelper.GetValidAutoHeight(height1);
      if (Math.Abs(height2 - hourHeight) <= 0.0)
        return;
      CalendarGeoHelper.SetCalendarHourHeight(height2);
      scroller.ScrollToVerticalOffset(scroller.VerticalOffset / hourHeight * height2);
    }

    private static int GetValidAutoHeight(double height)
    {
      int validAutoHeight = (int) height;
      switch (validAutoHeight % 4)
      {
        case 1:
          --validAutoHeight;
          break;
        case 2:
          validAutoHeight += 2;
          break;
        case 3:
          ++validAutoHeight;
          break;
      }
      return validAutoHeight;
    }

    private static double GetNextZoomHeight(bool zoomIn, double checkHeight)
    {
      List<double> hourZoomHeight = CalendarGeoHelper.HourZoomHeight;
      if (checkHeight >= hourZoomHeight.Last<double>())
        return !zoomIn ? hourZoomHeight[hourZoomHeight.Count - 2] : hourZoomHeight.Last<double>();
      if (checkHeight <= hourZoomHeight.First<double>())
        return zoomIn ? hourZoomHeight[1] : hourZoomHeight.First<double>();
      int num1 = 0;
      for (int index = 0; index < hourZoomHeight.Count - 1; ++index)
      {
        double num2 = hourZoomHeight[index];
        double num3 = hourZoomHeight[index + 1];
        if (num2 >= checkHeight && num2 < num3)
        {
          num1 = index;
          break;
        }
      }
      return !zoomIn ? hourZoomHeight[Math.Max(0, num1 - 1)] : hourZoomHeight[num1 + 1];
    }
  }
}
