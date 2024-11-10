// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Habit.WeekCheckInStatInfo
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;

#nullable disable
namespace ticktick_WPF.Views.Habit
{
  public class WeekCheckInStatInfo
  {
    public bool Completed;
    public int Count;
    public DateTime WeekEnd;
    public DateTime WeekStart;

    public WeekCheckInStatInfo(DateTime start, DateTime end)
    {
      this.WeekStart = start;
      this.WeekEnd = end;
    }
  }
}
