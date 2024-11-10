// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.TaskListDateConverter
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
  public class TaskListDateConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      if (values != null && values.Length == 9)
      {
        bool flag1 = (((values[3] as int?).GetValueOrDefault() != 0 ? 0 : (!(values[4] is bool flag2) ? 0 : 1)) & (flag2 ? 1 : 0)) != 0;
        bool? nullable1 = !(values[5] is bool flag3) ? (parameter is string str ? new bool?(str == "True") : new bool?()) : new bool?(flag3);
        DateTime? startDate = values[0] as DateTime?;
        DateTime? dueDate = values[1] as DateTime?;
        bool? isAllDay = values[2] as bool?;
        bool? nullable2 = values[6] as bool?;
        if (((!(values[7] is bool flag4) ? 0 : 1) & (flag4 ? 1 : 0)) != 0)
          nullable1 = new bool?(false);
        if (nullable2.GetValueOrDefault() && DateUtils.IsOutDated(startDate, dueDate, isAllDay))
          return (object) string.Empty;
        if (startDate.HasValue && !Utils.IsEmptyDate(startDate.Value))
        {
          if (flag1)
            return (object) DateUtils.FormatCountDownDateString(startDate.Value, dueDate, isAllDay.GetValueOrDefault(), nullable1.GetValueOrDefault());
          return !(values[8] as bool?).GetValueOrDefault() ? (object) DateUtils.FormatListDateString(startDate.Value, dueDate, isAllDay) : (object) Utils.GetString("Tomorrow");
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
