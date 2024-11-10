// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.RepeatVisibleConverter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Converter
{
  public class RepeatVisibleConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      if (values.Length == 2 && values[0] is int num && values[1] is string)
      {
        string flag = (string) values[1];
        if (num != 0)
          return (object) Visibility.Collapsed;
        if (!Utils.IsEmptyRepeatFlag(flag))
          return (object) Visibility.Visible;
      }
      return (object) Visibility.Collapsed;
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
