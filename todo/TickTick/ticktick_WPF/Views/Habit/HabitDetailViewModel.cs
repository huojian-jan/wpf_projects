// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Habit.HabitDetailViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using ticktick_WPF.Models;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Habit
{
  public class HabitDetailViewModel : BaseViewModel
  {
    public HabitDetailViewModel(HabitModel habit)
    {
      this.Habit = habit;
      this.Id = habit.Id;
      this.Name = habit.Name;
      this.Type = habit.Type;
      if (habit.IconRes.StartsWith("txt"))
        this.IconText = ((IEnumerable<string>) habit.IconRes.Split('_')).LastOrDefault<string>() ?? "";
      else
        this.ImageUrl = "pack://application:,,,/Assets/Habits/" + habit.IconRes.ToLower() + ".png";
      this.IsBoolHabit = habit.Type.ToLower() == "boolean";
      this.Unit = HabitUtils.GetUnitText(habit.Unit);
      this.Goal = habit.Goal;
      this.Step = habit.Step;
      this.AutoAmount = "+" + habit.Step.ToString() + " " + HabitUtils.GetUnitText(habit.Unit);
      this.Color = habit.Color;
      this.TargetDays = habit.TargetDays.GetValueOrDefault();
      this.CompleteCycles = habit.TotalCheckIns;
    }

    public int TargetDays { get; set; }

    public int CompleteCycles { get; set; }

    public HabitModel Habit { get; set; }

    public string Id { get; set; }

    public string Type { get; set; }

    public bool IsBoolHabit { get; set; }

    public string Unit { get; set; }

    public string Name { get; set; }

    public string ImageUrl { get; set; } = "";

    public double Goal { get; set; }

    public double Value { get; set; }

    public DateTime PivotDate { get; set; }

    public int CompletedRatio { get; set; }

    public double TotalCheckInCount { get; set; }

    public HabitCheckInModel TodayCheckIn { get; set; }

    public int Status { get; set; }

    public double Step { get; set; }

    public string AutoAmount { get; set; }

    public string Color { get; set; }

    public string IconText { get; set; } = "";
  }
}
