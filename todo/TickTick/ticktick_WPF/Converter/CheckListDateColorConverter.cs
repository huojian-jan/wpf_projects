// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.CheckListDateColorConverter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Converter
{
  public class CheckListDateColorConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      if (((IEnumerable<object>) values).Any<object>((Func<object, bool>) (e => e == DependencyProperty.UnsetValue)))
        return (object) ThemeUtil.GetColor("PrimaryColor");
      int valueOrDefault = (values[0] as int?).GetValueOrDefault();
      DateTime? nullable = values[1] as DateTime?;
      if (valueOrDefault != 0)
        return (object) ThemeUtil.GetColor("BaseColorOpacity20");
      return nullable.HasValue && nullable.Value < DateTime.Today.Date ? (object) ThemeUtil.GetColor("OutDateColor") : (object) ThemeUtil.GetColor("PrimaryColor");
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
