// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Habit.HabitReminderViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Habit
{
  public class HabitReminderViewModel : BaseViewModel
  {
    public DateTime Time { get; set; }

    public bool IsAdd { get; set; }
  }
}
