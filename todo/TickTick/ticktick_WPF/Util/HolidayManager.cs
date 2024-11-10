// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.HolidayManager
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;

#nullable disable
namespace ticktick_WPF.Util
{
  public static class HolidayManager
  {
    private static List<HolidayModel> _holidays;
    private static readonly ConcurrentDictionary<DateTime, HolidayModel> HolidayDict = new ConcurrentDictionary<DateTime, HolidayModel>();
    private static List<string> _twentyFourDay = new List<string>()
    {
      "小寒",
      "大寒",
      "立春",
      "雨水",
      "惊蛰",
      "春分",
      "谷雨",
      "立夏",
      "小满",
      "芒种",
      "夏至",
      "小暑",
      "大暑",
      "立秋",
      "处暑",
      "白露",
      "秋分",
      "寒露",
      "霜降",
      "立冬",
      "小雪",
      "大雪",
      "冬至"
    };

    public static async Task TryPullRemoteHolidays()
    {
      long lastPullHolidayTime = LocalSettings.Settings.ExtraSettings.LastPullHolidayTime;
      bool needPull = false;
      if (lastPullHolidayTime == 0L)
      {
        LocalSettings.Settings.ExtraSettings.LastPullHolidayTime = DateTime.Now.Ticks;
        needPull = true;
      }
      else
      {
        DateTime now = DateTime.Now;
        if (new TimeSpan(now.Ticks - lastPullHolidayTime).TotalDays >= 1.0)
        {
          ExtraSettings extraSettings = LocalSettings.Settings.ExtraSettings;
          now = DateTime.Now;
          long ticks = now.Ticks;
          extraSettings.LastPullHolidayTime = ticks;
          needPull = true;
        }
      }
      HolidayManager._holidays = await HolidayDao.GetRecentHolidays();
      if (HolidayManager._holidays != null && HolidayManager._holidays.Count > 0)
      {
        if (Utils.IsJp())
        {
          if (HolidayManager._holidays.Exists((Predicate<HolidayModel>) (item => item.region != HolidayRegion.jp)))
            needPull = true;
        }
        else if (Utils.IsCn() && HolidayManager._holidays.Exists((Predicate<HolidayModel>) (item => item.region != HolidayRegion.cn)))
          needPull = true;
      }
      if (HolidayManager._holidays == null || !HolidayManager._holidays.Any<HolidayModel>())
        needPull = true;
      if (!needPull)
        return;
      HolidayManager._holidays?.Clear();
      await HolidayManager.RefreshDataFromRemote();
    }

    public static async Task RefreshDataFromRemote()
    {
      if (!Utils.IsShowHoliday())
        return;
      List<HolidayModel> holidays = await Communicator.GetRecentHoliday();
      if (holidays != null && holidays.Count > 0)
      {
        await HolidayDao.ClearHolidays();
        await HolidayDao.SaveHolidays(holidays);
        HolidayManager.GetRecentHolidays();
      }
      holidays = (List<HolidayModel>) null;
    }

    public static List<HolidayModel> GetHolidays() => HolidayManager._holidays;

    public static async Task<List<HolidayModel>> GetRecentHolidays()
    {
      if (!Utils.IsShowHoliday())
        return new List<HolidayModel>();
      if (HolidayManager._holidays != null && HolidayManager._holidays.Count > 0)
        return HolidayManager._holidays;
      List<HolidayModel> recentHolidays = await HolidayDao.GetRecentHolidays();
      if (recentHolidays == null || recentHolidays.Count == 0)
      {
        await HolidayManager.RefreshDataFromRemote();
        HolidayManager._holidays = await HolidayDao.GetRecentHolidays();
      }
      else
        HolidayManager._holidays = recentHolidays;
      return HolidayManager._holidays;
    }

    public static async Task ForcePullHolidays()
    {
      await HolidayManager.RefreshDataFromRemote();
      HolidayManager._holidays = await HolidayDao.GetRecentHolidays();
    }

    public static List<HolidayModel> GetCacheHolidays()
    {
      return !Utils.IsShowHoliday() ? new List<HolidayModel>() : HolidayManager._holidays ?? new List<HolidayModel>();
    }

    public static HolidayModel GetHoliday(DateTime date)
    {
      if (HolidayManager.HolidayDict.Count == 0)
      {
        List<HolidayModel> holidays = HolidayManager._holidays;
        // ISSUE: explicit non-virtual call
        if ((holidays != null ? (__nonvirtual (holidays.Count) > 0 ? 1 : 0) : 0) != 0)
        {
          foreach (HolidayModel holiday in HolidayManager._holidays)
            HolidayManager.HolidayDict.TryAdd(holiday.date, holiday);
        }
      }
      HolidayModel holiday1;
      HolidayManager.HolidayDict.TryGetValue(date, out holiday1);
      return holiday1;
    }

    public static bool IsHolidayText(string text)
    {
      bool? nullable = LocalSettings.Settings.UserPreference.alternativeCalendar?.calendar.Equals("lunar");
      return (!nullable.HasValue || nullable.Value) && (string.IsNullOrWhiteSpace(text) || !text.Any<char>(new Func<char, bool>(char.IsNumber)) && !text.Contains("初") && !text.Contains("廿") && !text.Contains("十") && !text.Contains("三") && !text.Contains("月") && !HolidayManager._twentyFourDay.Contains(text));
    }

    public static bool IsWorkDay(DateTime date)
    {
      List<HolidayModel> holidayModelList = !LocalSettings.Settings.EnableHoliday || !Utils.IsDida() ? new List<HolidayModel>() : HolidayManager.GetCacheHolidays();
      if (holidayModelList.Exists((Predicate<HolidayModel>) (model => model.date.Date == date.Date && model.type == 1)))
        return true;
      return !holidayModelList.Exists((Predicate<HolidayModel>) (model => model.date.Date == date.Date && model.type == 0)) && date.DayOfWeek >= DayOfWeek.Monday && date.DayOfWeek <= DayOfWeek.Friday;
    }

    public static bool IsNonWorkWeekend(DateTime date)
    {
      return (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday) && !(!LocalSettings.Settings.EnableHoliday || !Utils.IsDida() ? new List<HolidayModel>() : HolidayManager.GetCacheHolidays()).Exists((Predicate<HolidayModel>) (model => model.date.Date == date.Date && model.type == 1));
    }
  }
}
