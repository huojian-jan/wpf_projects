// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.SectionOptionVisibleConverter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

#nullable disable
namespace ticktick_WPF.Converter
{
  public class SectionOptionVisibleConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      if (values.Length == 5)
      {
        if (((!(values[4] is bool flag1) ? 0 : 1) & (flag1 ? 1 : 0)) != 0)
          return (object) Visibility.Collapsed;
        if (values[3] is bool flag2 && !flag2)
          return (object) Visibility.Collapsed;
        if (((!(values[2] is bool flag3) ? 0 : 1) & (flag3 ? 1 : 0)) != 0)
          return (object) Visibility.Visible;
        if (values[0] is bool flag4 && flag4 && values[1] is bool flag5 && !flag5)
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
