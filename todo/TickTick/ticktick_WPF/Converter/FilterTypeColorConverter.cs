// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.FilterTypeColorConverter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Globalization;
using System.Windows.Data;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Converter
{
  public class FilterTypeColorConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value != null && parameter != null)
      {
        switch ((FilterMode) value)
        {
          case FilterMode.Normal:
            return !(parameter.ToString() == "normal") ? (object) ThemeUtil.GetColor("BaseColorOpacity60").ToString() : (object) ThemeUtil.GetColor("TextAccentColor").ToString();
          case FilterMode.Advanced:
            return !(parameter.ToString() == "normal") ? (object) ThemeUtil.GetColor("TextAccentColor").ToString() : (object) ThemeUtil.GetColor("BaseColorOpacity60").ToString();
        }
      }
      return (object) ThemeUtil.GetColor("black_04").ToString();
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
