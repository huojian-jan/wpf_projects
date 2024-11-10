// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.DateUtils
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Util
{
  public static class DateUtils
  {
    public const string Hour24 = "24Hour";
    public const string Hour12 = "12Hour";
    private static readonly Dictionary<int, string> ChineseMonthDict = new Dictionary<int, string>()
    {
      {
        1,
        "正月"
      },
      {
        2,
        "二月"
      },
      {
        3,
        "三月"
      },
      {
        4,
        "四月"
      },
      {
        5,
        "五月"
      },
      {
        6,
        "六月"
      },
      {
        7,
        "七月"
      },
      {
        8,
        "八月"
      },
      {
        9,
        "九月"
      },
      {
        10,
        "十月"
      },
      {
        11,
        "冬月"
      },
      {
        12,
        "腊月"
      }
    };
    private static readonly Dictionary<int, string> ChineseNumDict = new Dictionary<int, string>()
    {
      {
        1,
        "初一"
      },
      {
        2,
        "初二"
      },
      {
        3,
        "初三"
      },
      {
        4,
        "初四"
      },
      {
        5,
        "初五"
      },
      {
        6,
        "初六"
      },
      {
        7,
        "初七"
      },
      {
        8,
        "初八"
      },
      {
        9,
        "初九"
      },
      {
        10,
        "初十"
      },
      {
        11,
        "十一"
      },
      {
        12,
        "十二"
      },
      {
        13,
        "十三"
      },
      {
        14,
        "十四"
      },
      {
        15,
        "十五"
      },
      {
        16,
        "十六"
      },
      {
        17,
        "十七"
      },
      {
        18,
        "十八"
      },
      {
        19,
        "十九"
      },
      {
        20,
        "二十"
      },
      {
        21,
        "廿一"
      },
      {
        22,
        "廿二"
      },
      {
        23,
        "廿三"
      },
      {
        24,
        "廿四"
      },
      {
        25,
        "廿五"
      },
      {
        26,
        "廿六"
      },
      {
        27,
        "廿七"
      },
      {
        28,
        "廿八"
      },
      {
        29,
        "廿九"
      },
      {
        30,
        "三十"
      },
      {
        31,
        "三一"
      }
    };

    public static string FormatCommentTime(DateTime date)
    {
      TimeSpan timeSpan = DateTime.Now - date;
      if (0.0 < timeSpan.TotalSeconds && timeSpan.TotalSeconds < 60.0)
        return Utils.GetString("JustNow");
      if (1.0 <= timeSpan.TotalMinutes && timeSpan.TotalMinutes < 60.0)
        return Math.Abs(timeSpan.TotalMinutes - 1.0) <= 0.0001 ? ((int) timeSpan.TotalMinutes).ToString() + (Utils.IsCn() ? "" : " ") + Utils.GetString("PublicMinute") + (Utils.IsCn() ? "" : " ") + Utils.GetString("Ago") : ((int) timeSpan.TotalMinutes).ToString() + (Utils.IsCn() ? "" : " ") + Utils.GetString("PublicMinutes") + (Utils.IsCn() ? "" : " ") + Utils.GetString("Ago");
      if (date.Date == DateTime.Today)
        return date.ToString(DateUtils.GetTimeFormatString(), (IFormatProvider) App.Ci);
      DateTime dateTime = date.Date;
      int year1 = dateTime.Year;
      dateTime = DateTime.Today;
      int year2 = dateTime.Year;
      return year1 == year2 ? date.ToString("MM-dd " + DateUtils.GetTimeFormatString(), (IFormatProvider) App.Ci) : date.ToString("yyyy-MM-dd " + DateUtils.GetTimeFormatString(), (IFormatProvider) App.Ci);
    }

    public static string FormatSummaryTime(DateTime date, bool withTime)
    {
      return !withTime ? date.ToString("m", (IFormatProvider) App.Ci) : date.ToString("m", (IFormatProvider) App.Ci) + " " + date.ToString(DateUtils.GetTimeFormatString(), (IFormatProvider) App.Ci);
    }

    public static string FormatSummaryTimeOnly(DateTime date)
    {
      return date.ToString(DateUtils.GetTimeFormatString(), (IFormatProvider) App.Ci);
    }

    public static DateTime GetNextHour(DateTime date)
    {
      int hour = DateTime.Now.Hour;
      DateTime dateTime = DateUtils.SetHourAndMinuteOnly(date, hour, 0);
      return !(dateTime.Date != DateTime.Today) || dateTime.Hour != 23 ? dateTime.AddHours(1.0) : dateTime;
    }

    public static DateTime GetNextHour(string timeZone)
    {
      DateTime dateTime = string.IsNullOrEmpty(timeZone) || timeZone == TimeZoneData.LocalTimeZoneModel?.TimeZoneName ? DateTime.Now : TimeZoneUtils.LocalToTargetTzTime(DateTime.Now, timeZone);
      dateTime = DateUtils.SetHourAndMinuteOnly(dateTime, dateTime.Hour, 0).AddHours(1.0);
      return dateTime;
    }

    public static string FormatHourMinuteText(DateTime date)
    {
      return date.ToString(DateUtils.GetTimeFormatString(), (IFormatProvider) App.Ci);
    }

    public static string GetHourText(DateTime date)
    {
      return date.ToString(DateUtils.GetTimeDisplayFormat() == DateUtils.TimeDisplayFormat.Hour24 ? "HH" : "hh", (IFormatProvider) CultureInfo.CreateSpecificCulture("en-US"));
    }

    public static string GetMinuteText(DateTime date)
    {
      return date.ToString("mm", (IFormatProvider) CultureInfo.CreateSpecificCulture("en-US"));
    }

    public static string GetAmOrPm(DateTime date)
    {
      return date.ToString("tt", (IFormatProvider) CultureInfo.CreateSpecificCulture("en-US"));
    }

    public static DateTime SetHourAndMinuteOnly(DateTime dateTime, int hours, int minutes)
    {
      return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, hours, minutes, 0, 0, dateTime.Kind);
    }

    public static DateTime CropHourAndMinute(DateTime dateTime)
    {
      return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 0, 0, 0, 0, dateTime.Kind);
    }

    public static DateTime SetMinuteOnly(DateTime date, int minute)
    {
      return minute >= 0 && minute < 60 ? new DateTime(date.Year, date.Month, date.Day, date.Hour, minute, date.Second, date.Millisecond, date.Kind) : date;
    }

    private static int GetNextWeekDayDiff(DayOfWeek day)
    {
      DayOfWeek dayOfWeek = DateTime.Now.DayOfWeek;
      return dayOfWeek == day ? 0 : (int) (7 - dayOfWeek);
    }

    public static int GetNextWeekDayDiff()
    {
      int num;
      switch (LocalSettings.Settings.WeekStartFrom)
      {
        case "Monday":
          num = DateUtils.GetNextWeekDayDiff(DayOfWeek.Sunday) + 1;
          break;
        case "Saturday":
          num = DateUtils.GetNextWeekDayDiff(DayOfWeek.Sunday) - 1;
          break;
        default:
          num = DateUtils.GetNextWeekDayDiff(DayOfWeek.Sunday);
          break;
      }
      return num != 0 ? num : 7;
    }

    public static DateUtils.TimeDisplayFormat GetTimeDisplayFormat()
    {
      return !(LocalSettings.Settings.TimeFormat == "24Hour") ? DateUtils.TimeDisplayFormat.Hour12 : DateUtils.TimeDisplayFormat.Hour24;
    }

    public static string GetDefaultReminder()
    {
      return DateUtils.GetTimeDisplayFormat() != DateUtils.TimeDisplayFormat.Hour12 ? "(09:00)" : "(09:00 AM)";
    }

    public static string GetTriggerDisplayText(string trigger, DateTime pivotDate, bool isAllDay)
    {
      Match match = new Regex("(-)?P((\\d{0,})Y)?((\\d{0,})M)?((\\d{0,})W)?((\\d{0,})D)?T?((\\d{0,})H)?((\\d{0,})M)?((\\d{0,})S)?").Match(trigger);
      if (!match.Success)
        return string.Empty;
      int result1;
      int.TryParse(match.Groups[9].ToString(), out result1);
      int result2;
      int.TryParse(match.Groups[11].ToString(), out result2);
      int result3;
      int.TryParse(match.Groups[13].ToString(), out result3);
      return DateUtils.GetTriggerDisplayText(pivotDate, result1, result2, result3);
    }

    public static string GetTriggerDisplayText(DateTime pivotDate, int days, int hours, int mins)
    {
      ++days;
      DateTime dateTime = pivotDate.AddDays((double) (days * -1));
      dateTime = dateTime.AddHours((double) (hours * -1));
      DateTime date = dateTime.AddMinutes((double) (mins * -1));
      string str = Utils.GetString(days > 1 ? "PublicDays" : "PublicDay");
      return string.Format(Utils.GetString("ReminderText"), (object) days, (object) str) + "(" + DateUtils.FormatHourMinuteText(date) + ")";
    }

    public static Tuple<int, string> GetIntervalAndUnit(int day, int hour, int min)
    {
      int num1 = day + 1;
      if (num1 > 0 && num1 % 7 == 0)
      {
        int num2 = num1 / 7;
        return new Tuple<int, string>(num2, Utils.GetString(num2 > 1 ? "PublicWeeks" : "PublicWeek"));
      }
      if (num1 > 0)
        return new Tuple<int, string>(day, Utils.GetString(day > 1 ? "PublicDays" : "PublicDay"));
      if (hour > 0)
        return new Tuple<int, string>(hour, Utils.GetString(day > 1 ? "PublicHours" : "PublicHour"));
      return min > 0 ? new Tuple<int, string>(min, Utils.GetString(day > 1 ? "PublicMinutes" : "PublicMinute")) : new Tuple<int, string>(0, string.Empty);
    }

    public static DateTime SetDateOnly(DateTime original, DateTime modify)
    {
      return new DateTime(modify.Year, modify.Month, modify.Day, original.Hour, original.Minute, original.Second, original.Millisecond, original.Kind);
    }

    public static string FormatMonthDay(DateTime date)
    {
      return date.ToString("m", (IFormatProvider) App.Ci);
    }

    public static string FormatMonth(DateTime date) => DateUtils.FormatShortMonth(date);

    public static string FormatWeekDayName(DateTime date, bool withSmart = false)
    {
      if (withSmart)
      {
        switch ((date.Date - DateTime.Today).TotalDays)
        {
          case -1.0:
            return Utils.GetString("PublicYesterday");
          case 0.0:
            return Utils.GetString("Today");
          case 1.0:
            return Utils.GetString("Tomorrow");
        }
      }
      return date.ToString("ddd", (IFormatProvider) App.Ci);
    }

    public static string FormatSimpleWeekDayName(DateTime date)
    {
      return App.Ci.DateTimeFormat.GetShortestDayName(date.DayOfWeek);
    }

    public static string FormatYearMonth(DateTime date)
    {
      return date.ToString("yyyy", (IFormatProvider) App.Ci) + (Utils.IsCn() || App.Ci.ToString() == "ja-JP" ? "年" : (App.Ci.ToString() == "ko-KR" ? "년" : "/")) + date.ToString("MM", (IFormatProvider) App.Ci) + (Utils.IsCn() || App.Ci.ToString() == "ja-JP" ? "月" : (App.Ci.ToString() == "ko-KR" ? "월" : ""));
    }

    public static string FormatYear(DateTime date)
    {
      return date.ToString("yyyy", (IFormatProvider) App.Ci) + (Utils.IsCn() || App.Ci.ToString() == "ja-JP" ? "年" : (App.Ci.ToString() == "ko-KR" ? "년" : ""));
    }

    public static string FormatMonthAndDay(DateTime date)
    {
      try
      {
        return date.ToString(Utils.IsCn() ? "m" : "MMM d", (IFormatProvider) App.Ci);
      }
      catch (Exception ex)
      {
        return date.ToString("MM/dd", (IFormatProvider) App.Ci);
      }
    }

    public static string FormatShortDate(DateTime date)
    {
      return DateUtils.FormatShortDate(date, date.Year != DateTime.Now.Year);
    }

    public static string FormatDateCheckYear(DateTime date)
    {
      if (date.Year < DateTime.Today.Year)
        return date.ToString("yyyy/MM/dd");
      try
      {
        return date.ToString("m", (IFormatProvider) App.Ci);
      }
      catch (Exception ex)
      {
        return date.ToString("MM/dd", (IFormatProvider) App.Ci);
      }
    }

    private static string FormatShortDate(DateTime date, bool withYear)
    {
      try
      {
        return date.ToString(!withYear ? "m" : "D", (IFormatProvider) App.Ci);
      }
      catch (Exception ex)
      {
        return date.ToString("MM/dd", (IFormatProvider) App.Ci);
      }
    }

    public static string FormatShortMonth(DateTime date)
    {
      return date.ToString("MMM", (IFormatProvider) App.Ci) + (App.Ci.ToString() == "ja-JP" ? "月" : (App.Ci.ToString() == "ko-KR" ? "달" : ""));
    }

    public static string FormatDay(DateTime date)
    {
      return ((string[]) Application.Current?.FindResource((object) "OneMonthDay"))?[date.Day - 1];
    }

    public static string FormatShortMonthDay(DateTime date)
    {
      DateTimeFormatInfo dateTimeFormat = App.Ci.DateTimeFormat;
      return date.ToString(dateTimeFormat.MonthDayPattern.Replace("MMMM", "MMM"), (IFormatProvider) App.Ci);
    }

    private static string FormatCalendarDateDesc(DateTime date)
    {
      if (Utils.IsEmptyDate(date))
        return string.Empty;
      return date.Year - DateTime.Today.Year != 0 ? DateUtils.FormatFullDate(date) + ", " + DateUtils.FormatWeekDayName(date, true) : DateUtils.FormatMonthDay(date) + ", " + DateUtils.FormatWeekDayName(date, true);
    }

    public static string FormatTimeDesc(DateTime date, bool isAllDay, bool showDesc = true)
    {
      try
      {
        if (Utils.IsEmptyDate(date))
          return string.Empty;
        DateTime date1 = date.Date;
        DateTime today = DateTime.Today;
        DateTime date2 = today.Date;
        int totalDays1 = (int) (date1 - date2).TotalDays;
        DateTime weekStart = Utils.GetWeekStart(DateTime.Today);
        today = DateTime.Today;
        DateTime date3 = today.Date;
        double totalDays2 = (weekStart - date3).TotalDays;
        switch (totalDays1)
        {
          case -1:
            if (showDesc)
              return Utils.GetString("PublicYesterday");
            break;
          case 0:
            return !isAllDay ? DateUtils.FormatHourMinuteText(date) : Utils.GetString("Today");
          case 1:
            return Utils.GetString("Tomorrow");
          case 2:
            return Utils.GetString("In2Days");
          default:
            if (totalDays1 >= 2 && (double) totalDays1 < totalDays2 + 7.0)
              return date.ToString("ddd", (IFormatProvider) App.Ci);
            if ((double) totalDays1 >= totalDays2 + 7.0 && (double) totalDays1 < totalDays2 + 14.0)
              return string.Format(Utils.GetString("NextWeekDay"), (object) date.ToString("ddd", (IFormatProvider) App.Ci));
            break;
        }
        int year1 = date.Year;
        today = DateTime.Today;
        int year2 = today.Year;
        return year1 - year2 != 0 ? DateUtils.FormatFullDate(date) : DateUtils.FormatMonthDay(date);
      }
      catch (Exception ex)
      {
        return date.ToString("MM/dd", (IFormatProvider) App.Ci);
      }
    }

    public static string FormatFullDate(DateTime date)
    {
      try
      {
        return date.ToString("D", (IFormatProvider) App.Ci);
      }
      catch (FormatException ex)
      {
        return date.ToString("MM/dd/yyyy");
      }
    }

    public static string GetTimeFormatString()
    {
      return (DateUtils.GetTimeDisplayFormat() == DateUtils.TimeDisplayFormat.Hour12 ? ((IEnumerable<string>) App.Ci.DateTimeFormat.GetAllDateTimePatterns('t')).FirstOrDefault<string>((Func<string, bool>) (f => f.Contains("t"))) ?? "h:mm tt" : "H:mm").Replace("'", "\\'");
    }

    public static string FormatListDateString(
      DateTime startDate,
      DateTime? dueDate,
      bool? isAllDay)
    {
      if (!dueDate.HasValue || Utils.IsEmptyDate(dueDate))
        return DateUtils.FormatTimeDesc(startDate, ((int) isAllDay ?? 1) != 0);
      if (DateUtils.IsDateInToday(new DateTime?(startDate), dueDate, isAllDay))
        return DateUtils.FormatDayDescription(startDate, dueDate.Value, isAllDay, 0, "Today");
      if (!DateUtils.IsOutDated(new DateTime?(startDate), dueDate, isAllDay))
        return DateUtils.FormatTimeDesc(startDate, ((int) isAllDay ?? 1) != 0, false);
      DateTime? nullable = dueDate;
      if (isAllDay.HasValue && isAllDay.Value)
      {
        nullable = new DateTime?(dueDate.Value.AddDays(-1.0));
        if (nullable.Value.Date <= startDate.Date)
          nullable = new DateTime?();
      }
      return DateUtils.FormatTimeDesc(nullable ?? startDate, ((int) isAllDay ?? 1) != 0, false);
    }

    private static string FormatDayDescription(
      DateTime startDate,
      DateTime dueDate,
      bool? isAllDay,
      int dayDiff,
      string defaultStringKey)
    {
      if (isAllDay.HasValue && isAllDay.Value)
        return Utils.GetString(defaultStringKey);
      DateTime date1 = startDate.Date.Date;
      DateTime dateTime = DateTime.Today.AddDays((double) dayDiff);
      DateTime date2 = dateTime.Date;
      if (date1 == date2)
        return DateUtils.FormatHourMinuteText(startDate);
      dateTime = dueDate.Date;
      DateTime date3 = dateTime.Date;
      dateTime = DateTime.Today;
      dateTime = dateTime.AddDays((double) dayDiff);
      DateTime date4 = dateTime.Date;
      return !(date3 == date4) ? Utils.GetString(defaultStringKey) : DateUtils.FormatHourMinuteText(dueDate);
    }

    public static string FormatCalendarDateString(
      DateTime startDate,
      DateTime endDate,
      bool isAllDay)
    {
      if (DateUtils.IsDurationInSameDay(startDate, endDate))
      {
        string str1 = DateUtils.FormatCalendarDateDesc(startDate);
        if (isAllDay)
          return str1;
        string str2 = DateUtils.FormatHourMinute(startDate) + " - " + DateUtils.FormatHourMinute(endDate);
        return str1 + ", " + str2;
      }
      string str3 = DateUtils.FormatCalendarDateDesc(startDate);
      string str4 = DateUtils.FormatCalendarDateDesc(endDate);
      return !isAllDay ? Utils.GetString("Start") + ": " + str3 + ", " + DateUtils.FormatHourMinute(startDate) + "\n" + Utils.GetString("End") + ": " + str4 + ", " + DateUtils.FormatHourMinute(endDate) : Utils.GetString("Start") + ": " + str3 + "\n" + Utils.GetString("End") + ": " + str4;
    }

    public static string GetTaskActivityTimeString(
      DateTime start,
      DateTime? due,
      bool? isAllDay,
      string timeZone = null,
      bool isFloat = false)
    {
      return DateUtils.FormatDetailDateString(start, due, isAllDay, timeZone: timeZone, isFloat: isFloat, showTimeZone: true);
    }

    public static string FormatDateStringNoWord(DateTime start, DateTime? due, bool isAllDay)
    {
      return due.HasValue ? ToStr(start, isAllDay) + " - " + ToStr(due.Value, isAllDay) : ToStr(start, isAllDay);

      static string ToStr(DateTime date, bool noTime)
      {
        return noTime ? date.ToString("d", (IFormatProvider) App.Ci) : date.ToString("d", (IFormatProvider) App.Ci) + " " + DateUtils.FormatHourMinute(date);
      }
    }

    public static string FormatDateString(
      DateTime start,
      DateTime? due,
      bool? isAllDay,
      bool useSmartDate = true)
    {
      return DateUtils.FormatDetailDateString(start, due, isAllDay, useSmartDate);
    }

    public static string FormatQuickAddDateString(DateTime start, DateTime? due, bool? isAllDay)
    {
      return DateUtils.FormatDetailDateString(start, due, isAllDay, showSmartOnly: true);
    }

    private static string FormatDetailDateString(
      DateTime startDate,
      DateTime? dueDate,
      bool? isAllDay,
      bool useSmartDate = true,
      string timeZone = null,
      bool isFloat = false,
      bool showTimeZone = false,
      bool showSmartOnly = false)
    {
      string str = "";
      if (showTimeZone && !isAllDay.GetValueOrDefault() && !string.IsNullOrEmpty(timeZone) && timeZone != TimeZoneData.LocalTimeZoneModel?.TimeZoneName)
      {
        if (!Utils.IsEmptyDate(startDate))
          startDate = TimeZoneUtils.LocalToTargetTzTime(startDate, timeZone);
        if (!Utils.IsEmptyDate(dueDate) && dueDate.HasValue)
          dueDate = new DateTime?(TimeZoneUtils.LocalToTargetTzTime(dueDate.Value, timeZone));
        if (!isFloat)
          str = " (" + TimeZoneViewModel.GetShortName(TimeZoneData.GetDisplayNameForTimeZone(TimeZoneUtils.GetTimeZoneInfo(timeZone), App.Ci.ToString())) + ")";
      }
      DateTime? date = dueDate;
      if (!Utils.IsEmptyDate(dueDate) && (startDate == dueDate.Value || isAllDay.HasValue && isAllDay.Value && (startDate.Date == dueDate.Value.Date || startDate.Date == dueDate.Value.Date.AddDays(-1.0))))
        date = new DateTime?();
      if (Utils.IsEmptyDate(startDate))
        return Utils.GetString("DateAndReminder");
      if (!date.HasValue || Utils.IsEmptyDate(date))
        return DateUtils.FormatTime(startDate, isAllDay, useSmartDate, showSmartOnly) + str;
      return DateUtils.IsDurationInSameDay(startDate, date.Value) ? DateUtils.FormatTimeDuration(startDate, date.Value, isAllDay, useSmartDate, showSmartOnly) + str : DateUtils.FormatDateDuration(startDate, date.Value, isAllDay) + str;
    }

    private static bool IsDurationInSameDay(DateTime starDate, DateTime dueDate)
    {
      return starDate.Date == dueDate.Date;
    }

    private static string FormatDateDuration(DateTime startDate, DateTime dueDate, bool? isAllDay)
    {
      if (isAllDay.HasValue && isAllDay.Value)
      {
        bool withYear = startDate.Year != DateTime.Today.Year || dueDate.AddDays(-1.0).Year != DateTime.Today.Year;
        return DateUtils.FormatShortDate(startDate, withYear) + " - " + DateUtils.FormatShortDate(dueDate.AddDays(-1.0), withYear);
      }
      bool withYear1 = startDate.Year != DateTime.Today.Year || dueDate.Year != DateTime.Today.Year;
      return DateUtils.FormatShortDate(startDate, withYear1) + ", " + DateUtils.FormatHourMinute(startDate) + " - " + DateUtils.FormatShortDate(dueDate, withYear1) + ", " + DateUtils.FormatHourMinute(dueDate);
    }

    private static string FormatTimeDuration(
      DateTime startDate,
      DateTime dueDate,
      bool? isAllDay,
      bool useSmartDate = true,
      bool showSmartOnly = false)
    {
      string str = useSmartDate ? (showSmartOnly ? DateUtils.FormatSmartDateShortString(startDate) : DateUtils.FormatSmartDateString(startDate)) : DateUtils.FormatShortDate(startDate);
      if (isAllDay.HasValue && isAllDay.Value)
        return str;
      return str + ", " + DateUtils.FormatHourMinute(startDate) + " - " + DateUtils.FormatHourMinute(dueDate);
    }

    public static string FormatHourMinute(DateTime date, bool longTime = false)
    {
      string format = DateUtils.GetTimeFormatString();
      if (longTime && !format.ToLower().Contains("hh"))
        format = format.Replace("h", "hh").Replace("H", "HH");
      return date.ToString(format, (IFormatProvider) App.Ci);
    }

    public static bool IsEqualTime(DateTime a, DateTime b)
    {
      return a.Hour == b.Hour && a.Minute == b.Minute;
    }

    private static string FormatTime(
      DateTime startDate,
      bool? isAllDay,
      bool useSmartDate = true,
      bool showSmartOnly = false)
    {
      string str = useSmartDate ? (showSmartOnly ? DateUtils.FormatSmartDateShortString(startDate) : DateUtils.FormatSmartDateString(startDate)) : DateUtils.FormatShortDate(startDate);
      return isAllDay.HasValue && isAllDay.Value ? str : str + ", " + DateUtils.FormatHourMinute(startDate);
    }

    public static string FormatAddTaskDateString(DateTime date)
    {
      int totalDays = (int) (date.Date - DateTime.Today.Date).TotalDays;
      switch (totalDays)
      {
        case -1:
          return Utils.GetString("PublicYesterday");
        case 0:
          return Utils.GetString("Today");
        case 1:
          return Utils.GetString("Tomorrow");
        default:
          return totalDays >= 2 && totalDays < 7 ? date.ToString("ddd", (IFormatProvider) App.Ci) : DateUtils.FormatShortDate(date);
      }
    }

    public static string FormatSmartDateShortString(DateTime date)
    {
      int totalDays1 = (int) (date.Date - DateTime.Today.Date).TotalDays;
      double totalDays2 = (Utils.GetWeekStart(DateTime.Today) - DateTime.Today.Date).TotalDays;
      switch (totalDays1)
      {
        case -1:
          return Utils.GetString("PublicYesterday");
        case 0:
          return Utils.GetString("Today");
        case 1:
          return Utils.GetString("Tomorrow");
        case 2:
          return Utils.GetString("In2Days");
        default:
          if ((double) totalDays1 >= totalDays2 - 7.0 && (double) totalDays1 < totalDays2)
            return string.Format(Utils.GetString("LastWeekDay"), (object) date.ToString("ddd", (IFormatProvider) App.Ci));
          if ((double) totalDays1 >= totalDays2 && totalDays1 < -1)
            return DateUtils.DiffDescription(totalDays1, "PublicDays", "PublicDays");
          if (totalDays1 >= 2 && (double) totalDays1 < totalDays2 + 7.0)
            return date.ToString("ddd", (IFormatProvider) App.Ci);
          return (double) totalDays1 >= totalDays2 + 7.0 && (double) totalDays1 < totalDays2 + 14.0 ? string.Format(Utils.GetString("NextWeekDay"), (object) date.ToString("ddd", (IFormatProvider) App.Ci)) : DateUtils.FormatShortDate(date);
      }
    }

    public static string FormatSmartDateString(DateTime date)
    {
      int diff1 = date.Year - DateTime.Today.Year;
      string str;
      if (Math.Abs(diff1) > 1)
      {
        str = DateUtils.DiffDescription(diff1, "PublicYear", "PublicYears");
      }
      else
      {
        int totalDays1 = (int) (date.Date - DateTime.Today.Date).TotalDays;
        int num = (date.Date.Year - DateTime.Today.Year) * 12 + date.Date.Month;
        DateTime today = DateTime.Today;
        int month = today.Month;
        int diff2 = num - month;
        DateTime weekStart = Utils.GetWeekStart(DateTime.Today);
        today = DateTime.Today;
        DateTime date1 = today.Date;
        double totalDays2 = (weekStart - date1).TotalDays;
        if (Math.Abs(totalDays1) > 30)
        {
          str = DateUtils.DiffDescription(diff2, "PublicMonth", "PublicMonths");
        }
        else
        {
          switch (totalDays1)
          {
            case -1:
              str = Utils.GetString("PublicYesterday");
              break;
            case 0:
              str = Utils.GetString("Today");
              break;
            case 1:
              str = Utils.GetString("Tomorrow");
              break;
            case 2:
              str = Utils.GetString("In2Days");
              break;
            default:
              str = (double) totalDays1 < totalDays2 - 7.0 || (double) totalDays1 >= totalDays2 ? (totalDays1 < 2 || (double) totalDays1 >= totalDays2 + 7.0 ? ((double) totalDays1 < totalDays2 + 7.0 || (double) totalDays1 >= totalDays2 + 14.0 ? DateUtils.DiffDescription(totalDays1, "PublicDays", "PublicDays") : string.Format(Utils.GetString("NextWeekDay"), (object) date.ToString("ddd", (IFormatProvider) App.Ci))) : date.ToString("ddd", (IFormatProvider) App.Ci)) : string.Format(Utils.GetString("LastWeekDay"), (object) date.ToString("ddd", (IFormatProvider) App.Ci));
              break;
          }
        }
      }
      return str + ", " + DateUtils.FormatShortDate(date);
    }

    private static string DiffDescription(int diff, string single, string plur)
    {
      string str1 = Utils.GetString(Math.Abs(diff) == 1 ? single : plur);
      string str2 = Utils.GetString(diff < 0 ? "TimeBefore" : "TimeAfter");
      return string.Format(Utils.GetString("DeltaTimeFormat"), (object) Math.Abs(diff), (object) str1, (object) str2);
    }

    public static bool IsOutDated(DateTime? startDate, DateTime? dueDate, bool? isAllDay)
    {
      if (!dueDate.HasValue || Utils.IsEmptyDate(dueDate) || !isAllDay.HasValue || !isAllDay.Value)
        return DateUtils.IsOutDated(startDate, dueDate);
      DateTime dateTime = dueDate.Value.AddSeconds(-1.0);
      if (startDate.HasValue && dateTime.Date < startDate.Value.Date)
        dateTime = startDate.Value;
      return DateUtils.IsOutDated(startDate, new DateTime?(dateTime));
    }

    private static bool IsOutDated(DateTime? startDate, DateTime? dueDate)
    {
      if (!startDate.HasValue || Utils.IsEmptyDate(startDate))
        return true;
      if (!dueDate.HasValue || Utils.IsEmptyDate(dueDate))
      {
        if (startDate.Value.Date < DateTime.Now.Date)
          return true;
      }
      else
      {
        DateTime date1 = startDate.Value.Date;
        DateTime dateTime = DateTime.Now;
        DateTime date2 = dateTime.Date;
        if (date1 < date2)
        {
          dateTime = dueDate.Value;
          dateTime = dateTime.AddSeconds(-1.0);
          if (dateTime.Date < DateTime.Today)
            return true;
        }
      }
      return false;
    }

    public static DateUtils.DateSectionCategory GetSectionCategory(
      DateTime? startDate,
      DateTime? dueDate,
      bool? isAllDay)
    {
      if (!startDate.HasValue || Utils.IsEmptyDate(startDate))
        return DateUtils.DateSectionCategory.NoDate;
      if (DateUtils.IsOutDated(startDate, dueDate, isAllDay))
        return DateUtils.DateSectionCategory.OutDated;
      if (DateUtils.IsDateInToday(startDate, dueDate, isAllDay))
        return DateUtils.DateSectionCategory.Today;
      if (DateUtils.IsDateInTomorrow(startDate, dueDate, isAllDay))
        return DateUtils.DateSectionCategory.Tomorrow;
      return !DateUtils.IsDateInWeek(startDate, dueDate, isAllDay) ? DateUtils.DateSectionCategory.Future : DateUtils.DateSectionCategory.ThisWeek;
    }

    public static DateTime? GetDateSectionDate(DateUtils.DateSectionCategory section)
    {
      switch (section)
      {
        case DateUtils.DateSectionCategory.NoDate:
          return new DateTime?();
        case DateUtils.DateSectionCategory.OutDated:
          return new DateTime?(DateTime.Today.AddDays(-1.0));
        case DateUtils.DateSectionCategory.Today:
          return new DateTime?(DateTime.Today);
        case DateUtils.DateSectionCategory.Tomorrow:
          return new DateTime?(DateTime.Today.AddDays(1.0));
        case DateUtils.DateSectionCategory.ThisWeek:
          return new DateTime?(DateTime.Today.AddDays(2.0));
        case DateUtils.DateSectionCategory.Future:
          return new DateTime?();
        default:
          return new DateTime?();
      }
    }

    public static bool IsDateInToday(DateTime? start, DateTime? due, bool? isAllDay)
    {
      DateTime dateTime = DateTime.Today;
      DateTime date = dateTime.Date;
      dateTime = DateTime.Today;
      dateTime = dateTime.Date;
      DateTime spanEnd = dateTime.AddDays(1.0);
      DateTime? start1 = start;
      DateTime? due1 = due;
      bool? isAllDay1 = isAllDay;
      return DateUtils.IsDateInSpan(date, spanEnd, start1, due1, isAllDay1);
    }

    private static bool IsDateInTomorrow(DateTime? start, DateTime? due, bool? isAllDay)
    {
      DateTime dateTime = DateTime.Today;
      dateTime = dateTime.Date;
      DateTime spanStart = dateTime.AddDays(1.0);
      dateTime = DateTime.Today;
      dateTime = dateTime.Date;
      DateTime spanEnd = dateTime.AddDays(2.0);
      DateTime? start1 = start;
      DateTime? due1 = due;
      bool? isAllDay1 = isAllDay;
      return DateUtils.IsDateInSpan(spanStart, spanEnd, start1, due1, isAllDay1);
    }

    private static bool IsDateInWeek(DateTime? start, DateTime? due, bool? isAllDay)
    {
      DateTime dateTime = DateTime.Today;
      dateTime = dateTime.Date;
      DateTime spanStart = dateTime.AddDays(1.0);
      dateTime = DateTime.Today;
      dateTime = dateTime.Date;
      DateTime spanEnd = dateTime.AddDays(8.0);
      DateTime? start1 = start;
      DateTime? due1 = due;
      bool? isAllDay1 = isAllDay;
      return DateUtils.IsDateInSpan(spanStart, spanEnd, start1, due1, isAllDay1);
    }

    private static bool IsDateInSpan(
      DateTime spanStart,
      DateTime spanEnd,
      DateTime? start,
      DateTime? due,
      bool? isAllDay)
    {
      if (!start.HasValue || Utils.IsEmptyDate(start))
        return false;
      if (start.Value >= spanStart && start.Value < spanEnd)
        return true;
      if (!due.HasValue || Utils.IsEmptyDate(due))
        return start.Value >= spanStart && start.Value < spanEnd;
      if (Utils.IsEmptyDate(due))
        return false;
      if (isAllDay.HasValue && isAllDay.Value)
      {
        DateTime date;
        if (start.Value == due.Value)
        {
          ref DateTime? local = ref due;
          date = start.Value.Date;
          DateTime dateTime = date.AddDays(1.0);
          local = new DateTime?(dateTime);
        }
        ref DateTime? local1 = ref due;
        date = due.Value;
        DateTime dateTime1 = date.AddSeconds(-1.0);
        local1 = new DateTime?(dateTime1);
      }
      return start.Value < spanEnd && due.Value > spanStart;
    }

    public static DateTime? TryParseTime(string text, string format)
    {
      try
      {
        return new DateTime?(DateTime.ParseExact(text, format, (IFormatProvider) CultureInfo.InvariantCulture));
      }
      catch (Exception ex)
      {
        return new DateTime?();
      }
    }

    public static string GetLunarMonthDay(DateTime date)
    {
      if (Utils.IsEmptyDate(date))
        return string.Empty;
      ChineseLunisolarCalendar lunisolarCalendar = new ChineseLunisolarCalendar();
      int month = lunisolarCalendar.GetMonth(date);
      int dayOfMonth = lunisolarCalendar.GetDayOfMonth(date);
      int year = lunisolarCalendar.GetYear(date);
      if (lunisolarCalendar.IsLeapYear(year))
      {
        int leapMonth = lunisolarCalendar.GetLeapMonth(year);
        if (month >= leapMonth)
          --month;
      }
      return DateUtils.GetChineseMonth(month) + DateUtils.GetChineseDay(dayOfMonth);
    }

    private static string GetChineseMonth(int month)
    {
      return DateUtils.ChineseMonthDict.ContainsKey(month) ? DateUtils.ChineseMonthDict[month] : month.ToString();
    }

    private static string GetChineseDay(int dayOfMonth)
    {
      return DateUtils.ChineseNumDict.ContainsKey(dayOfMonth) ? DateUtils.ChineseNumDict[dayOfMonth] : dayOfMonth.ToString();
    }

    public static DateTime ConvertTimeZone(DateTime dateTime, string timeZone)
    {
      try
      {
        if (string.IsNullOrEmpty(timeZone) || string.IsNullOrEmpty(timeZone))
          return dateTime;
        TimeZoneInfo timeZoneInfo = TimeZoneUtils.GetTimeZoneInfo(timeZone);
        return TimeZoneInfo.ConvertTime(dateTime, timeZoneInfo);
      }
      catch (Exception ex)
      {
        return dateTime;
      }
    }

    public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
    {
      return TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(unixTimeStamp));
    }

    public static double DateTimeToUnixTimestamp(DateTime dateTime)
    {
      DateTime dateTime1 = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
      return (double) (dateTime.ToUniversalTime() - dateTime1).Ticks / 10000000.0;
    }

    public static bool IsTaskInDay(
      DateTime targetDate,
      DateTime? startDate,
      DateTime? dueDate,
      bool? isAllDay)
    {
      if (isAllDay.HasValue && isAllDay.Value || !startDate.HasValue)
        return false;
      if (!dueDate.HasValue)
        return startDate.Value.Date == targetDate.Date;
      double totalHours1 = (dueDate.Value - startDate.Value).TotalHours;
      double totalHours2 = (dueDate.Value - targetDate.Date).TotalHours;
      if (totalHours1 > 24.0 || totalHours2 <= 0.0)
        return false;
      return startDate.Value.Date == targetDate.Date || dueDate.Value.Date == targetDate.Date;
    }

    public static void CheckIfTomorrowWronglySet(TimeData date)
    {
      if (date == null || date.DueDate.HasValue || !date.StartDate.HasValue)
        return;
      DateTime date1 = date.StartDate.Value.Date;
      DateTime dateTime1 = DateTime.Today;
      DateTime dateTime2 = dateTime1.AddDays(1.0);
      if (!(date1 == dateTime2) || !DateUtils.IsPreDawnNow())
        return;
      string format = Utils.GetString("TomorrowSetHint");
      dateTime1 = DateTime.Now;
      string str = dateTime1.ToString("HH:mm");
      Utils.Toast(string.Format(format, (object) str));
    }

    private static bool IsPreDawnNow() => (DateTime.Now - DateTime.Today).TotalMinutes <= 180.0;

    public static int GetWeekNumOfYear(DateTime date)
    {
      if (Utils.IsEmptyDate(date))
        return 0;
      if (string.IsNullOrEmpty(LocalSettings.Settings.StartWeekOfYear))
      {
        CultureInfo currentCulture = new Thread((ThreadStart) (() => { })).CurrentCulture;
        DateTimeFormatInfo dateTimeFormatInfo = currentCulture.DateTimeFormat ?? App.Ci.DateTimeFormat;
        Calendar calendar = dateTimeFormatInfo.Calendar;
        DayOfWeek result;
        if (!Enum.TryParse<DayOfWeek>(LocalSettings.Settings.WeekStartFrom, out result))
          result = DayOfWeek.Sunday;
        DateTime time = date;
        int rule = Utils.IsCn(currentCulture.Name) ? 2 : (int) dateTimeFormatInfo.CalendarWeekRule;
        int firstDayOfWeek = (int) result;
        return calendar.GetWeekOfYear(time, (CalendarWeekRule) rule, (DayOfWeek) firstDayOfWeek);
      }
      DateTime startDate = new DateTime(date.Year, 1, 1, 0, 0, 0, 0);
      string startWeekOfYear = LocalSettings.Settings.StartWeekOfYear;
      int result1;
      int result2;
      if (startWeekOfYear != null && startWeekOfYear.Length == 4 && int.TryParse(startWeekOfYear.Substring(0, 2), out result1) && int.TryParse(startWeekOfYear.Substring(2, 2), out result2))
        startDate = startDate.AddMonths(result1 - 1).AddDays((double) (result2 - 1));
      if (date.Date < startDate.Date)
      {
        DateTime weekStart = Utils.GetWeekStart(date);
        if (weekStart < startDate && weekStart.AddDays(6.0) > startDate)
          return 1;
        startDate = startDate.AddYears(-1);
      }
      return DateUtils.GetWeekNum(date, startDate);
    }

    public static int GetWeekNum(DateTime date, DateTime startDate)
    {
      int num = (int) (date.Date - startDate.Date).TotalDays + 1;
      DayOfWeek dayOfWeek = startDate.DayOfWeek;
      switch (LocalSettings.Settings.WeekStartFrom)
      {
        case "Sunday":
          num += (int) dayOfWeek;
          break;
        case "Saturday":
          num += (int) (dayOfWeek + 1) % 7;
          break;
        case "Monday":
          num += (int) (dayOfWeek + 6) % 7;
          break;
      }
      int weekNum = num / 7;
      if (num % 7 > 0)
        ++weekNum;
      return weekNum;
    }

    public static string GetWeekNumStrOfYear(DateTime date)
    {
      int weekNumOfYear = DateUtils.GetWeekNumOfYear(date);
      return weekNumOfYear <= 0 ? "" : string.Format(Utils.GetString("WeekNum"), (object) weekNumOfYear);
    }

    public static int GetDateNum(DateTime date) => date.Year * 10000 + date.Month * 100 + date.Day;

    private static string GetLeftTimeString(int leftMinutes)
    {
      return (leftMinutes >= 60 ? leftMinutes / 60 : leftMinutes).ToString() + (Utils.IsCn() ? "" : " ") + Utils.GetString(leftMinutes >= 60 ? (leftMinutes / 60 > 1 ? "PublicUpHours" : "PublicUpHour") : (leftMinutes > 1 ? "PublicUpMinutes" : "PublicUpMinute"));
    }

    public static string FormatCountDownDateString(
      DateTime startDate,
      DateTime? dueDate,
      bool isAllDay,
      bool showDetail)
    {
      if (startDate >= DateTime.Now.AddMinutes(-1.0))
      {
        if (startDate.Date > DateTime.Today && (isAllDay || startDate >= DateTime.Now.AddHours(12.0)))
        {
          int totalDays = (int) (startDate.Date - DateTime.Today).TotalDays;
          return string.Format(Utils.GetString("DaysLeft"), (object) (totalDays.ToString() + (Utils.IsCn() ? "" : " ") + Utils.GetString(totalDays > 1 ? "PublicDays" : "PublicDay")));
        }
        int leftMinutes = (int) Math.Ceiling((startDate - DateTime.Now).TotalMinutes);
        return leftMinutes != 0 ? string.Format(Utils.GetString("DaysLeft"), (object) DateUtils.GetLeftTimeString(leftMinutes)) : Utils.GetString("Now");
      }
      if (isAllDay && dueDate.HasValue)
      {
        dueDate = new DateTime?(dueDate.Value.AddMinutes(-1.0));
        if (startDate.Date == dueDate.Value.Date)
          dueDate = new DateTime?();
      }
      if (dueDate.HasValue)
      {
        if (dueDate.Value >= DateTime.Now.AddMinutes(-1.0))
        {
          if (!showDetail)
            return isAllDay || dueDate.Value.Date != DateTime.Today && startDate.Date != DateTime.Today ? Utils.GetString("Today") : Utils.GetString("Now");
          if (isAllDay || dueDate.Value.Date != DateTime.Today && startDate.Date != DateTime.Today)
          {
            int totalDays1 = (int) (DateTime.Today - startDate.Date).TotalDays;
            int totalDays2 = (int) (dueDate.Value.Date - DateTime.Today).TotalDays;
            return string.Format(Utils.GetString("DaysPast"), (object) (totalDays1.ToString() + (Utils.IsCn() ? "" : " ") + Utils.GetString(totalDays1 > 1 ? "PublicDays" : "PublicDay")), (object) (totalDays2.ToString() + (Utils.IsCn() ? "" : " ") + Utils.GetString(totalDays2 > 1 ? "PublicDays" : "PublicDay")));
          }
          string str1;
          if (startDate.Date < DateTime.Today && DateTime.Now >= startDate.AddHours(12.0))
          {
            int totalDays = (int) (DateTime.Today - startDate.Date).TotalDays;
            str1 = totalDays.ToString() + (Utils.IsCn() ? "" : " ") + Utils.GetString(totalDays > 1 ? "PublicDays" : "PublicDay");
          }
          else
          {
            int totalMinutes = (int) (DateTime.Now - startDate).TotalMinutes;
            if (totalMinutes == 0)
              return Utils.GetString("Now");
            str1 = DateUtils.GetLeftTimeString(totalMinutes);
          }
          string str2;
          if (dueDate.Value.Date > DateTime.Today && dueDate.Value >= DateTime.Now.AddHours(12.0))
          {
            int totalDays = (int) (dueDate.Value.Date - DateTime.Today).TotalDays;
            str2 = totalDays.ToString() + (Utils.IsCn() ? "" : " ") + Utils.GetString(totalDays > 1 ? "PublicDays" : "PublicDay");
          }
          else
          {
            int leftMinutes = (int) Math.Ceiling((dueDate.Value - DateTime.Now).TotalMinutes);
            if (leftMinutes == 0)
              return Utils.GetString("Now");
            str2 = DateUtils.GetLeftTimeString(leftMinutes);
          }
          return string.Format(Utils.GetString("DaysPast"), (object) str1, (object) str2);
        }
        if (isAllDay || dueDate.Value.Date != DateTime.Today && Math.Abs((dueDate.Value - DateTime.Now).TotalHours) >= 12.0)
        {
          int totalDays = (int) (DateTime.Today - dueDate.Value.Date).TotalDays;
          return string.Format(Utils.GetString(totalDays == 0 ? "DaysLeft" : "DaysOver"), (object) (totalDays.ToString() + (Utils.IsCn() ? "" : " ") + Utils.GetString(totalDays > 1 ? "PublicDays" : "PublicDay")));
        }
        int totalMinutes1 = (int) (DateTime.Now - dueDate.Value).TotalMinutes;
        return totalMinutes1 != 0 ? string.Format(Utils.GetString("DaysOver"), (object) DateUtils.GetLeftTimeString(totalMinutes1)) : Utils.GetString("Now");
      }
      if (startDate.Date == DateTime.Today & isAllDay)
        return Utils.GetString("Today");
      if (isAllDay || startDate.Date < DateTime.Today && Math.Abs((startDate - DateTime.Now).TotalHours) >= 12.0)
      {
        int totalDays = (int) (DateTime.Today - startDate.Date).TotalDays;
        return string.Format(Utils.GetString("DaysOver"), (object) (totalDays.ToString() + (Utils.IsCn() ? "" : " ") + Utils.GetString(totalDays > 1 ? "PublicDays" : "PublicDay")));
      }
      int totalMinutes2 = (int) (DateTime.Now - startDate).TotalMinutes;
      return totalMinutes2 != 0 ? string.Format(Utils.GetString("DaysOver"), (object) DateUtils.GetLeftTimeString(totalMinutes2)) : Utils.GetString("Now");
    }

    public static bool CountDownOutDate(DateTime? startDate, DateTime? dueDate, bool isAllDay)
    {
      if (!startDate.HasValue)
        return false;
      if (isAllDay)
      {
        if ((!dueDate.HasValue || startDate.Value.Date == dueDate.Value.Date) && startDate.Value.Date < DateTime.Today || dueDate.HasValue && dueDate.Value.AddMinutes(-1.0).Date < DateTime.Today)
          return true;
      }
      else
      {
        DateTime now;
        if (!dueDate.HasValue)
        {
          DateTime dateTime1 = startDate.Value;
          now = DateTime.Now;
          DateTime dateTime2 = now.AddMinutes(-1.0);
          if (dateTime1 < dateTime2)
            return true;
        }
        if (dueDate.HasValue)
        {
          DateTime dateTime3 = dueDate.Value;
          now = DateTime.Now;
          DateTime dateTime4 = now.AddMinutes(-1.0);
          if (dateTime3 < dateTime4)
            return true;
        }
      }
      return false;
    }

    public static DateTime GetLocalTime(DateTime date)
    {
      return TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second, DateTimeKind.Utc));
    }

    public static bool IsSameTime(DateTime? left, DateTime? right)
    {
      if (!left.HasValue && !right.HasValue)
        return true;
      return left.HasValue && right.HasValue && left.Value.Hour == right.Value.Hour && left.Value.Minute == right.Value.Minute;
    }

    public static bool IsSameDate(DateTime? left, DateTime? right)
    {
      if (!left.HasValue && !right.HasValue)
        return true;
      return left.HasValue && right.HasValue && left.Value.Date == right.Value.Date;
    }

    public static DateTime ParseDateTime(string str, string format = "yyyyMMdd")
    {
      DateTime result = new DateTime();
      DateTime.TryParseExact(str, format, (IFormatProvider) App.Ci, DateTimeStyles.None, out result);
      return result;
    }

    public static DateTime IntToDateTime(int timeInt)
    {
      return new DateTime(timeInt / 10000, timeInt % 10000 / 100, timeInt % 100);
    }

    public static int DateTimeToInt(DateTime time)
    {
      return 0 + time.Year * 10000 + time.Month * 100 + time.Day;
    }

    public static bool IsWeekEnds(DateTime startDate)
    {
      return startDate.DayOfWeek == DayOfWeek.Sunday || startDate.DayOfWeek == DayOfWeek.Saturday;
    }

    public static bool CheckDateInSpan(
      DateTime checkStart,
      DateTime checkEnd,
      DateTime spanStart,
      DateTime spanEnd)
    {
      if (checkStart <= spanStart && checkEnd >= spanEnd || checkStart >= spanStart && checkStart <= spanEnd)
        return true;
      return spanStart <= checkEnd && checkEnd <= spanEnd;
    }

    public static DateTime GetDateByWeek(DateTime startDate, int week, int weekday)
    {
      DateTime weekStart = Utils.GetWeekStart(startDate);
      switch (LocalSettings.Settings.WeekStartFrom)
      {
        case "Sunday":
          weekday %= 7;
          break;
        case "Saturday":
          weekday = (weekday + 1) % 7;
          break;
        case "Monday":
          --weekday;
          break;
      }
      return weekStart.AddDays((double) ((week - 1) * 7 + weekday));
    }

    public static (int, int) GetHourAndMinuteInString(string lessonStart)
    {
      if (string.IsNullOrEmpty(lessonStart))
        return (0, 0);
      string[] strArray = lessonStart.Split(':');
      if (strArray.Length != 2)
        return (0, 0);
      int result1;
      int result2;
      return int.TryParse(strArray[0], out result1) && int.TryParse(strArray[1], out result2) ? (result1, result2) : (0, 0);
    }

    public static string GetWeekTextByWeekDay(int weekDay)
    {
      switch (weekDay)
      {
        case 0:
        case 7:
          return Utils.GetString("Sun");
        case 1:
          return Utils.GetString("Mon");
        case 2:
          return Utils.GetString("Tues");
        case 3:
          return Utils.GetString("Wed");
        case 4:
          return Utils.GetString("Thur");
        case 5:
          return Utils.GetString("Fri");
        case 6:
          return Utils.GetString("Sat");
        default:
          return (string) null;
      }
    }

    public static string GetShortWeekTextByWeekDay(int weekDay)
    {
      switch (weekDay)
      {
        case 0:
        case 7:
          return Utils.GetString("Su");
        case 1:
          return Utils.GetString("Mo");
        case 2:
          return Utils.GetString("Tu");
        case 3:
          return Utils.GetString("We");
        case 4:
          return Utils.GetString("Th");
        case 5:
          return Utils.GetString("Fr");
        case 6:
          return Utils.GetString("Sa");
        default:
          return (string) null;
      }
    }

    public static string GetTimeText(DateTime startTime)
    {
      return startTime.ToString(DateUtils.GetTimeFormatString(), (IFormatProvider) App.Ci);
    }

    public static string GetSystemTimeFormat()
    {
      return !new CultureInfo(LocalSettings.Settings.UserChooseLanguage).DateTimeFormat.ShortTimePattern.Contains("H") ? "12Hour" : "24Hour";
    }

    public static string GetLunarText(DateTime date, bool showWeek)
    {
      if (!string.IsNullOrEmpty(WeekName()))
        return WeekName();
      if (Utils.IsTickPackage())
      {
        if (Utils.IsJp())
        {
          HolidayModel holidayModel = HolidayManager.GetCacheHolidays().Find((Predicate<HolidayModel>) (item =>
          {
            DateTime date1 = item.date;
            date1 = date1.Date;
            return date1.Equals(date.Date);
          }));
          if (holidayModel != null && holidayModel.region == HolidayRegion.jp && LocalSettings.Settings.EnableHoliday)
            return holidayModel.name;
        }
        AlternativeCalendar alternativeCalendar = LocalSettings.Settings.UserPreference.alternativeCalendar;
        if (alternativeCalendar != null)
        {
          string lunarText = string.Equals(alternativeCalendar.calendar, "lunar") ? DateUtils.GetChineseLunarText(date) : DateUtils.GetDisplayLunarText(date, alternativeCalendar.calendar);
          if (!string.IsNullOrEmpty(lunarText))
            return lunarText;
        }
      }
      else if (LocalSettings.Settings.EnableLunar)
      {
        string chineseLunarText = DateUtils.GetChineseLunarText(date);
        if (!string.IsNullOrEmpty(chineseLunarText))
          return chineseLunarText;
      }
      return Utils.IsTickPackage() && !Utils.IsShowHoliday() && !LocalSettings.Settings.EnableLunar ? WeekName() : string.Empty;

      string WeekName()
      {
        return showWeek && (date.DayOfWeek.ToString() == LocalSettings.Settings.WeekStartFrom || !LocalSettings.Settings.ShowCalWeekend && date.DayOfWeek == DayOfWeek.Monday) ? DateUtils.GetWeekNumStrOfYear(date.AddDays(LocalSettings.Settings.ShowCalWeekend ? 6.0 : 4.0)) : string.Empty;
      }
    }

    private static string GetDisplayLunarText(DateTime date, string alternativeCalendar)
    {
      return !CalendarConverter.IsValidCalendarType(alternativeCalendar) ? string.Empty : new CalendarDisplay(date, alternativeCalendar).DisplayText();
    }

    private static string GetChineseLunarText(DateTime date)
    {
      LunarUtils.ChineseCalendar chineseCalendar = new LunarUtils.ChineseCalendar(date);
      if (Utils.IsEmptyDate(chineseCalendar.Date))
        return string.Empty;
      try
      {
        if (!string.IsNullOrEmpty(chineseCalendar.ChineseCalendarHoliday) && (chineseCalendar.Date.Month != 10 || chineseCalendar.Date.Day != 1))
        {
          string chineseCalendarHoliday = chineseCalendar.ChineseCalendarHoliday;
          if (chineseCalendarHoliday.Contains("除夕") || chineseCalendarHoliday.Contains("春节") || chineseCalendarHoliday.Contains("中秋") || chineseCalendarHoliday.Contains("端午"))
            return chineseCalendarHoliday;
        }
        if (LocalSettings.Settings.EnableHoliday)
        {
          if (!string.IsNullOrEmpty(chineseCalendar.DateHoliday))
            return chineseCalendar.DateHoliday;
          if (!string.IsNullOrEmpty(chineseCalendar.WeekDayHoliday))
            return chineseCalendar.WeekDayHoliday;
        }
        if (!string.IsNullOrEmpty(chineseCalendar.ChineseCalendarHoliday))
          return chineseCalendar.ChineseCalendarHoliday;
        string chineseTwentyFourDay = chineseCalendar.ChineseTwentyFourDay;
        if (!string.IsNullOrEmpty(chineseTwentyFourDay) && LocalSettings.Settings.EnableHoliday && chineseTwentyFourDay.Contains("清明"))
          return chineseTwentyFourDay;
        return chineseCalendar.ChineseDay == 1 ? chineseCalendar.ChineseMonthString : chineseCalendar.ChineseDayString;
      }
      catch (Exception ex)
      {
      }
      return string.Empty;
    }

    public static DateTime GetMonthStart(DateTime date)
    {
      return date.Day == 1 ? date : date.Date.AddDays((double) (1 - date.Day));
    }

    public static DateTime GetMonthEnd(DateTime date)
    {
      DateTime dateTime = DateUtils.GetMonthStart(date);
      dateTime = dateTime.AddMonths(1);
      return dateTime.AddDays(-1.0);
    }

    public static DateTime GetCurrentMonthDate(DateTime start, DateTime end)
    {
      DateTime dateTime1 = start.Date.AddDays((double) (1 - start.Day));
      DateTime dateTime2 = end.Date.AddDays((double) (1 - end.Day));
      if (dateTime1.AddMonths(1) == dateTime2)
      {
        int day = end.Day;
        return (int) (end.Date - start.Date).TotalDays + 1 - day <= day ? dateTime2 : dateTime1;
      }
      return !(dateTime1.AddMonths(2) == dateTime2) ? dateTime1 : dateTime1.AddMonths(1);
    }

    public static int GetWeekendsCountInSpan(DateTime start, DateTime end, bool checkEnd)
    {
      int days = (end - start).Days;
      bool flag = days < 0;
      int num1 = flag ? (int) end.DayOfWeek : (int) start.DayOfWeek;
      if (num1 == 0)
        num1 = 7;
      int num2 = 0;
      int num3 = Math.Abs(days) + (checkEnd ? 0 : -1);
      for (int index = 0; index <= num3; ++index)
      {
        if (num1 > 5)
          ++num2;
        if (num1 == 7)
          num1 = 1;
        else
          ++num1;
      }
      return !flag ? num2 : -1 * num2;
    }

    public static string GetZhTime(DateTime date)
    {
      int num = date.Hour % 12 == 0 ? 12 : date.Hour % 12;
      if (date.Hour <= 5)
        return "凌晨" + num.ToString() + "点";
      if (date.Hour < 12)
        return "上午" + num.ToString() + "点";
      if (date.Hour == 12)
        return "正午";
      if (date.Hour <= 17)
        return "下午" + num.ToString() + "点";
      return date.Hour < 24 ? "晚上" + num.ToString() + "点" : "";
    }

    public static string GetAmPmTimeText(DateTime date)
    {
      int num = date.Hour % 12 == 0 ? 12 : date.Hour % 12;
      if (date.Hour < 12)
        return num.ToString() + " AM";
      if (date.Hour == 12)
        return Utils.GetString("Noon");
      return date.Hour < 24 ? num.ToString() + " PM" : "";
    }

    public enum DateSectionCategory
    {
      NoDate,
      OutDated,
      Today,
      Tomorrow,
      ThisWeek,
      Future,
    }

    public enum TimeDisplayFormat
    {
      Hour12,
      Hour24,
    }
  }
}
