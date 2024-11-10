// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.BoolVisibleConverter
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
  public class BoolVisibleConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      Visibility visibility = Visibility.Collapsed;
      if (parameter != null)
        visibility = Visibility.Hidden;
      return value == null ? (object) visibility : (object) (Visibility) ((bool) value ? 0 : (int) visibility);
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
