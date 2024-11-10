// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Provider.NoAssingeeSection
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Util.Provider
{
  public class NoAssingeeSection : AssigneeSection
  {
    public NoAssingeeSection()
    {
      this.Name = Utils.GetString("NotAssigned");
      this.Ordinal = 9223372036854775804L;
      this.Assingee = "-1";
      this.SectionId = "-1";
      this.SectionEntityId = "-1";
    }

    public override string GetAssignee() => "-1";

    public override bool CanSwitch(DisplayType displayType)
    {
      return displayType == DisplayType.Note || displayType == DisplayType.Task || displayType == DisplayType.Agenda;
    }

    public override bool CanSort(string sortBy) => true;
  }
}
