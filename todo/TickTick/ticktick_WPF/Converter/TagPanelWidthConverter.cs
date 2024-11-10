// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.TagPanelWidthConverter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Globalization;
using System.Windows.Data;

#nullable disable
namespace ticktick_WPF.Converter
{
  public class TagPanelWidthConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      if (values.Length == 3 && values[0] is double num1 && values[1] is bool flag1 && values[2] is bool flag2)
      {
        if (flag1)
          num1 -= 150.0;
        if (flag2)
        {
          double num = num1 - 100.0;
        }
      }
      return (object) 300;
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
