// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Calendar.TimeLineExpandHeightConverter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Globalization;
using System.Windows.Data;

#nullable disable
namespace ticktick_WPF.Views.Calendar
{
  public class TimeLineExpandHeightConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      if (values.Length != 2 || !(values[0] is int num1) || !(values[1] is double num2) || !(parameter is string str))
        return (object) 0;
      return str == "True" ? (object) ((double) num1 * num2) : (object) ((double) (25 - num1) * num2);
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
