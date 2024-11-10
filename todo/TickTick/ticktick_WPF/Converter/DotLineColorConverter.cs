// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.DotLineColorConverter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Converter
{
  public class DotLineColorConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value != null)
      {
        switch ((CardType) value)
        {
          case CardType.InitFilter:
            return (object) ThemeUtil.GetColor("BaseColorOpacity10_20").ToString();
          case CardType.InitLogic:
            return (object) ThemeUtil.GetColor("BaseColorOpacity40").ToString();
        }
      }
      return (object) new SolidColorBrush(Colors.White);
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
