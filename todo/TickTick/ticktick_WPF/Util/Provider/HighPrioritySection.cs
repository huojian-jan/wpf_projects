// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Provider.HighPrioritySection
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Util.Provider
{
  public class HighPrioritySection : PrioritySection
  {
    public HighPrioritySection()
    {
      this.SectionId = "highPriority";
      this.Name = Utils.GetString("PriorityHigh");
      this.Ordinal = 5L;
      this.SectionEntityId = "5";
    }

    public override int GetPriority() => 5;

    public override bool CanSort(string sortBy) => true;

    public override bool CanSwitch(DisplayType displayType) => displayType != DisplayType.Habit;
  }
}
