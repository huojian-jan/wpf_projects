// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.FilterUtils
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ticktick_WPF.Resource;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Util
{
  public static class FilterUtils
  {
    public static (int?, int?) GetSpanPairInRule(string rule)
    {
      Match match = CommonRegex.SpanFilterRegex.Match(rule);
      int result1;
      int result2;
      return match.Success ? (int.TryParse(match.Groups[1].Value, out result1) ? new int?(result1) : new int?(), int.TryParse(match.Groups[2].Value, out result2) ? new int?(result2) : new int?()) : (new int?(), new int?());
    }

    public static string GetNthDayString(int nDay)
    {
      switch (nDay)
      {
        case -1:
          return Utils.GetString("PublicYesterday");
        case 0:
          return Utils.GetString("Today");
        case 1:
          return Utils.GetString("Tomorrow");
        default:
          return string.Format(nDay < 0 ? Utils.GetString("ThePreviousNDay") : Utils.GetString("TheNextNDay"), (object) Utils.GetNthString(nDay));
      }
    }

    public static object GetSpanDateValue(int? from, int? to)
    {
      return (object) string.Format("span({0}~{1})", !from.HasValue ? (object) "" : (object) from.ToString(), !to.HasValue ? (object) "" : (object) to.ToString());
    }

    public static List<FilterDatePair> GetFilterDatePairs(IEnumerable<string> dates)
    {
      if (dates == null)
        return (List<FilterDatePair>) null;
      List<FilterDatePair> filterDatePairs = new List<FilterDatePair>();
      foreach (string date in dates)
      {
        DateTime dateTime1;
        if (date != null)
        {
          switch (date.Length)
          {
            case 3:
              if (date == "all")
              {
                List<FilterDatePair> filterDatePairList = filterDatePairs;
                DateTime? start = new DateTime?(DateTime.Today);
                dateTime1 = DateTime.Today;
                DateTime? end = new DateTime?(dateTime1.AddDays(180.0));
                FilterDatePair filterDatePair = new FilterDatePair(start, end);
                filterDatePairList.Add(filterDatePair);
                continue;
              }
              break;
            case 5:
              switch (date[0])
              {
                case 'n':
                  if (date == "nodue")
                  {
                    filterDatePairs.Add(new FilterDatePair(true));
                    continue;
                  }
                  break;
                case 't':
                  if (date == "today")
                  {
                    List<FilterDatePair> filterDatePairList = filterDatePairs;
                    DateTime? start = new DateTime?(DateTime.Today);
                    dateTime1 = DateTime.Today;
                    DateTime? end = new DateTime?(dateTime1.AddDays(1.0));
                    FilterDatePair filterDatePair = new FilterDatePair(start, end);
                    filterDatePairList.Add(filterDatePair);
                    continue;
                  }
                  break;
              }
              break;
            case 7:
              if (date == "overdue")
              {
                filterDatePairs.Add(new FilterDatePair(new DateTime?(), new DateTime?(DateTime.Today)));
                continue;
              }
              break;
            case 8:
              switch (date[1])
              {
                case 'e':
                  if (date == "nextweek")
                  {
                    int nextWeekDayDiff = DateUtils.GetNextWeekDayDiff();
                    List<FilterDatePair> filterDatePairList = filterDatePairs;
                    dateTime1 = DateTime.Today;
                    DateTime? start = new DateTime?(dateTime1.AddDays((double) nextWeekDayDiff));
                    dateTime1 = DateTime.Today;
                    DateTime? end = new DateTime?(dateTime1.AddDays((double) (nextWeekDayDiff + 7)));
                    FilterDatePair filterDatePair = new FilterDatePair(start, end);
                    filterDatePairList.Add(filterDatePair);
                    continue;
                  }
                  break;
                case 'h':
                  if (date == "thisweek")
                  {
                    int nextWeekDayDiff = DateUtils.GetNextWeekDayDiff();
                    List<FilterDatePair> filterDatePairList = filterDatePairs;
                    dateTime1 = DateTime.Today;
                    DateTime? start = new DateTime?(dateTime1.AddDays((double) (nextWeekDayDiff - 7)));
                    dateTime1 = DateTime.Today;
                    DateTime? end = new DateTime?(dateTime1.AddDays((double) nextWeekDayDiff));
                    FilterDatePair filterDatePair = new FilterDatePair(start, end);
                    filterDatePairList.Add(filterDatePair);
                    continue;
                  }
                  break;
                case 'o':
                  if (date == "tomorrow")
                  {
                    List<FilterDatePair> filterDatePairList = filterDatePairs;
                    dateTime1 = DateTime.Today;
                    DateTime? start = new DateTime?(dateTime1.AddDays(1.0));
                    dateTime1 = DateTime.Today;
                    DateTime? end = new DateTime?(dateTime1.AddDays(2.0));
                    FilterDatePair filterDatePair = new FilterDatePair(start, end);
                    filterDatePairList.Add(filterDatePair);
                    continue;
                  }
                  break;
              }
              break;
            case 9:
              switch (date[0])
              {
                case 'r':
                  if (date == "recurring")
                  {
                    filterDatePairs.Add(new FilterDatePair(false));
                    continue;
                  }
                  break;
                case 't':
                  if (date == "thismonth")
                  {
                    dateTime1 = DateTime.Now;
                    dateTime1 = dateTime1.Date;
                    DateTime dateTime2 = dateTime1.AddDays((double) ((DateTime.Now.Day - 1) * -1));
                    dateTime1 = dateTime2.Date;
                    DateTime dateTime3 = dateTime1.AddMonths(1);
                    filterDatePairs.Add(new FilterDatePair(new DateTime?(dateTime2), new DateTime?(dateTime3)));
                    continue;
                  }
                  break;
              }
              break;
          }
        }
        if (date.EndsWith("days"))
        {
          int result;
          int.TryParse(date.Replace("days", string.Empty), out result);
          List<FilterDatePair> filterDatePairList = filterDatePairs;
          DateTime? start = new DateTime?(DateTime.Today);
          dateTime1 = DateTime.Today;
          DateTime? end = new DateTime?(dateTime1.AddDays((double) result));
          FilterDatePair filterDatePair = new FilterDatePair(start, end);
          filterDatePairList.Add(filterDatePair);
        }
        else if (date.EndsWith("dayslater"))
        {
          int result;
          int.TryParse(date.Replace("dayslater", string.Empty), out result);
          List<FilterDatePair> filterDatePairList = filterDatePairs;
          dateTime1 = DateTime.Today;
          FilterDatePair filterDatePair = new FilterDatePair(new DateTime?(dateTime1.AddDays((double) result)), new DateTime?());
          filterDatePairList.Add(filterDatePair);
        }
        else if (date.EndsWith("daysfromtoday"))
        {
          int result;
          int.TryParse(date.Replace("daysfromtoday", string.Empty), out result);
          List<FilterDatePair> filterDatePairList = filterDatePairs;
          dateTime1 = DateTime.Today;
          DateTime? start = new DateTime?(dateTime1.AddDays((double) result));
          dateTime1 = DateTime.Today;
          DateTime? end = new DateTime?(dateTime1.AddDays((double) (result + 1)));
          FilterDatePair filterDatePair = new FilterDatePair(start, end);
          filterDatePairList.Add(filterDatePair);
        }
        else if (date.StartsWith("span"))
        {
          (int? nullable1, int? nullable2) = FilterUtils.GetSpanPairInRule(date);
          DateTime? nullable3;
          if (nullable1.HasValue)
          {
            dateTime1 = DateTime.Today;
            nullable3 = new DateTime?(dateTime1.AddDays((double) nullable1.Value));
          }
          else
            nullable3 = new DateTime?();
          DateTime? start = nullable3;
          DateTime? nullable4;
          if (nullable2.HasValue)
          {
            dateTime1 = DateTime.Today;
            nullable4 = new DateTime?(dateTime1.AddDays((double) (nullable2.Value + 1)));
          }
          else
            nullable4 = new DateTime?();
          DateTime? end = nullable4;
          filterDatePairs.Add(new FilterDatePair(start, end));
        }
        else if (date.StartsWith("offset"))
        {
          (DateTime dateTime4, DateTime dateTime5) = FilterUtils.GetDateOffsetPair(date);
          filterDatePairs.Add(new FilterDatePair(new DateTime?(dateTime4), new DateTime?(dateTime5)));
        }
      }
      return filterDatePairs;
    }

    public static (DateTime, DateTime) GetDateOffsetPair(string rule)
    {
      if (rule.Length > 8)
      {
        string str1 = rule.Substring(7);
        string str2 = str1.Substring(0, str1.Length - 1);
        int result;
        if (int.TryParse(str2.Substring(0, str2.Length - 1), out result))
        {
          switch (str2.Substring(str2.Length - 1))
          {
            case "D":
              return (DateTime.Today.AddDays((double) result), DateTime.Today.AddDays((double) (result + 1)));
            case "W":
              result = Utils.GetWeekStartDiff(DateTime.Today) + result * 7;
              DateTime today = DateTime.Today;
              DateTime dateTime1 = today.AddDays((double) result);
              today = DateTime.Today;
              DateTime dateTime2 = today.AddDays((double) (result + 7));
              return (dateTime1, dateTime2);
            case "M":
              DateTime dateTime3 = DateTime.Today.AddDays((double) (1 - DateTime.Today.Day)).AddMonths(result);
              return (dateTime3, dateTime3.AddMonths(1));
          }
        }
      }
      return (DateTime.Today, DateTime.Today.AddDays(1.0));
    }
  }
}
