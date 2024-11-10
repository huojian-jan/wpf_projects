// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Provider.ProjectGroupData
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;

#nullable disable
namespace ticktick_WPF.Util.Provider
{
  public class ProjectGroupData : SortProjectData
  {
    public ProjectGroupData(ProjectGroupModel projectGroup, IEnumerable<ProjectModel> projects)
    {
      this.ProjectGroup = projectGroup;
      this.ShowProjectSort = true;
      this.EmptyPath = Utils.GetIconData("IcEmptyProject");
      List<ProjectModel> list = projects.ToList<ProjectModel>().Where<ProjectModel>((Func<ProjectModel, bool>) (p =>
      {
        if (!(p.groupId == projectGroup.id))
          return false;
        return !p.closed.HasValue || !p.closed.Value;
      })).ToList<ProjectModel>();
      this.DefaultProjectModel = list.FirstOrDefault<ProjectModel>((Func<ProjectModel, bool>) (p => p.IsProjectPermit()));
      if (this.DefaultProjectModel != null)
        this.TitleInProjectGroup = this.DefaultProjectModel.name;
      this.AddTaskHint = TeamDao.IsTeamExpired(projectGroup.teamId) || !list.Any<ProjectModel>((Func<ProjectModel, bool>) (p => p.IsProjectPermit())) ? string.Empty : string.Format(Utils.GetString("CenterAddTaskTextBoxPreviewText"), (object) this.TitleInProjectGroup, (object) Utils.GetString(this.DefaultProjectModel == null || !this.DefaultProjectModel.IsNote ? "Task" : "Notes").ToLower());
      this.ShowCustomSort = false;
      this.ShowLoadMore = true;
    }

    public ProjectGroupModel ProjectGroup { get; }

    public override string GetTitle() => this.ProjectGroup.name;

    public override async void SaveSortOption(SortOption sortOption)
    {
      this.ProjectGroup.SortOption = sortOption;
      this.ProjectGroup.sortType = sortOption.groupBy == "none" || sortOption.groupBy == "assignee" ? sortOption.orderBy : sortOption.groupBy;
      await ProjectGroupDao.TrySaveProjectGroup(this.ProjectGroup);
    }
  }
}
