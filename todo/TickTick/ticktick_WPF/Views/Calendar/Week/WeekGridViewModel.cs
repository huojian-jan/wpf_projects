// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Calendar.Week.WeekGridViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Calendar.Month;

#nullable disable
namespace ticktick_WPF.Views.Calendar.Week
{
  public class WeekGridViewModel : BaseViewModel
  {
    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public bool ShowWeekend { get; set; }

    public List<MonthDayViewModel> Models { get; private set; }

    public static DateTime GetDisplayStartDate(DateTime pivotDate)
    {
      return pivotDate.AddDays((double) ((int) pivotDate.DayOfWeek * -1)).AddDays((double) Utils.GetWeekFromDiff(new DateTime?(pivotDate)));
    }

    public static WeekGridViewModel Build(DateTime date, int days, bool showWeekend = true)
    {
      DateTime dateTime = date;
      List<HolidayModel> holidayModelList = !LocalSettings.Settings.EnableHoliday || !Utils.IsDida() ? new List<HolidayModel>() : HolidayManager.GetCacheHolidays();
      int month = dateTime.AddDays(3.0).Month;
      List<MonthDayViewModel> monthDayViewModelList = new List<MonthDayViewModel>();
      DateTime current = dateTime.AddDays(-1.0);
      for (int index = 0; index < days + 2; ++index)
      {
        bool flag1 = current.Date.DayOfWeek == DayOfWeek.Saturday || current.Date.DayOfWeek == DayOfWeek.Sunday;
        if (!showWeekend)
        {
          while (DateUtils.IsWeekEnds(current))
            current = current.AddDays(index == 0 ? -1.0 : 1.0);
        }
        bool flag2 = holidayModelList.Exists((Predicate<HolidayModel>) (day => day.date == current.Date && day.type == 1)) && SettingsHelper.GetShowHoliday();
        bool flag3 = holidayModelList.Exists((Predicate<HolidayModel>) (day => day.date == current.Date && day.type == 0)) && SettingsHelper.GetShowHoliday();
        monthDayViewModelList.Add(new MonthDayViewModel()
        {
          Date = current,
          IsToday = current.Date == DateTime.Today,
          IsCurrentMonth = month == current.Month,
          IsWeekend = flag1,
          IsWorkDay = flag2,
          IsRestDay = flag3
        });
        current = current.AddDays(1.0);
      }
      return new WeekGridViewModel()
      {
        StartDate = dateTime,
        EndDate = dateTime.AddDays((double) (days - 1 + (!showWeekend ? 2 : 0))),
        Models = monthDayViewModelList,
        ShowWeekend = showWeekend
      };
    }
  }
}
