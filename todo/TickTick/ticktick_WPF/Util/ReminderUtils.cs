// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.ReminderUtils
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ticktick_WPF.Models;
using ticktick_WPF.Views.Time;

#nullable disable
namespace ticktick_WPF.Util
{
  public static class ReminderUtils
  {
    public static string GetReminderListDisplayText(
      ICollection<TaskReminderModel> reminderList,
      bool isAllday,
      string emptyText = "")
    {
      if (reminderList != null && reminderList.Count != 0)
        return string.Join(", ", (IEnumerable<string>) reminderList.Where<TaskReminderModel>((Func<TaskReminderModel, bool>) (reminder => reminder != null && !string.IsNullOrEmpty(reminder.trigger))).Select<TaskReminderModel, AdvanceDateModel>((Func<TaskReminderModel, AdvanceDateModel>) (rule => new AdvanceDateModel(rule.trigger, isAllday))).ToList<AdvanceDateModel>().Select<AdvanceDateModel, string>((Func<AdvanceDateModel, string>) (model => model.ToString())).ToList<string>());
      return string.IsNullOrEmpty(emptyText) ? string.Empty : emptyText;
    }

    public static string GetCalendarReminderText(IEnumerable<int> minutes, bool isAllDay)
    {
      return string.Join(", ", minutes.OrderBy<int, int>((Func<int, int>) (min => min)).Select<int, string>((Func<int, string>) (min => ReminderUtils.GetReminderText(TriggerUtils.ReminderToTrigger(min), isAllDay))));
    }

    public static string GetReminderText(string trigger, bool isAllDay)
    {
      return !isAllDay ? ReminderUtils.ToTimeText(trigger) : ReminderUtils.ToAllDayText(trigger);
    }

    private static string ToTimeText(string trigger)
    {
      Match match = new Regex("(-)?P((\\d{0,})Y)?((\\d{0,})M)?((\\d{0,})W)?((\\d{0,})D)?T?((\\d{0,})H)?((\\d{0,})M)?((\\d{0,})S)?").Match(trigger);
      if (!match.Success)
        return string.Empty;
      int result1;
      int.TryParse(match.Groups[7].ToString(), out result1);
      int result2;
      int.TryParse(match.Groups[9].ToString(), out result2);
      int result3;
      int.TryParse(match.Groups[11].ToString(), out result3);
      int result4;
      int.TryParse(match.Groups[13].ToString(), out result4);
      if (result1 == 0 && result2 == 0 && result3 == 0 && result4 == 0)
        return Utils.GetString("OnTime");
      int num1 = result1 * 10080 + result2 * 1440 + result3 * 60 + result4;
      int num2 = 0;
      if (num1 > 0)
      {
        result2 = num1 / 1440;
        result3 = num1 % 1440 / 60;
        num1 %= 60;
      }
      string str = ReminderUtils.BuildUnitText(num2, ReminderUtils.Span.Week) + ReminderUtils.BuildUnitText(result2, ReminderUtils.Span.Day) + ReminderUtils.BuildUnitText(result3, ReminderUtils.Span.Hour) + ReminderUtils.BuildUnitText(num1, ReminderUtils.Span.Minute);
      return string.Format(Utils.GetString("AdvanceText"), (object) str);
    }

    public static string ToAllDayText(string trigger, DateTime date = default (DateTime))
    {
      Match match = new Regex("(-)?P((\\d{0,})Y)?((\\d{0,})M)?((\\d{0,})W)?((\\d{0,})D)?T?((\\d{0,})H)?((\\d{0,})M)?((\\d{0,})S)?").Match(trigger);
      if (!match.Success)
        return string.Empty;
      int num = trigger.Contains("-") ? 1 : 0;
      int result1;
      int.TryParse(match.Groups[7].ToString(), out result1);
      int result2;
      int.TryParse(match.Groups[9].ToString(), out result2);
      int result3;
      int.TryParse(match.Groups[11].ToString(), out result3);
      int result4;
      int.TryParse(match.Groups[13].ToString(), out result4);
      if (result1 > 0)
        result2 += result1 * 7;
      if (result2 % 7 == 0)
      {
        result1 += result2 / 7;
        result2 = 0;
      }
      if (num != 0)
      {
        if (Math.Abs(result3) > 24)
          result2 += Math.Abs(result3) / 24;
        if (Math.Abs(result3 % 24) > 0 || Math.Abs(result4 % 24) > 0)
          ++result2;
        result3 *= -1;
        result4 *= -1;
      }
      DateTime dateTime = date;
      if (Utils.IsEmptyDate(date))
        dateTime = DateTime.Today;
      DateTime date1 = dateTime.AddHours((double) result3).AddMinutes((double) result4);
      string str1 = DateUtils.FormatHourMinuteText(date1);
      if (result1 == 0 && result2 == 0 && Utils.IsEmptyDate(date))
        return Utils.GetString("CenterToday") + " (" + str1 + ")";
      if (Utils.IsEmptyDate(date))
      {
        string str2 = ReminderUtils.BuildUnitText(result1, ReminderUtils.Span.Week) + ReminderUtils.BuildUnitText(result2, ReminderUtils.Span.Day);
        return string.Format(Utils.GetString("AdvanceText"), (object) str2) + " (" + str1 + ")";
      }
      string str3 = " " + str1;
      if (result1 != 0 || result2 != 0)
      {
        string str4 = ReminderUtils.BuildUnitText(result1, ReminderUtils.Span.Week) + ReminderUtils.BuildUnitText(result2, ReminderUtils.Span.Day);
        string str5 = DateUtils.FormatFullDate(date1.AddDays((double) ((result1 * 7 + result2) * -1 + 1)));
        string str6 = string.Format(Utils.GetString("AdvanceText"), (object) str4);
        return string.Format(Utils.GetString("AdvanceFullText"), (object) str6, (object) str3, (object) str5);
      }
      string str7 = DateUtils.FormatFullDate(date1);
      string str8 = Utils.GetString("CenterToday");
      return string.Format(Utils.GetString("AdvanceFullText"), (object) str8, (object) str3, (object) str7);
    }

    private static string BuildUnitText(int num, ReminderUtils.Span unit)
    {
      if (num == 0)
        return string.Empty;
      string str1 = Utils.IsCn() ? "" : " ";
      string str2 = Utils.IsCn() ? " " : "";
      switch (unit)
      {
        case ReminderUtils.Span.Year:
          return str2 + num.ToString() + " " + Utils.GetString(num > 1 ? "PublicYears" : "PublicYear") + str1;
        case ReminderUtils.Span.Month:
          return str2 + num.ToString() + " " + Utils.GetString(num > 1 ? "PublicMonths" : "PublicMonth") + str1;
        case ReminderUtils.Span.Week:
          return str2 + num.ToString() + " " + Utils.GetString(num > 1 ? "PublicWeeks" : "PublicWeek") + str1;
        case ReminderUtils.Span.Day:
          return str2 + num.ToString() + " " + Utils.GetString(num > 1 ? "PublicDays" : "PublicDay") + str1;
        case ReminderUtils.Span.Hour:
          return str2 + num.ToString() + " " + Utils.GetString(num > 1 ? "PublicHours" : "PublicHour") + str1;
        case ReminderUtils.Span.Minute:
          return str2 + num.ToString() + " " + Utils.GetString(num > 1 ? "PublicMinutes" : "PublicMinute") + str1;
        default:
          return string.Empty;
      }
    }

    private enum Span
    {
      Year,
      Month,
      Week,
      Day,
      Hour,
      Minute,
    }
  }
}
