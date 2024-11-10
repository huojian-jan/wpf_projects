// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Provider.MediumPrioritySection
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Util.Provider
{
  public class MediumPrioritySection : PrioritySection
  {
    public MediumPrioritySection()
    {
      this.SectionId = "mediumPriority";
      this.Name = Utils.GetString("PriorityMedium");
      this.Ordinal = 3L;
      this.SectionEntityId = "3";
    }

    public override int GetPriority() => 3;

    public override bool CanSort(string sortBy) => true;

    public override bool CanSwitch(DisplayType displayType) => displayType != DisplayType.Habit;
  }
}
