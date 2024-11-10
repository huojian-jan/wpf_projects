// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.ToolTipTextConverter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Globalization;
using System.Windows.Data;
using ticktick_WPF.Util;
using ticktick_WPF.Views.Calendar;

#nullable disable
namespace ticktick_WPF.Converter
{
  public class ToolTipTextConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value != null)
      {
        int num = int.Parse(value.ToString());
        string str1 = DateUtils.FormatHourMinute(DateTime.Today);
        string str2 = DateUtils.FormatHourMinute(DateTime.Today.AddHours((double) CalendarGeoHelper.GetStartHour(false)));
        int endHour = CalendarGeoHelper.GetEndHour(false);
        if (num >= endHour)
        {
          DateTime today = DateTime.Today;
          str1 = DateUtils.FormatHourMinute(today.AddHours((double) CalendarGeoHelper.GetEndHour(false)));
          today = DateTime.Today;
          str2 = DateUtils.FormatHourMinute(today.AddHours(24.0));
        }
        if (CalendarGeoHelper.TopFolded)
          return (object) (Utils.GetString("ClickToShow") + " " + str1 + " - " + str2);
      }
      return (object) string.Empty;
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
