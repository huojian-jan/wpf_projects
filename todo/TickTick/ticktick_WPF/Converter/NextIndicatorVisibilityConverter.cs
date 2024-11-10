// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.NextIndicatorVisibilityConverter
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
  public class NextIndicatorVisibilityConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value != null && value is DateTime dateTime && parameter != null)
      {
        int month1 = dateTime.Month;
        DateTime today = DateTime.Today;
        int month2 = today.Month;
        int num;
        if (month1 == month2)
        {
          int year1 = dateTime.Year;
          today = DateTime.Today;
          int year2 = today.Year;
          num = year1 == year2 ? 1 : 0;
        }
        else
          num = 0;
        bool flag = num != 0;
        if (!flag && parameter.ToString() == "disable")
          return (object) Visibility.Visible;
        if (flag && parameter.ToString() == "enable")
          return (object) Visibility.Visible;
      }
      return (object) Visibility.Collapsed;
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
