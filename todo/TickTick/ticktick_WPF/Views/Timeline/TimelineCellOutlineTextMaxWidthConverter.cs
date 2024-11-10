// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Timeline.TimelineCellOutlineTextMaxWidthConverter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Globalization;
using System.Windows.Data;

#nullable disable
namespace ticktick_WPF.Views.Timeline
{
  public class TimelineCellOutlineTextMaxWidthConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      double val2 = 120.0;
      if (values.Length == 3 && values[0] is double num1 && values[1] is double num2 && values[2] is bool flag)
      {
        int num = flag ? 20 : 0;
        val2 = Math.Min(120.0, num1 - num2 - 9.0 - (double) num);
      }
      return (object) Math.Max(30.0, val2);
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
