// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.TaskDetailTimeZoneDateConverter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Globalization;
using System.Windows.Data;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Converter
{
  public class TaskDetailTimeZoneDateConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      string str = "";
      if (values.Length == 4)
      {
        DateTime? nullable = values[0] as DateTime?;
        DateTime? due = values[1] as DateTime?;
        bool? isAllDay = values[2] as bool?;
        string name = values[3] as string;
        if (!isAllDay.GetValueOrDefault() && name != TimeZoneData.LocalTimeZoneModel?.TimeZoneName && name != null)
        {
          if (!string.IsNullOrEmpty(name))
          {
            TimeZoneInfo timeZoneInfo = TimeZoneUtils.GetTimeZoneInfo(name);
            double totalMinutes = (timeZoneInfo.BaseUtcOffset - TimeZoneInfo.Local.BaseUtcOffset).TotalMinutes;
            nullable = nullable?.AddMinutes(totalMinutes);
            due = due?.AddMinutes(totalMinutes);
            str = timeZoneInfo.DisplayName;
          }
          if (nullable.HasValue)
            return (object) (DateUtils.FormatDateString(nullable.Value, due, isAllDay, false) + " " + str);
        }
      }
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
