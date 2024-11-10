// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.ShowCreateTextConverter
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
  public class ShowCreateTextConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      return values.Length == 3 && ((!(values[0] is DateTime) ? 0 : (!(values[1] is bool flag1) ? 0 : 1)) & (flag1 ? 1 : 0)) != 0 && values[2] is bool flag2 && !flag2 ? (object) Visibility.Visible : (object) Visibility.Collapsed;
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
