// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Habit.DayCheckInColumnInfo
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Habit
{
  public class DayCheckInColumnInfo
  {
    public DayCheckInColumnInfo(
      DateTime date,
      double height,
      double value,
      double goal,
      string unit)
    {
      this.Hint = date.ToString("M") + ", " + DateUtils.FormatWeekDayName(date) + "\r\n" + value.ToString() + " " + HabitUtils.GetUnitText(unit);
      this.Date = date;
      this.DayText = date.Day.ToString();
      this.Height = height;
      this.Opacity = value >= goal ? 1.0 : 0.36000001430511475;
      this.IsToday = date.Date == DateTime.Today;
    }

    public bool IsToday { get; set; }

    public string DayText { get; set; }

    public double Height { get; set; }

    public DateTime Date { get; set; }

    public string Hint { get; set; }

    public double Opacity { get; set; }
  }
}
