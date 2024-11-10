// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.SelectedOrDisabledTextColorConverter
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
  public class SelectedOrDisabledTextColorConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      FrameworkElement context = values[2] as FrameworkElement;
      if (values[0] != null && !(bool) values[0])
        return (object) ThemeUtil.GetColor("BaseColorOpacity40", context);
      return values[1] == null || !(bool) values[1] ? (object) ThemeUtil.GetColor("BaseColorOpacity100_80", context) : (object) ThemeUtil.GetColor("PrimaryColor", context);
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
