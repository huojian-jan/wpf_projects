// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.CalendarIconColorConverter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using ticktick_WPF.Models;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Converter
{
  public class CalendarIconColorConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      if (values != null && values.Length == 4 && values[0] is string accountId && values[1] is string str && values[3] is ResourceDictionary dict)
      {
        BindCalendarModel defaultCalendar = SubscribeCalendarHelper.GetDefaultCalendar(accountId);
        if (defaultCalendar != null && defaultCalendar.Id != str)
          return (object) ThemeUtil.GetColor(dict, "PrimaryColor");
      }
      return (object) ThemeUtil.GetColor("BaseColorOpacity60");
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
