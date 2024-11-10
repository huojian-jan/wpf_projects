// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Provider.AllProjectIdentity
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ticktick_WPF.Service;

#nullable disable
namespace ticktick_WPF.Util.Provider
{
  public class AllProjectIdentity : SmartProjectIdentity
  {
    public override string Id => "_special_id_all";

    public override string CatId => "all";

    public AllProjectIdentity()
    {
      this.SortOption = SmartProjectService.GetSmartProjectSortOption("all", false);
    }

    public override string GetDisplayTitle() => Utils.GetString("All");

    public override ProjectIdentity Copy(ProjectIdentity project)
    {
      return ProjectIdentity.CreateSmartIdentity(project.Id);
    }
  }
}
