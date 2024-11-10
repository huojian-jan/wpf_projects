// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.TabSelectedMultiConverter
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
  public class TabSelectedMultiConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      int result;
      return values.Length == 2 && (((!(values[0] is bool flag) ? 0 : 1) & (flag ? 1 : 0)) != 0 || values[1] is int num && parameter is string s && int.TryParse(s, out result) && num == result) ? (object) ThemeUtil.GetColor("BaseColorOpacity5") : (object) Brushes.Transparent;
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
