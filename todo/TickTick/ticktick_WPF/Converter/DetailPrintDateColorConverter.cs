// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.DetailPrintDateColorConverter
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
  public class DetailPrintDateColorConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      if (values.Length != 3)
        return (object) ThemeUtil.GetColorInString("#3F191919");
      DateTime? nullable = values[0] as DateTime?;
      DateTime? dueDate = values[1] as DateTime?;
      bool? isAllDay = values[2] as bool?;
      if (!nullable.HasValue || Utils.IsEmptyDate(nullable))
        return (object) ThemeUtil.GetColorInString("#3F191919");
      return DateUtils.IsOutDated(nullable, dueDate, isAllDay) ? (object) ThemeUtil.GetColor("OutDateColor") : (object) ThemeUtil.GetColorInString("#D8191919");
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
