// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.DateParser.OldDateParser
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Ical.Net.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace ticktick_WPF.Util.DateParser
{
  public static class OldDateParser
  {
    private static readonly string[] HalfExps = new string[3]
    {
      "半",
      "half a",
      "half an"
    };
    private static readonly string[] YearExps = new string[5]
    {
      "年",
      "year",
      "years",
      "yr",
      "yrs"
    };
    private static readonly string[] MonthExps = new string[3]
    {
      "月",
      "month",
      "months"
    };
    private static readonly string[] WeekExps = new string[6]
    {
      "周",
      "礼拜",
      "星期",
      "week",
      "wk",
      "weeks"
    };
    private static readonly string[] DayExps = new string[4]
    {
      "天",
      "day",
      "d",
      "days"
    };
    private static readonly string[] HourExps = new string[6]
    {
      "小时",
      "h",
      "hr",
      "hrs",
      "hour",
      "hours"
    };
    private static readonly string[] MinuteExps = new string[6]
    {
      "分钟",
      "分",
      "minute",
      "minutes",
      "min",
      "mins"
    };

    public static OldParseDueDate Parse(string content)
    {
      return OldDateParser.Parse(content, DateTime.Now);
    }

    public static OldParseDueDate Parse(string content, DateTime baseDate)
    {
      try
      {
        bool flag = Utils.ContainsChineseInText(content);
        OldParseDueDate oldParseDueDate = flag ? OldDateParser.ParseChinese(content, baseDate) : OldDateParser.ParseEnglish(content, baseDate);
        return oldParseDueDate.RecognizeStrings.Count > 0 ? oldParseDueDate : (flag ? OldDateParser.ParseEnglish(content, baseDate) : OldDateParser.ParseChinese(content, baseDate));
      }
      catch (Exception ex)
      {
        return new OldParseDueDate();
      }
    }

    public static DateTime GetNextDate(int month, bool hasDay, DateTime date)
    {
      return date.Month == month && !hasDay ? DateTime.Today : date.AddYears(1);
    }

    private static OldParseDueDate ParseEnglish(string content, DateTime baseDate)
    {
      return OldDateParser.Parse(content, (IParser) new EnglishParser(), baseDate);
    }

    private static OldParseDueDate ParseChinese(string content, DateTime baseDate)
    {
      return OldDateParser.Parse(content, (IParser) new ChineseParser(), baseDate);
    }

    private static OldParseDueDate Parse(string content, IParser parser, DateTime baseDate)
    {
      OldParseDueDate oldParseDueDate = new OldParseDueDate();
      DateTime? nullable1 = new DateTime?();
      RecurrencePattern recurrencePattern = parser.MatchRepeat(content, ref nullable1, baseDate, ref oldParseDueDate.RecognizeStrings);
      if (nullable1.HasValue)
      {
        DateTime dateTime1 = nullable1.Value;
        if (dateTime1.Date < baseDate.Date)
        {
          ref DateTime? local = ref nullable1;
          dateTime1 = nullable1.Value;
          DateTime dateTime2 = dateTime1.AddDays(1.0);
          local = new DateTime?(dateTime2);
        }
        oldParseDueDate.SetDueDate(nullable1.Value);
        if (recurrencePattern != null)
          oldParseDueDate.RepeatFlag = "RRULE:" + recurrencePattern?.ToString();
      }
      if (!nullable1.HasValue)
        nullable1 = new DateTime?(baseDate);
      if (parser.MatchAfterTime(content, ref nullable1, ref oldParseDueDate.RecognizeStrings) && nullable1.HasValue)
      {
        oldParseDueDate.DueDate = nullable1.Value;
        oldParseDueDate.SetReminder(TickTickDuration.OnTime);
      }
      DateTime? nullable2 = parser.MatchWeekday(content, baseDate, ref oldParseDueDate.RecognizeStrings);
      if (nullable2.HasValue)
        oldParseDueDate.DueDate = nullable2.Value;
      DateTime? nullable3 = parser.MatchMonth(content, baseDate, ref oldParseDueDate.RecognizeStrings);
      if (nullable3.HasValue)
        oldParseDueDate.DueDate = nullable3.Value;
      DateTime? nullable4 = parser.MatchDate(content, baseDate, ref oldParseDueDate.RecognizeStrings);
      if (nullable4.HasValue)
        oldParseDueDate.DueDate = nullable4.Value;
      DateTime? nullable5 = parser.MatchSpecialDay(content, baseDate, ref oldParseDueDate.RecognizeStrings);
      if (nullable5.HasValue)
        oldParseDueDate.DueDate = nullable5.Value;
      DateTime? parseDate = Utils.IsEmptyDate(oldParseDueDate.DueDate) ? new DateTime?() : new DateTime?(oldParseDueDate.DueDate);
      if (parser.MatchTime(content, ref parseDate, baseDate, ref oldParseDueDate.RecognizeStrings))
      {
        oldParseDueDate.SetReminder(TickTickDuration.OnTime);
        if (parseDate.HasValue)
          oldParseDueDate.DueDate = parseDate.Value;
      }
      else if (parser.MatchSpecialTime(content, ref parseDate, baseDate, ref oldParseDueDate.RecognizeStrings))
      {
        oldParseDueDate.SetReminder(TickTickDuration.OnTime);
        if (parseDate.HasValue)
          oldParseDueDate.DueDate = parseDate.Value;
      }
      if (!Utils.IsEmptyDate(oldParseDueDate.DueDate))
      {
        TickTickDuration tickTickDuration = parser.MatchAdvanceTime(content, ref oldParseDueDate.RecognizeStrings);
        oldParseDueDate.Duration = tickTickDuration;
      }
      return oldParseDueDate;
    }

    public static DateTime GetDateByDuration(DateTime date, TickTickDuration duration)
    {
      int num = duration.IsPositive() ? 1 : -1;
      if (duration.GetYears() != 0)
        date = date.AddYears(duration.GetYears() * num);
      if (duration.GetMonths() != 0)
        date = date.AddMonths(duration.GetMonths() * num);
      if (duration.GetDays() != 0)
        date = date.AddDays((double) (duration.GetDays() * num));
      if (duration.GetWeeks() != 0)
        date = date.AddDays((double) (duration.GetWeeks() * 7 * num));
      if (duration.GetHours() != 0)
        date = date.AddHours((double) (duration.GetHours() * num));
      if (duration.GetMinutes() != 0)
        date = date.AddMinutes((double) (duration.GetMinutes() * num));
      return date;
    }

    public static TickTickDuration GetDuration(string numStr, string timeUnit, bool isPositive = true)
    {
      int num = !OldDateParser.IsAnOrA(numStr) ? (!OldDateParser.IsNull(numStr) ? OldDateParser.TranslateNum(numStr) : 0) : 1;
      TickTickDuration duration = (TickTickDuration) null;
      if (OldDateParser.IsMinutesUnit(timeUnit))
      {
        if (OldDateParser.IsHalf(numStr))
          duration = new TickTickDuration(0, 0, 0, 0, 0, 0, 30);
        else if (num != 0)
          duration = new TickTickDuration(0, 0, 0, 0, 0, num, 0);
      }
      else if (OldDateParser.IsHourUnit(timeUnit))
      {
        if (OldDateParser.IsHalf(numStr))
          duration = new TickTickDuration(0, 0, 0, 0, 0, 30, 0);
        else if (num != 0)
          duration = new TickTickDuration(0, 0, 0, 0, num, 0, 0);
      }
      else if (OldDateParser.IsDayUnit(timeUnit))
      {
        if (OldDateParser.IsHalf(numStr))
          duration = new TickTickDuration(0, 0, 0, 0, 12, 0, 0);
        else if (num != 0)
          duration = new TickTickDuration(0, 0, 0, num, 0, 0, 0);
      }
      else if (OldDateParser.IsWeekUnit(timeUnit))
      {
        if (OldDateParser.IsHalf(numStr))
          duration = new TickTickDuration(0, 0, 0, 3, 0, 0, 0);
        else if (num != 0)
          duration = new TickTickDuration(0, 0, num, 0, 0, 0, 0);
      }
      else if (OldDateParser.IsMonthUnit(timeUnit))
        duration = new TickTickDuration(0, num, 0, 0, 0, 0, 0);
      else if (OldDateParser.IsYearUnit(timeUnit))
        duration = new TickTickDuration(num, 0, 0, 0, 0, 0, 0);
      duration?.SetIsPositive(isPositive);
      return duration;
    }

    public static TickTickDuration GetDuration(int num, string timeUnit, bool isPositive)
    {
      TickTickDuration duration = (TickTickDuration) null;
      if (OldDateParser.IsMinutesUnit(timeUnit))
        duration = new TickTickDuration(0, 0, 0, 0, 0, num, 0);
      else if (OldDateParser.IsHourUnit(timeUnit))
        duration = new TickTickDuration(0, 0, 0, 0, num, 0, 0);
      else if (OldDateParser.IsDayUnit(timeUnit))
        duration = new TickTickDuration(0, 0, 0, num, 0, 0, 0);
      else if (OldDateParser.IsWeekUnit(timeUnit))
        duration = new TickTickDuration(0, 0, num, 0, 0, 0, 0);
      else if (OldDateParser.IsMonthUnit(timeUnit))
        duration = new TickTickDuration(0, num, 0, 0, 0, 0, 0);
      else if (OldDateParser.IsYearUnit(timeUnit))
        duration = new TickTickDuration(num, 0, 0, 0, 0, 0, 0);
      duration?.SetIsPositive(isPositive);
      return duration;
    }

    private static bool ContainsExp(IEnumerable<string> exps, string str)
    {
      return exps.Contains<string>(str);
    }

    public static bool IsHalf(string str)
    {
      return OldDateParser.ContainsExp((IEnumerable<string>) OldDateParser.HalfExps, str);
    }

    public static bool IsYearUnit(string str)
    {
      return OldDateParser.ContainsExp((IEnumerable<string>) OldDateParser.YearExps, str);
    }

    public static bool IsMonthUnit(string str)
    {
      return OldDateParser.ContainsExp((IEnumerable<string>) OldDateParser.MonthExps, str);
    }

    public static bool IsWeekUnit(string str)
    {
      return OldDateParser.ContainsExp((IEnumerable<string>) OldDateParser.WeekExps, str);
    }

    public static bool IsDayUnit(string str)
    {
      return OldDateParser.ContainsExp((IEnumerable<string>) OldDateParser.DayExps, str);
    }

    public static bool IsHourUnit(string str)
    {
      return OldDateParser.ContainsExp((IEnumerable<string>) OldDateParser.HourExps, str);
    }

    public static bool IsMinutesUnit(string str)
    {
      return OldDateParser.ContainsExp((IEnumerable<string>) OldDateParser.MinuteExps, str);
    }

    public static int TranslateNum(string n)
    {
      if (OldDateParser.IsNull(n))
        return 0;
      try
      {
        return int.Parse(n);
      }
      catch (Exception ex)
      {
        return OldDateParser.MatchNum(n);
      }
    }

    public static bool IsNull(string group) => string.IsNullOrEmpty(group);

    public static bool IsAnOrA(string str) => "a" == str || "an" == str;

    public static int MatchNum(string str)
    {
      int result;
      if (int.TryParse(str, out result))
        return result;
      if (str != null)
      {
        switch (str.Length)
        {
          case 1:
            switch (str[0])
            {
              case '两':
                break;
              case '个':
                return 1;
              case '十':
                return 10;
              case '半':
                return 30;
              case '天':
              case '日':
                return 7;
              default:
                goto label_24;
            }
            break;
          case 2:
            switch (str[0])
            {
              case '1':
                if (str == "1刻")
                  break;
                goto label_24;
              case '3':
                if (str == "3刻")
                  goto label_16;
                else
                  goto label_24;
              case '一':
                if (str == "一刻")
                  break;
                goto label_24;
              case '三':
                if (str == "三刻")
                  goto label_16;
                else
                  goto label_24;
              case '上':
                if (str == "上旬")
                  return 1;
                goto label_24;
              case '下':
                if (str == "下旬")
                  return 21;
                goto label_24;
              case '中':
                if (str == "中旬")
                  return 11;
                goto label_24;
              case '零':
                if (str == "零两")
                  goto label_17;
                else
                  goto label_24;
              default:
                goto label_24;
            }
            return 15;
label_16:
            return 45;
          default:
            goto label_24;
        }
label_17:
        return 2;
      }
label_24:
      string[] strArray = new string[10]
      {
        "零",
        "一",
        "二",
        "三",
        "四",
        "五",
        "六",
        "七",
        "八",
        "九"
      };
      if (str.IndexOf("零", StringComparison.Ordinal) == 0)
        return str.Length == 1 ? 0 : OldDateParser.MatchNum(str.Substring(1, str.Length));
      if (str.Contains("十"))
      {
        int length = str.IndexOf("十", StringComparison.Ordinal);
        string str1 = str.Substring(0, length);
        string str2 = string.Empty;
        if (length + 2 <= str.Length)
          str2 = str.Substring(length + 1, 1);
        return str1.Length == 0 ? 10 + OldDateParser.MatchNum(str2) : OldDateParser.MatchNum(str1) * 10 + OldDateParser.MatchNum(str2);
      }
      for (int index = 0; index < strArray.Length; ++index)
      {
        if (str.Contains(strArray[index]))
          return index;
      }
      return 0;
    }

    public static void AddMatchString(ICollection<string> recgonizeStrList, string matched)
    {
      string str = matched.Trim().Replace(",", "");
      if (recgonizeStrList.Contains(str))
        return;
      recgonizeStrList.Add(str);
    }
  }
}
