// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Calendar.Month.MonthGridViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Calendar.Month
{
  public class MonthGridViewModel : BaseViewModel
  {
    private bool _showLastWeek;

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public List<MonthDayViewModel> Models { get; private set; }

    public Dictionary<int, List<MonthDayViewModel>> ModelDict { get; private set; }

    public bool ShowLastWeek
    {
      get => this._showLastWeek;
      set
      {
        this._showLastWeek = value;
        this.OnPropertyChanged(nameof (ShowLastWeek));
      }
    }

    public static MonthGridViewModel Build(DateTime startDate, int weeks)
    {
      DateTime end = startDate.AddDays((double) (weeks * 7 - 1));
      int month = DateUtils.GetCurrentMonthDate(startDate, end).Month;
      List<HolidayModel> holidayModelList = !LocalSettings.Settings.EnableHoliday || !Utils.IsDida() ? new List<HolidayModel>() : HolidayManager.GetCacheHolidays();
      Dictionary<int, List<MonthDayViewModel>> dictionary = new Dictionary<int, List<MonthDayViewModel>>();
      List<MonthDayViewModel> monthDayViewModelList1 = new List<MonthDayViewModel>();
      for (int key = -1; key < weeks + 1; ++key)
      {
        List<MonthDayViewModel> monthDayViewModelList2 = new List<MonthDayViewModel>();
        for (int index = 0; index < 7; ++index)
        {
          DateTime current = startDate.AddDays((double) (key * 7 + index));
          bool flag1 = holidayModelList.Exists((Predicate<HolidayModel>) (day => day.date == current.Date && day.type == 1)) && SettingsHelper.GetShowHoliday();
          bool flag2 = holidayModelList.Exists((Predicate<HolidayModel>) (day => day.date == current.Date && day.type == 0)) && SettingsHelper.GetShowHoliday();
          bool flag3 = current.Date.DayOfWeek == DayOfWeek.Saturday || current.Date.DayOfWeek == DayOfWeek.Sunday;
          bool flag4 = current.Date.Month == month;
          MonthDayViewModel monthDayViewModel = new MonthDayViewModel()
          {
            ShowMonthday = true,
            Date = current,
            IsToday = current.Date == DateTime.Today,
            IsWeekend = flag3,
            IsCurrentMonth = flag4,
            IsWorkDay = flag1,
            IsRestDay = flag2
          };
          monthDayViewModelList2.Add(monthDayViewModel);
          monthDayViewModelList1.Add(monthDayViewModel);
        }
        dictionary[key] = monthDayViewModelList2;
      }
      return new MonthGridViewModel()
      {
        StartDate = startDate,
        EndDate = startDate.AddDays((double) (weeks * 7 - 1)),
        ShowLastWeek = weeks == 6,
        ModelDict = dictionary,
        Models = monthDayViewModelList1
      };
    }

    public static DateTime GetDisplayStartDate(DateTime pivotDate)
    {
      DateTime dateTime1 = pivotDate.AddDays((double) (1 - pivotDate.Day));
      DateTime dateTime2 = dateTime1.AddDays((double) ((int) dateTime1.DayOfWeek * -1)).AddDays((double) Utils.GetWeekFromDiff(new DateTime?(dateTime1)));
      return pivotDate.Month != dateTime2.Month || dateTime2.Day == 1 ? dateTime2 : dateTime2.AddDays(-7.0);
    }

    public DateTime GetCurrentMonthDate()
    {
      return DateUtils.GetCurrentMonthDate(this.StartDate, this.EndDate);
    }
  }
}
