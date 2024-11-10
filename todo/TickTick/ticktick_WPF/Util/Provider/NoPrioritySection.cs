// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Provider.NoPrioritySection
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Util.Provider
{
  public class NoPrioritySection : PrioritySection
  {
    public NoPrioritySection()
    {
      this.SectionId = "noPriority";
      this.Name = Utils.GetString("PriorityNull");
      this.Ordinal = 0L;
      this.SectionEntityId = "0";
    }

    public override bool CanSort(string sortBy) => true;

    public override int GetPriority() => 0;

    public override bool CanSwitch(DisplayType displayType) => displayType != DisplayType.Habit;
  }
}
