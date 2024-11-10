// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.CalendarDetailDateConverter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Globalization;
using System.Windows.Data;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Converter
{
  public class CalendarDetailDateConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      if (values.Length == 3)
      {
        DateTime? nullable1 = values[0] as DateTime?;
        DateTime? nullable2 = values[1] as DateTime?;
        bool isAllDay = ((int) (values[2] as bool?) ?? 1) != 0;
        if (nullable1.HasValue)
        {
          nullable2 = new DateTime?(nullable2.HasValue ? nullable2.GetValueOrDefault().AddDays(isAllDay ? -1.0 : 0.0) : nullable1.Value);
          return (object) DateUtils.FormatCalendarDateString(nullable1.Value, nullable2.Value, isAllDay);
        }
      }
      return (object) string.Empty;
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
