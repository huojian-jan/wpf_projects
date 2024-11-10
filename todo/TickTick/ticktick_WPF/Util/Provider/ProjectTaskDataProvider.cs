// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Provider.ProjectTaskDataProvider
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Service.Display;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Search;

#nullable disable
namespace ticktick_WPF.Util.Provider
{
  public static class ProjectTaskDataProvider
  {
    public static async Task<SortProjectData> GetProjectData(ProjectIdentity project)
    {
      SortProjectData projectData;
      switch (project)
      {
        case TodayProjectIdentity _:
          projectData = await ProjectTaskDataProvider.GetSortDataInToday();
          break;
        case AllProjectIdentity _:
          projectData = await ProjectTaskDataProvider.GetSortDataInAll();
          break;
        case TomorrowProjectIdentity _:
          projectData = await ProjectTaskDataProvider.GetSortDataInTomorrow();
          break;
        case WeekProjectIdentity _:
          projectData = await ProjectTaskDataProvider.GetSortDataInWeek();
          break;
        case GroupProjectIdentity _:
          projectData = await ProjectTaskDataProvider.GetSortDataInGroupAll(project);
          break;
        case AssignToMeProjectIdentity _:
          projectData = await ProjectTaskDataProvider.GetSortDataAssignedToMe();
          break;
        case CompletedProjectIdentity _:
          projectData = await ProjectTaskDataProvider.GetSortDataInCompleted();
          break;
        case AbandonedProjectIdentity _:
          projectData = await ProjectTaskDataProvider.GetSortDataInAbandoned();
          break;
        case TrashProjectIdentity _:
          projectData = await ProjectTaskDataProvider.GetSortDataInTrash();
          break;
        case TagProjectIdentity _:
          projectData = await ProjectTaskDataProvider.GetSortDataInTag(project);
          break;
        case FilterProjectIdentity _:
          projectData = await ProjectTaskDataProvider.GetSortDataInFilter(project);
          break;
        case SearchProjectIdentity _:
          projectData = await ProjectTaskDataProvider.GetSortDataInSearch();
          break;
        case FilterPreviewIdentity preview:
          projectData = await ProjectTaskDataProvider.GetPreviewSortData(preview);
          break;
        case SubscribeCalendarProjectIdentity _:
          projectData = await ProjectTaskDataProvider.GetSortDataInSubscribeCalendar(project);
          break;
        case BindAccountCalendarProjectIdentity _:
          projectData = await ProjectTaskDataProvider.GetSortDataInBindCalendar(project);
          break;
        case DateProjectIdentity _:
          projectData = await ProjectTaskDataProvider.GetSortDataInDate(project);
          break;
        default:
          projectData = await ProjectTaskDataProvider.GetSortDataInProject(project);
          break;
      }
      if (projectData != null)
      {
        projectData.ProjectIdentity = project;
      }
      else
      {
        NormalProjectIdentity inbox = ProjectIdentity.CreateInboxProject();
        projectData = await ProjectTaskDataProvider.GetSortDataInProject((ProjectIdentity) inbox);
        projectData.ProjectIdentity = (ProjectIdentity) inbox;
        inbox = (NormalProjectIdentity) null;
      }
      return projectData;
    }

    public static async Task<List<TaskBaseViewModel>> GetDisplayModels(
      ProjectIdentity project,
      bool withChild = true,
      bool? showCompete = null)
    {
      List<TaskBaseViewModel> taskBaseViewModelList;
      switch (project)
      {
        case null:
          return new List<TaskBaseViewModel>();
        case AllProjectIdentity _:
          taskBaseViewModelList = await TaskDisplayService.GetDisplayTaskInAll();
          break;
        case TodayProjectIdentity _:
          taskBaseViewModelList = await TaskDisplayService.GetDisplayTaskInToday();
          break;
        case TomorrowProjectIdentity _:
          taskBaseViewModelList = await TaskDisplayService.GetDisplayTaskInTomorrow();
          break;
        case WeekProjectIdentity _:
          taskBaseViewModelList = await TaskDisplayService.GetDisplayTaskInWeek();
          break;
        case GroupProjectIdentity groupProjectIdentity:
          taskBaseViewModelList = await TaskDisplayService.GetDisplayTaskInGroup(groupProjectIdentity.GroupId);
          break;
        case AssignToMeProjectIdentity _:
          taskBaseViewModelList = await TaskDisplayService.GetDisplayTaskInAssigneeToMe(project);
          break;
        case CompletedProjectIdentity _:
          taskBaseViewModelList = await TaskDisplayService.GetDisplayTasksInCompleted();
          break;
        case AbandonedProjectIdentity _:
          taskBaseViewModelList = await TaskDisplayService.GetDisplayTasksInAbandoned();
          break;
        case TrashProjectIdentity trashProjectIdentity:
          taskBaseViewModelList = await TaskDisplayService.GetDisplayTasksInTrash(trashProjectIdentity.IsPerson);
          break;
        case TagProjectIdentity tagProjectIdentity:
          taskBaseViewModelList = await TaskDisplayService.GetDisplayTaskInTag(tagProjectIdentity.Tag);
          break;
        case FilterProjectIdentity filterProjectIdentity:
          taskBaseViewModelList = await TaskDisplayService.GetDisplayTaskInFilter(filterProjectIdentity.Filter.rule, ((int) showCompete ?? (!LocalSettings.Settings.HideComplete ? 1 : 0)) != 0, LocalSettings.Settings.ShowSubtasks, withChild);
          break;
        case FilterPreviewIdentity filterPreviewIdentity:
          taskBaseViewModelList = await TaskDisplayService.GetDisplayTaskInFilter(filterPreviewIdentity.Filter.rule, ((int) showCompete ?? (!LocalSettings.Settings.HideComplete ? 1 : 0)) != 0, LocalSettings.Settings.ShowSubtasks);
          break;
        case MatrixQuadrantIdentity quadrantIdentity:
          taskBaseViewModelList = await TaskDisplayService.GetDisplayTaskInFilter(quadrantIdentity.Quadrant.rule, ((int) showCompete ?? (LocalSettings.Settings.MatrixShowCompleted ? 1 : 0)) != 0, LocalSettings.Settings.ShowSubtasks, false, inAll: true);
          break;
        case SearchProjectIdentity _:
          taskBaseViewModelList = await TaskDisplayService.GetDisplayTaskInSearch(SearchHelper.SearchKey, SearchHelper.SearchFilter, SearchHelper.Tags);
          break;
        case SubscribeCalendarProjectIdentity calendarProjectIdentity1:
          string id1 = calendarProjectIdentity1.Profile.Id;
          DateTime date = DateTime.Now.Date;
          DateTime dateTime = DateTime.Now;
          dateTime = dateTime.Date;
          DateTime end = dateTime.AddMonths(4);
          taskBaseViewModelList = await CalendarEventDao.GetDisplayEventsInCalendar(id1, 1, date, end);
          break;
        case BindAccountCalendarProjectIdentity calendarProjectIdentity2:
          taskBaseViewModelList = await CalendarEventDao.GetDisplayEventsInBindAccount(calendarProjectIdentity2.Account.Id, DateTime.Today, DateTime.Now.AddMonths(3));
          break;
        case ColumnProjectIdentity model:
          string projectDefaultColumnId = await ColumnDao.GetProjectDefaultColumnId(model.GetProjectId());
          taskBaseViewModelList = await TaskDao.GetDisplayTasksInColumn(project.GetProjectId(), model.ColumnId, model.ColumnId == projectDefaultColumnId);
          break;
        case DateProjectIdentity dateProjectIdentity:
          taskBaseViewModelList = await TaskDao.GetDisplayTasksInDate(dateProjectIdentity.DateStamp);
          break;
        default:
          string id2 = project.Id;
          bool? nullable = showCompete;
          int num = (nullable.HasValue ? (int) new bool?(!nullable.GetValueOrDefault()) : (int) new bool?()) ?? (LocalSettings.Settings.HideComplete ? 1 : 0);
          taskBaseViewModelList = TaskDisplayService.GetDisplayTasksInProject(id2, num != 0);
          break;
      }
      model = (ColumnProjectIdentity) null;
      return taskBaseViewModelList ?? new List<TaskBaseViewModel>();
    }

    private static async Task<SortProjectData> GetSortDataInTag(ProjectIdentity projectId)
    {
      string tag = ((TagProjectIdentity) projectId).Tag;
      TagModel tagModel = CacheManager.GetTags().FirstOrDefault<TagModel>((Func<TagModel, bool>) (t => t.name == tag));
      if (tagModel != null)
        projectId.SortOption = tagModel.GetSortOption();
      return (SortProjectData) new TagListData(((TagProjectIdentity) projectId).Tag);
    }

    private static async Task<SortProjectData> GetSortDataInAll()
    {
      return (SortProjectData) new AllListData();
    }

    private static async Task<SortProjectData> GetSortDataInSearch()
    {
      return (SortProjectData) new SearchListData();
    }

    private static async Task<SortProjectData> GetPreviewSortData(FilterPreviewIdentity preview)
    {
      return (SortProjectData) new PreviewListData(preview);
    }

    private static async Task<SortProjectData> GetSortDataInBindCalendar(ProjectIdentity project)
    {
      return (SortProjectData) new BindAccountCalendarListData(((BindAccountCalendarProjectIdentity) project).Account);
    }

    private static async Task<SortProjectData> GetSortDataInDate(ProjectIdentity project)
    {
      return (SortProjectData) new DateListData(project is DateProjectIdentity dateProjectIdentity ? dateProjectIdentity.DateStamp : (string) null);
    }

    private static async Task<SortProjectData> GetSortDataInSubscribeCalendar(
      ProjectIdentity project)
    {
      return (SortProjectData) new SubscribeCalendarListData(((SubscribeCalendarProjectIdentity) project).Profile);
    }

    private static async Task<SortProjectData> GetSortDataInFilter(ProjectIdentity projectId)
    {
      return (SortProjectData) new FilterListData(((FilterProjectIdentity) projectId).Filter);
    }

    private static async Task<SortProjectData> GetSortDataInGroupAll(ProjectIdentity projectId)
    {
      List<ProjectGroupModel> projectGroups = CacheManager.GetProjectGroups();
      if (projectId != null)
      {
        ProjectGroupModel projectGroup = projectGroups.FirstOrDefault<ProjectGroupModel>((Func<ProjectGroupModel, bool>) (g => g.id == projectId.CatId));
        if (projectGroup != null)
          return (SortProjectData) new ProjectGroupData(projectGroup, (IEnumerable<ProjectModel>) CacheManager.GetProjects());
      }
      return new SortProjectData();
    }

    private static async Task<SortProjectData> GetSortDataInProject(ProjectIdentity projectId)
    {
      ProjectModel project = CacheManager.GetProjectById(projectId.Id);
      if (project == null)
        project = new ProjectModel()
        {
          id = "inbox" + LocalSettings.Settings.LoginUserId,
          name = Utils.GetString("Inbox"),
          Isinbox = true
        };
      return (SortProjectData) new NormalListData(project);
    }

    private static async Task<SortProjectData> GetSortDataAssignedToMe()
    {
      return (SortProjectData) new AssignListData();
    }

    private static async Task<SortProjectData> GetSortDataInTomorrow()
    {
      return (SortProjectData) new TomorrowListData();
    }

    private static async Task<SortProjectData> GetSortDataInWeek()
    {
      return (SortProjectData) new WeekListData();
    }

    private static async Task<SortProjectData> GetSortDataInToday()
    {
      return (SortProjectData) new TodayListData();
    }

    private static async Task<SortProjectData> GetSortDataInCompleted()
    {
      return (SortProjectData) new CompleteListData();
    }

    private static async Task<SortProjectData> GetSortDataInAbandoned()
    {
      return (SortProjectData) new AbandonedListData();
    }

    private static async Task<SortProjectData> GetSortDataInTrash()
    {
      return (SortProjectData) new TrashListData();
    }
  }
}
