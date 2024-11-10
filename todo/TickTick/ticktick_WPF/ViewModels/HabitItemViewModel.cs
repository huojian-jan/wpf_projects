// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.ViewModels.HabitItemViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Util;
using ticktick_WPF.Views.Habit;

#nullable disable
namespace ticktick_WPF.ViewModels
{
  public class HabitItemViewModel : HabitItemBaseViewModel
  {
    private bool _showCheckIns;
    private bool _showCompletedCycles;
    private int _currentStreak;

    public HabitListItem SelfControl { get; set; }

    public HabitModel Habit { get; set; }

    public bool IsSelected { get; set; }

    public override string Id => this.Habit.Id;

    public string IconText { get; set; }

    public int CompletedCycles
    {
      get => this.Habit.CompletedCycles.GetValueOrDefault();
      set
      {
        this.Habit.CompletedCycles = new int?(value);
        this.OnPropertyChanged(nameof (CompletedCycles));
        this.OnPropertyChanged("CompletedCyclesDisplayStr");
      }
    }

    public string CompletedCyclesDisplayStr
    {
      get
      {
        int? completedCycles = this.Habit.CompletedCycles;
        int num = 0;
        return completedCycles.GetValueOrDefault() > num & completedCycles.HasValue ? string.Format(Utils.GetString("HabitNCyclesCompleted"), (object) this.Habit.CompletedCycles) : "";
      }
    }

    public string Name
    {
      get => this.Habit.Name;
      set
      {
        this.Habit.Name = value;
        this.OnPropertyChanged(nameof (Name));
      }
    }

    public string IconRes
    {
      get => this.Habit.IconRes;
      set
      {
        this.Habit.IconRes = value;
        this.OnPropertyChanged("IconUrl");
      }
    }

    public string Color
    {
      get => this.Habit.Color;
      set
      {
        this.Habit.Color = value;
        this.OnPropertyChanged(nameof (Color));
      }
    }

    public new long SortOrder => this.Habit.SortOrder;

    public int TotalCheckIns
    {
      get => this.Habit.TotalCheckIns;
      set
      {
        this.Habit.TotalCheckIns = value;
        this.OnPropertyChanged("KeepingDays");
      }
    }

    public bool ShowCheckIns
    {
      get => this._showCheckIns;
      set
      {
        this._showCheckIns = value;
        this.OnPropertyChanged("IsArchive");
      }
    }

    public bool ShowCompletedCycles
    {
      get => this._showCompletedCycles;
      set
      {
        this._showCompletedCycles = value;
        this.OnPropertyChanged(nameof (ShowCompletedCycles));
      }
    }

    public int CurrentStreak
    {
      get => this._currentStreak;
      set
      {
        this._currentStreak = value;
        this.OnPropertyChanged(nameof (CurrentStreak));
      }
    }

    public override bool Archived => this.Habit.Status == 1;

    public string IconUrl => "../../Assets/Habits/" + this.IconRes.ToLower() + ".png";

    public bool IsIcon => !this.IconRes.Contains("txt");

    public string KeepTotalStr
    {
      get
      {
        int? targetDays = this.Habit.TargetDays;
        int num = 0;
        return targetDays.GetValueOrDefault() > num & targetDays.HasValue ? Utils.GetString("HabitCurrentInsist") : Utils.GetString("KeepTotal");
      }
    }

    public string KeepingDays
    {
      get
      {
        int? targetDays = this.Habit.TargetDays;
        int num = 0;
        if (!(targetDays.GetValueOrDefault() > num & targetDays.HasValue))
          return this.TotalCheckIns.ToString() + " " + Utils.GetString(this.TotalCheckIns > 1 ? "PublicUpDays" : "PublicUpDay");
        List<CompletedCycle> completedCyclesList = this.Habit.CompletedCyclesList;
        if (completedCyclesList == null || !completedCyclesList.Any<CompletedCycle>())
          return "0 / " + this.Habit.TargetDays.ToString();
        CompletedCycle completedCycle = completedCyclesList.Last<CompletedCycle>();
        return completedCycle.Streak.ToString() + " / " + completedCycle.TargetDays.ToString();
      }
    }

    public ObservableCollection<HabitDayCheckModel> WeekCheckIns { get; set; } = new ObservableCollection<HabitDayCheckModel>();

    public HabitItemViewModel(HabitModel habit)
    {
      this.Habit = habit;
      this.IconText = ((IEnumerable<string>) habit.IconRes.Split('_')).LastOrDefault<string>() ?? "";
      this.ShowCheckIns = habit.Status == 0;
      this.ShowCompletedCycles = habit.Status == 1;
      this.UpdateTotalCheckDaysAndCurrentStreak();
    }

    public void SetCheckIns(IEnumerable<HabitCheckInModel> checkIns)
    {
      List<HabitCheckInModel> list = checkIns != null ? checkIns.Where<HabitCheckInModel>((Func<HabitCheckInModel, bool>) (c => c.HabitId == this.Id)).ToList<HabitCheckInModel>() : (List<HabitCheckInModel>) null;
      for (int index1 = -6; index1 < 1; ++index1)
      {
        DateTime dateTime = DateTime.Today;
        dateTime = dateTime.AddDays((double) index1);
        string stamp = dateTime.ToString("yyyyMMdd");
        HabitCheckInModel habitCheckInModel = list != null ? list.FirstOrDefault<HabitCheckInModel>((Func<HabitCheckInModel, bool>) (c => c.CheckinStamp == stamp)) : (HabitCheckInModel) null;
        if (this.WeekCheckIns.Count > index1 + 6)
        {
          ObservableCollection<HabitDayCheckModel> weekCheckIns = this.WeekCheckIns;
          int index2 = index1 + 6;
          HabitCheckInModel checkIn = habitCheckInModel;
          dateTime = DateTime.Today;
          DateTime date = dateTime.AddDays((double) index1);
          HabitModel habit = this.Habit;
          HabitDayCheckModel habitDayCheckModel = new HabitDayCheckModel(checkIn, date, habit);
          weekCheckIns[index2] = habitDayCheckModel;
        }
        else
        {
          ObservableCollection<HabitDayCheckModel> weekCheckIns = this.WeekCheckIns;
          HabitCheckInModel checkIn = habitCheckInModel;
          dateTime = DateTime.Today;
          DateTime date = dateTime.AddDays((double) index1);
          HabitModel habit = this.Habit;
          HabitDayCheckModel habitDayCheckModel = new HabitDayCheckModel(checkIn, date, habit);
          weekCheckIns.Add(habitDayCheckModel);
        }
      }
    }

    public async Task UpdateWeekCheckIns()
    {
      HabitItemViewModel habitItemViewModel = this;
      List<string> ids = new List<string>();
      ids.Add(habitItemViewModel.Id);
      DateTime dateTime = DateTime.Today;
      List<HabitCheckInModel> checkInsByIdsInSpan = await HabitCheckInDao.GetCheckInsByIdsInSpan(ids, dateTime.AddDays(-6.0), DateTime.Today);
      for (int index1 = -6; index1 < 1; ++index1)
      {
        dateTime = DateTime.Today;
        dateTime = dateTime.AddDays((double) index1);
        string stamp = dateTime.ToString("yyyyMMdd", (IFormatProvider) CultureInfo.InvariantCulture);
        HabitCheckInModel habitCheckInModel = checkInsByIdsInSpan != null ? checkInsByIdsInSpan.FirstOrDefault<HabitCheckInModel>((Func<HabitCheckInModel, bool>) (c => c.CheckinStamp == stamp)) : (HabitCheckInModel) null;
        if (habitItemViewModel.WeekCheckIns.Count > index1 + 6)
        {
          ObservableCollection<HabitDayCheckModel> weekCheckIns = habitItemViewModel.WeekCheckIns;
          int index2 = index1 + 6;
          HabitCheckInModel checkIn = habitCheckInModel;
          dateTime = DateTime.Today;
          DateTime date = dateTime.AddDays((double) index1);
          HabitModel habit = habitItemViewModel.Habit;
          HabitDayCheckModel habitDayCheckModel = new HabitDayCheckModel(checkIn, date, habit);
          weekCheckIns[index2] = habitDayCheckModel;
        }
        else
        {
          ObservableCollection<HabitDayCheckModel> weekCheckIns = habitItemViewModel.WeekCheckIns;
          HabitCheckInModel checkIn = habitCheckInModel;
          dateTime = DateTime.Today;
          DateTime date = dateTime.AddDays((double) index1);
          HabitModel habit = habitItemViewModel.Habit;
          HabitDayCheckModel habitDayCheckModel = new HabitDayCheckModel(checkIn, date, habit);
          weekCheckIns.Add(habitDayCheckModel);
        }
      }
    }

    public async Task UpdateTotalCheckDaysAndCurrentStreak()
    {
      HabitStatisticsModel habitStatInfo = await HabitStatisticsUtils.CalculateHabitStatInfo(this.Habit.Id, this.Habit.RepeatRule);
      if (habitStatInfo == null)
      {
        this.TotalCheckIns = 0;
        this.CurrentStreak = 0;
      }
      else
      {
        this.TotalCheckIns = habitStatInfo.TotalCheckIns;
        this.CurrentStreak = habitStatInfo.CurrentStreak;
      }
    }

    public void SetCyclesList(List<CompletedCycle> newCycles)
    {
      this.Habit.CompletedCyclesList = newCycles;
      this.OnPropertyChanged("KeepingDays");
    }
  }
}
