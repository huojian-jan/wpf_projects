// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Misc.StopPointConverter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

#nullable disable
namespace ticktick_WPF.Views.Misc
{
  public class StopPointConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      if (values.Length != 2)
        return (object) new Point(20.0, 0.0);
      double num1 = (values[0] as double?).GetValueOrDefault();
      double valueOrDefault = (values[1] as double?).GetValueOrDefault();
      if (num1 >= 100.0)
        num1 = 99.9;
      double num2 = 20.0 - valueOrDefault;
      double num3 = 2.0 * Math.PI * num1 / 100.0;
      return (object) new Point(20.0 + Math.Sin(num3) * num2, 20.0 - Math.Cos(num3) * num2);
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
