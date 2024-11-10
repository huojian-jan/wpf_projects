// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.TaskCellDateTextConverter
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
  public class TaskCellDateTextConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      if (values == null || values.Length != 3)
        return (object) string.Empty;
      bool? nullable = values[0] as bool?;
      string str = string.Empty;
      if (values[1] is DateTime date1)
      {
        if (((int) nullable ?? 1) != 0)
          return (object) DateUtils.FormatDateCheckYear(date1);
        str = DateUtils.FormatHourMinuteText(date1);
      }
      if (values[1] is DateTime dateTime && values[2] is DateTime date2 && date2 != dateTime)
        str = str + " - " + DateUtils.FormatHourMinuteText(date2);
      return (object) str;
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
