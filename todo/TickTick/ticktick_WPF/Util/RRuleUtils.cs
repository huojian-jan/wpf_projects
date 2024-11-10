// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.RRuleUtils
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Ical.Net;
using Ical.Net.DataTypes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;

#nullable disable
namespace ticktick_WPF.Util
{
  public class RRuleUtils
  {
    private readonly string _repeatFrom;
    private readonly string _rrule;
    private readonly DateTime _startDate;
    private RecurrencePattern _pattern;

    private RRuleUtils(string rrule, string repeatFrom, DateTime? startDate)
    {
      this._rrule = rrule;
      this._repeatFrom = repeatFrom;
      if (Utils.IsEmptyDate(startDate) || !startDate.HasValue)
        return;
      this._startDate = startDate.Value;
    }

    public static string RRule2String(
      string repeatFrom,
      string repeatFlag,
      DateTime? startDate,
      bool showUntil = true,
      bool showDesc = false)
    {
      string repeatFrom1 = string.IsNullOrEmpty(repeatFrom) ? "0" : repeatFrom;
      return new RRuleUtils(string.IsNullOrEmpty(repeatFlag) ? string.Empty : repeatFlag, repeatFrom1, startDate).RRule2String(startDate, showUntil, showDesc);
    }

    private string RRule2String(DateTime? startDate, bool showUntil = true, bool showDesc = false)
    {
      RecurrencePattern recurrenceModel;
      try
      {
        recurrenceModel = (RecurrencePattern) RecurrenceModel.GetRecurrenceModel(this._rrule);
      }
      catch (Exception ex)
      {
        return string.Empty;
      }
      string str1 = "";
      if (this._rrule.Contains("FORGETTINGCURVE"))
      {
        str1 = Utils.GetString("Ebbinghaus");
        if (showUntil | showDesc)
        {
          string str2 = ((IEnumerable<string>) this._rrule.Split(';')).FirstOrDefault<string>((Func<string, bool>) (r => r.Contains("CYCLE")));
          int result;
          if (str2 != null && int.TryParse(str2.Replace("CYCLE=", ""), out result))
          {
            int num = result + 1;
            str1 = str1 + ", " + string.Format(Utils.GetString("NTimes"), (object) Utils.GetNthString(num));
          }
        }
      }
      else if (this._rrule.Contains("LUNAR"))
      {
        str1 = "农历每年";
        if (startDate.HasValue)
        {
          LunarUtils.ChineseCalendar chineseCalendar = new LunarUtils.ChineseCalendar(startDate.Value);
          if (!Utils.IsEmptyDate(chineseCalendar.Date))
            str1 = "农历每年 " + chineseCalendar.ChineseMonthString + chineseCalendar.ChineseDayString;
        }
      }
      else
      {
        if (this._rrule.Contains("TT_WORKDAY=1"))
          return recurrenceModel.Interval == 1 ? Utils.GetString("MonthlyFirstWorkday") : string.Format(Utils.GetString("EveryMonthFirstWorkday"), (object) recurrenceModel.Interval);
        if (this._rrule.Contains("TT_WORKDAY=-1"))
          return recurrenceModel.Interval == 1 ? Utils.GetString("MonthlyLastWorkday") : string.Format(Utils.GetString("EveryMonthLastWorkday"), (object) recurrenceModel.Interval);
        this._pattern = (RecurrencePattern) RecurrenceModel.GetRecurrenceModel(this._rrule);
        switch (this._pattern.Frequency)
        {
          case FrequencyType.Daily:
            str1 = this.GetDailyRRule();
            break;
          case FrequencyType.Weekly:
            str1 = this.GetWeeklyRRule(showDesc ? startDate : new DateTime?());
            break;
          case FrequencyType.Monthly:
            str1 = this.GetMonthlyRRule(showDesc ? startDate : new DateTime?());
            break;
          case FrequencyType.Yearly:
            str1 = this.GetYearlyyRRule(showDesc ? startDate : new DateTime?());
            break;
        }
        if (this._pattern.Frequency == FrequencyType.None && this._rrule.Contains("RRULE:FREQ=DAILY"))
          str1 = this.GetDailyRRule();
      }
      if (showUntil)
      {
        string untilText = RRuleUtils.GetUntilText(recurrenceModel, this._rrule);
        if (!string.IsNullOrEmpty(untilText))
          str1 = str1 + ", " + untilText;
      }
      return str1;
    }

    public static string GetUntilText(RecurrencePattern pattern, string erule, bool toLower = true)
    {
      string untilText = "";
      if (erule != null && erule.Contains("FORGETTINGCURVE"))
      {
        string[] source = erule.Split(';');
        int result1;
        if (int.TryParse(((IEnumerable<string>) source).FirstOrDefault<string>((Func<string, bool>) (r => r.Contains("COUNT")))?.Replace("COUNT=", ""), out result1))
          return string.Format(Utils.GetString("InCountTimes").ToLower(), (object) result1);
        DateTime result2;
        if (DateTime.TryParseExact(((IEnumerable<string>) source).FirstOrDefault<string>((Func<string, bool>) (r => r.Contains("UNTIL")))?.Replace("UNTIL=", ""), "yyyyMMdd", (IFormatProvider) CultureInfo.InvariantCulture, DateTimeStyles.None, out result2))
          return string.Format(Utils.GetString("Until"), (object) result2.ToString("yyyy'-'MM'-'dd", (IFormatProvider) App.Ci));
      }
      if (pattern.Count >= 1)
        return string.Format(Utils.GetString("InCountTimes").ToLower(), (object) pattern.Count);
      if (!Utils.IsEmptyDate(pattern.Until))
        untilText += string.Format(toLower ? Utils.GetString("Until").ToLower() : Utils.GetString("Until"), (object) pattern.Until.ToString("yyyy-MM-dd"));
      return untilText;
    }

    private string GetDailyRRule()
    {
      if (this._repeatFrom == "1")
        return RRuleUtils.GetQuantityString(this._pattern.Interval, "RepeatFromCompleteTimeDay", "RepeatFromCompleteTimeDays");
      return this._pattern.Interval > 1 ? string.Format(Utils.GetString("DescriptionDailySetRepeatMore"), (object) this._pattern.Interval) : string.Format(Utils.GetString("EveryDay"));
    }

    private string GetWeeklyRRule(DateTime? date = null)
    {
      if (this._repeatFrom == "1")
        return RRuleUtils.GetQuantityString(this._pattern.Interval, "RepeatFromCompleteTimeWeek", "RepeatFromCompleteTimeWeeks");
      List<WeekDay> list = this._pattern.ByDay.OrderBy<WeekDay, DayOfWeek>((Func<WeekDay, DayOfWeek>) (p => p.DayOfWeek)).ToList<WeekDay>();
      if (this._pattern.Interval > 1)
        return string.Format(Utils.GetString("DescriptionWeekdaysSetRepeatMore"), (object) RRuleUtils.GetWeekdaysText(list), (object) this._pattern.Interval);
      return list.Count == 0 ? Utils.GetString("EveryWeek") + (!date.HasValue || Utils.IsEmptyDate(date.Value) ? "" : " (" + DateUtils.GetWeekTextByWeekDay((int) date.Value.DayOfWeek) + ")") : string.Format(Utils.GetString("DescriptionWeekdaysSetRepeatOne"), (object) RRuleUtils.GetWeekdaysText(list));
    }

    private string GetMonthlyRRule(DateTime? date = null)
    {
      if (this._repeatFrom == "1")
        return RRuleUtils.GetQuantityString(this._pattern.Interval, "RepeatFromCompleteTimeMonth", "RepeatFromCompleteTimeMonths");
      List<WeekDay> list1 = this._pattern.ByDay.OrderBy<WeekDay, DayOfWeek>((Func<WeekDay, DayOfWeek>) (p => p.DayOfWeek)).ToList<WeekDay>();
      List<int> list2 = this._pattern.ByMonthDay.OrderBy<int, int>((Func<int, int>) (p => p)).ToList<int>();
      if (this._pattern.Interval > 1 && list2.Count == 0 && !Utils.IsEmptyDate(this._startDate))
        list2.Add(this._startDate.Day);
      if (list1.Count != 0)
      {
        string weekOnDayString = RRuleUtils.GetWeekOnDayString((IReadOnlyList<WeekDay>) list1);
        return string.IsNullOrEmpty(weekOnDayString) ? (this._pattern.Interval > 1 ? string.Format(Utils.GetString("DescriptionMonthDaySetRepeatMore"), (object) "", (object) this._pattern.Interval) : string.Format(Utils.GetString("DescriptionMonthDaySetRepeatOne"), (object) "")) : (this._pattern.Interval > 1 ? string.Format(Utils.GetString("DescriptionMonthWeekSetRepeatMore"), (object) weekOnDayString, (object) this._pattern.Interval) : string.Format(Utils.GetString("DescriptionMonthWeekSetRepeatOne"), (object) weekOnDayString));
      }
      if (list2.Count <= 0)
        return Utils.GetString("EveryMonth") + (!date.HasValue || Utils.IsEmptyDate(date.Value) ? "" : " (" + DateUtils.FormatDay(date.Value) + ")");
      if (this._repeatFrom == "2")
        return this._pattern.Interval > 1 ? string.Format(Utils.GetString("EveryNMonth"), (object) this._pattern.Interval) : Utils.GetString("EveryMonth");
      string monthDayString = RRuleUtils.GetMonthDayString((IReadOnlyList<int>) list2);
      return string.IsNullOrWhiteSpace(monthDayString) ? (this._pattern.Interval > 1 ? string.Format(Utils.GetString("DescriptionMonthDaySetRepeatMore"), (object) "", (object) this._pattern.Interval) : Utils.GetString("EveryMonth")) : (this._pattern.Interval > 1 ? string.Format(Utils.GetString("DescriptionMonthDaySetRepeatMore"), (object) monthDayString, (object) this._pattern.Interval) : string.Format(Utils.GetString("DescriptionMonthWeekSetRepeatOne"), (object) monthDayString));
    }

    private string GetYearlyyRRule(DateTime? date = null)
    {
      if (this._repeatFrom == "1")
        return RRuleUtils.GetQuantityString(this._pattern.Interval, "RepeatFromCompleteTimeYear", "RepeatFromCompleteTimeYears");
      string str = this._pattern.Interval > 1 ? string.Format(Utils.GetString("DescriptionYearlySetRepeatMore"), (object) this._pattern.Interval) : string.Format(Utils.GetString("EveryYear"));
      List<int> byMonth = this._pattern.ByMonth;
      // ISSUE: explicit non-virtual call
      if ((byMonth != null ? (__nonvirtual (byMonth.Count) > 0 ? 1 : 0) : 0) != 0)
      {
        List<WeekDay> byDay = this._pattern.ByDay;
        // ISSUE: explicit non-virtual call
        if ((byDay != null ? (__nonvirtual (byDay.Count) > 0 ? 1 : 0) : 0) != 0)
        {
          int num = this._pattern.ByMonth[0];
          string weekOnDayString = RRuleUtils.GetWeekOnDayString((IReadOnlyList<WeekDay>) this._pattern.ByDay.OrderBy<WeekDay, DayOfWeek>((Func<WeekDay, DayOfWeek>) (p => p.DayOfWeek)).ToList<WeekDay>());
          return string.Format(Utils.GetString("YearMonthRepeat"), (object) str, (object) DateTime.Today.AddMonths(num - DateTime.Today.Month).ToString("MMM", (IFormatProvider) App.Ci), (object) weekOnDayString);
        }
      }
      return this._pattern.Interval > 1 ? str : Utils.GetString("EveryYear") + (!date.HasValue || Utils.IsEmptyDate(date.Value) ? "" : " (" + DateUtils.FormatShortMonthDay(date.Value) + ")");
    }

    private static string GetQuantityString(int interval, string oneString, string moreString)
    {
      return string.Format(interval > 1 ? Utils.GetString(moreString) : Utils.GetString(oneString), (object) interval);
    }

    private static string GetWeekdaysText(List<WeekDay> weekdayNums)
    {
      StringBuilder stringBuilder = new StringBuilder();
      for (int index = 0; index < weekdayNums.Count; ++index)
      {
        stringBuilder.Append(Utils.GetShortWeekName((int) weekdayNums[index].DayOfWeek));
        if (index < weekdayNums.Count - 1)
          stringBuilder.Append(", ");
      }
      return stringBuilder.ToString();
    }

    private static string GetWeekOnDayString(IReadOnlyList<WeekDay> byDays)
    {
      string[] resource = (string[]) Application.Current?.FindResource((object) "DescriptionOrdinalLabels");
      WeekDay byDay = byDays[0];
      int index = byDay.Offset - 1;
      if (index < 0 && resource != null)
        index = resource.Length - 1;
      if (byDays.Count == 1)
      {
        int dayOfWeek = (int) byDay.DayOfWeek;
        if (resource != null && index >= 0 && index <= 5)
          return resource[index] + Utils.GetShortWeekName(dayOfWeek);
      }
      return "";
    }

    private static string GetMonthDayString(IReadOnlyList<int> byMonthDay)
    {
      if (byMonthDay.Count <= 0)
        return "";
      string[] resource = (string[]) Application.Current?.FindResource((object) "OneMonthDay");
      StringBuilder stringBuilder1 = new StringBuilder();
      StringBuilder stringBuilder2 = new StringBuilder();
      int index = 0;
      for (int count = byMonthDay.Count; index < count; ++index)
      {
        int num = byMonthDay[index];
        if (index == 0)
        {
          if (num == -1)
          {
            if (count > 1)
              stringBuilder2.Append(", " + Utils.GetString("LastDay"));
            else
              stringBuilder2.Append(Utils.GetString("LastDay"));
          }
          else if (num >= -1 && resource != null)
            stringBuilder1.Append(resource[num - 1]);
        }
        else if (num == -1)
          stringBuilder2.Append(", " + Utils.GetString("LastDay"));
        else if (num >= -1)
        {
          if (stringBuilder1.Length == 0)
          {
            if (resource != null)
              stringBuilder1.Append(resource[num - 1]);
          }
          else if (resource != null)
            stringBuilder1.Append(", " + resource[num - 1]);
        }
      }
      return !string.IsNullOrWhiteSpace(stringBuilder1.ToString()) && string.IsNullOrWhiteSpace(stringBuilder2.ToString()) ? stringBuilder1.ToString() : stringBuilder1.Append((object) stringBuilder2).ToString();
    }

    public static string GetNextCount(string flag, bool handleEbbingCount)
    {
      RecurrenceModel recurrenceModel = RecurrenceModel.GetRecurrenceModel(flag);
      int count1 = recurrenceModel.Count > 0 ? recurrenceModel.Count - 1 : 0;
      int count2 = 1;
      if (flag != null && flag.Contains("FORGETTINGCURVE"))
      {
        count1 = RepeatUtils.GetRepeatCount(flag) - 1;
        if (handleEbbingCount)
          flag = RepeatUtils.GetNextEbbinghausCycle(flag, count2);
      }
      if (count1 > 0)
        flag = RepeatUtils.GetRepeatFlag(flag, new DateTime(), count1);
      return flag;
    }

    public static string HandleUntilText(string rrule, string timeZone)
    {
      if (rrule.Contains("UNTIL"))
      {
        string oldValue = ((IEnumerable<string>) rrule.Split(';')).FirstOrDefault<string>((Func<string, bool>) (p => p.StartsWith("UNTIL")));
        DateTime result;
        if (oldValue != null && oldValue.Contains("T") && DateTime.TryParseExact(oldValue.Replace("UNTIL=", ""), "yyyyMMdd'T'HHmmss'Z'", (IFormatProvider) CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
        {
          result = TimeZoneUtils.UtcToTargetTzTime(result, timeZone);
          rrule = rrule.Replace(oldValue, "UNTIL=" + result.ToString("yyyyMMdd", (IFormatProvider) CultureInfo.InvariantCulture));
        }
      }
      return rrule;
    }
  }
}
