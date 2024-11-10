// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.CheckListIconVisibleConverter
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
  public class CheckListIconVisibleConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      if (((IEnumerable<object>) values).Any<object>((Func<object, bool>) (e => e == DependencyProperty.UnsetValue)))
        return (object) Visibility.Collapsed;
      if (values.Length == 4)
      {
        if ((int) values[3] != 0)
          return (object) Visibility.Collapsed;
        DateTime? date1 = (DateTime?) values[0];
        bool? nullable = (bool?) values[1];
        DateTime? date2 = (DateTime?) values[2];
        if (date1.HasValue && !Utils.IsEmptyDate(date1) && date1.Value >= DateTime.Now && nullable.HasValue && !nullable.Value)
          return (object) Visibility.Visible;
        if (date2.HasValue && !Utils.IsEmptyDate(date2) && date2.Value > DateTime.Now)
          return (object) Visibility.Visible;
      }
      return (object) Visibility.Collapsed;
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
