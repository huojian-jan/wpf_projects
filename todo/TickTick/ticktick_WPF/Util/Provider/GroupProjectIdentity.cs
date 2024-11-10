// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Provider.GroupProjectIdentity
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Resource;
using ticktick_WPF.Util.Sort;
using ticktick_WPF.Views.MainListView;
using ticktick_WPF.Views.TaskList;
using ticktick_WPF.Views.Widget;

#nullable disable
namespace ticktick_WPF.Util.Provider
{
  public sealed class GroupProjectIdentity : ProjectIdentity
  {
    public int DisplayKind;
    public bool ContainsShare;

    public override string ViewMode => this.Group?.viewMode ?? "list";

    public GroupProjectIdentity(ProjectGroupModel group)
    {
      this.Group = group;
      this.SortOption = group.GetSortOption();
    }

    public GroupProjectIdentity(ProjectGroupModel group, List<ProjectModel> projects)
    {
      this.Group = group;
      this.DefaultProject = projects != null ? projects.Where<ProjectModel>((Func<ProjectModel, bool>) (p => p.IsProjectPermit())).OrderBy<ProjectModel, long>((Func<ProjectModel, long>) (p => p.sortOrder)).FirstOrDefault<ProjectModel>() : (ProjectModel) null;
      this.DisplayKind = this.GetDisplayKind(projects);
      ProjectModel defaultProject = this.DefaultProject;
      this.IsNote = defaultProject != null && defaultProject.IsNote;
      this.ContainsShare = projects != null && projects.Any<ProjectModel>((Func<ProjectModel, bool>) (p => p.IsShareList()));
      this.SortOption = group.GetSortOption();
    }

    public void SetGroup(ProjectGroupModel group)
    {
      if (group == null)
        return;
      this.Group = group;
      this.SortOption = group.GetSortOption();
    }

    private int GetDisplayKind(List<ProjectModel> projects)
    {
      TaskType displayKind = TaskType.Task;
      if (projects == null)
        return (int) displayKind;
      bool flag1 = false;
      bool flag2 = false;
      foreach (ProjectModel project in projects)
      {
        if (!(flag2 & flag1))
        {
          if (project.IsNote)
            flag2 = true;
          else if (!project.IsNote)
            flag1 = true;
        }
      }
      return flag2 ? (flag1 ? 2 : 1) : 0;
    }

    public override string Id => this.Group?.id ?? "_special_id_group_all";

    public override string SortProjectId => this.Group?.id;

    public override string LoadMoreId => this.Group?.id;

    public override string QueryId => this.Group?.id ?? "GroupAll";

    public override string CatId => this.Group?.id;

    public override string Title => this.Group?.name;

    public ProjectGroupModel Group { get; private set; }

    private ProjectModel DefaultProject { get; }

    public string GroupId => this.Group?.id;

    public override string GetProjectId() => this.DefaultProject?.id;

    public override string GetProjectName() => this.DefaultProject?.name;

    public override string GetDisplayTitle() => this.Group?.name;

    public override Geometry GetProjectIcon() => Utils.GetIcon("IcClosedFolder");

    public override bool CanAddTask() => this.DefaultProject != null;

    public override ProjectIdentity Copy(ProjectIdentity project)
    {
      ProjectGroupModel groupById = CacheManager.GetGroupById(project.CatId);
      return groupById != null ? (ProjectIdentity) new GroupProjectIdentity(groupById, CacheManager.GetProjectsInGroup(groupById.id)) : (ProjectIdentity) this.MemberwiseClone();
    }

    public override TimelineModel GetTimelineModel()
    {
      if (this.Group == null)
        return (TimelineModel) null;
      TimelineModel timeline = this.Group.Timeline;
      if (timeline != null)
        return timeline.Copy();
      this.Group.Timeline = new TimelineModel(Constants.SortType.project.ToString());
      this.CommitTimeline(this.Group.Timeline);
      return this.Group.Timeline.Copy();
    }

    public override void CommitTimeline(TimelineModel model)
    {
      if (model == null)
        return;
      ProjectGroupModel groupById = CacheManager.GetGroupById(this.Group?.id);
      if (groupById != null && (groupById.Timeline == null || !groupById.Timeline.IsEquals(model)))
      {
        bool needSync = groupById.Timeline == null || groupById.Timeline.SyncPropertyChanged(model);
        if (model.SortType == "assignee" || model.SortType == "priority")
          model.SortType = groupById.Timeline?.SortType ?? "project";
        groupById.Timeline = model;
        if (groupById.SyncTimeline != null)
        {
          groupById.SyncTimeline.SortType = model.SortType;
          groupById.SyncTimeline.SortOption = model.sortOption;
        }
        else
          groupById.SyncTimeline = new TimelineSyncModel()
          {
            SortType = model.SortType,
            SortOption = model.sortOption
          };
        ProjectGroupDao.SaveProjectGroup(groupById, needSync);
      }
      if (this.Group == null)
        return;
      this.Group.Timeline = model;
    }

    public override List<SortTypeViewModel> GetTimelineSortTypes()
    {
      return SortOptionHelper.GetGroupTimelineSortTypeModels(this.ContainsShare);
    }

    public override async Task SwitchViewMode(string viewMode)
    {
      ProjectGroupModel group = CacheManager.GetGroupById(this.Group?.id);
      if (group == null)
      {
        group = (ProjectGroupModel) null;
      }
      else
      {
        group.viewMode = viewMode;
        switch (viewMode)
        {
          case "kanban":
            if (group.SortOption?.groupBy == "none")
            {
              group.SortOption.groupBy = "project";
              group.sortType = "project";
              break;
            }
            break;
          case "timeline":
            TimelineModel.CheckTimelineEmpty((ITimeline) group, Constants.SortType.project);
            break;
        }
        await ProjectGroupDao.SaveProjectGroup(group);
        DataChangedNotifier.NotifyProjectViewModeChanged((ProjectIdentity) new GroupProjectIdentity(group, CacheManager.GetProjectsInGroup(group.id)));
        SyncManager.TryDelaySync();
        group = (ProjectGroupModel) null;
      }
    }

    public override List<string> GetSwitchViewModes()
    {
      return new List<string>()
      {
        "list",
        "kanban",
        "timeline"
      };
    }

    public void CheckSortOption()
    {
      SortOption sortOption = this.Group?.GetSortOption();
      if (sortOption == null)
        return;
      bool flag = false;
      if (this.DisplayKind == 1)
      {
        if (sortOption.groupBy == "priority" || sortOption.groupBy == "dueDate")
        {
          sortOption.groupBy = "project";
          flag = true;
        }
        if (sortOption.orderBy == "priority" || sortOption.orderBy == "dueDate")
        {
          sortOption.orderBy = "createdTime";
          flag = true;
        }
      }
      if (this.DisplayKind == 0 && (sortOption.orderBy == "createdTime" || sortOption.orderBy == "modifiedTime"))
      {
        sortOption.orderBy = "dueDate";
        flag = true;
      }
      if (!flag)
        return;
      this.SortOption = sortOption;
      ProjectGroupModel groupById = CacheManager.GetGroupById(this.Group?.id);
      if (groupById == null)
        return;
      groupById.SortOption = sortOption;
      this.Group = groupById;
      ProjectGroupDao.TrySaveProjectGroup(groupById);
      SyncManager.TryDelaySync();
    }

    public override string CheckTitleValid(string text)
    {
      return string.IsNullOrEmpty(text.Trim()) ? Utils.GetString("AddOrEditFolderNameCantNull") : (string) null;
    }

    public override async void SaveTitle(string text)
    {
      this.Group.name = text.Trim();
      await ProjectGroupDao.TrySaveProjectGroup(this.Group);
      SyncManager.Sync();
      ProjectWidgetsHelper.OnProjectChanged((ProjectIdentity) new GroupProjectIdentity(this.Group, (List<ProjectModel>) null));
      ListViewContainer.ReloadProjectData(false);
    }
  }
}
