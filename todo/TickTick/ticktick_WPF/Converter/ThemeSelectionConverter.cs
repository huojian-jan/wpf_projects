// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.ThemeSelectionConverter
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
  public class ThemeSelectionConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (!(value is string str1))
        return (object) new Thickness(0.0, 0.0, 0.0, 0.0);
      string str2 = parameter?.ToString();
      return str1 == str2 ? (object) new Thickness(2.0, 2.0, 2.0, 2.0) : (object) new Thickness(0.0, 0.0, 0.0, 0.0);
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
