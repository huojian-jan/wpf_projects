// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.DayNameForegroundConverter
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
  public class DayNameForegroundConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      if (values.Length == 2 && values[0] is string text)
      {
        if (HolidayManager.IsHolidayText(text))
          return Utils.IsJp() ? (object) ThemeUtil.GetColorInString("#FF0000") : (object) ThemeUtil.GetColorInString("#5DCA94");
        if (values[1] is SolidColorBrush solidColorBrush)
          return (object) solidColorBrush;
      }
      return (object) ThemeUtil.GetColor("BaseColorOpacity40");
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
