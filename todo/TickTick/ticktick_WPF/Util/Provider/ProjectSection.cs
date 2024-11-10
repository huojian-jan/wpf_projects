// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Provider.ProjectSection
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Util.Provider
{
  public class ProjectSection : Section
  {
    private bool _canSort;

    public ProjectSection(bool canSort)
      : base()
    {
      this._canSort = canSort;
    }

    public new string ProjectId { private get; set; }

    public override string GetProjectId() => this.ProjectId;

    public override bool CanSwitch(DisplayType displayType) => this._canSort;

    public override bool CanSort(string sortBy) => this._canSort;
  }
}
