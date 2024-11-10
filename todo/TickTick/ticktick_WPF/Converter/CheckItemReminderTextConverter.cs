// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.CheckItemReminderTextConverter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Converter
{
  public class CheckItemReminderTextConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      if (((IEnumerable<object>) values).Any<object>((Func<object, bool>) (e => e == DependencyProperty.UnsetValue)))
        return (object) string.Empty;
      if (values.Length == 3)
      {
        DateTime? date1 = (DateTime?) values[0];
        bool? isAllDay = (bool?) values[1];
        DateTime? date2 = (DateTime?) values[2];
        if (date2.HasValue && !Utils.IsEmptyDate(date2) && date2.Value > DateTime.Now)
        {
          string str = (date2.Value.Date == DateTime.Today ? Utils.GetString("Today") : Utils.GetString("Tomorrow")) + DateUtils.FormatHourMinuteText(date2.Value);
          return (object) (Utils.GetString("Today") + "," + string.Format(Utils.GetString("PreviewSnoozeText"), (object) str));
        }
        if (date1.HasValue && !Utils.IsEmptyDate(date1))
          return (object) DateUtils.FormatListDateString(date1.Value, new DateTime?(), isAllDay);
      }
      return (object) string.Empty;
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
