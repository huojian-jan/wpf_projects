// Decompiled with JetBrains decompiler
// Type: ProjectAndTaskIdsCache
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Service;
using ticktick_WPF.Service.Display;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Provider;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.MainListView;

#nullable disable
public static class ProjectAndTaskIdsCache
{
  private static ConcurrentDictionary<string, ProjectTaskIds> _projectTasksDict = new ConcurrentDictionary<string, ProjectTaskIds>();
  private static ConcurrentDictionary<string, ProjectModel> _projectDict = new ConcurrentDictionary<string, ProjectModel>();
  private static ConcurrentDictionary<string, int> _projectEventCount = new ConcurrentDictionary<string, int>();

  public static void TryDelayLoadCount()
  {
    DelayActionHandlerCenter.TryDoAction(nameof (ProjectAndTaskIdsCache), (EventHandler) ((sender, args) => Utils.RunOnBackgroundThread(Application.Current?.Dispatcher, new Action(ListViewContainer.ReloadCount))), 500);
  }

  public static ProjectModel GetProjectById(string id) => CacheManager.GetProjectById(id);

  public static async Task<int> GetCountByIdentity(ProjectIdentity identity, bool withNote = false)
  {
    ProjectTaskIds projectTaskIdsInDict = await ProjectAndTaskIdsCache.GetProjectTaskIdsInDict(identity);
    int num1 = 0;
    if (projectTaskIdsInDict != null)
    {
      num1 += projectTaskIdsInDict.UnCompletedTaskIds.Count;
      if (withNote || LocalSettings.Settings.ExtraSettings.NumDisplayType == 0)
        num1 += projectTaskIdsInDict.UnCompletedNoteIds.Count;
      if (LocalSettings.Settings.ShowSubtasks)
        num1 += projectTaskIdsInDict.UnCompletedItemIds.Count;
    }
    int num = num1;
    return num + await ProjectAndTaskIdsCache.TryGetEventsAndHabitsCountByIdentity(identity);
  }

  private static async Task<ProjectTaskIds> GetProjectTaskIdsInDict(ProjectIdentity identity)
  {
    if (ProjectAndTaskIdsCache._projectTasksDict.ContainsKey(identity.QueryId))
    {
      ProjectTaskIds projectTaskIdsInDict = ProjectAndTaskIdsCache._projectTasksDict[identity.QueryId];
      if (projectTaskIdsInDict != null)
        return projectTaskIdsInDict;
      ProjectAndTaskIdsCache._projectTasksDict.TryRemove(identity.QueryId, out ProjectTaskIds _);
    }
    ProjectTaskIds projectTaskIds = await ProjectAndTaskIdsCache.GetProjectTaskIds(identity);
    if (projectTaskIds != null && !string.IsNullOrEmpty(identity.QueryId))
      ProjectAndTaskIdsCache._projectTasksDict[identity.QueryId] = projectTaskIds;
    return projectTaskIds;
  }

  public static async Task ResetProjectTaskIds(
    ProjectIdentity identity,
    List<TaskBaseViewModel> models)
  {
    ProjectTaskIds idsModel = new ProjectTaskIds()
    {
      Project = identity
    };
    ProjectAndTaskIdsCache.ArrangeModelIds(idsModel, models);
    if (string.IsNullOrEmpty(identity.QueryId))
      return;
    ProjectAndTaskIdsCache._projectTasksDict[identity.QueryId] = idsModel;
  }

  public static async Task ResetProjectTaskIds(ProjectIdentity identity)
  {
    ProjectTaskIds projectTaskIds = await ProjectAndTaskIdsCache.GetProjectTaskIds(identity);
    if (string.IsNullOrEmpty(identity.QueryId))
      return;
    ProjectAndTaskIdsCache._projectTasksDict[identity.QueryId] = projectTaskIds;
  }

  private static async Task<int> TryGetEventsAndHabitsCountByIdentity(ProjectIdentity identity)
  {
    int count = 0;
    DateTime dateTime;
    if (!ProjectAndTaskIdsCache._projectEventCount.TryGetValue(identity.QueryId, out count))
    {
      switch (identity)
      {
        case AllProjectIdentity _:
          dateTime = DateTime.Now;
          DateTime date1 = dateTime.Date;
          dateTime = DateTime.Now;
          dateTime = dateTime.Date;
          DateTime due1 = dateTime.AddDays(90.0);
          count = (await TaskDisplayService.GetCalendarEventsInSpan(date1, due1)).Item1;
          break;
        case TodayProjectIdentity _:
          dateTime = DateTime.Now;
          DateTime date2 = dateTime.Date;
          dateTime = DateTime.Now;
          dateTime = dateTime.Date;
          DateTime due2 = dateTime.AddDays(1.0);
          count = (await TaskDisplayService.GetCalendarEventsInSpan(date2, due2)).Item1;
          break;
        case TomorrowProjectIdentity _:
          dateTime = DateTime.Now;
          dateTime = dateTime.Date;
          DateTime start = dateTime.AddDays(1.0);
          dateTime = DateTime.Now;
          dateTime = dateTime.Date;
          DateTime due3 = dateTime.AddDays(2.0);
          count = (await TaskDisplayService.GetCalendarEventsInSpan(start, due3)).Item1;
          break;
        case WeekProjectIdentity _:
          dateTime = DateTime.Now;
          DateTime date3 = dateTime.Date;
          dateTime = DateTime.Now;
          dateTime = dateTime.Date;
          DateTime due4 = dateTime.AddDays(7.0);
          count = (await TaskDisplayService.GetCalendarEventsInSpan(date3, due4)).Item1;
          break;
        case FilterProjectIdentity filterProjectIdentity:
          count = await TaskDisplayService.GetDisplayEventsCountInFilter(filterProjectIdentity.Filter, false);
          break;
        case SubscribeCalendarProjectIdentity calendarProjectIdentity1:
          count = (await CalendarEventDao.GetDisplayEventsInCalendar(calendarProjectIdentity1.Profile.Id, 1, DateTime.Now.Date, DateTime.Now.AddMonths(3))).Count;
          break;
        case BindAccountCalendarProjectIdentity calendarProjectIdentity2:
          count = (await CalendarEventDao.GetDisplayEventsInBindAccount(calendarProjectIdentity2.Account.Id, DateTime.Today, DateTime.Now.AddMonths(3))).Count;
          break;
      }
      ProjectAndTaskIdsCache._projectEventCount[identity.QueryId] = count;
    }
    switch (identity)
    {
      case TodayProjectIdentity _:
      case WeekProjectIdentity _:
        List<HabitBaseViewModel> habitsInToday = await TaskDisplayService.GetHabitsInToday();
        count += habitsInToday != null ? habitsInToday.Count<HabitBaseViewModel>((Func<HabitBaseViewModel, bool>) (h => h.Status == 0)) : 0;
        if (LocalSettings.Settings.UserPreference?.TimeTable?.ShowInSmart.GetValueOrDefault())
        {
          DateTime today = DateTime.Today;
          dateTime = DateTime.Today;
          DateTime end = dateTime.AddDays(identity is TodayProjectIdentity ? 1.0 : 7.0);
          List<TaskBaseViewModel> viewModelsInSpan = await TaskDisplayService.GetCourseViewModelsInSpan(today, end);
          // ISSUE: explicit non-virtual call
          count += viewModelsInSpan != null ? __nonvirtual (viewModelsInSpan.Count) : 0;
          break;
        }
        break;
      case TomorrowProjectIdentity _ when LocalSettings.Settings.UserPreference?.TimeTable?.ShowInSmart.GetValueOrDefault():
        dateTime = DateTime.Today;
        DateTime start1 = dateTime.AddDays(1.0);
        dateTime = DateTime.Today;
        DateTime end1 = dateTime.AddDays(2.0);
        List<TaskBaseViewModel> viewModelsInSpan1 = await TaskDisplayService.GetCourseViewModelsInSpan(start1, end1);
        // ISSUE: explicit non-virtual call
        count += viewModelsInSpan1 != null ? __nonvirtual (viewModelsInSpan1.Count) : 0;
        break;
    }
    return count;
  }

  private static async Task<ProjectTaskIds> GetProjectTaskIds(ProjectIdentity project)
  {
    if (project == null || !TaskViewModelHelper.Loaded)
      return (ProjectTaskIds) null;
    ProjectTaskIds idsModel = new ProjectTaskIds()
    {
      Project = project
    };
    switch (project)
    {
      case AllProjectIdentity _:
        ProjectAndTaskIdsCache.ArrangeTaskIds(idsModel, TaskCache.LocalTaskViewModels.Values.Where<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (t =>
        {
          if (t == null || t.Deleted != 0 || t.Status != 0)
            return false;
          ProjectModel projectById = ProjectAndTaskIdsCache.GetProjectById(t.ProjectId);
          return projectById != null && projectById.IsValid() && (projectById.inAll || t.IsAssignToMe());
        })));
        break;
      case TodayProjectIdentity _:
        List<TaskBaseViewModel> list1 = TaskCache.LocalTaskViewModels.Values.ToList<TaskBaseViewModel>();
        List<FilterDatePair> startEndPairs1 = new List<FilterDatePair>();
        DateTime? start1 = new DateTime?(DateTime.Today);
        DateTime today1 = DateTime.Today;
        DateTime? end1 = new DateTime?(today1.AddDays(1.0));
        startEndPairs1.Add(new FilterDatePair(start1, end1));
        ProjectAndTaskIdsCache.ArrangeTaskIds(idsModel, TaskViewModelHelper.GetTasks(list1, startEndPairs: startEndPairs1, showComplete: false, inTodayOrWeek: true, inAll: true));
        List<TaskBaseViewModel> list2 = TaskDetailItemCache.LocalCheckItemViewModels.Values.ToList<TaskBaseViewModel>();
        List<FilterDatePair> startEndPairs2 = new List<FilterDatePair>();
        DateTime? start2 = new DateTime?(DateTime.Today);
        today1 = DateTime.Today;
        DateTime? end2 = new DateTime?(today1.AddDays(1.0));
        startEndPairs2.Add(new FilterDatePair(start2, end2));
        ProjectAndTaskIdsCache.ArrangeItemIds(idsModel, TaskViewModelHelper.GetItems(list2, startEndPairs: startEndPairs2, showComplete: false, inTodayOrWeek: true, checkInAll: true));
        break;
      case TomorrowProjectIdentity _:
        List<TaskBaseViewModel> list3 = TaskCache.LocalTaskViewModels.Values.ToList<TaskBaseViewModel>();
        ProjectAndTaskIdsCache.ArrangeTaskIds(idsModel, TaskViewModelHelper.GetTasks(list3, startEndPairs: new List<FilterDatePair>()
        {
          new FilterDatePair(new DateTime?(DateTime.Today.AddDays(1.0)), new DateTime?(DateTime.Today.AddDays(2.0)))
        }, showComplete: false, inAll: true));
        List<TaskBaseViewModel> list4 = TaskDetailItemCache.LocalCheckItemViewModels.Values.ToList<TaskBaseViewModel>();
        ProjectAndTaskIdsCache.ArrangeItemIds(idsModel, TaskViewModelHelper.GetItems(list4, startEndPairs: new List<FilterDatePair>()
        {
          new FilterDatePair(new DateTime?(DateTime.Today.AddDays(1.0)), new DateTime?(DateTime.Today.AddDays(2.0)))
        }, showComplete: false, checkInAll: true));
        break;
      case WeekProjectIdentity _:
        List<TaskBaseViewModel> list5 = TaskCache.LocalTaskViewModels.Values.ToList<TaskBaseViewModel>();
        List<FilterDatePair> startEndPairs3 = new List<FilterDatePair>();
        DateTime? start3 = new DateTime?(DateTime.Today);
        DateTime today2 = DateTime.Today;
        DateTime? end3 = new DateTime?(today2.AddDays(7.0));
        startEndPairs3.Add(new FilterDatePair(start3, end3));
        ProjectAndTaskIdsCache.ArrangeTaskIds(idsModel, TaskViewModelHelper.GetTasks(list5, startEndPairs: startEndPairs3, showComplete: false, inTodayOrWeek: true, inAll: true));
        List<TaskBaseViewModel> list6 = TaskDetailItemCache.LocalCheckItemViewModels.Values.ToList<TaskBaseViewModel>();
        List<FilterDatePair> startEndPairs4 = new List<FilterDatePair>();
        DateTime? start4 = new DateTime?(DateTime.Today);
        today2 = DateTime.Today;
        DateTime? end4 = new DateTime?(today2.AddDays(7.0));
        startEndPairs4.Add(new FilterDatePair(start4, end4));
        ProjectAndTaskIdsCache.ArrangeItemIds(idsModel, TaskViewModelHelper.GetItems(list6, startEndPairs: startEndPairs4, showComplete: false, inTodayOrWeek: true, checkInAll: true));
        break;
      case GroupProjectIdentity groupProjectIdentity:
        List<TaskBaseViewModel> list7 = TaskCache.LocalTaskViewModels.Values.ToList<TaskBaseViewModel>();
        ProjectAndTaskIdsCache.ArrangeTaskIds(idsModel, TaskViewModelHelper.GetTasks(list7, new List<string>()
        {
          groupProjectIdentity.GroupId
        }, showComplete: false));
        break;
      case AssignToMeProjectIdentity _:
        List<TaskBaseViewModel> list8 = TaskCache.LocalTaskViewModels.Values.ToList<TaskBaseViewModel>();
        ProjectAndTaskIdsCache.ArrangeTaskIds(idsModel, TaskViewModelHelper.GetTasks(list8, showComplete: false, assignTo: new List<string>()
        {
          "me"
        }));
        break;
      default:
        TagProjectIdentity tagProjectIdentity = project as TagProjectIdentity;
        if (tagProjectIdentity == null)
        {
          switch (project)
          {
            case FilterProjectIdentity filterProjectIdentity:
              ProjectAndTaskIdsCache.ArrangeModelIds(idsModel, await TaskDisplayService.GetDisplayTaskInFilter(filterProjectIdentity.Filter.rule, false, true, false));
              break;
            case NormalProjectIdentity _:
              ProjectAndTaskIdsCache.ArrangeModelIds(idsModel, TaskCache.GetModelsInProject(project.Id));
              break;
          }
        }
        else
        {
          List<string> list9 = CacheManager.GetTags().Where<TagModel>((Func<TagModel, bool>) (t => t.name == tagProjectIdentity.Tag || t.parent == tagProjectIdentity.Tag)).Select<TagModel, string>((Func<TagModel, string>) (t => t.name)).ToList<string>();
          ProjectAndTaskIdsCache.ArrangeTaskIds(idsModel, TaskViewModelHelper.GetTasks(TaskCache.LocalTaskViewModels.Values.ToList<TaskBaseViewModel>(), tags: list9, showComplete: false));
          break;
        }
        break;
    }
    return idsModel;
  }

  private static void ArrangeModelIds(ProjectTaskIds idsModel, List<TaskBaseViewModel> models)
  {
    foreach (TaskBaseViewModel model in models)
    {
      if (model.IsTaskOrNote)
      {
        if (model.IsNote)
          idsModel.UnCompletedNoteIds.Add(model.Id);
        else if (model.Status == 0)
          idsModel.UnCompletedTaskIds.Add(model.Id);
      }
      else if (model.IsCheckItem && model.Status == 0)
        idsModel.UnCompletedItemIds.Add(model.Id);
    }
  }

  private static void ArrangeTaskIds(ProjectTaskIds idsModel, IEnumerable<TaskBaseViewModel> tasks)
  {
    foreach (TaskBaseViewModel task in tasks)
    {
      if (task.IsNote)
        idsModel.UnCompletedNoteIds.Add(task.Id);
      else if (task.Status == 0)
        idsModel.UnCompletedTaskIds.Add(task.Id);
    }
  }

  private static void ArrangeItemIds(ProjectTaskIds idsModel, IEnumerable<TaskBaseViewModel> items)
  {
    foreach (TaskBaseViewModel taskBaseViewModel in items)
    {
      if (taskBaseViewModel.Status == 0)
        idsModel.UnCompletedItemIds.Add(taskBaseViewModel.Id);
    }
  }

  public static async void OnTasksChanged(
    List<TaskBaseViewModel> tasks,
    CheckMatchedType checkMatched)
  {
    ParallelQuery<ProjectTaskIds> identities = ProjectAndTaskIdsCache._projectTasksDict.Values.ToList<ProjectTaskIds>().AsParallel<ProjectTaskIds>();
    Task.Run((Action) (() =>
    {
      foreach (ProjectTaskIds projectTasksModel in identities)
        ProjectAndTaskIdsCache.CheckTasksMatched(projectTasksModel, tasks, checkMatched);
      if (checkMatched == CheckMatchedType.None)
        return;
      ProjectAndTaskIdsCache.TryDelayLoadCount();
    }));
  }

  private static async void CheckTasksMatched(
    ProjectTaskIds projectTasksModel,
    List<TaskBaseViewModel> tasks,
    CheckMatchedType checkMatched)
  {
    if (tasks == null || projectTasksModel == null || checkMatched == CheckMatchedType.None)
      return;
    ProjectAndTaskIdsCache.CheckMatchedTasks(projectTasksModel, tasks, checkMatched);
  }

  public static void OnTaskChanged(TaskBaseViewModel task, CheckMatchedType checkMatched)
  {
    ProjectAndTaskIdsCache.OnTasksChanged(new List<TaskBaseViewModel>()
    {
      task
    }, checkMatched);
  }

  private static async Task CheckMatchedTasks(
    ProjectTaskIds projectTasksModel,
    List<TaskBaseViewModel> tasks,
    CheckMatchedType checkMatched)
  {
    if (tasks == null || tasks.Count == 0)
      return;
    List<TaskBaseViewModel> source = (List<TaskBaseViewModel>) null;
    List<TaskBaseViewModel> taskBaseViewModelList1 = (List<TaskBaseViewModel>) null;
    List<TaskBaseViewModel> taskBaseViewModelList2 = (List<TaskBaseViewModel>) null;
    ProjectIdentity project = projectTasksModel.Project;
    switch (project)
    {
      case AllProjectIdentity _:
        if (checkMatched != CheckMatchedType.All && checkMatched != CheckMatchedType.CheckProject)
          return;
        source = tasks.Where<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (t =>
        {
          if (t == null || t.Deleted != 0)
            return false;
          ProjectModel projectById = ProjectAndTaskIdsCache.GetProjectById(t.ProjectId);
          return projectById != null && projectById.IsValid() && (projectById.inAll || t.IsAssignToMe());
        })).ToList<TaskBaseViewModel>();
        break;
      case TodayProjectIdentity _:
        if (checkMatched != CheckMatchedType.All && checkMatched != CheckMatchedType.CheckSmart)
          return;
        source = TaskViewModelHelper.GetTasks(tasks, startEndPairs: new List<FilterDatePair>()
        {
          new FilterDatePair(new DateTime?(DateTime.Today), new DateTime?(DateTime.Today.AddDays(1.0)))
        }, inTodayOrWeek: true).ToList<TaskBaseViewModel>();
        if (checkMatched == CheckMatchedType.All)
        {
          taskBaseViewModelList1 = TaskDetailItemCache.GetCheckItemsInTaskIds(tasks.Select<TaskBaseViewModel, string>((Func<TaskBaseViewModel, string>) (t => t.Id)).ToList<string>());
          taskBaseViewModelList2 = TaskViewModelHelper.GetItems(taskBaseViewModelList1, startEndPairs: new List<FilterDatePair>()
          {
            new FilterDatePair(new DateTime?(DateTime.Today), new DateTime?(DateTime.Today.AddDays(1.0)))
          }, inTodayOrWeek: true).ToList<TaskBaseViewModel>();
          break;
        }
        break;
      case TomorrowProjectIdentity _:
        if (checkMatched != CheckMatchedType.All && checkMatched != CheckMatchedType.CheckSmart)
          return;
        List<TaskBaseViewModel> allTasks = tasks;
        List<FilterDatePair> startEndPairs1 = new List<FilterDatePair>();
        DateTime today = DateTime.Today;
        DateTime? start1 = new DateTime?(today.AddDays(1.0));
        today = DateTime.Today;
        DateTime? end1 = new DateTime?(today.AddDays(2.0));
        startEndPairs1.Add(new FilterDatePair(start1, end1));
        source = TaskViewModelHelper.GetTasks(allTasks, startEndPairs: startEndPairs1).ToList<TaskBaseViewModel>();
        if (checkMatched == CheckMatchedType.All)
        {
          taskBaseViewModelList1 = TaskDetailItemCache.GetCheckItemsInTaskIds(tasks.Select<TaskBaseViewModel, string>((Func<TaskBaseViewModel, string>) (t => t.Id)).ToList<string>());
          List<TaskBaseViewModel> allItems = taskBaseViewModelList1;
          List<FilterDatePair> startEndPairs2 = new List<FilterDatePair>();
          today = DateTime.Today;
          DateTime? start2 = new DateTime?(today.AddDays(1.0));
          today = DateTime.Today;
          DateTime? end2 = new DateTime?(today.AddDays(2.0));
          startEndPairs2.Add(new FilterDatePair(start2, end2));
          taskBaseViewModelList2 = TaskViewModelHelper.GetItems(allItems, startEndPairs: startEndPairs2).ToList<TaskBaseViewModel>();
          break;
        }
        break;
      case WeekProjectIdentity _:
        if (checkMatched != CheckMatchedType.All && checkMatched != CheckMatchedType.CheckSmart)
          return;
        source = TaskViewModelHelper.GetTasks(tasks, startEndPairs: new List<FilterDatePair>()
        {
          new FilterDatePair(new DateTime?(DateTime.Today), new DateTime?(DateTime.Today.AddDays(7.0)))
        }, inTodayOrWeek: true).ToList<TaskBaseViewModel>();
        if (checkMatched == CheckMatchedType.All)
        {
          taskBaseViewModelList1 = TaskDetailItemCache.GetCheckItemsInTaskIds(tasks.Select<TaskBaseViewModel, string>((Func<TaskBaseViewModel, string>) (t => t.Id)).ToList<string>());
          taskBaseViewModelList2 = TaskViewModelHelper.GetItems(taskBaseViewModelList1, startEndPairs: new List<FilterDatePair>()
          {
            new FilterDatePair(new DateTime?(DateTime.Today), new DateTime?(DateTime.Today.AddDays(7.0)))
          }, inTodayOrWeek: true).ToList<TaskBaseViewModel>();
          break;
        }
        break;
      case GroupProjectIdentity groupProjectIdentity:
        if (checkMatched != CheckMatchedType.All && checkMatched != CheckMatchedType.CheckGroup && checkMatched != CheckMatchedType.CheckProject)
          return;
        source = TaskViewModelHelper.GetTasks(tasks, new List<string>()
        {
          groupProjectIdentity.GroupId
        }).ToList<TaskBaseViewModel>();
        break;
      case AssignToMeProjectIdentity _:
        if (checkMatched != CheckMatchedType.All && checkMatched != CheckMatchedType.CheckAssign)
          return;
        source = TaskViewModelHelper.GetTasks(tasks, assignTo: new List<string>()
        {
          "me"
        }).ToList<TaskBaseViewModel>();
        break;
      default:
        TagProjectIdentity tagProjectIdentity = project as TagProjectIdentity;
        if (tagProjectIdentity == null)
        {
          if (!(project is FilterProjectIdentity filterProjectIdentity))
          {
            NormalProjectIdentity normalProjectIdentity = project as NormalProjectIdentity;
            if (normalProjectIdentity != null)
            {
              if (checkMatched != CheckMatchedType.All && checkMatched != CheckMatchedType.CheckProject)
                return;
              source = tasks.Where<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (p => p.ProjectId == normalProjectIdentity.Id)).ToList<TaskBaseViewModel>();
              break;
            }
            break;
          }
          source = TaskService.GetTasksMatchedFilter(filterProjectIdentity.Filter, tasks);
          taskBaseViewModelList1 = TaskDetailItemCache.GetCheckItemsInTaskIds(tasks.Select<TaskBaseViewModel, string>((Func<TaskBaseViewModel, string>) (t => t.Id)).ToList<string>());
          taskBaseViewModelList2 = TaskService.GetCheckItemsMatchedFilter(filterProjectIdentity.Filter, taskBaseViewModelList1);
          break;
        }
        if (checkMatched != CheckMatchedType.All && checkMatched != CheckMatchedType.CheckTag)
          return;
        source = TaskViewModelHelper.GetTasks(tasks, tags: CacheManager.GetTags().Where<TagModel>((Func<TagModel, bool>) (t => t.name == tagProjectIdentity.Tag || t.parent == tagProjectIdentity.Tag)).Select<TagModel, string>((Func<TagModel, string>) (t => t.name)).ToList<string>(), showComplete: !LocalSettings.Settings.HideComplete).ToList<TaskBaseViewModel>();
        break;
    }
    foreach (TaskBaseViewModel task in tasks)
    {
      projectTasksModel.UnCompletedTaskIds.Remove(task.Id);
      projectTasksModel.UnCompletedNoteIds.Remove(task.Id);
    }
    if (source != null)
    {
      foreach (TaskBaseViewModel taskBaseViewModel in source.Where<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (task => task.Status == 0 && task.Deleted == 0)))
      {
        if (taskBaseViewModel.IsNote)
          projectTasksModel.UnCompletedNoteIds.Add(taskBaseViewModel.Id);
        else
          projectTasksModel.UnCompletedTaskIds.Add(taskBaseViewModel.Id);
      }
    }
    // ISSUE: explicit non-virtual call
    if (taskBaseViewModelList1 != null && __nonvirtual (taskBaseViewModelList1.Count) > 0)
    {
      foreach (TaskBaseViewModel taskBaseViewModel in taskBaseViewModelList1)
        projectTasksModel.UnCompletedItemIds.Remove(taskBaseViewModel.Id);
    }
    // ISSUE: explicit non-virtual call
    if (taskBaseViewModelList2 == null || __nonvirtual (taskBaseViewModelList2.Count) <= 0)
      return;
    foreach (TaskBaseViewModel taskBaseViewModel in taskBaseViewModelList2)
    {
      if (taskBaseViewModel.Status == 0)
        projectTasksModel.UnCompletedItemIds.Add(taskBaseViewModel.Id);
    }
  }

  public static void OnTaskRemoved(TaskBaseViewModel task)
  {
    foreach (ProjectTaskIds projectTaskIds in (IEnumerable<ProjectTaskIds>) ProjectAndTaskIdsCache._projectTasksDict.Values)
    {
      projectTaskIds.UnCompletedTaskIds.Remove(task.Id);
      projectTaskIds.UnCompletedNoteIds.Remove(task.Id);
    }
  }

  public static async void OnCheckItemsChanged(List<TaskBaseViewModel> models)
  {
    bool changed = false;
    List<TaskBaseViewModel> source = models;
    models = source != null ? source.Where<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (m => m != null)).ToList<TaskBaseViewModel>() : (List<TaskBaseViewModel>) null;
    if (models == null || models.Count == 0)
      return;
    foreach (ProjectTaskIds projectTasksModel in (IEnumerable<ProjectTaskIds>) ProjectAndTaskIdsCache._projectTasksDict.Values)
    {
      if (await ProjectAndTaskIdsCache.GetMatchedItems(projectTasksModel, models))
        changed = true;
    }
    if (!changed)
      return;
    ProjectAndTaskIdsCache.TryDelayLoadCount();
    await Task.Delay(200);
  }

  private static async Task<bool> GetMatchedItems(
    ProjectTaskIds projectTasksModel,
    List<TaskBaseViewModel> items)
  {
    if (items == null || items.Count == 0 || projectTasksModel == null)
      return false;
    bool matchedItems = false;
    List<TaskBaseViewModel> taskBaseViewModelList = (List<TaskBaseViewModel>) null;
    switch (projectTasksModel.Project)
    {
      case TodayProjectIdentity _:
        taskBaseViewModelList = TaskViewModelHelper.GetItems(items, startEndPairs: new List<FilterDatePair>()
        {
          new FilterDatePair(new DateTime?(DateTime.Today), new DateTime?(DateTime.Today.AddDays(1.0)))
        }, inTodayOrWeek: true).ToList<TaskBaseViewModel>();
        break;
      case TomorrowProjectIdentity _:
        List<TaskBaseViewModel> allItems = items;
        List<FilterDatePair> startEndPairs = new List<FilterDatePair>();
        DateTime today = DateTime.Today;
        DateTime? start = new DateTime?(today.AddDays(1.0));
        today = DateTime.Today;
        DateTime? end = new DateTime?(today.AddDays(2.0));
        startEndPairs.Add(new FilterDatePair(start, end));
        taskBaseViewModelList = TaskViewModelHelper.GetItems(allItems, startEndPairs: startEndPairs, inTodayOrWeek: true).ToList<TaskBaseViewModel>();
        break;
      case WeekProjectIdentity _:
        taskBaseViewModelList = TaskViewModelHelper.GetItems(items, startEndPairs: new List<FilterDatePair>()
        {
          new FilterDatePair(new DateTime?(DateTime.Today), new DateTime?(DateTime.Today.AddDays(7.0)))
        }, inTodayOrWeek: true).ToList<TaskBaseViewModel>();
        break;
      case FilterProjectIdentity filterProjectIdentity:
        taskBaseViewModelList = TaskService.GetCheckItemsMatchedFilter(filterProjectIdentity.Filter, items);
        break;
    }
    foreach (TaskBaseViewModel taskBaseViewModel in items)
    {
      if (projectTasksModel.UnCompletedItemIds.Remove(taskBaseViewModel.Id))
        matchedItems = true;
    }
    // ISSUE: explicit non-virtual call
    if (taskBaseViewModelList != null && __nonvirtual (taskBaseViewModelList.Count) > 0)
    {
      foreach (TaskBaseViewModel taskBaseViewModel in taskBaseViewModelList)
      {
        if (taskBaseViewModel.Status == 0 && projectTasksModel.UnCompletedItemIds.Add(taskBaseViewModel.Id))
          matchedItems = true;
      }
    }
    return matchedItems;
  }

  public static async void OnCheckItemsRemoved(List<TaskBaseViewModel> items)
  {
    bool flag = false;
    foreach (ProjectTaskIds projectTaskIds in (IEnumerable<ProjectTaskIds>) ProjectAndTaskIdsCache._projectTasksDict.Values)
    {
      if (projectTaskIds != null)
      {
        foreach (TaskBaseViewModel taskBaseViewModel in items)
        {
          if (projectTaskIds.UnCompletedItemIds.Remove(taskBaseViewModel.Id))
            flag = true;
        }
      }
    }
    if (!flag)
      return;
    ProjectAndTaskIdsCache.TryDelayLoadCount();
  }

  public static void RemoveIdsModelByKey(string tag)
  {
    try
    {
      ProjectAndTaskIdsCache._projectTasksDict.TryRemove(tag, out ProjectTaskIds _);
    }
    catch (Exception ex)
    {
    }
  }

  public static async void ResetIds(bool all = false)
  {
    List<Task> taskList = new List<Task>()
    {
      ProjectAndTaskIdsCache.ResetProjectTaskIds((ProjectIdentity) new TodayProjectIdentity()),
      ProjectAndTaskIdsCache.ResetProjectTaskIds((ProjectIdentity) new TomorrowProjectIdentity()),
      ProjectAndTaskIdsCache.ResetProjectTaskIds((ProjectIdentity) new WeekProjectIdentity())
    };
    foreach (FilterModel filter in CacheManager.GetFilters())
      taskList.Add(ProjectAndTaskIdsCache.ResetProjectTaskIds((ProjectIdentity) new FilterProjectIdentity(filter)));
    if (all)
    {
      taskList.Add(ProjectAndTaskIdsCache.ResetProjectTaskIds((ProjectIdentity) new AllProjectIdentity()));
      taskList.Add(ProjectAndTaskIdsCache.ResetProjectTaskIds((ProjectIdentity) new AssignToMeProjectIdentity()));
      foreach (TagModel tag in CacheManager.GetTags())
        taskList.Add(ProjectAndTaskIdsCache.ResetProjectTaskIds((ProjectIdentity) new TagProjectIdentity(tag)));
      foreach (ProjectModel project in CacheManager.GetProjects())
        taskList.Add(ProjectAndTaskIdsCache.ResetProjectTaskIds((ProjectIdentity) new NormalProjectIdentity(project)));
      foreach (ProjectGroupModel projectGroup in CacheManager.GetProjectGroups())
        taskList.Add(ProjectAndTaskIdsCache.ResetProjectTaskIds((ProjectIdentity) new GroupProjectIdentity(projectGroup)));
    }
    Task task = await Task.WhenAny(Task.WhenAll((IEnumerable<Task>) taskList), Task.Delay(1000));
    ProjectAndTaskIdsCache.TryDelayLoadCount();
  }

  public static void Clear()
  {
    ProjectAndTaskIdsCache._projectTasksDict.Clear();
    ProjectAndTaskIdsCache._projectDict.Clear();
    TaskCountCache.Clear();
  }

  public static void RemoveEventCache(ProjectIdentity project)
  {
    if (project == null)
      return;
    ProjectAndTaskIdsCache._projectEventCount.TryRemove(project.QueryId, out int _);
  }
}
