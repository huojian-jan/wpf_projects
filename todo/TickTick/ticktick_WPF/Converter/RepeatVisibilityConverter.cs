// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.RepeatVisibilityConverter
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
  public class RepeatVisibilityConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (!(value is string str))
        return (object) Visibility.Collapsed;
      return string.IsNullOrEmpty(str) || str.Contains("FREQ=NONE") || str.Contains("FREQ = NONE") || str.Contains("CUSTOM") ? (object) Visibility.Collapsed : (object) Visibility.Visible;
    }

    public object ConvertBack(
      object value,
      Type targetType,
      object parameter,
      CultureInfo culture)
    {
      return (object) null;
    }
  }
}
