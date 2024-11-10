// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Provider.CompletedProjectIdentity
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ticktick_WPF.Models;
using ticktick_WPF.Views.Completed;

#nullable disable
namespace ticktick_WPF.Util.Provider
{
  public class CompletedProjectIdentity : SmartProjectIdentity
  {
    public override string Id => "_special_id_completed";

    public override string LoadMoreId => "_special_id_completed";

    public static ClosedFilterViewModel Filter { get; set; } = new ClosedFilterViewModel();

    public override string SortProjectId => (string) null;

    public CompletedProjectIdentity()
    {
      this.SortOption = new SortOption();
      this.CanDrag = false;
    }

    public override bool CanAddTask() => false;

    public override ProjectIdentity Copy(ProjectIdentity project)
    {
      return (ProjectIdentity) new CompletedProjectIdentity();
    }

    public override string GetDisplayTitle() => Utils.GetString("Completed");
  }
}
