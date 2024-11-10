// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.TaskProgressVisibleConverter
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
  public class TaskProgressVisibleConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      if (values != null && values.Length == 4 && values[0] is int num && values[1] is bool flag1 && values[2] is bool flag2 && values[3] is string str)
      {
        if (str == "NOTE")
          return (object) Visibility.Collapsed;
        if (num != 0)
          return (object) Visibility.Collapsed;
        if (!flag2)
          return (object) Visibility.Collapsed;
        if (flag1)
          return (object) Visibility.Collapsed;
      }
      return (object) Visibility.Visible;
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
