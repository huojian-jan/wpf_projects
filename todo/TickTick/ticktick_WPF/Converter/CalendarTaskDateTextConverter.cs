// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.CalendarTaskDateTextConverter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Globalization;
using System.Windows.Data;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Converter
{
  public class CalendarTaskDateTextConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      if (values == null || values.Length < 3)
        return (object) string.Empty;
      string str = values[0] as string;
      if (values[1] != null && values[1] is bool flag2 && values[2] != null && values[2] is DateTime dateTime)
      {
        if (((values.Length != 5 ? 0 : (!(values[4] is bool flag1) ? 0 : 1)) & (flag1 ? 1 : 0)) != 0)
        {
          DateTime? nullable = values[3] as DateTime?;
          if (!nullable.HasValue)
            nullable = new DateTime?(dateTime);
          else if (flag2)
            nullable = new DateTime?(nullable.Value.AddSeconds(-1.0));
          DateTime today = nullable.Value;
          int year1 = today.Year;
          today = DateTime.Today;
          int year2 = today.Year;
          if (year1 < year2)
          {
            today = nullable.Value;
            return (object) today.ToString("yyyy/MM/dd", (IFormatProvider) CultureInfo.InvariantCulture);
          }
          try
          {
            today = nullable.Value;
            return (object) today.ToString("m", (IFormatProvider) App.Ci);
          }
          catch (Exception ex)
          {
            return (object) nullable.Value.ToString("MM/dd", (IFormatProvider) App.Ci);
          }
        }
        else
        {
          if (!flag2)
            return !string.IsNullOrEmpty(str) ? (object) (str + "\n" + DateUtils.FormatHourMinuteText(dateTime)) : (object) DateUtils.FormatHourMinuteText(dateTime);
          if (parameter != null && parameter.ToString() == "ShowAllDay")
            return (object) (str + "\n" + DateUtils.FormatDateString(dateTime, new DateTime?(), new bool?(true), false));
        }
      }
      return (object) str;
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
