// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.SnoozeTextConverter
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
  public class SnoozeTextConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value == null || !((DateTime) value > DateTime.Now))
        return (object) string.Empty;
      DateTime date = (DateTime) value;
      TimeSpan timeSpan = DateTime.Now.Date - date.Date;
      if (Math.Abs(timeSpan.TotalDays - -1.0) <= 0.001)
        return (object) string.Format(Utils.GetString("PreviewSnoozeText"), (object) Utils.GetString("Tomorrow"));
      timeSpan = DateTime.Now.Date - date.Date;
      return Math.Abs(timeSpan.TotalDays) <= 0.001 ? (object) string.Format(Utils.GetString("PreviewSnoozeText"), (object) DateUtils.FormatHourMinute(date)) : (object) string.Format(Utils.GetString("PreviewSnoozeText"), (object) (DateUtils.FormatFullDate(date) + " " + DateUtils.FormatHourMinute(date)));
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
