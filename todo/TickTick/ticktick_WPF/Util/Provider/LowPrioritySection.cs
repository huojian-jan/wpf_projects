﻿// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Provider.LowPrioritySection
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Util.Provider
{
  public class LowPrioritySection : PrioritySection
  {
    public LowPrioritySection()
    {
      this.SectionId = "lowPriority";
      this.Name = Utils.GetString("PriorityLow");
      this.Ordinal = 1L;
      this.SectionEntityId = "1";
    }

    public override bool CanSort(string sortBy) => true;

    public override int GetPriority() => 1;

    public override bool CanSwitch(DisplayType displayType) => displayType != DisplayType.Habit;
  }
}