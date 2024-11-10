// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Timeline.TimelineFloatingTextWidthConverter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Timeline
{
  public class TimelineFloatingTextWidthConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      if (values.Length != 4 || !(values[0] is double num1) || !(values[1] is int num2) || !(values[2] is string str))
        return (object) double.PositiveInfinity;
      bool flag1 = str != "-1" && !string.IsNullOrEmpty(str);
      bool flag2 = num2 > 0;
      if (num1 < 70.0 && values[3] is string text && text.Length < 10)
      {
        double num3 = string.IsNullOrEmpty(text) ? 0.0 : Utils.MeasureString(text, new Typeface(Utils.GetDefaultFontFamily(), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal), 12.0).Width;
        if (num1 < 40.0)
          flag2 = num1 - 16.0 - num3 >= 14.0;
        flag1 = num1 - 16.0 - num3 - (flag2 ? 14.0 : 0.0) >= 22.0;
      }
      return (object) Math.Max(0.0, num1 - 16.0 - (flag2 ? 14.0 : 0.0) - (flag1 ? 22.0 : 0.0));
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
