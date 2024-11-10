// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.ViewModels.HabitBaseViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using ticktick_WPF.Models;

#nullable disable
namespace ticktick_WPF.ViewModels
{
  public class HabitBaseViewModel : TaskBaseViewModel
  {
    public HabitModel Habit { get; set; }

    public HabitCheckInModel HabitCheckIn { get; set; }

    public HabitBaseViewModel(HabitModel habit)
    {
      this.Id = habit.Id;
      this.Type = DisplayType.Habit;
      this.Title = habit.Name;
      this.Priority = 0;
      this.Status = 0;
      this.Habit = habit;
      this.SortOrder = habit.SortOrder;
      this.StartDate = new DateTime?(DateTime.Today);
      this.IsAllDay = new bool?(true);
      this.ReminderString = habit.Reminder;
    }
  }
}
