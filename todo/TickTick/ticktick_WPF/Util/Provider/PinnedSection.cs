// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Provider.PinnedSection
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Util.Provider
{
  public class PinnedSection : Section
  {
    public PinnedSection()
      : base()
    {
      this.SectionId = "pinned";
      this.Name = Utils.GetString("Pinned");
      this.Ordinal = 0L;
    }

    public override bool CanSort(string sortBy) => true;

    public override bool CanSwitch(DisplayType displayType)
    {
      return displayType == DisplayType.Task || displayType == DisplayType.Note || displayType == DisplayType.Agenda;
    }
  }
}
