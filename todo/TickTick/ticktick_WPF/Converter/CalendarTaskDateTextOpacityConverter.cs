// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.CalendarTaskDateTextOpacityConverter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Globalization;
using System.Windows.Data;

#nullable disable
namespace ticktick_WPF.Converter
{
  public class CalendarTaskDateTextOpacityConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      if (values == null || values.Length != 3 || values[0] == null || !(values[0] is double num))
        return (object) 0.6;
      return values[1] is DateTime dateTime && values[2] is bool flag && dateTime < DateTime.Today & flag ? (object) 1 : (object) num;
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
