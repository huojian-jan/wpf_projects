// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Provider.AssigneeSection
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Util.Provider
{
  public class AssigneeSection : Section
  {
    public string Assingee { get; set; }

    public override string GetAssignee() => this.Assingee;

    public override bool CanSwitch(DisplayType displayType)
    {
      return displayType == DisplayType.Note || displayType == DisplayType.Task || displayType == DisplayType.Agenda;
    }

    public override bool CanSort(string sortType) => true;

    public AssigneeSection()
      : base()
    {
    }
  }
}
