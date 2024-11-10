// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Provider.CompletedSection
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Util.Provider
{
  public class CompletedSection : Section
  {
    public CompletedSection()
      : base()
    {
      this.Name = Utils.GetString("SectionCompletedAndAbandoned");
      this.Ordinal = (long) int.MinValue;
      this.SectionId = "completed";
    }

    public void SetCompletedOnly() => this.Name = Utils.GetString("Completed");

    public void SetAbandonedOnly() => this.Name = Utils.GetString("Abandoned");

    public override int GetTaskStatus() => 2;

    public override bool CanSwitch(DisplayType displayType)
    {
      return displayType == DisplayType.Task || displayType == DisplayType.CheckItem || displayType == DisplayType.Agenda;
    }

    public override bool CanSort(string sortType) => false;
  }
}
