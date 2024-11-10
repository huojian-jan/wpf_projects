// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Timeline.TimelineIconFillConverter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Globalization;
using System.Windows.Data;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Timeline
{
  public class TimelineIconFillConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      if (values.Length == 2)
      {
        bool? nullable = values[0] as bool?;
        if (((int) nullable ?? 1) == 0)
        {
          nullable = values[1] as bool?;
          if (nullable.GetValueOrDefault())
            return (object) ThemeUtil.GetColor("PrimaryColor");
        }
      }
      return (object) ThemeUtil.GetColor("BaseColorOpacity40");
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
