// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Misc.FilterHabitViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Misc
{
  public class FilterHabitViewModel : SelectableItemViewModel
  {
    public FilterHabitViewModel()
    {
      this.Id = "Habit2e4c103c57ef480997943206";
      this.Title = Utils.GetString("statistics_habit");
      this.Icon = Utils.GetIconData("IcLineHabit");
      this.Type = "normal";
    }
  }
}
