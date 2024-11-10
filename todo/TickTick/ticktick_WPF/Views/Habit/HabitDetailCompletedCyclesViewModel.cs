// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Habit.HabitDetailCompletedCyclesViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ticktick_WPF.Models;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Habit
{
  public class HabitDetailCompletedCyclesViewModel : BaseViewModel
  {
    private ImageSource _icon;

    public HabitDetailCompletedCyclesViewModel(HabitModel habit, int cycle)
    {
      int? targetDays = habit.TargetDays;
      int num1 = 0;
      if (targetDays.GetValueOrDefault() <= num1 & targetDays.HasValue)
        return;
      int? nullable = habit.TargetDays;
      this.TargetDays = nullable.GetValueOrDefault();
      nullable = habit.CompletedCycles;
      this.CompleteCycles = nullable.GetValueOrDefault();
      if (habit.CompletedCyclesList == null || habit.CompletedCyclesList.Count == 0 || habit.CompletedCyclesList.Count < cycle)
      {
        this.CurrentTargetDays = 0;
        this.CurrentCyclesStartDate = string.Format(Utils.GetString("HabitCompleteNDayLeft"), (object) (this.TargetDays - this.CurrentTargetDays));
        this.IsCurrentCyclesCompleted = false;
      }
      else
      {
        int num2 = cycle;
        nullable = habit.CompletedCycles;
        int valueOrDefault = nullable.GetValueOrDefault();
        if (num2 >= valueOrDefault & nullable.HasValue)
          cycle = habit.CompletedCyclesList.Count - 1;
        if (cycle > habit.CompletedCyclesList.Count - 1 || cycle < 0)
          return;
        this.CurrentTargetDays = habit.CompletedCyclesList[cycle].Streak;
        if (this.CurrentTargetDays != this.TargetDays)
        {
          this.CurrentCyclesStartDate = string.Format(Utils.GetString("HabitCompleteNDayLeft"), (object) (this.TargetDays - this.CurrentTargetDays));
        }
        else
        {
          this.CurrentCyclesStartDate = DateUtils.FormatShortDate(DateUtils.ParseDateTime(habit.CompletedCyclesList[cycle].StartDate.ToString()));
          this.CurrentCyclesEndDate = " - " + DateUtils.FormatShortDate(DateUtils.ParseDateTime(habit.CompletedCyclesList[cycle].EndDate.ToString()));
        }
        this.IsCurrentCyclesCompleted = habit.CompletedCyclesList[cycle].isComplete;
      }
      this.UpdateIcon();
    }

    public void UpdateIcon()
    {
      this.Icon = this.IsCurrentCyclesCompleted ? (Application.Current?.FindResource((object) "HabitCompletedCycles") as Image).Source : (Application.Current?.FindResource((object) "HabitUncompletedCycles") as Image).Source;
    }

    public ImageSource Icon
    {
      get => this._icon;
      set
      {
        this._icon = value;
        this.OnPropertyChanged(nameof (Icon));
      }
    }

    public bool IsCurrentCyclesCompleted { get; set; }

    public string CurrentCyclesStartDate { get; set; }

    public string CurrentCyclesEndDate { get; set; }

    public int CurrentTargetDays { get; set; }

    public int TargetDays { get; set; }

    public int CompleteCycles { get; set; }
  }
}
