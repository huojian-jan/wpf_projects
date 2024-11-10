// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.DateColorBackgroundConverter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Globalization;
using System.Windows.Data;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Converter
{
  public class DateColorBackgroundConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return ((value == null ? 0 : (!(value is bool flag) ? 0 : 1)) & (flag ? 1 : 0)) != 0 ? (object) ThemeUtil.GetPrimaryColor(1.0) : (object) ThemeUtil.GetColor("BaseColorOpacity60");
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
