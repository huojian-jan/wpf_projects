// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.ShowFoldTaskPathConverter
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
  public class ShowFoldTaskPathConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      return values != null && values.Length == 2 && ((!(values[0] is bool flag) ? 0 : (!(values[1] is int num) ? 0 : 1)) & (flag ? 1 : 0)) != 0 && num < 4 ? (object) Visibility.Visible : (object) Visibility.Collapsed;
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
