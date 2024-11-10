// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Provider.AbandonedProjectIdentity
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ticktick_WPF.Models;
using ticktick_WPF.Views.Completed;

#nullable disable
namespace ticktick_WPF.Util.Provider
{
  public class AbandonedProjectIdentity : SmartProjectIdentity
  {
    public override string Id => "_special_id_abandoned";

    public override string LoadMoreId => "_special_id_abandoned";

    public static ClosedFilterViewModel Filter { get; set; } = new ClosedFilterViewModel()
    {
      IsCompleted = false
    };

    public override string SortProjectId => (string) null;

    public AbandonedProjectIdentity()
    {
      this.SortOption = new SortOption();
      this.CanDrag = false;
    }

    public override bool CanAddTask() => false;

    public override ProjectIdentity Copy(ProjectIdentity project)
    {
      return (ProjectIdentity) new AbandonedProjectIdentity();
    }

    public override string GetDisplayTitle() => Utils.GetString("Abandoned");
  }
}
