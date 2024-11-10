// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Provider.UnpinnedSection
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Util.Provider
{
  public class UnpinnedSection : Section
  {
    public UnpinnedSection()
      : base()
    {
      this.SectionId = "NotPinned";
      this.Name = Utils.GetString("NotPinned");
      this.Ordinal = 1L;
    }

    public override bool CanSwitch(DisplayType displayType) => true;

    public override bool CanSort(string sortBy) => true;
  }
}
