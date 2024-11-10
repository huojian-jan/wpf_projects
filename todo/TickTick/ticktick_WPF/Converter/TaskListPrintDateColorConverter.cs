// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.TaskListPrintDateColorConverter
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
  public class TaskListPrintDateColorConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      if (values != null && values.Length == 5)
      {
        DateTime? startDate = values[0] as DateTime?;
        DateTime? dueDate = values[1] as DateTime?;
        bool? isAllDay = values[2] as bool?;
        if (values[3] is int num && num != 0)
          return (object) ThemeUtil.GetAlphaColor("#191919", 56);
        if (DateUtils.IsOutDated(startDate, dueDate, isAllDay))
          return (object) ThemeUtil.GetColor("OutDateColor");
      }
      return (object) ThemeUtil.GetAlphaColor("#191919", 56);
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
