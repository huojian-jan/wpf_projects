// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.TaskProgressTextMarginConverter
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
  public class TaskProgressTextMarginConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      if (values == null || values.Length != 2)
        return (object) new Thickness(0.0, 0.0, 0.0, 4.0);
      int percent = (int) values[0];
      double width = (double) values[1];
      int left;
      switch (percent)
      {
        case 0:
          left = 5;
          break;
        case 100:
          left = (int) width - 40;
          break;
        default:
          left = (int) (TaskProgressUtils.GetPercentWidth(percent, width) - 5.0);
          break;
      }
      return (object) new Thickness((double) left, 2.0, 0.0, 4.0);
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
