// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.TaskDetailDateConverter
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
  public class TaskDetailDateConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      if (values.Length != 6)
        return (object) Utils.GetString("DateAndReminder");
      DateTime? nullable1 = values[0] as DateTime?;
      DateTime? nullable2 = values[1] as DateTime?;
      bool? nullable3 = values[2] as bool?;
      int? nullable4 = values[3] as int?;
      string str = values[4] as string;
      int valueOrDefault = (values[5] as int?).GetValueOrDefault();
      if (nullable1.HasValue)
      {
        DateTime start = nullable1.Value;
        DateTime? due = nullable2;
        bool? isAllDay = nullable3;
        int? nullable5 = nullable4;
        int num1 = 0;
        int num2 = nullable5.GetValueOrDefault() == num1 & nullable5.HasValue ? 1 : 0;
        return (object) DateUtils.FormatDateString(start, due, isAllDay, num2 != 0);
      }
      if (valueOrDefault != 0)
        return (object) Utils.GetString("DateNotSet");
      return !(str == "NOTE") ? (object) Utils.GetString("DateAndReminder") : (object) Utils.GetString("SetReminder");
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
