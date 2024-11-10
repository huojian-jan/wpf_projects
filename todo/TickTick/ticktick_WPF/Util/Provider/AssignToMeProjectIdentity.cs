// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Provider.AssignToMeProjectIdentity
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections.Generic;
using ticktick_WPF.Service;

#nullable disable
namespace ticktick_WPF.Util.Provider
{
  public class AssignToMeProjectIdentity : SmartProjectIdentity
  {
    public AssignToMeProjectIdentity()
    {
      this.SortOption = SmartProjectService.GetSmartProjectSortOption("assignToMe", false);
    }

    public override string Id => "_special_id_assigned";

    public override string CatId => "assigned";

    public override string ViewMode => "list";

    public override string SortProjectId => "assignee";

    public override bool CanAddTask() => false;

    public override string GetDisplayTitle() => Utils.GetString("AssignToMe");

    public override ProjectIdentity Copy(ProjectIdentity project)
    {
      return ProjectIdentity.CreateSmartIdentity(project.Id);
    }

    public override List<string> GetSwitchViewModes() => (List<string>) null;
  }
}
