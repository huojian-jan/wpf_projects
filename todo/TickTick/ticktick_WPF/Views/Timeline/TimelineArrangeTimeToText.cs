// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Timeline.TimelineArrangeTimeToText
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
  public class TimelineArrangeTimeToText : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      if (values.Length == 3 && values[2] is bool flag)
      {
        DateTime? nullable1 = values[0] as DateTime?;
        DateTime? nullable2 = values[1] as DateTime?;
        if (flag)
          nullable2 = nullable2?.AddDays(-1.0);
        DateTime? nullable3 = nullable2 ?? nullable1;
        if (nullable3.HasValue)
          return (object) DateUtils.FormatDateCheckYear(nullable3.GetValueOrDefault());
      }
      return (object) string.Empty;
    }

    public object[] ConvertBack(
      object value,
      Type[] targetTypes,
      object parameter,
      CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
