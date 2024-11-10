// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.GetDiffConverter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Globalization;
using System.Windows.Data;

#nullable disable
namespace ticktick_WPF.Converter
{
  public class GetDiffConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      if (values.Length != 2 || !(values[0] is double num1) || !(values[1] is double num2))
        return (object) double.PositiveInfinity;
      double num3 = Math.Abs(num1 - num2);
      int result;
      if (parameter is string s && int.TryParse(s, out result) && num3 > (double) result)
        num3 -= (double) result;
      return (object) num3;
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
