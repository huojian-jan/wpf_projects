// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.RepeatUtils
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util.KotlinUtils;
using ticktick_WPF.Util.Sync;
using TickTickUtils;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Util
{
  public static class RepeatUtils
  {
    public const string OfficalWorkingDaysFlag = "RRULE:FREQ=DAILY;INTERVAL=1;TT_SKIP=HOLIDAY,WEEKEND";
    public const string MonthlyFirstWorkdayFlag = "TT_WORKDAY=1";
    public const string MonthlyLastWorkdayFlag = "TT_WORKDAY=-1";

    public static List<DateTime> GetValidRepeatDates(
      string rrule,
      string repeatFrom,
      DateTime startDate,
      DateTime spanStart,
      DateTime spanEnd,
      List<string> exDate = null,
      string timeZone = null,
      bool toLocalTime = true)
    {
      try
      {
        return RepeatCalculator.GetRepeatDates(rrule, repeatFrom, startDate, spanStart, spanEnd, exDate, timeZone, toLocalTime);
      }
      catch (Exception ex)
      {
      }
      List<HolidayModel> holidays = Utils.IsDida() ? HolidayManager.GetCacheHolidays() : new List<HolidayModel>();
      List<string> source1 = exDate;
      exDate = source1 != null ? source1.Select<string, string>((Func<string, string>) (text => text.Replace("\"", ""))).ToList<string>() : (List<string>) null;
      List<DateTime> source2 = new List<DateTime>();
      if (repeatFrom == "1" || Utils.IsEmptyRepeatFlag(rrule))
        return source2;
      if (rrule.ToUpper().Contains("LUNAR"))
        return RepeatUtils.GetValidLunarRepeatDates(rrule.ToUpper(), startDate, spanEnd, exDate);
      if (rrule.ToUpper().Contains("FORGETTINGCURVE"))
        return RepeatUtils.GetValidEbbinghausRepeatDates(rrule.ToUpper(), startDate, spanStart, spanEnd, exDate, timeZone);
      if (RepeatUtils.IsMonthlyWorkday(rrule))
        return RepeatUtils.GetValidWorkingDays(rrule, spanStart, spanEnd, startDate);
      int count = -1;
      RecurrencePattern recurrenceModel;
      try
      {
        recurrenceModel = (RecurrencePattern) RecurrenceModel.GetRecurrenceModel(rrule);
        if (recurrenceModel.Count > 0)
        {
          count = recurrenceModel.Count;
          recurrenceModel.Count = int.MinValue;
        }
        switch (recurrenceModel.Frequency)
        {
          case FrequencyType.Weekly:
            if (recurrenceModel.ByDay.Count == 0)
            {
              spanStart = spanStart.AddDays(-7.0);
              recurrenceModel.ByDay.Add(new WeekDay(startDate.DayOfWeek));
              break;
            }
            break;
          case FrequencyType.Monthly:
            if (recurrenceModel.ByMonthDay.Count == 0 && recurrenceModel.ByDay.Count == 0)
            {
              spanStart = spanStart.AddMonths(-1);
              recurrenceModel.ByMonthDay.Add(startDate.Day);
            }
            if (recurrenceModel.ByMonthDay.Count == 1)
            {
              if (recurrenceModel.ByMonthDay[0] != 29 && recurrenceModel.ByMonthDay[0] != 30)
              {
                if (recurrenceModel.ByMonthDay[0] != 31)
                  break;
              }
              List<DateTime> source3 = RepeatUtils.GetCreditCardDate(spanStart, spanEnd, startDate, recurrenceModel, count, rrule, holidays);
              if (repeatFrom == "2")
                source3 = source3.Where<DateTime>((Func<DateTime, bool>) (date => date >= DateTime.Today)).ToList<DateTime>();
              return source3;
            }
            break;
          case FrequencyType.Yearly:
            if (recurrenceModel.ByMonthDay.Count == 0)
            {
              if (recurrenceModel.ByMonth.Count == 0)
              {
                recurrenceModel.ByMonth.Add(startDate.Month);
                recurrenceModel.ByMonthDay.Add(startDate.Day);
                break;
              }
              break;
            }
            break;
        }
      }
      catch (Exception ex)
      {
        UtilLog.Error("get_repeat_dates_exception_2:" + ExceptionUtils.BuildExceptionMessage(ex));
        return source2;
      }
      DateTime? endDate = new DateTime?();
      if (!Utils.IsEmptyDate(recurrenceModel.Until) && recurrenceModel.Until.Date <= spanEnd.Date)
      {
        DateTime until;
        if (!string.IsNullOrEmpty(timeZone))
        {
          until = recurrenceModel.Until;
          endDate = new DateTime?(TimeZoneUtils.ToLocalTime(until.AddDays(1.0), timeZone));
        }
        else
        {
          until = recurrenceModel.Until;
          endDate = new DateTime?(until.AddDays(1.0));
        }
        RecurrencePattern recurrencePattern = recurrenceModel;
        until = recurrenceModel.Until;
        DateTime dateTime = until.AddDays(2.0);
        recurrencePattern.Until = dateTime;
        spanEnd = recurrenceModel.Until;
      }
      DateTime dateTime1 = startDate;
      if (repeatFrom == "2" && count <= 0)
        dateTime1 = dateTime1 < DateTime.Today ? DateTime.Today : dateTime1;
      RecurringComponent recurringComponent = new RecurringComponent()
      {
        Start = (IDateTime) new CalDateTime()
        {
          Value = dateTime1
        }
      };
      recurringComponent.RecurrenceRules.Add(recurrenceModel);
      HashSet<Occurrence> occurrenceSet = (HashSet<Occurrence>) null;
      try
      {
        if (count > 0)
          spanStart = startDate;
        occurrenceSet = recurringComponent.GetOccurrences(spanStart, spanEnd);
        occurrenceSet.RemoveWhere((Predicate<Occurrence>) (occ => RepeatUtils.IsOccurrenceEqualStartDate(occ, startDate)));
        if (exDate != null && exDate.Any<string>())
          occurrenceSet.RemoveWhere((Predicate<Occurrence>) (occ => RepeatUtils.IsOccurenceInExDates(occ, (ICollection<string>) exDate)));
        if (rrule.ToUpper() == "RRULE:FREQ=DAILY;INTERVAL=1;TT_SKIP=HOLIDAY,WEEKEND")
        {
          occurrenceSet.RemoveWhere((Predicate<Occurrence>) (occ => RepeatUtils.IsOccurenceNonOfficalWorkDay(occ, (IEnumerable<HolidayModel>) holidays)));
        }
        else
        {
          if (rrule.ToUpper().Contains("HOLIDAY"))
            occurrenceSet.RemoveWhere((Predicate<Occurrence>) (occ => RepeatUtils.IsOccurenceHoilday(occ, (IEnumerable<HolidayModel>) holidays)));
          if (rrule.ToUpper().Contains("WEEKEND"))
            occurrenceSet.RemoveWhere(new Predicate<Occurrence>(RepeatUtils.IsOccurenceWeekend));
        }
      }
      catch (Exception ex)
      {
        UtilLog.Error("get_repeat_dates_exception_2:" + ExceptionUtils.BuildExceptionMessage(ex));
      }
      if (occurrenceSet != null && occurrenceSet.Count > 0)
      {
        foreach (Occurrence occurrence in occurrenceSet)
        {
          DateTime? nullable;
          try
          {
            int year = occurrence.Period.StartTime.Value.Year;
            DateTime dateTime2 = occurrence.Period.StartTime.Value;
            int month = dateTime2.Month;
            dateTime2 = occurrence.Period.StartTime.Value;
            int day = dateTime2.Day;
            int hour = startDate.Hour;
            int millisecond = startDate.Millisecond;
            int second = startDate.Second;
            nullable = new DateTime?(new DateTime(year, month, day, hour, millisecond, second));
          }
          catch (Exception ex)
          {
            nullable = new DateTime?();
          }
          if (nullable.HasValue)
            source2.Add(nullable.Value);
        }
        if (count > 0)
          source2 = source2.Take<DateTime>(count - 1).ToList<DateTime>();
      }
      if (!endDate.HasValue)
        return source2;
      return source2 == null ? (List<DateTime>) null : source2.Where<DateTime>((Func<DateTime, bool>) (r =>
      {
        DateTime dateTime3 = r;
        DateTime? nullable = endDate;
        return nullable.HasValue && dateTime3 < nullable.GetValueOrDefault();
      })).ToList<DateTime>();
    }

    internal static bool RepeatOutDate(string repeatFlag)
    {
      try
      {
        RecurrenceModel recurrenceModel = RecurrenceModel.GetRecurrenceModel(repeatFlag);
        return !Utils.IsEmptyDate(recurrenceModel.Until) && recurrenceModel.Until < DateTime.Today;
      }
      catch
      {
        return false;
      }
    }

    private static List<DateTime> GetCreditCardDate(
      DateTime spanStart,
      DateTime spanEnd,
      DateTime startDate,
      RecurrencePattern pattern,
      int count,
      string rule,
      List<HolidayModel> holidays)
    {
      List<DateTime> dateTimeList1 = new List<DateTime>();
      List<DateTime> creditCardDate = new List<DateTime>();
      if (!Utils.IsEmptyDate(pattern.Until) && pattern.Until < spanStart)
        return creditCardDate;
      int val2 = (spanEnd.Year - startDate.Year) * 12 + (spanEnd.Month - startDate.Month);
      if (count > 0)
        val2 = Math.Min(count * pattern.Interval, val2);
      for (int months = 0; months <= val2; ++months)
      {
        DateTime dateTime1 = new DateTime(startDate.Year, startDate.Month, 1);
        dateTime1 = dateTime1.AddMonths(months + 1);
        DateTime date = dateTime1.AddDays(-1.0);
        if (date.Day > pattern.ByMonthDay[0])
        {
          dateTime1 = new DateTime(startDate.Year, startDate.Month, pattern.ByMonthDay[0]);
          date = dateTime1.AddMonths(months);
        }
        if (date > startDate && (!rule.ToUpper().Contains("HOLIDAY") || !holidays.Any<HolidayModel>((Func<HolidayModel, bool>) (h => h.date == date && h.type == 0))) && (!rule.ToUpper().Contains("WEEKEND") || date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday))
        {
          List<DateTime> dateTimeList2 = dateTimeList1;
          dateTime1 = date.AddHours((double) startDate.Hour);
          DateTime dateTime2 = dateTime1.AddMinutes((double) startDate.Minute);
          dateTimeList2.Add(dateTime2);
        }
      }
      for (int index = 0; index < dateTimeList1.Count; ++index)
      {
        if ((Utils.IsEmptyDate(pattern.Until) || !(dateTimeList1[index] > pattern.Until)) && (count <= 0 || (index + 1) % pattern.Interval == 0 && (index + 1) / pattern.Interval <= count - 1) && dateTimeList1[index] >= spanStart && dateTimeList1[index] <= spanEnd && (index + 1) % pattern.Interval == 0)
          creditCardDate.Add(dateTimeList1[index]);
      }
      return creditCardDate;
    }

    private static List<DateTime> GetValidEbbinghausRepeatDates(
      string rrule,
      DateTime startDate,
      DateTime spanStart,
      DateTime spanEnd,
      List<string> exDate,
      string timeZone = null)
    {
      List<DateTime> ebbinghausRepeatDates = new List<DateTime>();
      if (spanStart < startDate)
        spanStart = startDate;
      if (spanEnd < startDate)
        return ebbinghausRepeatDates;
      if (exDate == null)
        exDate = new List<string>();
      if (rrule.Contains("CYCLE"))
      {
        string[] source = rrule.Split(';');
        int result1;
        if (int.TryParse(((IEnumerable<string>) source).FirstOrDefault<string>((Func<string, bool>) (r => r.Contains("CYCLE")))?.Replace("CYCLE=", ""), out result1))
        {
          int num1 = 10000;
          string str1 = ((IEnumerable<string>) source).FirstOrDefault<string>((Func<string, bool>) (r => r.Contains("COUNT")));
          int result2;
          if (str1 != null && int.TryParse(str1?.Replace("COUNT=", ""), out result2))
          {
            num1 = result2;
            spanStart = startDate;
          }
          string str2 = ((IEnumerable<string>) source).FirstOrDefault<string>((Func<string, bool>) (r => r.Contains("UNTIL")));
          if (str2 != null)
          {
            DateTime date = new DateTime();
            try
            {
              date = DateTime.ParseExact(str2?.Replace("UNTIL=", ""), "yyyyMMdd", (IFormatProvider) App.Ci);
            }
            catch (Exception ex)
            {
            }
            if (!Utils.IsEmptyDate(date))
            {
              DateTime time = date.AddDays(1.0);
              if (!string.IsNullOrEmpty(timeZone))
                time = TimeZoneUtils.ToLocalTime(time, timeZone);
              spanEnd = spanEnd.AddDays(1.0);
              spanEnd = spanEnd > time ? time : spanEnd;
            }
          }
          DateTime dateTime1 = startDate;
          if (result1 < 5)
          {
            switch (result1)
            {
              case 0:
                dateTime1 = startDate.AddDays(0.0);
                break;
              case 1:
                dateTime1 = startDate.AddDays(-1.0);
                break;
              case 2:
                dateTime1 = startDate.AddDays(-2.0);
                break;
              case 3:
                dateTime1 = startDate.AddDays(-4.0);
                break;
              case 4:
                dateTime1 = startDate.AddDays(-7.0);
                break;
            }
          }
          int num2 = 1;
          double totalDays = (spanStart - dateTime1).TotalDays;
          DateTime dateTime2 = dateTime1;
          if (totalDays < 15.0)
          {
            for (int index = result1; index < 5; ++index)
            {
              switch (index)
              {
                case 0:
                  dateTime2 = dateTime1.AddDays(0.0);
                  break;
                case 1:
                  dateTime2 = dateTime1.AddDays(1.0);
                  break;
                case 2:
                  dateTime2 = dateTime1.AddDays(2.0);
                  break;
                case 3:
                  dateTime2 = dateTime1.AddDays(4.0);
                  break;
                case 4:
                  dateTime2 = dateTime1.AddDays(7.0);
                  break;
              }
              if (!(dateTime2 < spanEnd) || num2 >= num1)
                return ebbinghausRepeatDates;
              if (dateTime2 >= spanStart && !exDate.Contains(dateTime2.ToString("yyyyMMdd")) && dateTime2 != startDate)
              {
                ebbinghausRepeatDates.Add(dateTime2);
                ++num2;
              }
            }
          }
          for (dateTime2 = dateTime1.AddDays((double) ((int) (totalDays / 15.0) * 15)); dateTime2 < spanEnd && num2 < num1; dateTime2 = dateTime2.AddDays(15.0))
          {
            if (dateTime2 >= spanStart && !exDate.Contains(dateTime2.ToString("yyyyMMdd")) && dateTime2 != startDate)
            {
              ebbinghausRepeatDates.Add(dateTime2);
              ++num2;
            }
          }
        }
      }
      return ebbinghausRepeatDates;
    }

    private static List<DateTime> GetValidWorkingDays(
      string rrule,
      DateTime spanStart,
      DateTime spanEnd,
      DateTime startDate)
    {
      try
      {
        RecurrenceModel pattern = RecurrenceModel.GetRecurrenceModel(rrule);
        if (!Utils.IsEmptyDate(spanStart) && !Utils.IsEmptyDate(startDate) && spanStart < startDate)
          spanStart = startDate;
        List<DateTime> repeatDates = rrule.Contains("TT_WORKDAY=1") ? RepeatUtils.GetValidFirstWorkingDays(spanStart, spanEnd, pattern.Interval) : RepeatUtils.GetValidLastWorkingDays(spanStart, spanEnd, pattern.Interval);
        if (pattern.Count >= 1)
          repeatDates = repeatDates.Where<DateTime>((Func<DateTime, bool>) (date => repeatDates.IndexOf(date) < pattern.Count - 1)).ToList<DateTime>();
        if (!Utils.IsEmptyDate(pattern.Until))
          repeatDates = repeatDates.Where<DateTime>((Func<DateTime, bool>) (date =>
          {
            DateTime dateTime1 = date;
            DateTime dateTime2 = pattern.Until;
            dateTime2 = dateTime2.Date;
            DateTime dateTime3 = dateTime2.AddDays(1.0);
            return dateTime1 < dateTime3;
          })).ToList<DateTime>();
        return repeatDates;
      }
      catch (Exception ex)
      {
        return new List<DateTime>();
      }
    }

    private static List<DateTime> GetValidFirstWorkingDays(
      DateTime startDate,
      DateTime spanEnd,
      int interval)
    {
      List<DateTime> firstWorkingDays = new List<DateTime>();
      spanEnd = Utils.IsEmptyDate(spanEnd) ? startDate.AddMonths(interval + 1) : spanEnd;
      int num = (spanEnd.Year - startDate.Year) * 12 + (spanEnd.Month - startDate.Month);
      for (int months = 0; months < num + 1; months += interval)
      {
        DateTime dateTime1 = startDate.AddDays((double) (1 - startDate.Day));
        DateTime monthlyFirstWorkday = RepeatUtils.GetMonthlyFirstWorkday(new DateTime?(dateTime1.AddMonths(months)));
        if (monthlyFirstWorkday <= spanEnd && monthlyFirstWorkday.Date > startDate.Date)
        {
          dateTime1 = monthlyFirstWorkday.AddHours((double) startDate.Hour);
          DateTime dateTime2 = dateTime1.AddMinutes((double) startDate.Minute);
          firstWorkingDays.Add(dateTime2);
        }
      }
      return firstWorkingDays;
    }

    private static List<DateTime> GetValidLastWorkingDays(
      DateTime startDate,
      DateTime spanEnd,
      int interval)
    {
      List<DateTime> validLastWorkingDays = new List<DateTime>();
      spanEnd = Utils.IsEmptyDate(spanEnd) ? startDate.AddMonths(interval + 1) : spanEnd;
      int num = (spanEnd.Year - startDate.Year) * 12 + (spanEnd.Month - startDate.Month);
      for (int months = 0; months < num + 1; months += interval)
      {
        DateTime dateTime1 = startDate.AddDays((double) (1 - startDate.Day));
        DateTime monthlyLastWorkday = RepeatUtils.GetMonthlyLastWorkday(new DateTime?(dateTime1.AddMonths(months)));
        if (monthlyLastWorkday <= spanEnd && monthlyLastWorkday > startDate)
        {
          dateTime1 = monthlyLastWorkday.AddHours((double) startDate.Hour);
          DateTime dateTime2 = dateTime1.AddMinutes((double) startDate.Minute);
          validLastWorkingDays.Add(dateTime2);
        }
      }
      return validLastWorkingDays;
    }

    public static bool IsMonthlyWorkday(string flag)
    {
      if (string.IsNullOrEmpty(flag))
        return false;
      return flag.Contains("TT_WORKDAY=1") || flag.Contains("TT_WORKDAY=-1");
    }

    private static bool IsOccurenceNonOfficalWorkDay(
      Occurrence occ,
      IEnumerable<HolidayModel> holidays)
    {
      DateTime startDate = occ.Period.StartTime.Date;
      List<HolidayModel> list = holidays.ToList<HolidayModel>();
      return list.FirstOrDefault<HolidayModel>((Func<HolidayModel, bool>) (day => day.date == startDate.Date && day.type == 0)) != null || list.FirstOrDefault<HolidayModel>((Func<HolidayModel, bool>) (day => day.date == startDate.Date && day.type == 1)) == null && (startDate.DayOfWeek < DayOfWeek.Monday || startDate.DayOfWeek > DayOfWeek.Friday);
    }

    private static bool IsOccurenceHoilday(Occurrence occ, IEnumerable<HolidayModel> holidays)
    {
      DateTime startDate = occ.Period.StartTime.Date;
      return holidays.FirstOrDefault<HolidayModel>((Func<HolidayModel, bool>) (day => day.date == startDate.Date && day.type == 0)) != null;
    }

    private static bool IsOccurenceWeekend(Occurrence occ)
    {
      return occ.Period.StartTime.DayOfWeek == DayOfWeek.Saturday || occ.Period.StartTime.DayOfWeek == DayOfWeek.Sunday;
    }

    private static bool IsOccurrenceEqualStartDate(Occurrence occ, DateTime startDate)
    {
      return occ.Period.StartTime.Year == startDate.Date.Year && occ.Period.StartTime.Month == startDate.Date.Month && occ.Period.StartTime.Day == startDate.Date.Day;
    }

    private static RecurrencePattern GetRecurrencePattern(string flag, int count = -1)
    {
      RecurrenceModel recurrenceModel = RecurrenceModel.GetRecurrenceModel(flag);
      recurrenceModel.Until = new DateTime();
      recurrenceModel.Count = count > 0 ? count : int.MinValue;
      return (RecurrencePattern) recurrenceModel;
    }

    public static string GetRepeatFlag(string flag, DateTime until, int count = -1)
    {
      bool flag1 = flag.ToUpper().Contains("HOLIDAY");
      bool flag2 = flag.ToUpper().Contains("WEEKEND");
      bool flag3 = flag.ToUpper().Contains("TT_WORKDAY");
      int num = flag.ToUpper().Contains("LUNAR") ? 1 : 0;
      string str1 = string.Empty;
      if (!Utils.IsEmptyDate(until))
        str1 = ";UNTIL=" + until.ToString("yyyyMMdd");
      if (num != 0)
        return "LUNAR:" + RepeatUtils.GetRecurrencePattern(flag, count)?.ToString() + str1;
      if (flag.ToUpper().Contains("FORGETTINGCURVE"))
      {
        List<string> list = ((IEnumerable<string>) flag.Split(';')).ToList<string>();
        list.Remove("");
        string str2 = list.FirstOrDefault<string>((Func<string, bool>) (r => r.Contains("UNTIL")));
        if (str2 != null)
          list.Remove(str2);
        string str3 = list.FirstOrDefault<string>((Func<string, bool>) (r => r.Contains("COUNT")));
        if (str3 != null)
          list.Remove(str3);
        if (count > 0)
          list.Add("COUNT=" + count.ToString());
        return string.Join(";", (IEnumerable<string>) list) + str1;
      }
      if (flag3)
      {
        string str4 = "RRULE:" + RepeatUtils.GetRecurrencePattern(flag, count)?.ToString();
        if (!str4.ToUpper().Contains("INTERVAL"))
          str4 += ";INTERVAL=1";
        string str5 = flag;
        char[] chArray = new char[1]{ ';' };
        foreach (string str6 in str5.Split(chArray))
        {
          if (str6.Contains("TT_WORKDAY"))
            str4 = str4 + ";" + str6;
        }
        return str4 + str1;
      }
      if (!(flag2 | flag1))
        return "RRULE:" + RepeatUtils.GetRecurrencePattern(flag, count)?.ToString() + str1;
      string str7 = string.Empty;
      if (flag2 & flag1)
        str7 = ";TT_SKIP=HOLIDAY,WEEKEND";
      if (flag2 && !flag1)
        str7 = ";TT_SKIP=WEEKEND";
      if (!flag2)
        str7 = ";TT_SKIP=HOLIDAY";
      return "RRULE:" + RepeatUtils.GetRecurrencePattern(flag, count)?.ToString() + str7 + str1;
    }

    public static DateTime? GetRepeatStartDate(DateTime? repeatStartDate)
    {
      if (!Utils.IsEmptyDate(repeatStartDate))
      {
        if (repeatStartDate.HasValue)
        {
          ref DateTime? local = ref repeatStartDate;
          int year = DateTime.Now.Year;
          int month = DateTime.Now.Month;
          int day = DateTime.Now.Day;
          DateTime dateTime1 = repeatStartDate.Value;
          int hour = dateTime1.Hour;
          dateTime1 = repeatStartDate.Value;
          int minute = dateTime1.Minute;
          DateTime dateTime2 = new DateTime(year, month, day, hour, minute, 0);
          local = new DateTime?(dateTime2);
        }
      }
      else
        repeatStartDate = new DateTime?(DateTime.Now);
      return repeatStartDate;
    }

    public static DateTime GetNextRepeatDate(TaskModel task, ref int addForgotCycle)
    {
      List<string> stringList = new List<string>();
      if (!string.IsNullOrEmpty(task.exDates))
        stringList = ((IEnumerable<string>) ExDateSerilizer.ToArray(task.exDates)).ToList<string>();
      string timeZone = task.isAllDay.HasValue && task.isAllDay.Value || task.Floating || !(task.timeZone != TimeZoneData.LocalTimeZoneModel?.TimeZoneName) ? (string) null : task.timeZone;
      DateTime? date1 = task.startDate;
      if (date1.HasValue)
        date1 = new DateTime?(TimeZoneUtils.LocalToTargetTzTime(date1.Value, timeZone));
      DateTime date2 = RepeatUtils.RRule2NextDateTime(task.repeatFlag, HolidayManager.GetCacheHolidays(), date1, task.repeatFrom == "1" ? new DateTime?(DateTime.Now) : new DateTime?(), task.repeatFrom, stringList, timeZone);
      if (date1.HasValue)
      {
        int result;
        List<int> list = stringList.Select<string, int>((Func<string, int>) (ex => !int.TryParse(ex, out result) ? 0 : result)).ToList<int>();
        int startDateNum = DateUtils.GetDateNum(date1.Value);
        int nextDateNum = DateUtils.GetDateNum(date2);
        addForgotCycle += list.Count<int>((Func<int, bool>) (num => num > startDateNum && num < nextDateNum && num != 0));
      }
      return date2;
    }

    public static DateTime RRule2NextDateTime(
      string rrule,
      List<HolidayModel> holidays,
      DateTime? date = null,
      DateTime? completeDate = null,
      string repeatFrom = "",
      List<string> exDate = null,
      string timeZone = null)
    {
      try
      {
        DateTime repeatNextDate = RepeatCalculator.GetRepeatNextDate(rrule, repeatFrom, date ?? DateTime.Now, completeDate, exDate, timeZone);
        if (Utils.IsEmptyDate(repeatNextDate) && RepeatUtils.IsValidRepeatRule(rrule))
          UtilLog.Info("RRule2NextDateTime  " + string.Format("NextRepeatDate is Empty {0},{1},{2},{3},{4},{5},{6}", (object) rrule, (object) repeatFrom, (object) date, (object) completeDate, (object) repeatFrom, exDate == null ? (object) "" : (object) JsonConvert.SerializeObject((object) exDate), (object) timeZone));
        return repeatNextDate;
      }
      catch (Exception ex)
      {
      }
      try
      {
        List<string> source = exDate;
        exDate = source != null ? source.Select<string, string>((Func<string, string>) (text => text.Replace("\"", ""))).ToList<string>() : (List<string>) null;
        if (rrule.Contains("LUNAR"))
        {
          try
          {
            return RepeatUtils.GetValidLunarRepeatDate(rrule, date);
          }
          catch (Exception ex)
          {
            return new DateTime();
          }
        }
        else if (rrule.Contains("FORGETTINGCURVE"))
        {
          try
          {
            return RepeatUtils.GetNextEbbinghausRepeatDate(rrule, date, exDate, timeZone);
          }
          catch (Exception ex)
          {
            return new DateTime();
          }
        }
        else
        {
          if (RepeatUtils.IsMonthlyWorkday(rrule))
          {
            DateTime startDate = date ?? DateTime.Now;
            return RepeatUtils.GetValidWorkingDays(rrule, startDate, new DateTime(), new DateTime()).FirstOrDefault<DateTime>((Func<DateTime, bool>) (repeatDate => repeatDate >= startDate));
          }
          if (rrule == "RRULE:FREQ=DAILY;INTERVAL=1;TT_SKIP=HOLIDAY,WEEKEND")
            return RepeatUtils.GetNextValidWorkingDay(date, holidays);
          if (rrule.Contains("FREQ=NONE") || rrule.Contains("FREQ = NONE"))
            return new DateTime();
          RecurringComponent recurringComponent1 = new RecurringComponent();
          RecurrenceModel recurrenceModel1 = RecurrenceModel.GetRecurrenceModel(rrule);
          int count = recurrenceModel1.Count;
          if (recurrenceModel1.Count == 1)
            return new DateTime();
          recurrenceModel1.Count = int.MinValue;
          DateTime? nullable1 = new DateTime?();
          DateTime dateTime1 = new DateTime();
          try
          {
            DateTime dateTime2;
            if (!Utils.IsEmptyDate(recurrenceModel1.Until))
            {
              nullable1 = string.IsNullOrEmpty(timeZone) ? new DateTime?(recurrenceModel1.Until.AddDays(1.0)) : new DateTime?(TimeZoneUtils.ToLocalTime(recurrenceModel1.Until.AddDays(1.0), timeZone));
              RecurrenceModel recurrenceModel2 = recurrenceModel1;
              dateTime2 = recurrenceModel1.Until;
              DateTime dateTime3 = dateTime2.AddDays(2.0);
              recurrenceModel2.Until = dateTime3;
            }
            if (!Utils.IsEmptyDate(date) && date.HasValue)
            {
              switch (recurrenceModel1.Frequency)
              {
                case FrequencyType.Weekly:
                  if (recurrenceModel1.ByDay.Count == 0)
                  {
                    List<WeekDay> byDay = recurrenceModel1.ByDay;
                    dateTime2 = date.Value;
                    WeekDay weekDay = new WeekDay(dateTime2.DayOfWeek);
                    byDay.Add(weekDay);
                    break;
                  }
                  break;
                case FrequencyType.Monthly:
                  if (recurrenceModel1.ByMonthDay.Count == 0 && recurrenceModel1.ByDay.Count == 0)
                  {
                    List<int> byMonthDay = recurrenceModel1.ByMonthDay;
                    dateTime2 = date.Value;
                    int day = dateTime2.Day;
                    byMonthDay.Add(day);
                  }
                  if (recurrenceModel1.ByMonthDay.Count == 1 && (recurrenceModel1.ByMonthDay[0] == 29 || recurrenceModel1.ByMonthDay[0] == 30 || recurrenceModel1.ByMonthDay[0] == 31))
                  {
                    if (date.Value < DateTime.Today)
                      date = new DateTime?(DateTime.Today);
                    DateTime spanStart = date.Value;
                    dateTime2 = date.Value;
                    DateTime spanEnd = dateTime2.AddMonths(2);
                    DateTime startDate = date.Value;
                    RecurrenceModel pattern = recurrenceModel1;
                    string rule = rrule;
                    List<HolidayModel> holidays1 = holidays;
                    DateTime dateTime4 = RepeatUtils.GetCreditCardDate(spanStart, spanEnd, startDate, (RecurrencePattern) pattern, 3, rule, holidays1).First<DateTime>();
                    if (nullable1.HasValue)
                    {
                      dateTime2 = dateTime4;
                      DateTime? nullable2 = nullable1;
                      if ((nullable2.HasValue ? (dateTime2 > nullable2.GetValueOrDefault() ? 1 : 0) : 0) != 0)
                        dateTime4 = new DateTime();
                    }
                    return dateTime4;
                  }
                  break;
                case FrequencyType.Yearly:
                  if (recurrenceModel1.ByMonthDay.Count == 0 && recurrenceModel1.ByMonth.Count == 0)
                  {
                    List<int> byMonth = recurrenceModel1.ByMonth;
                    dateTime2 = date.Value;
                    int month = dateTime2.Month;
                    byMonth.Add(month);
                    List<int> byMonthDay = recurrenceModel1.ByMonthDay;
                    dateTime2 = date.Value;
                    int day = dateTime2.Day;
                    byMonthDay.Add(day);
                    break;
                  }
                  break;
              }
              DateTime until = recurrenceModel1.Until;
              dateTime2 = new DateTime();
              DateTime dateTime5 = dateTime2;
              if (!(until == dateTime5))
              {
                dateTime2 = recurrenceModel1.Until;
                DateTime date1 = dateTime2.Date;
                dateTime2 = date.Value;
                DateTime date2 = dateTime2.Date;
                if (!(date1 > date2))
                  goto label_81;
              }
              recurringComponent1.RecurrenceRules.Add((RecurrencePattern) recurrenceModel1);
              dateTime2 = date.Value;
              DateTime dateTime6 = dateTime2.Date;
              if (repeatFrom == "2" && count <= 0)
                dateTime6 = DateTime.Today;
              recurringComponent1.Start = (IDateTime) new CalDateTime()
              {
                Value = dateTime6
              };
              int num = 0;
              HashSet<Occurrence> occurrences;
              DateTime date3;
              DateTime date4;
              do
              {
                do
                {
                  RecurringComponent recurringComponent2 = recurringComponent1;
                  dateTime2 = date.Value;
                  dateTime2 = dateTime2.AddMonths(2 * num);
                  DateTime startTime = dateTime2.AddDays(0.0);
                  dateTime2 = date.Value;
                  dateTime2 = dateTime2.AddMonths(2 * (num + 1));
                  DateTime endTime = dateTime2.AddDays(1.0);
                  occurrences = recurringComponent2.GetOccurrences(startTime, endTime);
                  if (rrule.ToUpper().Contains("HOLIDAY"))
                    occurrences.RemoveWhere((Predicate<Occurrence>) (occ => RepeatUtils.IsOccurenceHoilday(occ, (IEnumerable<HolidayModel>) holidays)));
                  if (exDate != null && exDate.Any<string>())
                    occurrences.RemoveWhere((Predicate<Occurrence>) (occ => RepeatUtils.IsOccurenceInExDates(occ, (ICollection<string>) exDate)));
                  if (rrule.ToUpper().Contains("WEEKEND"))
                    occurrences.RemoveWhere(new Predicate<Occurrence>(RepeatUtils.IsOccurenceWeekend));
                  ++num;
                }
                while (occurrences.Count == 0 && num < 30);
                if (occurrences.Count == 1)
                {
                  dateTime2 = occurrences.ToList<Occurrence>()[0].Period.StartTime.Value;
                  date3 = dateTime2.Date;
                  dateTime2 = date.Value;
                  date4 = dateTime2.Date;
                }
                else
                  break;
              }
              while (date3 == date4);
              if (occurrences.Count != 0)
              {
                foreach (Occurrence occurrence in occurrences)
                {
                  dateTime2 = occurrence.Period.StartTime.Value;
                  DateTime date5 = dateTime2.Date;
                  dateTime2 = date.Value;
                  DateTime date6 = dateTime2.Date;
                  if (date5 != date6)
                  {
                    dateTime2 = occurrence.Period.StartTime.Value;
                    int year = dateTime2.Year;
                    dateTime2 = occurrence.Period.StartTime.Value;
                    int month = dateTime2.Month;
                    dateTime2 = occurrence.Period.StartTime.Value;
                    int day = dateTime2.Day;
                    dateTime2 = date.Value;
                    int hour = dateTime2.Hour;
                    dateTime2 = date.Value;
                    int minute = dateTime2.Minute;
                    dateTime2 = date.Value;
                    int second = dateTime2.Second;
                    DateTime dateTime7 = new DateTime(year, month, day, hour, minute, second);
                    if (nullable1.HasValue)
                    {
                      dateTime2 = dateTime7;
                      DateTime? nullable3 = nullable1;
                      if ((nullable3.HasValue ? (dateTime2 >= nullable3.GetValueOrDefault() ? 1 : 0) : 0) != 0)
                        dateTime7 = new DateTime();
                    }
                    return dateTime7;
                  }
                }
              }
            }
            else
            {
              switch (recurrenceModel1.Frequency)
              {
                case FrequencyType.Weekly:
                  if (recurrenceModel1.ByDay.Count == 0)
                  {
                    List<WeekDay> byDay = recurrenceModel1.ByDay;
                    dateTime2 = DateTime.Now;
                    WeekDay weekDay = new WeekDay(dateTime2.DayOfWeek);
                    byDay.Add(weekDay);
                    break;
                  }
                  break;
                case FrequencyType.Monthly:
                  if (recurrenceModel1.ByMonthDay.Count == 0)
                  {
                    List<int> byMonthDay = recurrenceModel1.ByMonthDay;
                    dateTime2 = DateTime.Now;
                    int day = dateTime2.Day;
                    byMonthDay.Add(day);
                    break;
                  }
                  break;
                case FrequencyType.Yearly:
                  if (recurrenceModel1.ByMonthDay.Count == 0 && recurrenceModel1.ByMonth.Count == 0)
                  {
                    List<int> byMonth = recurrenceModel1.ByMonth;
                    dateTime2 = DateTime.Now;
                    int month = dateTime2.Month;
                    byMonth.Add(month);
                    List<int> byMonthDay = recurrenceModel1.ByMonthDay;
                    dateTime2 = DateTime.Now;
                    int day = dateTime2.Day;
                    byMonthDay.Add(day);
                    break;
                  }
                  break;
              }
              DateTime until = recurrenceModel1.Until;
              dateTime2 = new DateTime();
              DateTime dateTime8 = dateTime2;
              if (!(until == dateTime8))
              {
                dateTime2 = recurrenceModel1.Until;
                DateTime date7 = dateTime2.Date;
                dateTime2 = DateTime.Now;
                DateTime date8 = dateTime2.Date;
                if (!(date7 >= date8))
                  goto label_81;
              }
              recurringComponent1.RecurrenceRules.Add((RecurrencePattern) recurrenceModel1);
              recurringComponent1.Start = (IDateTime) new CalDateTime();
              int months = 0;
              HashSet<Occurrence> occurrences;
              do
              {
                RecurringComponent recurringComponent3 = recurringComponent1;
                dateTime2 = DateTime.Now;
                DateTime startTime = dateTime2.AddMonths(months);
                dateTime2 = DateTime.Now;
                DateTime endTime = dateTime2.AddMonths(months + 1);
                occurrences = recurringComponent3.GetOccurrences(startTime, endTime);
                ++months;
              }
              while (occurrences.Count == 0 && months < 60);
              if (occurrences.Count != 0)
              {
                using (HashSet<Occurrence>.Enumerator enumerator = occurrences.GetEnumerator())
                {
                  if (enumerator.MoveNext())
                  {
                    DateTime dateTime9 = enumerator.Current.Period.StartTime.Value;
                    if (nullable1.HasValue)
                    {
                      dateTime2 = dateTime9;
                      DateTime? nullable4 = nullable1;
                      if ((nullable4.HasValue ? (dateTime2 >= nullable4.GetValueOrDefault() ? 1 : 0) : 0) != 0)
                        dateTime9 = new DateTime();
                    }
                    return dateTime9;
                  }
                }
              }
            }
          }
          catch (Exception ex)
          {
          }
label_81:
          return new DateTime();
        }
      }
      catch (Exception ex)
      {
        return new DateTime();
      }
    }

    private static bool IsValidRepeatRule(string rrule)
    {
      return !Utils.IsEmptyRepeatFlag(rrule) && !rrule.Contains("Count=1") && (!rrule.Contains("Until") || RepeatUtils.RepeatOutDate(rrule));
    }

    private static DateTime GetNextEbbinghausRepeatDate(
      string rrule,
      DateTime? date,
      List<string> exDate,
      string timeZone = null)
    {
      string[] source = rrule.Split(';');
      if (exDate == null)
        exDate = new List<string>();
      DateTime startDate = date ?? DateTime.Today;
      int result1;
      if (!int.TryParse(((IEnumerable<string>) source).FirstOrDefault<string>((Func<string, bool>) (r => r.Contains("CYCLE")))?.Replace("CYCLE=", ""), out result1))
        return new DateTime();
      DateTime nextEbbinghausDate;
      for (nextEbbinghausDate = RepeatUtils.GetNextEbbinghausDate(startDate, result1); exDate.Contains(nextEbbinghausDate.ToString("yyyyMMdd")); nextEbbinghausDate = RepeatUtils.GetNextEbbinghausDate(nextEbbinghausDate, result1))
        ++result1;
      int result2;
      if (rrule.Contains("COUNT") && int.TryParse(((IEnumerable<string>) source).FirstOrDefault<string>((Func<string, bool>) (r => r.Contains("COUNT")))?.Replace("COUNT=", ""), out result2))
        return result2 <= 1 ? new DateTime() : nextEbbinghausDate;
      if (rrule.Contains("UNTIL"))
      {
        string str = ((IEnumerable<string>) source).FirstOrDefault<string>((Func<string, bool>) (r => r.Contains("UNTIL")));
        try
        {
          DateTime dateTime = DateTime.ParseExact(str?.Replace("UNTIL=", ""), "yyyyMMdd", (IFormatProvider) App.Ci);
          if (!Utils.IsEmptyDate(dateTime))
          {
            dateTime = dateTime.AddDays(1.0);
            if (!string.IsNullOrEmpty(timeZone))
              dateTime = TimeZoneUtils.ToLocalTime(dateTime, timeZone);
            if (dateTime < nextEbbinghausDate)
              return new DateTime();
          }
        }
        catch (Exception ex)
        {
        }
      }
      return nextEbbinghausDate;
    }

    private static DateTime GetNextEbbinghausDate(DateTime startDate, int cycle)
    {
      switch (cycle)
      {
        case 0:
          return startDate.AddDays(1.0);
        case 1:
          return startDate.AddDays(1.0);
        case 2:
          return startDate.AddDays(2.0);
        case 3:
          return startDate.AddDays(3.0);
        case 4:
          return startDate.AddDays(8.0);
        default:
          return startDate.AddDays(15.0);
      }
    }

    private static bool IsOccurenceInExDates(Occurrence occ, ICollection<string> exDate)
    {
      string str = occ.Period.StartTime.Date.ToString("yyyyMMdd");
      return exDate.Contains(str);
    }

    private static DateTime GetMonthlyFirstWorkday(DateTime? date)
    {
      if (!date.HasValue)
        date = new DateTime?(DateTime.Today);
      return RepeatUtils.GetFirstWorkdayInMonth(date.Value.Year, date.Value.Month);
    }

    private static DateTime GetFirstWorkdayInMonth(int year, int month)
    {
      DateTime dateTime = new DateTime(year, month, 1);
      for (int index = 0; index < 31; ++index)
      {
        DateTime date = dateTime.AddDays((double) index);
        if (HolidayManager.IsWorkDay(date))
          return date;
      }
      return new DateTime();
    }

    private static DateTime GetMonthlyLastWorkday(DateTime? date)
    {
      if (!date.HasValue)
        date = new DateTime?(DateTime.Today);
      return RepeatUtils.GetLastWorkdayInMonth(date.Value.Year, date.Value.Month);
    }

    private static DateTime GetLastWorkdayInMonth(int year, int month)
    {
      DateTime dateTime1 = new DateTime(year, month, 1);
      dateTime1 = dateTime1.AddMonths(1);
      DateTime dateTime2 = dateTime1.AddDays(-1.0);
      for (int index = 0; index < 31; ++index)
      {
        DateTime date = dateTime2.AddDays((double) (index * -1));
        if (HolidayManager.IsWorkDay(date))
          return date;
      }
      return new DateTime();
    }

    private static DateTime GetNextValidWorkingDay(DateTime? date, List<HolidayModel> holidays)
    {
      DateTime nextValidWorkingDay = date ?? DateTime.Today;
      if (nextValidWorkingDay < DateTime.Today)
        nextValidWorkingDay = DateTime.Today.AddDays(-1.0);
      if (date.HasValue)
      {
        DateTime dateTime1 = nextValidWorkingDay;
        DateTime dateTime2 = date.Value;
        int hour = dateTime2.Hour;
        dateTime2 = date.Value;
        int minute = dateTime2.Minute;
        nextValidWorkingDay = DateUtils.SetHourAndMinuteOnly(dateTime1, hour, minute);
      }
      for (int index = 1; index < 20; ++index)
      {
        DateTime checkDay = nextValidWorkingDay.AddDays((double) index);
        if (RepeatUtils.IsWorkingDay(checkDay, holidays))
          return checkDay;
      }
      return nextValidWorkingDay;
    }

    private static bool IsWorkingDay(DateTime checkDay, List<HolidayModel> holidays)
    {
      HolidayModel holidayModel1 = holidays.FirstOrDefault<HolidayModel>((Func<HolidayModel, bool>) (day => day.date == checkDay.Date && day.type == 1));
      HolidayModel holidayModel2 = holidays.FirstOrDefault<HolidayModel>((Func<HolidayModel, bool>) (day => day.date == checkDay.Date && day.type == 0));
      return holidayModel1 != null || holidayModel2 == null && checkDay.DayOfWeek != DayOfWeek.Saturday && checkDay.DayOfWeek != DayOfWeek.Sunday;
    }

    private static DateTime GetValidLunarRepeatDate(string rrule, DateTime? date)
    {
      DateTime dateTime = date ?? DateTime.Now;
      for (int index = 0; index < 100; ++index)
      {
        DateTime validLunarRepeatDate;
        try
        {
          validLunarRepeatDate = RepeatUtils.GetLunarRepeatDate(rrule, new DateTime?(dateTime.Date.AddYears(index)));
        }
        catch (Exception ex)
        {
          validLunarRepeatDate = dateTime;
        }
        if (validLunarRepeatDate.Date > DateTime.Now)
          return validLunarRepeatDate;
      }
      return DateTime.Now;
    }

    private static List<DateTime> GetValidLunarRepeatDates(
      string rrule,
      DateTime date,
      DateTime endDate,
      List<string> exDate)
    {
      List<DateTime> lunarRepeatDates = new List<DateTime>();
      for (int index = 0; index < 20; ++index)
      {
        DateTime dateTime;
        try
        {
          dateTime = RepeatUtils.GetValidLunarRepeatDate(rrule, new DateTime?(date));
        }
        catch (Exception ex)
        {
          dateTime = date;
        }
        if (!(dateTime > endDate))
        {
          if (exDate == null)
            exDate = new List<string>();
          if (!exDate.Contains(dateTime.ToString("yyyyMMdd")))
            lunarRepeatDates.Add(dateTime);
          date = dateTime;
        }
        else
          break;
      }
      return lunarRepeatDates;
    }

    private static DateTime GetLunarRepeatDate(string rrule, DateTime? date)
    {
      ChineseLunisolarCalendar lunisolarCalendar1 = new ChineseLunisolarCalendar();
      int year1 = !date.HasValue || date.Value.Year == 1 ? DateTime.Now.Year + 1 : lunisolarCalendar1.GetYear(date.Value) + 1;
      int month1 = int.Parse(rrule.Substring(rrule.IndexOf("BYMONTH") + 8, rrule.IndexOf(";", rrule.IndexOf("BYMONTH")) - (rrule.IndexOf("BYMONTH") + 8)));
      int day1 = int.Parse(rrule.Substring(rrule.IndexOf("BYMONTHDAY") + 11).Split(';')[0]);
      int leapMonth = lunisolarCalendar1.GetLeapMonth(year1);
      if (leapMonth - 1 > -1 && month1 > leapMonth - 1)
        ++month1;
      if (month1 == 12 && day1 == 30)
        day1 = 29;
      DateTime dateTime1;
      if (date.HasValue && date.Value.Year != 1)
      {
        ChineseLunisolarCalendar lunisolarCalendar2 = lunisolarCalendar1;
        int year2 = year1;
        int month2 = month1;
        int day2 = day1 <= lunisolarCalendar1.GetDaysInMonth(year1, month1) ? day1 : lunisolarCalendar1.GetDaysInMonth(year1, month1);
        int hour = date.Value.Hour;
        int minute = date.Value.Minute;
        DateTime dateTime2 = date.Value;
        int second = dateTime2.Second;
        dateTime2 = date.Value;
        int millisecond = dateTime2.Millisecond;
        dateTime1 = lunisolarCalendar2.ToDateTime(year2, month2, day2, hour, minute, second, millisecond);
      }
      else
        dateTime1 = lunisolarCalendar1.ToDateTime(year1, month1, day1, 0, 0, 0, 0);
      return dateTime1;
    }

    public static string GetLunarRepeatFlag(DateTime date)
    {
      ChineseLunisolarCalendar lunisolarCalendar = new ChineseLunisolarCalendar();
      int year = lunisolarCalendar.GetYear(date);
      int month = lunisolarCalendar.GetMonth(date);
      int dayOfMonth = lunisolarCalendar.GetDayOfMonth(date);
      int leapMonth = lunisolarCalendar.GetLeapMonth(year);
      if (leapMonth > 0 && month >= leapMonth)
        --month;
      return string.Format("LUNAR:FREQ=YEARLY;INTERVAL=1;BYMONTH={0};BYMONTHDAY={1}", (object) month, (object) dayOfMonth);
    }

    public static string GetEbbinghausRepeatFlag() => "ERULE:NAME=FORGETTINGCURVE;CYCLE=0";

    public static int GetRepeatCount(string erule)
    {
      if (erule.Contains("COUNT"))
      {
        int result;
        if (int.TryParse(((IEnumerable<string>) erule.Split(';')).FirstOrDefault<string>((Func<string, bool>) (r => r.Contains("COUNT")))?.Replace("COUNT=", ""), out result))
          return result;
      }
      return -1;
    }

    public static string GetNextEbbinghausCycle(string erule, int count = 1)
    {
      if (erule.Contains("FORGETTINGCURVE"))
      {
        List<string> list = ((IEnumerable<string>) erule.Split(';')).ToList<string>();
        string oldValue = list.FirstOrDefault<string>((Func<string, bool>) (r => r.Contains("CYCLE")));
        int result;
        if (oldValue != null && int.TryParse(oldValue.Replace("CYCLE=", ""), out result))
        {
          list.Remove("");
          return erule.Replace(oldValue, "CYCLE=" + (result + count).ToString());
        }
      }
      return erule;
    }

    public static bool IsNonRepeatTask(
      DateTime? startDate,
      bool? isAllDay,
      string rFlag,
      string rFrom,
      string timeZone,
      bool isFloating,
      List<string> exDates)
    {
      if (!startDate.HasValue || string.IsNullOrEmpty(rFlag))
        return true;
      timeZone = isAllDay.HasValue && isAllDay.Value || isFloating || !(timeZone != TimeZoneData.LocalTimeZoneModel.TimeZoneName) ? (string) null : timeZone;
      startDate = new DateTime?(TimeZoneUtils.LocalToTargetTzTime(startDate.Value, timeZone));
      return Utils.IsEmptyDate(RepeatUtils.RRule2NextDateTime(rFlag, new List<HolidayModel>(), startDate, rFrom == "1" ? new DateTime?(DateTime.Now) : new DateTime?(), rFrom, exDates, timeZone));
    }

    public static RepeatFromType GetRepeatType(string repeatFrom, string repeatFlag)
    {
      if (!string.IsNullOrEmpty(repeatFlag) && repeatFlag.StartsWith("ERULE") && repeatFlag.Contains("NAME=CUSTOM"))
        return RepeatFromType.Custom;
      return repeatFrom == "1" ? RepeatFromType.CompleteTime : RepeatFromType.Duedate;
    }
  }
}
