// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.CalendarTaskCellColorConverter
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
  public class CalendarTaskCellColorConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      if (values == null || values.Length != 3 || !(values[0] is int num))
        return (object) new SolidColorBrush(Colors.Transparent);
      string color = values[1] as string;
      if (ThemeUtil.IsEmptyColor(color))
        color = ThemeUtil.GetColor("PrimaryColor").ToString().Replace("#FF", "#");
      return num != 0 ? (object) ThemeUtil.GetAlphaColor(color, 36) : (object) ThemeUtil.GetColorInString(color);
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
