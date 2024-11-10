// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.TaskCellTopMarginConverter
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
  public class TaskCellTopMarginConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      double val1 = 0.0;
      int val2 = 4;
      bool flag = parameter is "False";
      if (value is double num1)
      {
        double num = flag ? 10.0 : 14.0;
        val1 = (num1 - num) / 2.0 - 1.0;
        val2 = (22 - (flag ? 10 : 14)) / 2;
      }
      return (object) new Thickness(4.0, Math.Min(val1, (double) val2), flag ? 0.0 : 4.0, 0.0);
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
