// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Timeline.TimelineInlineFrontWidthConverter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Globalization;
using System.Windows.Data;

#nullable disable
namespace ticktick_WPF.Views.Timeline
{
  public class TimelineInlineFrontWidthConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      if (values.Length == 5 && values[0] is double num1 && values[1] is double num2 && values[2] is double num3 && values[3] is double num4 && values[4] is bool flag)
      {
        double num = flag ? num4 : 0.0;
        if (num2 < num1 + num && num3 > 104.0)
          return (object) Math.Min(num1 + num - num2 + 8.0, num3 - 104.0);
      }
      return (object) 8;
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
