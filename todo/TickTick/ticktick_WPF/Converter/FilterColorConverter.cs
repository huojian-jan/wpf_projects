// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.FilterColorConverter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Globalization;
using System.Windows.Data;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Converter
{
  public class FilterColorConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value != null)
      {
        switch ((CardType) value)
        {
          case CardType.LogicAnd:
            return (object) ThemeUtil.GetColor("LogicAndColor").ToString();
          case CardType.LogicOr:
            return (object) ThemeUtil.GetColor("LogicOrColor").ToString();
          case CardType.Filter:
            return (object) ThemeUtil.GetColor("BaseColorOpacity20").ToString();
        }
      }
      return (object) ThemeUtil.GetColor("BaseColorOpacity20").ToString();
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
