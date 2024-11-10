// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.WidgetTaskDateColorConverter
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
  public class WidgetTaskDateColorConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      if (values == null || values.Length != 5 || !(values[0] is ResourceDictionary dict))
        return (object) ThemeUtil.GetColor("PrimaryColor");
      DateTime? startDate = values[1] as DateTime?;
      DateTime? dueDate = values[2] as DateTime?;
      bool? isAllDay = values[3] as bool?;
      int? nullable1 = values[4] as int?;
      if (nullable1.HasValue)
      {
        int? nullable2 = nullable1;
        int num = 0;
        if (!(nullable2.GetValueOrDefault() == num & nullable2.HasValue))
          return (object) ThemeUtil.GetColor(dict, "BaseColorOpacity20");
      }
      if (DateUtils.IsOutDated(startDate, dueDate, isAllDay))
        return (object) ThemeUtil.GetColor("OutDateColor");
      if (!nullable1.HasValue)
        return (object) ThemeUtil.GetColor(dict, "PrimaryColor");
      int? nullable3 = nullable1;
      int num1 = 0;
      int num2 = nullable3.GetValueOrDefault() == num1 & nullable3.HasValue ? 1 : 0;
      return (object) ThemeUtil.GetColor(dict, "PrimaryColor");
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
