// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Timeline.TimelineYearClipConverter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

#nullable disable
namespace ticktick_WPF.Views.Timeline
{
  public class TimelineYearClipConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      if (values.Length == 4 && values[0] is double num1 && values[1] is double x && values[2] is double num2 && values[3] is double height)
      {
        double width = num1 - num2 - x;
        if (width > 0.0)
          return (object) new RectangleGeometry(new Rect(x, 0.0, width, height));
      }
      return (object) null;
    }

    public object[] ConvertBack(
      object value,
      Type[] targetTypes,
      object parameter,
      CultureInfo culture)
    {
      return (object[]) null;
    }
  }
}
