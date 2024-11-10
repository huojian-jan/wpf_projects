// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Provider.NoteSection
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Util.Provider
{
  public class NoteSection : Section
  {
    public NoteSection()
      : base()
    {
      this.Name = Utils.GetString("Notes");
      this.Ordinal = 2L;
      this.SectionId = "note";
    }

    public override bool CanSwitch(DisplayType displayType) => displayType == DisplayType.Note;

    public override bool CanSort(string sortBy) => false;
  }
}
