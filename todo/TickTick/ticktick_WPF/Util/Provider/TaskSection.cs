// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Provider.TaskSection
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Util.Provider
{
  public class TaskSection : Section
  {
    public TaskSection()
      : base()
    {
      this.Name = Utils.GetString("Task");
      this.Ordinal = 2L;
      this.SectionId = "task";
    }

    public override bool CanSwitch(DisplayType displayType)
    {
      return displayType == DisplayType.Task || displayType == DisplayType.Agenda;
    }

    public override bool CanSort(string sortBy) => true;
  }
}
