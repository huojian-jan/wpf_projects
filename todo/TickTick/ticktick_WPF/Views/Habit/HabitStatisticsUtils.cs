// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Habit.HabitStatisticsUtils
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Habit
{
  public static class HabitStatisticsUtils
  {
    public static MonthPlanInfo GetPlanDaysInMonth(
      string repeatRule,
      DateTime start,
      DateTime end,
      List<HabitCheckInModel> checkIns)
    {
      HabitRepeatInfo info = HabitUtils.BuildHabitRepeatInfo(repeatRule);
      return HabitStatisticsUtils.GetMonthPlanInfo(start.Date, end.Date, info, checkIns);
    }

    private static MonthPlanInfo GetMonthPlanInfo(
      DateTime start,
      DateTime end,
      HabitRepeatInfo info,
      List<HabitCheckInModel> checkIns)
    {
      MonthPlanInfo monthPlanInfo = new MonthPlanInfo();
      if (info.Type == HabitRepeatType.TimesInWeek)
      {
        DateTime weekStart1 = Utils.GetWeekStart(start);
        DateTime dateTime = weekStart1.AddDays(6.0);
        DateTime weekStart2 = Utils.GetWeekStart(end);
        DateTime date = weekStart2.AddDays(6.0);
        int num = (int) ((weekStart2 - dateTime).TotalDays / 7.0);
        int firstWeekCount = info.Count;
        int lastWeekCount = info.Count;
        if (weekStart1 != start || date != end)
        {
          int dateNum1 = DateUtils.GetDateNum(weekStart1);
          int dateNum2 = DateUtils.GetDateNum(start);
          int dateNum3 = DateUtils.GetDateNum(date);
          int dateNum4 = DateUtils.GetDateNum(end);
          foreach (HabitCheckInModel habitCheckInModel in checkIns.Where<HabitCheckInModel>((Func<HabitCheckInModel, bool>) (checkIn => firstWeekCount > 0 || lastWeekCount > 0)))
          {
            int result;
            if ((habitCheckInModel.CheckStatus == 2 || habitCheckInModel.Value > habitCheckInModel.Goal && habitCheckInModel.CheckStatus != 1) && int.TryParse(habitCheckInModel.CheckinStamp, out result))
            {
              if (firstWeekCount > 0 && result >= dateNum1 && result < dateNum2)
                --firstWeekCount;
              if (lastWeekCount > 0 && result <= dateNum3 && result > dateNum4)
                --lastWeekCount;
            }
          }
        }
        firstWeekCount = !(weekStart1 == weekStart2) || weekStart1.Month != start.Month ? Math.Min(firstWeekCount, (dateTime.Date - start.Date).Days + 1) : 0;
        lastWeekCount = !(weekStart1 == weekStart2) || date.Month != end.Month ? Math.Min(lastWeekCount, (end.Date - weekStart2.Date).Days + 1) : 0;
        monthPlanInfo.Count = Math.Min((end.Date - start.Date).Days + 1, num * info.Count + firstWeekCount + lastWeekCount);
        monthPlanInfo.StartDate = start;
        monthPlanInfo.EndDate = end;
      }
      else if (info.Type == HabitRepeatType.ByDay)
      {
        int num = 0;
        for (DateTime dateTime = start; dateTime <= end; dateTime = dateTime.AddDays(1.0))
        {
          if (info.ByDays.Contains(dateTime.DayOfWeek))
            ++num;
        }
        monthPlanInfo.StartDate = start;
        monthPlanInfo.EndDate = end;
        monthPlanInfo.Count = num;
      }
      else
      {
        monthPlanInfo.Count = (end - start).Days / info.Interval + 1;
        monthPlanInfo.StartDate = start;
        monthPlanInfo.EndDate = end;
      }
      return monthPlanInfo;
    }

    public static int GetBestStreakDays(string repeatRule, List<HabitCheckInModel> checkIns)
    {
      if (checkIns == null || !checkIns.Any<HabitCheckInModel>())
        return 0;
      if (checkIns.Count == 1)
        return checkIns[0].Value >= checkIns[0].Goal ? 1 : 0;
      HabitRepeatInfo habitRepeatInfo = HabitUtils.BuildHabitRepeatInfo(repeatRule);
      if (habitRepeatInfo.Type == HabitRepeatType.Daily)
        return HabitStatisticsUtils.CalculateDailyBestStreak(checkIns, habitRepeatInfo.Interval);
      if (habitRepeatInfo.Type == HabitRepeatType.TimesInWeek)
        return HabitStatisticsUtils.CalculateNDaysInWeekBestStreak(checkIns, habitRepeatInfo.Count);
      return habitRepeatInfo.Type == HabitRepeatType.ByDay ? HabitStatisticsUtils.CalculateByDaysBestStreak(checkIns, habitRepeatInfo.ByDays) : HabitStatisticsUtils.CalculateDailyBestStreak(checkIns, 1);
    }

    private static int CalculateNDaysInWeekCurrentStreakDays(
      List<HabitCheckInModel> checkIns,
      int count)
    {
      DateTime dateTime = DateTime.Today;
      dateTime = dateTime.AddDays((double) ((int) DateTime.Today.DayOfWeek * -1));
      DateTime start1 = dateTime.AddDays((double) Utils.GetWeekFromDiff());
      DateTime today = DateTime.Today;
      int matchCountInSpan1 = HabitStatisticsUtils.GetMatchCountInSpan(start1, today, checkIns);
      for (int index = 1; index < 10000; ++index)
      {
        DateTime start2 = start1.AddDays((double) (7 * index * -1));
        DateTime end = start2.AddDays(6.0);
        int matchCountInSpan2 = HabitStatisticsUtils.GetMatchCountInSpan(start2, end, checkIns);
        matchCountInSpan1 += HabitStatisticsUtils.GetMatchCountInSpan(start2, end, checkIns);
        int num = count;
        if (matchCountInSpan2 < num)
          break;
      }
      return matchCountInSpan1;
    }

    private static DateTime GetFirstStartDate(
      List<HabitCheckInModel> checkIns,
      List<DayOfWeek> byDays)
    {
      DateTime checkDate = DateTime.Today;
      if (checkIns.Exists((Predicate<HabitCheckInModel>) (checkIn => checkIn.CheckinStamp == checkDate.ToString("yyyyMMdd", (IFormatProvider) CultureInfo.InvariantCulture) && checkIn.Value >= checkIn.Goal)))
        return checkDate;
      for (int index = 1; index < 7; ++index)
      {
        checkDate = checkDate.AddDays(-1.0);
        if (checkIns.Exists((Predicate<HabitCheckInModel>) (checkIn => checkIn.CheckinStamp == checkDate.ToString("yyyyMMdd", (IFormatProvider) CultureInfo.InvariantCulture) && checkIn.Value >= checkIn.Goal)) || byDays.Contains(checkDate.DayOfWeek) || byDays.Contains(checkDate.DayOfWeek))
          return checkDate;
      }
      return checkDate;
    }

    private static int CalculateByDaysCurrentStreakDays(
      List<HabitCheckInModel> checkIns,
      List<DayOfWeek> byDays)
    {
      if (checkIns == null || !checkIns.Any<HabitCheckInModel>())
        return 0;
      int currentStreakDays = 0;
      DateTime checkDate = HabitStatisticsUtils.GetFirstStartDate(checkIns, byDays);
      DateTime exact = DateTime.ParseExact(checkIns.First<HabitCheckInModel>().CheckinStamp, "yyyyMMdd", (IFormatProvider) App.Ci);
      while (checkDate >= exact)
      {
        int num = byDays.Contains(checkDate.DayOfWeek) ? 1 : (checkDate == DateTime.Today ? 1 : 0);
        bool flag = checkIns.Exists((Predicate<HabitCheckInModel>) (checkIn => checkIn.CheckinStamp == checkDate.ToString("yyyyMMdd", (IFormatProvider) CultureInfo.InvariantCulture) && checkIn.Value >= checkIn.Goal));
        if (num == 0 || flag)
        {
          checkDate = checkDate.AddDays(-1.0);
          if (flag)
            ++currentStreakDays;
        }
        else
          break;
      }
      return currentStreakDays;
    }

    public static async Task<HabitStatisticsModel> CalculateHabitStatInfo(
      string habitId,
      string repeatRule)
    {
      List<HabitCheckInModel> checkInsByHabitId = await HabitCheckInDao.GetHabitCheckInsByHabitId(habitId);
      return checkInsByHabitId == null || !checkInsByHabitId.Any<HabitCheckInModel>() ? new HabitStatisticsModel() : HabitStatisticsUtils.CalculateHabitStatInfo(checkInsByHabitId, repeatRule);
    }

    public static HabitStatisticsModel CalculateHabitStatInfo(
      List<HabitCheckInModel> checkIns,
      string repeatRule)
    {
      checkIns = checkIns.Where<HabitCheckInModel>((Func<HabitCheckInModel, bool>) (c => c.IsComplete())).OrderBy<HabitCheckInModel, int>((Func<HabitCheckInModel, int>) (checkIn => int.Parse(checkIn.CheckinStamp))).ToList<HabitCheckInModel>();
      HabitRepeatInfo habitRepeatInfo = HabitUtils.BuildHabitRepeatInfo(repeatRule);
      if (habitRepeatInfo.Type == HabitRepeatType.Daily)
        return HabitStatisticsUtils.CalculateDailyStatInfo(checkIns, habitRepeatInfo.Interval);
      if (habitRepeatInfo.Type == HabitRepeatType.TimesInWeek)
        return HabitStatisticsUtils.CalculateNDaysInWeekStatInfo(checkIns, habitRepeatInfo.Count);
      if (habitRepeatInfo.Type != HabitRepeatType.ByDay)
        return new HabitStatisticsModel();
      return habitRepeatInfo.ByDays.Count == 7 ? HabitStatisticsUtils.CalculateDailyStatInfo(checkIns, 1) : HabitStatisticsUtils.CalculateByDaysStatInfo(checkIns, habitRepeatInfo.ByDays);
    }

    private static HabitStatisticsModel CalculateByDaysStatInfo(
      List<HabitCheckInModel> checkIns,
      List<DayOfWeek> byDays)
    {
      return new HabitStatisticsModel()
      {
        TotalCheckIns = HabitStatisticsUtils.GetTotalCheckInCount((IEnumerable<HabitCheckInModel>) checkIns),
        CurrentStreak = HabitStatisticsUtils.CalculateByDaysCurrentStreakDays(checkIns, byDays),
        MaxStreak = HabitStatisticsUtils.CalculateByDaysBestStreak(checkIns, byDays)
      };
    }

    private static int CalculateByDaysBestStreak(
      List<HabitCheckInModel> checkIns,
      List<DayOfWeek> byDays)
    {
      if (checkIns == null || checkIns.Count == 0)
        return 0;
      if (checkIns.Count == 1)
        return checkIns[0].Value >= checkIns[0].Goal ? 1 : 0;
      List<WeekCheckInStatInfo> source1 = new List<WeekCheckInStatInfo>();
      DateTime exact1 = DateTime.ParseExact(checkIns[checkIns.Count - 1].CheckinStamp, "yyyyMMdd", (IFormatProvider) App.Ci);
      DateTime exact2 = DateTime.ParseExact(checkIns[0].CheckinStamp, "yyyyMMdd", (IFormatProvider) App.Ci);
      for (DateTime start = exact1.AddDays((double) ((int) exact1.DayOfWeek * -1)).AddDays((double) Utils.GetWeekFromDiff(new DateTime?(exact1))); start.AddDays(7.0) > exact2; start = start.AddDays(-7.0))
      {
        WeekCheckInStatInfo weekCheckInStatInfo = new WeekCheckInStatInfo(start, start.AddDays(6.0));
        source1.Add(weekCheckInStatInfo);
      }
      if (source1.Any<WeekCheckInStatInfo>())
      {
        foreach (WeekCheckInStatInfo weekCheckInStatInfo in source1)
        {
          weekCheckInStatInfo.Count = HabitStatisticsUtils.GetMatchCountInSpan(weekCheckInStatInfo.WeekStart, weekCheckInStatInfo.WeekEnd, checkIns);
          weekCheckInStatInfo.Completed = HabitStatisticsUtils.IsByDaysMatchInWeek(weekCheckInStatInfo.WeekStart, weekCheckInStatInfo.WeekEnd, byDays, checkIns);
        }
      }
      Dictionary<int, List<WeekCheckInStatInfo>> source2 = new Dictionary<int, List<WeekCheckInStatInfo>>();
      for (int index = 0; index < source1.Count; ++index)
      {
        if (source1[index].Completed)
        {
          source2.Add(index, new List<WeekCheckInStatInfo>()
          {
            source1[index]
          });
          int num1;
          for (num1 = index; num1 < source1.Count - 1 && source1[num1 + 1].Completed; ++num1)
            source2[index].Add(source1[num1 + 1]);
          int num2 = num1 - index;
          index += num2;
        }
      }
      if (source2.Any<KeyValuePair<int, List<WeekCheckInStatInfo>>>())
      {
        int byDaysBestStreak = 0;
        for (int index = 0; index < source2.Count; ++index)
        {
          List<WeekCheckInStatInfo> source3 = source2.ElementAt<KeyValuePair<int, List<WeekCheckInStatInfo>>>(index).Value;
          WeekCheckInStatInfo weekCheckInStatInfo1 = source3.Last<WeekCheckInStatInfo>();
          WeekCheckInStatInfo weekCheckInStatInfo2 = source3.First<WeekCheckInStatInfo>();
          int num = source3.Sum<WeekCheckInStatInfo>((Func<WeekCheckInStatInfo, int>) (week => week.Count)) + HabitStatisticsUtils.GetLastContinueCount(weekCheckInStatInfo1.WeekStart.AddDays(-7.0), weekCheckInStatInfo1.WeekStart.AddDays(-1.0), checkIns, byDays) + HabitStatisticsUtils.GetFirstContinueCount(weekCheckInStatInfo2.WeekEnd.AddDays(1.0), weekCheckInStatInfo2.WeekEnd.AddDays(7.0), checkIns, byDays);
          if (num > byDaysBestStreak)
            byDaysBestStreak = num;
        }
        return byDaysBestStreak;
      }
      int byDaysBestStreak1 = 0;
      foreach (WeekCheckInStatInfo weekCheckInStatInfo in source1)
      {
        int maxMatchInSpan = HabitStatisticsUtils.GetMaxMatchInSpan(weekCheckInStatInfo.WeekStart, weekCheckInStatInfo.WeekEnd, checkIns, byDays);
        if (maxMatchInSpan >= byDaysBestStreak1)
          byDaysBestStreak1 = maxMatchInSpan;
      }
      return byDaysBestStreak1;
    }

    private static int GetFirstContinueCount(
      DateTime start,
      DateTime end,
      List<HabitCheckInModel> checkIns,
      List<DayOfWeek> byDays)
    {
      int firstContinueCount = 0;
      DateTime checkDate = start;
      while (checkDate <= end)
      {
        int num = byDays.Contains(checkDate.DayOfWeek) ? 1 : 0;
        bool flag = checkIns.Exists((Predicate<HabitCheckInModel>) (checkIn => checkIn.CheckinStamp == checkDate.Date.ToString("yyyyMMdd", (IFormatProvider) CultureInfo.InvariantCulture) && checkIn.Value >= checkIn.Goal));
        if (num == 0 || flag)
        {
          checkDate = checkDate.AddDays(1.0);
          if (flag)
            ++firstContinueCount;
        }
        else
          break;
      }
      return firstContinueCount;
    }

    private static int GetLastContinueCount(
      DateTime start,
      DateTime end,
      List<HabitCheckInModel> checkIns,
      List<DayOfWeek> byDays)
    {
      int lastContinueCount = 0;
      DateTime checkDate = end;
      while (checkDate >= start)
      {
        int num = byDays.Contains(checkDate.DayOfWeek) ? 1 : 0;
        bool flag = checkIns.Exists((Predicate<HabitCheckInModel>) (checkIn => checkIn.CheckinStamp == checkDate.Date.ToString("yyyyMMdd", (IFormatProvider) CultureInfo.InvariantCulture) && checkIn.Value >= checkIn.Goal));
        if (num == 0 || flag)
        {
          checkDate = checkDate.AddDays(-1.0);
          if (flag)
            ++lastContinueCount;
        }
        else
          break;
      }
      return lastContinueCount;
    }

    private static bool IsByDaysMatchInWeek(
      DateTime start,
      DateTime end,
      List<DayOfWeek> byDays,
      List<HabitCheckInModel> checkIns)
    {
      foreach (DayOfWeek byDay in byDays)
      {
        for (DateTime checkDate = start; checkDate <= end; checkDate = checkDate.AddDays(1.0))
        {
          if (checkDate.DayOfWeek == byDay && !checkIns.Exists((Predicate<HabitCheckInModel>) (checkIn => checkIn.CheckinStamp == checkDate.ToString("yyyyMMdd", (IFormatProvider) CultureInfo.InvariantCulture) && checkIn.Value >= checkIn.Goal)))
            return false;
        }
      }
      return true;
    }

    private static HabitStatisticsModel CalculateNDaysInWeekStatInfo(
      List<HabitCheckInModel> checkIns,
      int count)
    {
      return new HabitStatisticsModel()
      {
        TotalCheckIns = HabitStatisticsUtils.GetTotalCheckInCount((IEnumerable<HabitCheckInModel>) checkIns),
        CurrentStreak = HabitStatisticsUtils.CalculateNDaysInWeekCurrentStreakDays(checkIns, count),
        MaxStreak = HabitStatisticsUtils.CalculateNDaysInWeekBestStreak(checkIns, count)
      };
    }

    private static int CalculateNDaysInWeekBestStreak(List<HabitCheckInModel> checkIns, int count)
    {
      int result;
      List<HabitCheckInModel> list = checkIns != null ? checkIns.Where<HabitCheckInModel>((Func<HabitCheckInModel, bool>) (c => int.TryParse(c.CheckinStamp, out result) && result > 20000101)).ToList<HabitCheckInModel>() : (List<HabitCheckInModel>) null;
      if (list == null || list.Count == 0)
        return 0;
      if (list.Count == 1)
        return list[0].Value >= list[0].Goal ? 1 : 0;
      List<WeekCheckInStatInfo> source1 = new List<WeekCheckInStatInfo>();
      DateTime exact1 = DateTime.ParseExact(list[list.Count - 1].CheckinStamp, "yyyyMMdd", (IFormatProvider) App.Ci);
      DateTime exact2 = DateTime.ParseExact(list[0].CheckinStamp, "yyyyMMdd", (IFormatProvider) App.Ci);
      for (DateTime start = exact1.AddDays((double) ((int) exact1.DayOfWeek * -1)).AddDays((double) Utils.GetWeekFromDiff(new DateTime?(exact1))); start.AddDays(7.0) > exact2; start = start.AddDays(-7.0))
      {
        WeekCheckInStatInfo weekCheckInStatInfo = new WeekCheckInStatInfo(start, start.AddDays(6.0));
        source1.Add(weekCheckInStatInfo);
      }
      if (source1.Any<WeekCheckInStatInfo>())
      {
        foreach (WeekCheckInStatInfo weekCheckInStatInfo in source1)
        {
          weekCheckInStatInfo.Count = HabitStatisticsUtils.GetMatchCountInSpan(weekCheckInStatInfo.WeekStart, weekCheckInStatInfo.WeekEnd, list);
          weekCheckInStatInfo.Completed = weekCheckInStatInfo.Count >= count || weekCheckInStatInfo.WeekStart <= DateTime.Today && weekCheckInStatInfo.WeekEnd >= DateTime.Today;
        }
      }
      Dictionary<int, List<WeekCheckInStatInfo>> source2 = new Dictionary<int, List<WeekCheckInStatInfo>>();
      int index1;
      for (int index2 = 0; index2 < source1.Count; index2 = Math.Max(index2, index1 - 1) + 1)
      {
        source2.Add(index2, new List<WeekCheckInStatInfo>()
        {
          source1[index2]
        });
        index1 = index2;
        while (index1 < source1.Count - 1 && (source1[index1].Completed || source1[index1 + 1].Completed))
        {
          ++index1;
          source2[index2].Add(source1[index1]);
          if (!source1[index1].Completed)
            break;
        }
      }
      if (!source2.Any<KeyValuePair<int, List<WeekCheckInStatInfo>>>())
        return HabitStatisticsUtils.CalculateDailyBestStreak(list, 1);
      int inWeekBestStreak = 0;
      for (int index3 = 0; index3 < source2.Count; ++index3)
      {
        int num = source2.ElementAt<KeyValuePair<int, List<WeekCheckInStatInfo>>>(index3).Value.Sum<WeekCheckInStatInfo>((Func<WeekCheckInStatInfo, int>) (week => week.Count));
        if (num > inWeekBestStreak)
          inWeekBestStreak = num;
      }
      return inWeekBestStreak;
    }

    private static int GetMaxMatchInSpan(
      DateTime start,
      DateTime end,
      List<HabitCheckInModel> checkIns,
      List<DayOfWeek> byDays)
    {
      int matchCountInSpan = HabitStatisticsUtils.GetMatchCountInSpan(start, end, checkIns);
      int startContinuousDays1 = HabitStatisticsUtils.GetWeekStartContinuousDays(start, checkIns, byDays);
      int endContinuousDays1 = HabitStatisticsUtils.GetWeekEndContinuousDays(end, checkIns, byDays);
      int endContinuousDays2 = HabitStatisticsUtils.GetWeekEndContinuousDays(start.AddDays(-1.0), checkIns, byDays);
      int startContinuousDays2 = HabitStatisticsUtils.GetWeekStartContinuousDays(end.AddDays(1.0), checkIns, byDays);
      if (startContinuousDays1 <= 0 && endContinuousDays1 <= 0)
        return matchCountInSpan;
      if (startContinuousDays1 > 0 && endContinuousDays1 <= 0)
        return startContinuousDays1 + endContinuousDays2;
      if (startContinuousDays1 <= 0 && endContinuousDays1 > 0)
        return endContinuousDays1 + startContinuousDays2;
      if (startContinuousDays1 <= 0 || endContinuousDays1 <= 0)
        return matchCountInSpan;
      int num1 = startContinuousDays1 + endContinuousDays2;
      int num2 = endContinuousDays1 + startContinuousDays2;
      return num1 <= num2 ? num2 : num1;
    }

    private static int GetWeekStartContinuousDays(
      DateTime start,
      List<HabitCheckInModel> checkIns,
      List<DayOfWeek> byDays)
    {
      int startContinuousDays = 0;
      for (int index = 0; index < 7; ++index)
      {
        DateTime currentDay = start.AddDays((double) index);
        int num = checkIns.Exists((Predicate<HabitCheckInModel>) (day => day.Value >= day.Goal && day.CheckinStamp == currentDay.ToString("yyyyMMdd", (IFormatProvider) CultureInfo.InvariantCulture))) ? 1 : 0;
        bool flag = byDays.Contains(currentDay.DayOfWeek);
        if (num != 0)
          ++startContinuousDays;
        if (num == 0 & flag)
          break;
      }
      return startContinuousDays;
    }

    private static int GetWeekEndContinuousDays(
      DateTime end,
      List<HabitCheckInModel> checkIns,
      List<DayOfWeek> byDays)
    {
      int endContinuousDays = 0;
      for (int index = 0; index < 7; ++index)
      {
        DateTime currentDay = end.AddDays((double) (index * -1));
        int num = checkIns.Exists((Predicate<HabitCheckInModel>) (day => day.Value >= day.Goal && day.CheckinStamp == currentDay.ToString("yyyyMMdd", (IFormatProvider) CultureInfo.InvariantCulture))) ? 1 : 0;
        bool flag = byDays.Contains(currentDay.DayOfWeek);
        if (num != 0)
          ++endContinuousDays;
        if (num == 0 & flag)
          break;
      }
      return endContinuousDays;
    }

    private static int GetMatchCountInSpan(
      DateTime start,
      DateTime end,
      List<HabitCheckInModel> checkIns)
    {
      int matchCountInSpan = 0;
      for (DateTime checkDate = start; checkDate <= end; checkDate = checkDate.AddDays(1.0))
      {
        if (checkIns.Exists((Predicate<HabitCheckInModel>) (checkIn => checkIn.CheckinStamp == checkDate.ToString("yyyyMMdd", (IFormatProvider) CultureInfo.InvariantCulture) && checkIn.IsComplete())))
          ++matchCountInSpan;
      }
      return matchCountInSpan;
    }

    private static int GetTotalCheckInCount(IEnumerable<HabitCheckInModel> checkIns)
    {
      return checkIns == null ? 0 : checkIns.Count<HabitCheckInModel>((Func<HabitCheckInModel, bool>) (checkIn => checkIn.Value >= checkIn.Goal));
    }

    private static HabitStatisticsModel CalculateDailyStatInfo(
      List<HabitCheckInModel> checkIns,
      int interval)
    {
      return new HabitStatisticsModel()
      {
        TotalCheckIns = HabitStatisticsUtils.GetTotalCheckInCount((IEnumerable<HabitCheckInModel>) checkIns),
        CurrentStreak = HabitStatisticsUtils.CalculateDailyCurrentStreak(checkIns, interval),
        MaxStreak = HabitStatisticsUtils.CalculateDailyBestStreak(checkIns, interval)
      };
    }

    private static int CalculateDailyBestStreak(List<HabitCheckInModel> checkIns, int interval)
    {
      int result;
      List<HabitCheckInModel> list1 = checkIns != null ? checkIns.Where<HabitCheckInModel>((Func<HabitCheckInModel, bool>) (c => int.TryParse(c.CheckinStamp, out result) && result > 20000101)).ToList<HabitCheckInModel>() : (List<HabitCheckInModel>) null;
      if (list1 == null || list1.Count == 0)
        return 0;
      if (list1.Count == 1)
        return list1[0].Value >= list1[0].Goal ? 1 : 0;
      List<HabitCheckInModel> list2 = list1.Where<HabitCheckInModel>((Func<HabitCheckInModel, bool>) (c => c.IsComplete())).ToList<HabitCheckInModel>();
      int val1 = 0;
      if (list2.Count >= 1)
      {
        List<DateTime> list3 = list2.Select<HabitCheckInModel, DateTime>((Func<HabitCheckInModel, DateTime>) (ch => DateTime.ParseExact(ch.CheckinStamp, "yyyyMMdd", (IFormatProvider) CultureInfo.InvariantCulture))).ToList<DateTime>();
        list3.Sort();
        int val2 = 0;
        DateTime dateTime1 = list3.Min<DateTime>();
        foreach (DateTime dateTime2 in list3)
        {
          if ((dateTime2 - dateTime1).TotalDays <= (double) interval)
          {
            ++val2;
          }
          else
          {
            val1 = Math.Max(val1, val2);
            val2 = 1;
          }
          dateTime1 = dateTime2;
        }
        val1 = Math.Max(val1, val2);
      }
      return val1;
    }

    private static int CalculateDailyCurrentStreak(List<HabitCheckInModel> checkIns, int interval)
    {
      int result;
      List<HabitCheckInModel> list1 = checkIns != null ? checkIns.Where<HabitCheckInModel>((Func<HabitCheckInModel, bool>) (c => int.TryParse(c.CheckinStamp, out result) && result > 20000101)).ToList<HabitCheckInModel>() : (List<HabitCheckInModel>) null;
      if (checkIns == null || checkIns.Count == 0 || list1.Last<HabitCheckInModel>((Func<HabitCheckInModel, bool>) (checkIn => checkIn.IsComplete())) == null)
        return 0;
      int dailyCurrentStreak = 0;
      try
      {
        List<DateTime> list2 = list1.Select<HabitCheckInModel, DateTime>((Func<HabitCheckInModel, DateTime>) (checkIn => DateTime.ParseExact(checkIn.CheckinStamp, "yyyyMMdd", (IFormatProvider) App.Ci))).ToList<DateTime>();
        list2.Sort((Comparison<DateTime>) ((a, b) => b.CompareTo(a)));
        DateTime dateTime1 = DateTime.Today;
        foreach (DateTime dateTime2 in list2)
        {
          if ((dateTime1 - dateTime2).TotalDays <= (double) interval)
          {
            ++dailyCurrentStreak;
            dateTime1 = dateTime2;
          }
          else
            break;
        }
      }
      catch (Exception ex)
      {
      }
      return dailyCurrentStreak;
    }
  }
}
