// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Provider.NormalProjectIdentity
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
using ticktick_WPF.Service;
using ticktick_WPF.Util.Sort;
using ticktick_WPF.Views.MainListView;
using ticktick_WPF.Views.TaskList;

#nullable disable
namespace ticktick_WPF.Util.Provider
{
  public class NormalProjectIdentity : ProjectIdentity
  {
    public override string ViewMode => this.Project?.viewMode ?? "list";

    public NormalProjectIdentity(ProjectModel project)
    {
      this.Id = project?.id;
      this.Project = project;
      if (project != null)
      {
        this.SortOption = !project.Isinbox ? project.GetSortOption() : SmartProjectService.GetSmartProjectSortOption("inbox", false);
        if (project.Isinbox)
          project.viewMode = SmartProjectService.GetSmartProjectViewMode(project.id);
        if (project.IsNote && project.viewMode == "timeline")
          project.viewMode = "list";
      }
      else
        this.SortOption = SortOptionUtils.GetSortOptionBySortType(Constants.SortType.sortOrder, false);
      this.IsNote = project != null && project.IsNote;
    }

    public override string LoadMoreId => this.Project?.id;

    public override string QueryId => this.Project?.id;

    public override string CatId => this.Project?.id;

    public override string Title => this.Project?.name;

    public override bool CanEdit
    {
      get
      {
        ProjectModel project = this.Project;
        return project != null && project.IsEnable();
      }
    }

    public ProjectModel Project { get; private set; }

    public override string SortProjectId => this.Project?.id;

    public override string GetProjectId() => this.Id;

    public override string GetProjectName() => this.Project?.name;

    public override string GetDisplayTitle() => this.Project?.name;

    public override Geometry GetProjectIcon()
    {
      if (this.Project == null)
        return Utils.GetIcon("IcNormalProject");
      if (this.Project.Isinbox)
        return Utils.GetIcon("IcInboxProject");
      return !this.Project.IsNote ? Utils.GetIcon(this.Project.IsShareList() ? "IcSharedProject" : "IcNormalProject") : Utils.GetIcon(this.Project.IsShareList() ? "IcShareNoteProject" : "IcNoteProject");
    }

    public override ProjectIdentity Copy(ProjectIdentity project)
    {
      return (ProjectIdentity) new NormalProjectIdentity(CacheManager.GetProjectById(project.Id) ?? ((NormalProjectIdentity) project).Project);
    }

    public override bool CanAddTask() => this.Project != null && this.Project.IsEnable();

    public override TimelineModel GetTimelineModel()
    {
      if (this.Project == null)
        return (TimelineModel) null;
      if (this.Project.Isinbox)
        return SmartProjectService.GetSmartProjectTimeline("inbox");
      TimelineModel timeline = this.Project.Timeline;
      if (timeline != null)
        return timeline.Copy();
      this.Project.Timeline = new TimelineModel();
      this.CommitTimeline(this.Project.Timeline);
      return this.Project.Timeline.Copy();
    }

    public override void CommitTimeline(TimelineModel model)
    {
      if (model == null)
        return;
      if (this.Project.Isinbox)
      {
        SmartProjectService.SaveSmartProjectTimeline("inbox", model);
        DataChangedNotifier.NotifySortOptionChanged(this.Project.id);
      }
      else
      {
        ProjectModel projectById = CacheManager.GetProjectById(this.Id);
        if (projectById == null || projectById.Timeline != null && projectById.Timeline.IsEquals(model))
          return;
        if (projectById.Timeline == null || projectById.Timeline.SyncPropertyChanged(model))
          projectById.sync_status = Constants.SyncStatus.SYNC_UPDATE.ToString();
        if (model.SortType == "priority")
          model.SortType = projectById.Timeline?.SortType ?? "sortOrder";
        if (projectById.SyncTimeline != null)
        {
          projectById.SyncTimeline.SortType = model.SortType;
          projectById.SyncTimeline.SortOption = model.sortOption;
        }
        else
          projectById.SyncTimeline = new TimelineSyncModel()
          {
            SortType = model.SortType,
            SortOption = model.sortOption
          };
        projectById.Timeline = model;
        this.Project = projectById;
        ProjectDao.TryUpdateProject(projectById);
      }
    }

    public override List<SortTypeViewModel> GetTimelineSortTypes()
    {
      return SortOptionHelper.GetNormalProjectSortTypeModels(this.Project.IsShareList(), this.Project.IsNote, true);
    }

    public override async Task SwitchViewMode(string viewMode)
    {
      NormalProjectIdentity identity = this;
      if (identity.Project.Isinbox)
      {
        SmartProjectService.SaveSmartProjectViewMode(identity.Id, viewMode);
        DataChangedNotifier.NotifyProjectViewModeChanged((ProjectIdentity) identity);
      }
      ProjectModel project = CacheManager.GetProjectById(identity.Id);
      if (project == null)
      {
        project = (ProjectModel) null;
      }
      else
      {
        await ProjectDao.SwitchViewModel(project, viewMode);
        if (viewMode == "kanban" && project.GetSortOption().groupBy == "sortOrder")
        {
          List<ColumnModel> columnModelList = await ColumnDao.CheckProjectColumns(identity.Id);
        }
        DataChangedNotifier.NotifyProjectViewModeChanged((ProjectIdentity) new NormalProjectIdentity(project));
        SyncManager.TryDelaySync();
        project = (ProjectModel) null;
      }
    }

    public override List<string> GetSwitchViewModes()
    {
      if (!this.Project.IsValid())
        return (List<string>) null;
      if (this.Project.IsNote)
        return new List<string>() { "list", "kanban" };
      return new List<string>()
      {
        "list",
        "kanban",
        "timeline"
      };
    }

    public override string CheckTitleValid(string text)
    {
      if (string.IsNullOrEmpty(text.Trim()))
        return Utils.GetString("AddOrEditProjectNameCantNull");
      if (text[0] == '#' || text[0] == '＃')
        return Utils.GetString("ListNameBeginError");
      if (!NameUtils.IsValidNameNoCheckSharp(text, false))
        return Utils.GetString("ListNameCantContain");
      if (this.Project != null)
      {
        ProjectModel proj = this.Project;
        if (CacheManager.GetProjects().FirstOrDefault<ProjectModel>((Func<ProjectModel, bool>) (p => p.name.Trim() == text.Trim() && p.id != proj.id && p.teamId == proj.teamId && p.IsBelongGroup(proj.groupId) && !p.Isinbox)) != null)
          return Utils.GetString("AddOrEditProjectNameRepeat");
      }
      return (string) null;
    }

    public override async void SaveTitle(string text)
    {
      await ProjectDao.SaveProjectName(this.Project.id, text);
      ListViewContainer.ReloadProjectData(false);
    }
  }
}
