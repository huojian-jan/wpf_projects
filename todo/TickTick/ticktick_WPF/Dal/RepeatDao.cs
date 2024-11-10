// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Dal.RepeatDao
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using ticktick_WPF.Util;
using ticktick_WPF.Util.KotlinUtils;
using ticktick_WPF.Util.Sync;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Dal
{
  public class RepeatDao
  {
    public static DateTime GetEventNextRepeatDate(string rrule, DateTime start, string timeZone)
    {
      DateTime today = DateTime.Today;
      DateTime spanEnd = today.AddDays(3.0);
      List<DateTime> repeatDates = RepeatCalculator.GetRepeatDates(rrule, "0", start, today, spanEnd, timeZone: timeZone);
      return repeatDates != null && repeatDates.Count > 0 ? repeatDates[0] : RepeatCalculator.GetRepeatNextDate(rrule, "2", start, new DateTime?(), timeZone: timeZone);
    }

    public static List<DateTime> GetEventRepeatDates(
      string repeatFlag,
      DateTime calStart,
      DateTime checkStart,
      DateTime checkEnd,
      string timezone,
      bool isAllDay)
    {
      List<string> exDate = new List<string>();
      return RepeatUtils.GetValidRepeatDates(repeatFlag, "0", calStart, checkStart, checkEnd, exDate, timezone, !isAllDay);
    }

    public static List<DateTime> GetRepeatFlagDate(
      TaskBaseViewModel model,
      DateTime startTime,
      DateTime endTime)
    {
      List<DateTime> repeatFlagDate = new List<DateTime>();
      if (model.StartDate.HasValue && !string.IsNullOrEmpty(model.RepeatFlag))
      {
        List<string> exDate = new List<string>();
        if (!string.IsNullOrEmpty(model.ExDates))
          exDate = ((IEnumerable<string>) ExDateSerilizer.ToArray(model.ExDates)).ToList<string>();
        DateTime? startDate = model.StartDate;
        bool toLocalTime = ((int) model.IsAllDay ?? 1) == 0 && !model.IsFloating;
        DateTime? nullable = !toLocalTime ? startDate : new DateTime?(TimeZoneUtils.LocalToTargetTzTime(startDate.Value, model.TimeZoneName));
        repeatFlagDate = RepeatUtils.GetValidRepeatDates(model.RepeatFlag, model.RepeatFrom, nullable.Value, startTime, endTime, exDate, model.TimeZoneName, toLocalTime);
      }
      return repeatFlagDate;
    }
  }
}
