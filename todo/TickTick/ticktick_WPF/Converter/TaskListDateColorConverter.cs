// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.TaskListDateColorConverter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Converter
{
  public class TaskListDateColorConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      if (values == null || values.Length != 6)
        return (object) ThemeUtil.GetColor("DateColorPrimary", ThemeUtil.GetColor("ThemeBlue"));
      DateTime? startDate = values[0] as DateTime?;
      DateTime? dueDate = values[1] as DateTime?;
      bool? isAllDay = values[2] as bool?;
      int valueOrDefault = (values[3] as int?).GetValueOrDefault();
      FrameworkElement context = values[4] as FrameworkElement;
      if (valueOrDefault != 0)
        return (object) ThemeUtil.GetColor("BaseColorOpacity20", context);
      if (((!(values[5] is bool flag) ? 0 : 1) & (flag ? 1 : 0)) != 0)
      {
        if (DateUtils.CountDownOutDate(startDate, dueDate, isAllDay.GetValueOrDefault()))
          return (object) ThemeUtil.GetColor("OutDateColor", context);
      }
      else if (DateUtils.IsOutDated(startDate, dueDate, isAllDay))
        return (object) ThemeUtil.GetColor("OutDateColor", context);
      return (object) ThemeUtil.GetColor("DateColorPrimary", context, ThemeUtil.GetColor("ThemeBlue"));
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
