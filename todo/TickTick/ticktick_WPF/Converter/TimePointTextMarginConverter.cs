// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.TimePointTextMarginConverter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using ticktick_WPF.Resource;

#nullable disable
namespace ticktick_WPF.Converter
{
  public class TimePointTextMarginConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      if (values != null && values.Length == 3 && values[0] is int num)
      {
        if (num == 0 || num == LocalSettings.Settings.CollapsedEnd)
          return (object) new Thickness(0.0, 0.0, 10.0, 0.0);
        if (num == LocalSettings.Settings.CollapsedStart)
          return (object) new Thickness(0.0, -12.0, 10.0, 0.0);
      }
      return (object) new Thickness(0.0, -6.0, 10.0, 0.0);
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
