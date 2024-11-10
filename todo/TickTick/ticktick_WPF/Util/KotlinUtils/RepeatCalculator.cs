// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.KotlinUtils.RepeatCalculator
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using KotlinModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using TickTickHandler;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Util.KotlinUtils
{
  public static class RepeatCalculator
  {
    public static Dictionary<string, RepeatModel> repeatCache = new Dictionary<string, RepeatModel>();
    public static Dictionary<string, System.DateTime> nextDateCatch = new Dictionary<string, System.DateTime>();

    public static List<System.DateTime> GetRepeatDates(
      string rrule,
      string repeatFrom,
      System.DateTime startDate,
      System.DateTime spanStart,
      System.DateTime spanEnd,
      List<string> exDates = null,
      string timeZone = null,
      bool toLocalTime = true)
    {
      if (!TTCalendarDllInitializer.CheckInitialized() || repeatFrom == "1" || string.IsNullOrEmpty(rrule))
        return new List<System.DateTime>();
      List<HolidayModel> holidayModelList = ticktick_WPF.Util.Utils.IsDida() ? HolidayManager.GetCacheHolidays() : new List<HolidayModel>();
      spanStart = spanStart.Date;
      spanEnd = spanEnd.Date;
      string key = new StringBuilder(rrule).Append(repeatFrom).Append(startDate.ToString("yyyyMMddHHmmss")).Append(exDates == null ? string.Empty : JsonConvert.SerializeObject((object) exDates)).Append(timeZone).Append((object) holidayModelList?.Count).ToString();
      System.DateTime spanStart1 = spanStart;
      System.DateTime spanEnd1 = spanEnd;
      if (RepeatCalculator.repeatCache.ContainsKey(key))
      {
        RepeatModel repeatModel = RepeatCalculator.repeatCache[key];
        (System.DateTime, System.DateTime) notExistRange = repeatModel.GetNotExistRange(spanStart, spanEnd);
        if (notExistRange.Item1 == notExistRange.Item2)
          return repeatModel.GetRepeatDates(spanStart, spanEnd);
        spanStart = notExistRange.Item1;
        spanEnd = notExistRange.Item2;
      }
      if (spanStart > System.DateTime.Today.AddYears(-1) && spanEnd < System.DateTime.Today.AddYears(1))
      {
        spanStart = System.DateTime.Today.AddYears(-1);
        spanEnd = System.DateTime.Today.AddYears(1);
      }
      List<TTCalendar> ttCalendarList1 = new List<TTCalendar>();
      if (exDates != null)
      {
        exDates = exDates.Select<string, string>((Func<string, string>) (text => text.Replace("\"", ""))).ToList<string>();
        foreach (string exDate in exDates)
        {
          System.DateTime result;
          if (System.DateTime.TryParseExact(exDate, "yyyyMMdd", (IFormatProvider) CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
          {
            TTCalendar ttCalendar = ModelsDao.NewTTCalendar(result, (string) null);
            ttCalendarList1.Add(ttCalendar);
          }
        }
      }
      List<TTCalendar> except = new List<TTCalendar>();
      for (int index = 0; index < ttCalendarList1.Count; ++index)
        except.Add(ttCalendarList1[index]);
      List<TTCalendar> ttCalendarList2 = new List<TTCalendar>();
      List<System.DateTime> dates = new List<System.DateTime>();
      TTCalendar start = ModelsDao.NewTTCalendar(startDate, timeZone);
      TTCalendar spanStart2 = ModelsDao.NewTTCalendar(spanStart, timeZone);
      TTCalendar spanEnd2 = ModelsDao.NewTTCalendar(spanEnd, timeZone);
      TTCalendar completeStamp = new TTCalendar();
      timeZone = string.IsNullOrEmpty(timeZone) ? TimeZoneData.LocalTimeZoneModel.TimeZoneName : timeZone;
      foreach (TTCalendar repeat in RepeatCalculator.GetRepeats(except, start, completeStamp, timeZone, rrule, repeatFrom, spanStart2, spanEnd2, 0, 0))
      {
        if (repeat.year > 0)
        {
          System.DateTime time = ModelsDao.TTCalendarToDateTime(repeat);
          if (!(time.Date <= startDate.Date))
          {
            if (toLocalTime)
              time = TimeZoneUtils.ToLocalTime(time, repeat.zone);
            dates.Add(time);
          }
        }
      }
      RepeatModel repeatModel1;
      if (RepeatCalculator.repeatCache.ContainsKey(key))
      {
        repeatModel1 = RepeatCalculator.repeatCache[key];
      }
      else
      {
        repeatModel1 = new RepeatModel();
        RepeatCalculator.repeatCache[key] = repeatModel1;
      }
      repeatModel1.AddRange(spanStart, spanEnd, dates);
      return repeatModel1.GetRepeatDates(spanStart1, spanEnd1);
    }

    public static System.DateTime GetRepeatNextDate(
      string rrule,
      string repeatFrom,
      System.DateTime startDate,
      System.DateTime? completeTime,
      List<string> exDates = null,
      string timeZone = null)
    {
      if (!TTCalendarDllInitializer.CheckInitialized())
      {
        UtilLog.Info(string.Format("GetRepeatNextDate UnInitialized {0}  {1}  {2} {3} {4}", (object) rrule, (object) repeatFrom, (object) completeTime, (object) exDates?.Count, (object) timeZone));
        return startDate;
      }
      StringBuilder stringBuilder = new StringBuilder(rrule).Append(repeatFrom).Append(startDate.ToString("yyyyMMddHHmm"));
      System.DateTime repeatNextDate;
      string str;
      if (!completeTime.HasValue)
      {
        str = (string) null;
      }
      else
      {
        repeatNextDate = completeTime.GetValueOrDefault();
        str = repeatNextDate.ToString("yyyyMMddHHmm");
      }
      string key = stringBuilder.Append(str).Append(exDates == null ? string.Empty : JsonConvert.SerializeObject((object) exDates)).Append(timeZone).ToString();
      if (RepeatCalculator.nextDateCatch.ContainsKey(key))
        return RepeatCalculator.nextDateCatch[key];
      List<TTCalendar> source = new List<TTCalendar>();
      if (exDates != null)
      {
        exDates = exDates.Select<string, string>((Func<string, string>) (text => text.Replace("\"", ""))).ToList<string>();
        foreach (string exDate in exDates)
        {
          System.DateTime result;
          if (System.DateTime.TryParseExact(exDate, "yyyyMMdd", (IFormatProvider) CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
          {
            TTCalendar ttCalendar = ModelsDao.NewTTCalendar(result, (string) null);
            source.Add(ttCalendar);
          }
        }
      }
      List<TTCalendar> list = source.ToList<TTCalendar>();
      TTCalendar ttCalendar1 = ModelsDao.NewTTCalendar(startDate, timeZone);
      TTCalendar ttCalendar2 = !completeTime.HasValue ? new TTCalendar() : ModelsDao.NewTTCalendar(completeTime.Value, timeZone);
      TTCalendar start = ttCalendar1;
      TTCalendar completeStamp = ttCalendar2;
      string timeZone1 = timeZone;
      string rrule1 = rrule;
      string repeatFrom1 = repeatFrom;
      TTCalendar spanStart = new TTCalendar();
      TTCalendar spanEnd = new TTCalendar();
      foreach (TTCalendar repeat in RepeatCalculator.GetRepeats(list, start, completeStamp, timeZone1, rrule1, repeatFrom1, spanStart, spanEnd, 1, 1))
      {
        if (repeat.year > 0)
        {
          System.DateTime localTime = TimeZoneUtils.ToLocalTime(ModelsDao.TTCalendarToDateTime(repeat), repeat.zone);
          if (ticktick_WPF.Util.Utils.IsEmptyDate(localTime) && !string.IsNullOrEmpty(rrule))
            UtilLog.Info(string.Format("GetRepeatNextDate is Empty {0}  {1}  {2} {3} {4}", (object) rrule, (object) repeatFrom, (object) completeTime, (object) exDates?.Count, (object) timeZone));
          RepeatCalculator.nextDateCatch[key] = localTime;
          repeatNextDate = localTime;
          return repeatNextDate;
        }
      }
      if (!string.IsNullOrEmpty(rrule))
        UtilLog.Info(string.Format("GetRepeatNextDate is Empty {0}  {1}  {2} {3} {4}", (object) rrule, (object) repeatFrom, (object) completeTime, (object) exDates?.Count, (object) timeZone));
      RepeatCalculator.nextDateCatch[key] = new System.DateTime();
      return new System.DateTime();
    }

    private static List<TTCalendar> GetRepeats(
      List<TTCalendar> except,
      TTCalendar start,
      TTCalendar completeStamp,
      string timeZone,
      string rrule,
      string repeatFrom,
      TTCalendar spanStart,
      TTCalendar spanEnd,
      int limit,
      int skip)
    {
      rrule = RRuleUtils.HandleUntilText(rrule, timeZone);
      return UtilRun.InvokeOnUI<List<TTCalendar>>(string.Format("GetRepeats except[{0}], start={1}, completeStamp={2}, timeZone={3}, rrule={4}, repeatFrom={5}", (object) except?.Count, (object) start, (object) completeStamp, (object) timeZone, (object) rrule, (object) repeatFrom), (Func<List<TTCalendar>>) (() =>
      {
        List<TTCalendar> except1 = except;
        List<TTCalendar> ttCalendarList = except;
        // ISSUE: explicit non-virtual call
        int count = ttCalendarList != null ? __nonvirtual (ttCalendarList.Count) : 0;
        TTCalendar start1 = start;
        TTCalendar spanstart = spanStart;
        TTCalendar spanend = spanEnd;
        TTCalendar compDate = completeStamp;
        string zone = timeZone;
        string flag = rrule;
        string from = repeatFrom;
        int limit1 = limit;
        int skip1 = skip;
        return RecurrenceCalcualte.GetRepeats(except1, count, start1, spanstart, spanend, compDate, zone, flag, from, limit1, skip1);
      }));
    }
  }
}
