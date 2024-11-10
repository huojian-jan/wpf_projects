// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Habit.HabitRepeatInfo
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;

#nullable disable
namespace ticktick_WPF.Views.Habit
{
  public class HabitRepeatInfo
  {
    public HabitRepeatType Type { get; set; }

    public int Count { get; set; }

    public List<DayOfWeek> ByDays { get; set; }

    public int Interval { get; set; } = 1;
  }
}
