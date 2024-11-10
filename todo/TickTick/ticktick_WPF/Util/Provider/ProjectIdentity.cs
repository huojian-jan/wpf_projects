// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Provider.ProjectIdentity
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
using ticktick_WPF.Resource;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.QuickAdd;
using ticktick_WPF.Views.TaskList;
using ticktick_WPF.Views.Timeline;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Util.Provider
{
  public class ProjectIdentity : IProjectTaskDefault, ITimelineProject
  {
    private SortOption _sortOption;

    protected ProjectIdentity()
    {
    }

    public virtual string Id { get; protected set; }

    public virtual string Title { get; protected set; }

    public virtual string LoadMoreId => "_special_id_all";

    public virtual string SortProjectId => "all";

    public virtual string QueryId => this.Id;

    public virtual bool CanEdit => true;

    public virtual bool LoadAll => false;

    public virtual string CatId => this.Id;

    public virtual bool IsNote { get; set; }

    public bool CanDrag { get; set; } = true;

    public virtual string ViewMode { get; set; } = "list";

    public SortOption SortOption
    {
      get
      {
        if (this._sortOption == null)
        {
          UtilLog.Info("IdentitySortOptionIsNull, " + this.QueryId + this.GetType().Name);
          this._sortOption = new SortOption()
          {
            groupBy = "dueDate",
            orderBy = "dueDate"
          };
        }
        return this._sortOption;
      }
      set => this._sortOption = value;
    }

    public virtual string GetProjectId() => TaskDefaultDao.GetDefaultSafely().ProjectId;

    public virtual string GetProjectName()
    {
      return CacheManager.GetProjects().FirstOrDefault<ProjectModel>((Func<ProjectModel, bool>) (p => p.id == this.GetProjectId()))?.name;
    }

    public virtual string GetColumnId() => string.Empty;

    public virtual TimeData GetTimeData()
    {
      return !this.IsNote ? TimeData.BuildFromDefault(TaskDefaultDao.GetDefaultSafely()) : new TimeData();
    }

    public virtual int GetPriority() => TaskDefaultDao.GetDefaultSafely().Priority;

    public virtual List<string> GetTags() => TaskDefaultDao.GetDefaultSafely().Tags;

    public virtual bool UseDefaultTags() => true;

    public virtual bool IsCalendar() => false;

    public virtual string GetAccountId() => string.Empty;

    public virtual bool GetIsNote() => this.IsNote;

    public virtual string GetAssignee() => (string) null;

    public virtual async Task<int> LoadCount() => 0;

    public virtual DateTime? GetCompletedFromTime() => new DateTime?();

    public virtual bool IsNormalProject() => false;

    public virtual string CheckTitleValid(string text) => (string) null;

    public virtual void SaveTitle(string text)
    {
    }

    public static NormalProjectIdentity CreateInboxProject()
    {
      string projectId = "inbox" + LocalSettings.Settings.LoginUserId;
      ProjectModel project = CacheManager.GetProjectById(projectId);
      if (project == null)
        project = new ProjectModel()
        {
          id = projectId,
          name = Utils.GetString("Inbox"),
          Isinbox = true
        };
      return new NormalProjectIdentity(project);
    }

    public static NormalProjectIdentity GetDefaultProject()
    {
      ProjectModel project = CacheManager.GetProjects().FirstOrDefault<ProjectModel>((Func<ProjectModel, bool>) (p => p.id == TaskDefaultDao.GetDefaultSafely().ProjectId));
      return project == null ? ProjectIdentity.CreateInboxProject() : new NormalProjectIdentity(project);
    }

    public virtual ProjectIdentity Copy(ProjectIdentity project) => project;

    public virtual bool CanAddTask() => true;

    public virtual string GetDisplayTitle() => ProjectIdentity.GetDefaultProject().GetProjectName();

    public virtual Geometry GetProjectIcon() => (Geometry) null;

    public virtual string GetSortProjectId()
    {
      return !(this.SortOption?.groupBy == "dueDate") && !(this.SortOption?.groupBy == "project") ? this.CatId : this.SortProjectId;
    }

    public static ProjectIdentity CreateSmartIdentity(string id)
    {
      if (id != null)
      {
        switch (id.Length)
        {
          case 15:
            if (id == "_special_id_all")
              return (ProjectIdentity) new AllProjectIdentity();
            break;
          case 16:
            if (id == "_special_id_week")
              return (ProjectIdentity) new WeekProjectIdentity();
            break;
          case 17:
            switch (id[13])
            {
              case 'o':
                if (id == "_special_id_today")
                  return (ProjectIdentity) new TodayProjectIdentity();
                break;
              case 'r':
                if (id == "_special_id_trash")
                  return (ProjectIdentity) new TrashProjectIdentity();
                break;
            }
            break;
          case 19:
            if (id == "_special_id_summary")
              return (ProjectIdentity) new SummaryProjectIdentity();
            break;
          case 20:
            switch (id[12])
            {
              case 'a':
                if (id == "_special_id_assigned")
                  return (ProjectIdentity) new AssignToMeProjectIdentity();
                break;
              case 't':
                if (id == "_special_id_tomorrow")
                  return (ProjectIdentity) new TomorrowProjectIdentity();
                break;
            }
            break;
          case 21:
            switch (id[12])
            {
              case 'a':
                if (id == "_special_id_abandoned")
                  return (ProjectIdentity) new AbandonedProjectIdentity();
                break;
              case 'c':
                if (id == "_special_id_completed")
                  return (ProjectIdentity) new CompletedProjectIdentity();
                break;
            }
            break;
        }
      }
      return (ProjectIdentity) ProjectIdentity.CreateInboxProject();
    }

    public static ProjectIdentity BuildProject(string saveId, bool getDefault = true)
    {
      if (!string.IsNullOrEmpty(saveId) && saveId.Contains(":"))
      {
        string[] strArray = saveId.Split(':');
        if (strArray.Length == 2)
        {
          string str = strArray[0];
          string identity = strArray[1];
          if (str != null)
          {
            switch (str.Length)
            {
              case 3:
                if (str == "tag")
                {
                  TagModel tag = CacheManager.GetTags().FirstOrDefault<TagModel>((Func<TagModel, bool>) (t => t.name == identity));
                  if (tag != null)
                    return (ProjectIdentity) new TagProjectIdentity(tag);
                  break;
                }
                break;
              case 5:
                switch (str[0])
                {
                  case 'g':
                    if (str == "group")
                    {
                      ProjectGroupModel group = CacheManager.GetProjectGroups().FirstOrDefault<ProjectGroupModel>((Func<ProjectGroupModel, bool>) (p => p.id == identity));
                      if (group != null)
                      {
                        List<ProjectModel> projectsInGroup = CacheManager.GetProjectsInGroup(identity);
                        return (ProjectIdentity) new GroupProjectIdentity(group, projectsInGroup);
                      }
                      break;
                    }
                    break;
                  case 's':
                    if (str == "smart")
                      return ProjectIdentity.CreateSmartIdentity(identity);
                    break;
                }
                break;
              case 6:
                if (str == "filter")
                {
                  FilterModel filter = CacheManager.GetFilters().FirstOrDefault<FilterModel>((Func<FilterModel, bool>) (f => f.id == identity));
                  if (filter != null)
                    return (ProjectIdentity) new FilterProjectIdentity(filter);
                  break;
                }
                break;
              case 7:
                if (str == "project")
                {
                  ProjectModel project = CacheManager.GetProjects().FirstOrDefault<ProjectModel>((Func<ProjectModel, bool>) (p => p.id == identity));
                  if (project != null)
                    return (ProjectIdentity) new NormalProjectIdentity(project);
                  break;
                }
                break;
              case 12:
                if (str == "bind_account")
                {
                  BindCalendarAccountModel account1 = CacheManager.GetBindCalendarAccounts().FirstOrDefault<BindCalendarAccountModel>((Func<BindCalendarAccountModel, bool>) (account => account.Id == identity));
                  if (account1 != null)
                    return (ProjectIdentity) new BindAccountCalendarProjectIdentity(account1);
                  break;
                }
                break;
              case 18:
                if (str == "subscribe_calendar")
                {
                  CalendarSubscribeProfileModel profile = CacheManager.GetSubscribeCalendars().FirstOrDefault<CalendarSubscribeProfileModel>((Func<CalendarSubscribeProfileModel, bool>) (c => c.Id == identity));
                  if (profile != null)
                    return (ProjectIdentity) new SubscribeCalendarProjectIdentity(profile);
                  break;
                }
                break;
            }
          }
        }
      }
      return !getDefault ? (ProjectIdentity) null : (ProjectIdentity) ProjectIdentity.CreateInboxProject();
    }

    public virtual TimelineModel GetTimelineModel() => (TimelineModel) null;

    public virtual void CommitTimeline(TimelineModel model)
    {
    }

    public virtual List<SortTypeViewModel> GetTimelineSortTypes() => (List<SortTypeViewModel>) null;

    public virtual async Task SwitchViewMode(string viewMode)
    {
    }

    public virtual List<string> GetSwitchViewModes() => (List<string>) null;
  }
}
