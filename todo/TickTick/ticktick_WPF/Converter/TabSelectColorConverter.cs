// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.TabSelectColorConverter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Converter
{
  public class TabSelectColorConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      int result;
      return value is int num && parameter is string s && int.TryParse(s, out result) && num == result ? (object) ThemeUtil.GetColor("PrimaryColor") : (object) Brushes.Transparent;
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
