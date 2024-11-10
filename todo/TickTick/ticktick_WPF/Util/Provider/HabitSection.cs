// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Provider.HabitSection
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Util.Provider
{
  public class HabitSection : Section
  {
    public HabitSection(bool useInWeek = false, bool sortAsDate = false)
      : base()
    {
      this.Name = Utils.GetString("Habit");
      if (useInWeek)
      {
        if (Utils.IsCn())
          this.Name = Utils.GetString("TodayHabit") + ", " + DateUtils.FormatWeekDayName(DateTime.Today);
        else
          this.Name = DateUtils.FormatWeekDayName(DateTime.Today) + ", " + Utils.GetString("TodayHabit");
      }
      this.Ordinal = 24L;
      this.SectionId = "habits";
    }

    public override bool CanSwitch(DisplayType displayType) => false;

    public override bool CanSort(string sortType) => false;
  }
}
