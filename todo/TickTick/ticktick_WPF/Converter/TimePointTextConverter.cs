// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.TimePointTextConverter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Globalization;
using System.Windows.Data;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Views.Calendar;

#nullable disable
namespace ticktick_WPF.Converter
{
  public class TimePointTextConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      int result;
      if (values.Length != 2 || !int.TryParse(values[0].ToString(), out result))
        return (object) string.Empty;
      int num = (values[1] as bool?).GetValueOrDefault() ? 1 : 0;
      DateTime date = DateTime.Today.AddHours((double) result);
      if (num != 0 && Math.Abs((DateTime.Now - DateTime.Today).TotalSeconds - (double) (result * 3600)) / 3600.0 * CalendarGeoHelper.HourHeight < 10.0)
        return (object) string.Empty;
      if (result == 12)
        return (object) Utils.GetString("Noon");
      if (LocalSettings.Settings.TimeFormat == "24Hour")
        return (object) DateUtils.FormatHourMinute(date, true);
      return !Utils.IsZhCn() ? (object) DateUtils.GetAmPmTimeText(date) : (object) DateUtils.GetZhTime(date);
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
