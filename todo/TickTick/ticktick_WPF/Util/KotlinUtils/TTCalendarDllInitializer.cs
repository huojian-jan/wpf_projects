// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.KotlinUtils.TTCalendarDllInitializer
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using KotlinModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using TickTickHandler;
using TickTickModels;

#nullable disable
namespace ticktick_WPF.Util.KotlinUtils
{
  public static class TTCalendarDllInitializer
  {
    private static bool _inited;

    static TTCalendarDllInitializer()
    {
      TickTickHandler.Utils.ToastTimeCallback(new Action<TTCalendar>(TTCalendarDllInitializer.ToastTime));
      TickTickHandler.Utils.ToastCallback(new Action<string>(TTCalendarDllInitializer.Toast));
      TickTickHandler.DateTime.GetNowCallback(new Func<TTCalendar>(TTCalendarDllInitializer.GetNowFun));
      TickTickHandler.DateTime.GetNowByZoneCallback(new Func<string, TTCalendar>(TTCalendarDllInitializer.GetNowByZoneFun));
      TickTickHandler.DateTime.GetTimeInMillisCallback(new Func<TTCalendar, long>(TTCalendarDllInitializer.GetTimeInMillisFun));
      TickTickHandler.DateTime.GetTimeByMillisAndZoneCallback(new Func<long, string, TTCalendar>(TTCalendarDllInitializer.GetTimeByMillisAndZoneFun));
      TickTickHandler.DateTime.GetTimeCallback(new Func<int, int, int, int, int, int, int, string, TTCalendar>(TTCalendarDllInitializer.GetTimeFun));
      TickTickHandler.DateTime.GetDayOfWeekCallback(new Func<int, int, int, int, int, int, int, string, int>(TTCalendarDllInitializer.GetDayOfWeekFun));
      TickTickHandler.DateTime.GetLocalZoneCallback(new Func<string>(TTCalendarDllInitializer.GetLocalZoneFun));
      TickTickHandler.DateTime.GetZoneOffsetCallback(new Func<string, long, int>(TTCalendarDllInitializer.GetZoneOffsetFun));
      TickTickHandler.DateTime.GetIsWorkDayCallback(new Func<TTCalendar, int>(TTCalendarDllInitializer.IsWorkDay));
      TickTickHandler.DateTime.GetIsRestCallback(new Func<TTCalendar, int>(TTCalendarDllInitializer.IsRest));
      TickTickHandler.DateTime.GetWeekOfYearCallback(new Func<int, int, int, int, int, int, int, string, int>(TTCalendarDllInitializer.GetWeekOfYearFun));
      TickTickHandler.DateTime.GetFirstDayOfWeekCallback(new Func<int>(TTCalendarDllInitializer.GetFirstDayOfWeekFun));
      TickTickHandler.DateTime.SetDayOfWeekCallback(new Func<int, int, int, string, int, int, TTCalendar>(TTCalendarDllInitializer.SetDayOfWeekFun));
      TickTickHandler.DateTime.CurrentTimeMillisCallback(new Func<long>(TTCalendarDllInitializer.CurrentTimeMillisFun));
      TickTickHandler.DateTime.FormatServerDateCallback(new Func<TTCalendar, string>(TTCalendarDllInitializer.FormatServerDateFun));
      TickTickHandler.DateTime.ParseServerDateCallback(new Func<string, TTCalendar>(TTCalendarDllInitializer.ParseServerDateFun));
      TickTickHandler.DateTime.ConvertDateToTimeCallback(new Func<TTCalendar, long>(TTCalendarDllInitializer.ConvertDateToTimeFun));
      TickTickHandler.DateTime.ConvertDateToUTCCallback(new Func<string, TTCalendar, TTCalendar>(TTCalendarDllInitializer.ConvertDateToUTCFun));
      TickTickHandler.DateTime.FormatDateByTimeZoneCallback(new Func<string, long, string, string>(TTCalendarDllInitializer.FormatDateByTimeZoneFun));
      TickTickHandler.DateTime.ConvertDateFromOneTimeZoneToAnotherTimeZoneCallback(new Func<string, TTCalendar, string, TTCalendar>(TTCalendarDllInitializer.ConvertDateFromOneTimeZoneToAnotherTimeZoneFun));
      Tools.GetUUIDFunCallback(new Func<string>(TTCalendarDllInitializer.GetUUIDFun));
      TickTickHandler.Utils.InitDll();
      TTCalendarDllInitializer._inited = true;
    }

    public static void Init()
    {
    }

    private static string FormatDateByTimeZoneFun(string format, long timeStamp, string zoneName)
    {
      return TimeZoneInfo.ConvertTime(new System.DateTime(1970, 1, 1).AddMilliseconds((double) timeStamp), TimeZoneUtils.GetTimeZoneInfo(zoneName)).ToString(format);
    }

    private static string GetUUIDFun() => Guid.NewGuid().ToString("N").Substring(8);

    private static TTCalendar ConvertDateFromOneTimeZoneToAnotherTimeZoneFun(
      string zoneName,
      TTCalendar date,
      string targetZoneName)
    {
      System.DateTime dateTime = ModelsDao.TTCalendarToDateTime(date);
      return ModelsDao.NewTTCalendar(TimeZoneUtils.ZoneToTargetTzTime(zoneName, dateTime, targetZoneName), targetZoneName);
    }

    private static TTCalendar ConvertDateToUTCFun(string zoneName, TTCalendar date)
    {
      return ModelsDao.NewTTCalendar(TimeZoneInfo.ConvertTime(ModelsDao.TTCalendarToDateTime(date), TimeZoneUtils.GetTimeZoneInfo(zoneName), TimeZoneInfo.Utc), "Etc/UTC");
    }

    private static long ConvertDateToTimeFun(TTCalendar date)
    {
      if (date.year > 1970 && date.month >= 0 && date.day >= 0 && date.hour >= 0)
      {
        int minute = date.minute;
      }
      return (long) (TimeZoneUtils.ToLocalTime(ModelsDao.TTCalendarToDateTime(date), date.zone) - new System.DateTime(1970, 1, 1)).TotalMilliseconds;
    }

    private static TTCalendar ParseServerDateFun(string str)
    {
      return ModelsDao.NewTTCalendar(JsonConvert.DeserializeObject<System.DateTime>(str, (JsonConverter) new UtcDateTimeConverter()), TimeZoneData.LocalTimeZoneModel.TimeZoneName);
    }

    private static string FormatServerDateFun(TTCalendar ttCal)
    {
      return JsonConvert.SerializeObject((object) ModelsDao.TTCalendarToDateTime(ttCal), (JsonConverter) new UtcDateTimeConverter());
    }

    private static long CurrentTimeMillisFun()
    {
      return (System.DateTime.UtcNow.Ticks - 621355968000000000L) / 10000000L;
    }

    private static TTCalendar SetDayOfWeekFun(
      int year,
      int month,
      int day,
      string zoneName,
      int firstdayofweek,
      int dayofweek)
    {
      System.DateTime dateTime1 = new System.DateTime(1, 1, 1);
      dateTime1 = dateTime1.AddYears(year - 1);
      dateTime1 = dateTime1.AddMonths(month);
      System.DateTime dateTime2 = dateTime1.AddDays((double) (day - 1));
      System.DateTime dateTime3 = dateTime2.AddDays((double) ((int) dateTime2.DayOfWeek * -1)).AddDays((double) ticktick_WPF.Util.Utils.GetWeekFromDiff(new System.DateTime?(dateTime2)));
      int num = dayofweek - firstdayofweek;
      if (num < 0)
        num += 7;
      return ModelsDao.NewTTCalendar(dateTime3.AddDays((double) num), zoneName);
    }

    private static int GetFirstDayOfWeekFun()
    {
      switch (LocalSettings.Settings.WeekStartFrom)
      {
        case "Sunday":
          return 1;
        case "Saturday":
          return 7;
        case "Monday":
          return 2;
        default:
          return 1;
      }
    }

    private static int GetWeekOfYearFun(
      int year,
      int month,
      int day,
      int hour,
      int minute,
      int second,
      int milliSecond,
      string zoneName)
    {
      System.DateTime dateTime = new System.DateTime(1, 1, 1, 0, 0, 0);
      dateTime = dateTime.AddYears(year - 1);
      dateTime = dateTime.AddMonths(month);
      dateTime = dateTime.AddDays((double) (day - 1));
      dateTime = dateTime.AddHours((double) hour);
      dateTime = dateTime.AddMinutes((double) minute);
      dateTime = dateTime.AddSeconds((double) second);
      return DateUtils.GetWeekNumOfYear(dateTime.AddMilliseconds((double) milliSecond));
    }

    private static int IsRest(TTCalendar date)
    {
      System.DateTime time = ModelsDao.TTCalendarToDateTime(date);
      List<HolidayModel> holidays = HolidayManager.GetHolidays();
      return (holidays != null ? holidays.FirstOrDefault<HolidayModel>((Func<HolidayModel, bool>) (h => h.type == 0 && h.date.Date == time.Date)) : (HolidayModel) null) != null ? 1 : 0;
    }

    private static int IsWorkDay(TTCalendar date)
    {
      System.DateTime time = ModelsDao.TTCalendarToDateTime(date);
      List<HolidayModel> holidays = HolidayManager.GetHolidays();
      return (holidays != null ? holidays.FirstOrDefault<HolidayModel>((Func<HolidayModel, bool>) (h => h.type == 1 && h.date.Date == time.Date)) : (HolidayModel) null) != null ? 1 : 0;
    }

    private static void ToastTime(TTCalendar zone)
    {
    }

    private static void Toast(string str)
    {
    }

    public static bool CheckInitialized() => TTCalendarDllInitializer._inited;

    public static int GetZoneOffsetFun(string zoneName, long millis)
    {
      System.DateTime dateTime = new System.DateTime(1970, 1, 1).AddMilliseconds((double) millis);
      return (int) (dateTime - TimeZoneInfo.ConvertTime(dateTime, TimeZoneUtils.GetTimeZoneInfo(zoneName), TimeZoneInfo.Utc)).TotalSeconds;
    }

    public static string GetLocalZoneFun() => TimeZoneData.LocalTimeZoneModel.TimeZoneName;

    public static int GetDayOfWeekFun(
      int year,
      int month,
      int day,
      int hour,
      int minute,
      int second,
      int millisecond,
      string timezone)
    {
      System.DateTime dateTime = new System.DateTime(1, 1, 1);
      dateTime = dateTime.AddYears(year - 1);
      dateTime = dateTime.AddMonths(month);
      dateTime = dateTime.AddDays((double) (day - 1));
      dateTime = dateTime.AddHours((double) hour);
      dateTime = dateTime.AddMinutes((double) minute);
      dateTime = dateTime.AddSeconds((double) second);
      return (int) (dateTime.AddMilliseconds((double) millisecond).DayOfWeek + 1);
    }

    public static TTCalendar GetTimeFun(
      int year,
      int month,
      int day,
      int hour,
      int minute,
      int second,
      int millisecond,
      string zoneName)
    {
      try
      {
        System.DateTime dateTime = new System.DateTime(1, 1, 1);
        dateTime = dateTime.AddYears(year - 1);
        dateTime = dateTime.AddMonths(month);
        dateTime = dateTime.AddDays((double) (day - 1));
        dateTime = dateTime.AddHours((double) hour);
        dateTime = dateTime.AddMinutes((double) minute);
        dateTime = dateTime.AddSeconds((double) second);
        return ModelsDao.NewTTCalendar(dateTime.AddMilliseconds((double) millisecond), zoneName ?? TimeZoneData.LocalTimeZoneModel.TimeZoneName);
      }
      catch (Exception ex)
      {
        return new TTCalendar();
      }
    }

    public static TTCalendar GetTimeByMillisAndZoneFun(long millis, string zoneName)
    {
      try
      {
        return new TTCalendar(new System.DateTime(1970, 1, 1).AddMilliseconds((double) millis), zoneName);
      }
      catch (Exception ex)
      {
        return new TTCalendar();
      }
    }

    public static long GetTimeInMillisFun(TTCalendar date)
    {
      return date.year <= 0 ? (long) (new System.DateTime() - new System.DateTime(1970, 1, 1)).TotalMilliseconds : (long) (ModelsDao.TTCalendarToDateTime(date) - new System.DateTime(1970, 1, 1)).TotalMilliseconds;
    }

    public static TTCalendar GetNowByZoneFun(string zoneName)
    {
      System.DateTime targetTzTime = TimeZoneUtils.LocalToTargetTzTime(System.DateTime.Now, zoneName);
      return new TTCalendar()
      {
        year = targetTzTime.Year,
        month = targetTzTime.Month,
        day = targetTzTime.Day,
        hour = targetTzTime.Hour,
        minute = targetTzTime.Minute,
        second = targetTzTime.Second,
        zone = zoneName
      };
    }

    public static TTCalendar GetNowFun()
    {
      System.DateTime now = System.DateTime.Now;
      return new TTCalendar()
      {
        year = now.Year,
        month = now.Month,
        day = now.Day,
        hour = now.Hour,
        minute = now.Minute,
        second = now.Second,
        zone = TimeZoneData.LocalTimeZoneModel.TimeZoneName
      };
    }
  }
}
