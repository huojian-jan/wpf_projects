// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Search.SearchDateTextConverter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Search
{
  public class SearchDateTextConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      if (values == null || values.Length != 3 || !(values[0] is DateFilter key))
        return (object) Utils.GetString("AllDate");
      DateTime? nullable1 = (DateTime?) values[1];
      DateTime? nullable2 = (DateTime?) values[2];
      if (key == DateFilter.Custom)
      {
        if (!nullable1.HasValue || !nullable2.HasValue)
          return (object) Utils.GetString("AllDate");
        if (!(nullable1.Value.Date != nullable2.Value.Date))
          return (object) (Utils.GetString("Custom") + " (" + nullable1.Value.ToString("m", (IFormatProvider) App.Ci) + ")");
        return (object) string.Format("{0} {1}{2} - {3}{4}", (object) Utils.GetString("Custom"), (object) " (", (object) DateUtils.FormatMonthDay(nullable1.Value), (object) DateUtils.FormatMonthDay(nullable2.Value), (object) ") ");
      }
      Dictionary<DateFilter, string> dictionary = new Dictionary<DateFilter, string>();
      dictionary.Add(DateFilter.All, "AllDate");
      dictionary.Add(DateFilter.Today, "Today");
      dictionary.Add(DateFilter.Tomorrow, "Tomorrow");
      dictionary.Add(DateFilter.Yesterday, "PublicYesterday");
      dictionary.Add(DateFilter.ThisWeek, "ThisWeek");
      dictionary.Add(DateFilter.NextWeek, "NextWeek");
      dictionary.Add(DateFilter.LastWeek, "LastWeek");
      dictionary.Add(DateFilter.ThisMonth, "ThisMonth");
      dictionary.Add(DateFilter.LastMonth, "LastMonth");
      string str = Utils.GetString(dictionary.ContainsKey(key) ? dictionary[key] : "AllDate");
      if (nullable1.HasValue && nullable2.HasValue)
      {
        if (((IEnumerable<DateFilter>) new DateFilter[3]
        {
          DateFilter.ThisWeek,
          DateFilter.NextWeek,
          DateFilter.LastWeek
        }).Contains<DateFilter>(key))
          str += this.GetDateSpanDesc(nullable1.Value, nullable2.Value);
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

    private string GetDateSpanDesc(DateTime startDate, DateTime endDate)
    {
      return " (" + DateUtils.FormatMonthDay(startDate.Date) + " - " + DateUtils.FormatMonthDay(endDate.Date) + ")";
    }

    public static (DateTime, DateTime) ConvertDateFilter2Span(DateFilter filter)
    {
      (DateTime, DateTime) valueTuple = (DateTime.Today, DateTime.Today);
      switch (filter)
      {
        case DateFilter.Today:
          valueTuple.Item1 = DateTime.Today;
          valueTuple.Item2 = DateTime.Today;
          break;
        case DateFilter.Tomorrow:
          ref (DateTime, DateTime) local1 = ref valueTuple;
          DateTime today = DateTime.Today;
          DateTime dateTime1 = today.AddDays(1.0);
          local1.Item1 = dateTime1;
          ref (DateTime, DateTime) local2 = ref valueTuple;
          today = DateTime.Today;
          DateTime dateTime2 = today.AddDays(1.0);
          local2.Item2 = dateTime2;
          break;
        case DateFilter.Yesterday:
          valueTuple.Item1 = DateTime.Today.AddDays(-1.0);
          valueTuple.Item2 = valueTuple.Item1;
          break;
        case DateFilter.NextWeek:
          ref (DateTime, DateTime) local3 = ref valueTuple;
          DateTime dateTime3 = DateTime.Today;
          dateTime3 = dateTime3.AddDays((double) ((int) DateTime.Today.DayOfWeek * -1));
          dateTime3 = dateTime3.AddDays((double) Utils.GetWeekFromDiff());
          DateTime dateTime4 = dateTime3.AddDays(7.0);
          local3.Item1 = dateTime4;
          valueTuple.Item2 = valueTuple.Item1.AddDays(6.0);
          break;
        case DateFilter.LastWeek:
          ref (DateTime, DateTime) local4 = ref valueTuple;
          DateTime dateTime5 = DateTime.Today;
          dateTime5 = dateTime5.AddDays((double) ((int) DateTime.Today.DayOfWeek * -1));
          dateTime5 = dateTime5.AddDays((double) Utils.GetWeekFromDiff());
          DateTime dateTime6 = dateTime5.AddDays(-7.0);
          local4.Item1 = dateTime6;
          valueTuple.Item2 = valueTuple.Item1.AddDays(6.0);
          break;
        case DateFilter.ThisMonth:
          ref (DateTime, DateTime) local5 = ref valueTuple;
          DateTime dateTime7 = DateTime.Today;
          DateTime dateTime8 = dateTime7.AddDays((double) (1 - DateTime.Today.Day));
          local5.Item1 = dateTime8;
          ref (DateTime, DateTime) local6 = ref valueTuple;
          dateTime7 = valueTuple.Item1.AddMonths(1);
          DateTime dateTime9 = dateTime7.AddDays(-1.0);
          local6.Item2 = dateTime9;
          break;
        case DateFilter.LastMonth:
          ref (DateTime, DateTime) local7 = ref valueTuple;
          DateTime dateTime10 = DateTime.Today;
          dateTime10 = dateTime10.AddDays((double) (1 - DateTime.Today.Day));
          DateTime dateTime11 = dateTime10.AddMonths(-1);
          local7.Item1 = dateTime11;
          ref (DateTime, DateTime) local8 = ref valueTuple;
          dateTime10 = valueTuple.Item1.AddMonths(1);
          DateTime dateTime12 = dateTime10.AddDays(-1.0);
          local8.Item2 = dateTime12;
          break;
        default:
          ref (DateTime, DateTime) local9 = ref valueTuple;
          DateTime dateTime13 = DateTime.Today;
          dateTime13 = dateTime13.AddDays((double) ((int) DateTime.Today.DayOfWeek * -1));
          DateTime dateTime14 = dateTime13.AddDays((double) Utils.GetWeekFromDiff());
          local9.Item1 = dateTime14;
          valueTuple.Item2 = valueTuple.Item1.AddDays(6.0);
          break;
      }
      return valueTuple;
    }
  }
}
