// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.SelectionWidthConverter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using ticktick_WPF.Views.Time;

#nullable disable
namespace ticktick_WPF.Converter
{
  public class SelectionWidthConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      if (values == null || values.Length != 2)
        return (object) Visibility.Collapsed;
      SelectionMode valueOrDefault1 = (values[0] as SelectionMode?).GetValueOrDefault();
      double valueOrDefault2 = (values[1] as double?).GetValueOrDefault();
      return valueOrDefault1 == SelectionMode.End || valueOrDefault1 == SelectionMode.Start || valueOrDefault1 == SelectionMode.Full ? (object) (valueOrDefault2 - 2.0) : (object) valueOrDefault2;
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
