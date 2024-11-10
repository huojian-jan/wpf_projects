// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Habit.HabitStatisticsViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Habit
{
  public class HabitStatisticsViewModel
  {
    public int MonthCheckinCount { get; set; }

    public int TotalCheckinCount { get; set; }

    public string Unit { get; set; }

    public int MonthCheckinRate { get; set; }

    public double MonthCompletion { get; set; }

    public double TotalCompletion { get; set; }

    public int CurrentStreak { get; set; }

    public int BestStreak { get; set; }

    public string MonthCheckUnit => this.GetDayOrDays(this.MonthCheckinCount);

    public string TotalCheckUnit => this.GetDayOrDays(this.TotalCheckinCount);

    public string CurrentStreakUnit => this.GetDayOrDays(this.CurrentStreak);

    public string BestStreakUnit => this.GetDayOrDays(this.BestStreak);

    public string GetDayOrDays(int day)
    {
      return Utils.GetString(day > 1 ? "PublicUpDays" : "PublicUpDay");
    }
  }
}
