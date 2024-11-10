// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Timeline.TimelineInlineMarginConverter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

#nullable disable
namespace ticktick_WPF.Views.Timeline
{
  public class TimelineInlineMarginConverter : IMultiValueConverter
  {
    public static bool IgnoreLeftOutSide;

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      if (values.Length > 6 || values.Length == 6 && !TimelineInlineMarginConverter.IgnoreLeftOutSide)
      {
        if (((int) (values[0] as bool?) ?? 1) == 0)
          return (object) new Thickness(3.0, 0.0, 0.0, 0.0);
        if (values[1] is double num1 && values[2] is double num2 && values[3] is double num3 && values[4] is double num4 && values[5] is bool flag)
        {
          double num = flag ? num4 : 0.0;
          if (values.Length > 6)
            return num - num2 > 0.0 ? (object) new Thickness(num - num2 - 4.0, 0.0, 0.0, 0.0) : (object) new Thickness(0.0);
          if (num2 < num1 + num && num3 > 104.0)
            return (object) new Thickness(Math.Min(num1 + num - num2 + 4.0, num3 - 104.0), 0.0, 8.0, 0.0);
        }
      }
      return (object) new Thickness(8.0, 0.0, 8.0, 0.0);
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
