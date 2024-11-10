// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.TaskDetailDateColorConverter
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
  public class TaskDetailDateColorConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      if (values.Length != 4)
        return (object) ThemeUtil.GetColor("BaseColorOpacity60");
      DateTime? nullable = values[0] as DateTime?;
      DateTime? dueDate = values[1] as DateTime?;
      bool? isAllDay = values[2] as bool?;
      if (!nullable.HasValue || Utils.IsEmptyDate(nullable))
        return (object) ThemeUtil.GetColor("BaseColorOpacity60");
      return DateUtils.IsOutDated(nullable, dueDate, isAllDay) ? (object) ThemeUtil.GetColor("OutDateColor") : (object) ThemeUtil.GetColor("DateColorPrimary", ThemeUtil.GetColor("ThemeBlue"));
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
