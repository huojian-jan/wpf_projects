// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.TaskViewModelHelper
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Service;
using ticktick_WPF.Util.Filter;
using ticktick_WPF.Util.Provider;
using ticktick_WPF.Util.Sync;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Project;
using ticktick_WPF.Views.Search;
using ticktick_WPF.Views.Summary;

#nullable disable
namespace ticktick_WPF.Util
{
  public static class TaskViewModelHelper
  {
    public static async Task Init()
    {
      try
      {
        await TaskDetailItemCache.InitLocalCheckItems(await TaskCache.InitLocalTasks());
      }
      finally
      {
        TaskViewModelHelper.Loaded = true;
      }
    }

    public static bool Loaded { get; set; }

    public static List<TaskBaseViewModel> GetModel(
      List<string> groups = null,
      List<string> projects = null,
      List<string> tags = null,
      List<FilterDatePair> startEndPairs = null,
      bool showComplete = true,
      bool showCheckItem = false,
      bool inTodayOrWeek = false,
      bool inAll = false,
      List<string> assignTo = null,
      List<int> priorities = null,
      LogicType type = LogicType.Or,
      bool withChild = false,
      bool inCal = false,
      bool orTag = false,
      TaskType taskType = TaskType.TaskAndNote,
      string keyword = null)
    {
      List<TaskBaseViewModel> list = TaskViewModelHelper.GetTasks(TaskCache.GetAllTask(), groups, projects, tags, startEndPairs, showComplete, inTodayOrWeek, inAll, assignTo, priorities, type, withChild, inCal, orTag, taskType, keyword).Where<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (t => t != null)).ToList<TaskBaseViewModel>();
      if (showCheckItem && taskType != TaskType.Note)
      {
        IEnumerable<TaskBaseViewModel> items = TaskViewModelHelper.GetItems(TaskDetailItemCache.GetAllCheckItems(), groups, projects, tags, startEndPairs, showComplete, inTodayOrWeek, inAll, assignTo, priorities, type, orTag, keyword);
        list.AddRange(items);
      }
      return list;
    }

    public static IEnumerable<TaskBaseViewModel> GetItems(
      List<TaskBaseViewModel> allItems,
      List<string> groups = null,
      List<string> projects = null,
      List<string> tags = null,
      List<FilterDatePair> startEndPairs = null,
      bool showComplete = true,
      bool inTodayOrWeek = false,
      bool checkInAll = false,
      List<string> assignTo = null,
      List<int> priorities = null,
      LogicType type = LogicType.Or,
      bool orTag = false,
      string keyword = null)
    {
      Dictionary<string, ProjectModel> projectDicts = CacheManager.GetProjects().ToDictionary<ProjectModel, string, ProjectModel>((Func<ProjectModel, string>) (p => p.id), (Func<ProjectModel, ProjectModel>) (p => p));
      keyword = keyword?.Trim();
      string str = keyword;
      List<string> stringList1;
      if (str == null)
        stringList1 = (List<string>) null;
      else
        stringList1 = ((IEnumerable<string>) str.Split(' ')).ToList<string>();
      List<string> keys = stringList1;
      keys?.Remove("");
      return allItems.Where<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (item =>
      {
        if (string.IsNullOrEmpty(item.ParentId) || !TaskCache.LocalTaskViewModels.ContainsKey(item.ParentId))
          return false;
        TaskBaseViewModel localTaskViewModel = TaskCache.LocalTaskViewModels[item.ParentId];
        if (Utils.IsEmptyDate(item.StartDate) || !TaskViewModelHelper.CheckTask(localTaskViewModel, false) || localTaskViewModel.Kind != "CHECKLIST" || !showComplete && item.Status != 0 || string.IsNullOrEmpty(localTaskViewModel.ProjectId) || !projectDicts.ContainsKey(localTaskViewModel.ProjectId))
          return false;
        ProjectModel project = projectDicts[localTaskViewModel.ProjectId];
        if (orTag)
        {
          List<string> stringList2 = projects;
          // ISSUE: explicit non-virtual call
          if ((stringList2 != null ? (__nonvirtual (stringList2.Count) > 0 ? 1 : 0) : 0) == 0)
          {
            List<string> stringList3 = groups;
            // ISSUE: explicit non-virtual call
            if ((stringList3 != null ? (__nonvirtual (stringList3.Count) > 0 ? 1 : 0) : 0) == 0)
              goto label_10;
          }
          List<string> stringList4 = tags;
          // ISSUE: explicit non-virtual call
          if ((stringList4 != null ? (__nonvirtual (stringList4.Count) > 0 ? 1 : 0) : 0) != 0)
          {
            if (!TaskViewModelHelper.CheckProjectAndGroups(project, projects, groups, checkInAll, type, localTaskViewModel.IsAssignToMe()) && !TaskViewModelHelper.CheckTag(localTaskViewModel, tags, type))
              return false;
            goto label_12;
          }
        }
label_10:
        if (!TaskViewModelHelper.CheckProjectAndGroups(project, projects, groups, checkInAll, type, localTaskViewModel.IsAssignToMe()) || !TaskViewModelHelper.CheckTag(localTaskViewModel, tags, type))
          return false;
label_12:
        if (!TaskViewModelHelper.CheckPriority(localTaskViewModel, priorities) || !TaskViewModelHelper.CheckAssignTo(localTaskViewModel, project, assignTo))
          return false;
        bool flag1 = true;
        List<FilterDatePair> filterDatePairList = startEndPairs;
        // ISSUE: explicit non-virtual call
        if ((filterDatePairList != null ? (__nonvirtual (filterDatePairList.Count) > 0 ? 1 : 0) : 0) != 0)
        {
          bool flag2 = false;
          foreach (FilterDatePair startEndPair in startEndPairs)
          {
            if (!startEndPair.isRepeat)
            {
              DateTime? start = startEndPair.Start;
              DateTime? end = startEndPair.End;
              if (start.HasValue || end.HasValue || startEndPair.IsNoDate != !Utils.IsEmptyDate(item.StartDate))
              {
                if (start.HasValue && !end.HasValue)
                {
                  DateTime? startDate = item.StartDate;
                  DateTime? nullable = start;
                  if ((startDate.HasValue & nullable.HasValue ? (startDate.GetValueOrDefault() < nullable.GetValueOrDefault() ? 1 : 0) : 0) != 0 && !inTodayOrWeek)
                    continue;
                }
                if (!start.HasValue && end.HasValue)
                {
                  DateTime? startDate = item.StartDate;
                  DateTime? nullable = end;
                  if ((startDate.HasValue & nullable.HasValue ? (startDate.GetValueOrDefault() >= nullable.GetValueOrDefault() ? 1 : 0) : 0) != 0)
                    continue;
                }
                if (!Utils.IsEmptyDate(start) && !Utils.IsEmptyDate(end))
                {
                  DateTime? startDate1 = item.StartDate;
                  DateTime? nullable1 = end;
                  if ((startDate1.HasValue & nullable1.HasValue ? (startDate1.GetValueOrDefault() >= nullable1.GetValueOrDefault() ? 1 : 0) : 0) == 0)
                  {
                    if (!inTodayOrWeek)
                    {
                      DateTime? startDate2 = item.StartDate;
                      DateTime? nullable2 = start;
                      if ((startDate2.HasValue & nullable2.HasValue ? (startDate2.GetValueOrDefault() < nullable2.GetValueOrDefault() ? 1 : 0) : 0) != 0)
                        continue;
                    }
                    if (inTodayOrWeek)
                    {
                      DateTime? startDate3 = item.StartDate;
                      DateTime? nullable3 = start;
                      if ((startDate3.HasValue & nullable3.HasValue ? (startDate3.GetValueOrDefault() < nullable3.GetValueOrDefault() ? 1 : 0) : 0) != 0 && item.Status != 0)
                      {
                        DateTime? completedTime = item.CompletedTime;
                        DateTime today = DateTime.Today;
                        if ((completedTime.HasValue ? (completedTime.GetValueOrDefault() < today ? 1 : 0) : 0) != 0)
                          continue;
                      }
                    }
                  }
                  else
                    continue;
                }
                flag2 = true;
                break;
              }
            }
          }
          flag1 = type == LogicType.Or ? flag2 : !flag2;
        }
        if (!flag1)
          return false;
        return string.IsNullOrEmpty(keyword) || SearchHelper.KeyWordMatched(item.Title, keyword, keys);
      }));
    }

    public static IEnumerable<TaskBaseViewModel> GetTasks(
      List<TaskBaseViewModel> allTasks,
      List<string> groups = null,
      List<string> projects = null,
      List<string> tags = null,
      List<FilterDatePair> startEndPairs = null,
      bool showComplete = true,
      bool inTodayOrWeek = false,
      bool inAll = false,
      List<string> assignTo = null,
      List<int> priorities = null,
      LogicType type = LogicType.Or,
      bool withChild = false,
      bool inCal = false,
      bool orTag = false,
      TaskType taskType = TaskType.TaskAndNote,
      string keyword = null)
    {
      Dictionary<string, ProjectModel> projectDicts = CacheManager.GetProjects().ToDictionary<ProjectModel, string, ProjectModel>((Func<ProjectModel, string>) (p => p.id), (Func<ProjectModel, ProjectModel>) (p => p));
      List<TaskBaseViewModel> list1 = allTasks.Where<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (task =>
      {
        if (taskType == TaskType.Note && task.Kind != "NOTE" || taskType == TaskType.Task && task.Kind == "NOTE" || !TaskViewModelHelper.CheckTask(task, showComplete) || string.IsNullOrEmpty(task.ProjectId) || !projectDicts.ContainsKey(task.ProjectId))
          return false;
        ProjectModel project = projectDicts[task.ProjectId];
        if (orTag)
        {
          List<string> stringList1 = projects;
          // ISSUE: explicit non-virtual call
          if ((stringList1 != null ? (__nonvirtual (stringList1.Count) > 0 ? 1 : 0) : 0) == 0)
          {
            List<string> stringList2 = groups;
            // ISSUE: explicit non-virtual call
            if ((stringList2 != null ? (__nonvirtual (stringList2.Count) > 0 ? 1 : 0) : 0) == 0)
              goto label_8;
          }
          List<string> stringList3 = tags;
          // ISSUE: explicit non-virtual call
          if ((stringList3 != null ? (__nonvirtual (stringList3.Count) > 0 ? 1 : 0) : 0) != 0)
          {
            if (!TaskViewModelHelper.CheckProjectAndGroups(project, projects, groups, inAll, type, task.IsAssignToMe()) && !TaskViewModelHelper.CheckTag(task, tags, type))
              return false;
            goto label_10;
          }
        }
label_8:
        if (!TaskViewModelHelper.CheckProjectAndGroups(project, projects, groups, inAll, type, task.IsAssignToMe()) || !TaskViewModelHelper.CheckTag(task, tags, type))
          return false;
label_10:
        return TaskViewModelHelper.CheckPriority(task, priorities) && TaskViewModelHelper.CheckAssignTo(task, project, assignTo) && TaskUtils.CheckDate(task, startEndPairs, inTodayOrWeek, type, inCal, inCal || taskType == TaskType.Note);
      })).ToList<TaskBaseViewModel>();
      if (list1.Count > 0 && !string.IsNullOrEmpty(keyword?.Trim()))
      {
        keyword = keyword.Trim();
        List<string> list2 = ((IEnumerable<string>) keyword.Split(' ')).ToList<string>();
        list2.Remove("");
        Dictionary<string, string> dictionary = new Dictionary<string, string>();
        foreach (TaskBaseViewModel taskBaseViewModel in list1)
        {
          if (!dictionary.ContainsKey(taskBaseViewModel.Id))
          {
            string seed = taskBaseViewModel.Title + "\n" + taskBaseViewModel.Content + "\n" + taskBaseViewModel.Desc;
            BlockingList<TaskBaseViewModel> checkItems = taskBaseViewModel.CheckItems;
            if ((checkItems != null ? (checkItems.Count > 0 ? 1 : 0) : 0) != 0)
              seed = taskBaseViewModel.CheckItems.Value.Aggregate<TaskBaseViewModel, string>(seed, (Func<string, TaskBaseViewModel, string>) ((current, check) => current + "\n" + check.Title));
            List<AttachmentModel> attachmentsByTaskId = AttachmentCache.GetAttachmentsByTaskId(taskBaseViewModel.Id);
            // ISSUE: explicit non-virtual call
            if (attachmentsByTaskId != null && __nonvirtual (attachmentsByTaskId.Count) > 0)
              seed = attachmentsByTaskId.Aggregate<AttachmentModel, string>(seed, (Func<string, AttachmentModel, string>) ((current, attachment) => current + "\n[" + attachment.fileName + "]"));
            dictionary.Add(taskBaseViewModel.Id, seed.ToLower());
          }
        }
        HashSet<string> matchTaskIds = new HashSet<string>();
        foreach (string key in dictionary.Keys)
        {
          if (SearchHelper.KeyWordMatched(dictionary[key], keyword, list2))
            matchTaskIds.Add(key);
        }
        list1 = list1.Where<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (t => matchTaskIds.Contains(t.Id))).ToList<TaskBaseViewModel>();
      }
      if (withChild)
      {
        List<TaskBaseViewModel> pinCheckChildren = TaskCache.GetPinCheckChildren(list1.Select<TaskBaseViewModel, string>((Func<TaskBaseViewModel, string>) (t => t.Id)).ToList<string>());
        list1.AddRange((IEnumerable<TaskBaseViewModel>) pinCheckChildren);
      }
      return (IEnumerable<TaskBaseViewModel>) list1;
    }

    public static List<TaskBaseViewModel> GetTasksInArrange(ProjectExtra filter)
    {
      List<TaskBaseViewModel> list = TaskCache.LocalTaskViewModels.Values.ToList<TaskBaseViewModel>();
      List<string> projectIds = filter.ProjectIds;
      // ISSUE: explicit non-virtual call
      if ((projectIds != null ? (__nonvirtual (projectIds.Count) > 0 ? 1 : 0) : 0) == 0)
      {
        List<string> groupIds = filter.GroupIds;
        // ISSUE: explicit non-virtual call
        if ((groupIds != null ? (__nonvirtual (groupIds.Count) > 0 ? 1 : 0) : 0) == 0 && LocalSettings.Settings.ArrangeDisplayType == 0)
          goto label_5;
      }
      if (LocalSettings.Settings.ArrangeDisplayType == 1)
      {
        List<string> tags = filter.Tags;
        // ISSUE: explicit non-virtual call
        if ((tags != null ? (__nonvirtual (tags.Count) > 0 ? 1 : 0) : 0) == 0)
          goto label_5;
      }
      int num = LocalSettings.Settings.ArrangeDisplayType == 2 ? 1 : 0;
      goto label_6;
label_5:
      num = 1;
label_6:
      bool checkInAll = num != 0;
      Func<TaskBaseViewModel, bool> predicate = (Func<TaskBaseViewModel, bool>) (task =>
      {
        if (!TaskViewModelHelper.CheckTask(task, false))
          return false;
        if (LocalSettings.Settings.ArrangeTaskDateType == 1)
        {
          DateTime? nullable = task.StartDate;
          if (!nullable.HasValue)
            return false;
          nullable = task.DueDate;
          if (nullable.HasValue)
          {
            nullable = task.DueDate;
            if (nullable.Value > DateTime.Today)
              goto label_9;
          }
          nullable = task.DueDate;
          if (!nullable.HasValue)
          {
            nullable = task.StartDate;
            DateTime today = DateTime.Today;
            if ((nullable.HasValue ? (nullable.GetValueOrDefault() >= today ? 1 : 0) : 0) == 0)
              goto label_12;
          }
          else
            goto label_12;
label_9:
          return false;
        }
        if (task.StartDate.HasValue)
          return false;
label_12:
        ProjectModel projectById = CacheManager.GetProjectById(task.ProjectId);
        return projectById != null && projectById.IsEnable() && TaskViewModelHelper.CheckProjectAndGroups(projectById, filter.ProjectIds, filter.GroupIds, checkInAll, isMe: task.IsAssignToMe()) && TaskViewModelHelper.CheckTag(task, filter.Tags);
      });
      return list.Where<TaskBaseViewModel>(predicate).ToList<TaskBaseViewModel>();
    }

    public static List<TaskBaseViewModel> GetTaskInSummary(SummaryFilterModel filter)
    {
      List<TaskBaseViewModel> list = TaskCache.LocalTaskViewModels.Values.ToList<TaskBaseViewModel>().Where<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (task =>
      {
        if (!TaskViewModelHelper.CheckTask(task) || task.Kind == "NOTE")
          return false;
        ProjectModel projectById = CacheManager.GetProjectById(task.ProjectId);
        if (projectById == null || !TaskViewModelHelper.CheckProjectAndGroups(projectById, filter.SelectedProjectIds, filter.SelectedProjectGroupIds, false) || !TaskViewModelHelper.CheckTag(task, filter.SelectedTags) || !TaskViewModelHelper.CheckAssignTo(task, projectById, filter.Assigns))
          return false;
        int num;
        if (filter.SelectedPriorities != null && filter.SelectedPriorities.Count > 0)
        {
          List<string> selectedPriorities = filter.SelectedPriorities;
          num = task.Priority;
          string str = num.ToString();
          if (!selectedPriorities.Contains(str))
            return false;
        }
        if (filter.SelectedStatus != null && filter.SelectedStatus.Count > 0 && !filter.SelectedStatus.Contains("all"))
        {
          string str = "uncompleted";
          num = task.Status;
          switch (num)
          {
            case -1:
              str = "wontDo";
              break;
            case 0:
              str = task.Progress <= 0 ? "uncompleted" : "inProgress";
              break;
            case 1:
            case 2:
              str = "completed";
              break;
          }
          if (!filter.SelectedStatus.Contains(str))
            return false;
        }
        if (!filter.StartDate.HasValue || !filter.EndDate.HasValue)
          return true;
        DateTime dateTime1 = filter.EndDate.Value.AddDays(1.0);
        DateTime? nullable;
        if (task.Status != 0)
        {
          nullable = task.CompletedTime;
          if (nullable.HasValue)
          {
            nullable = task.CompletedTime;
            DateTime dateTime2 = nullable.Value;
            nullable = filter.StartDate;
            DateTime dateTime3 = nullable.Value;
            if (dateTime2 >= dateTime3)
            {
              nullable = task.CompletedTime;
              if (nullable.Value < dateTime1)
                return true;
            }
          }
        }
        nullable = task.StartDate;
        if (nullable.HasValue)
        {
          nullable = task.StartDate;
          DateTime dateTime4 = nullable.Value;
          nullable = filter.StartDate;
          DateTime dateTime5 = nullable.Value;
          if (dateTime4 >= dateTime5)
          {
            nullable = task.StartDate;
            if (nullable.Value < dateTime1)
              goto label_28;
          }
          nullable = task.DueDate;
          if (nullable.HasValue)
          {
            nullable = task.DueDate;
            DateTime dateTime6 = nullable.Value;
            nullable = filter.StartDate;
            DateTime dateTime7 = nullable.Value;
            if (dateTime6 > dateTime7)
            {
              nullable = task.DueDate;
              if (nullable.Value < dateTime1)
                goto label_28;
            }
            nullable = task.DueDate;
            if (nullable.Value >= dateTime1)
            {
              nullable = task.StartDate;
              DateTime dateTime8 = nullable.Value;
              nullable = filter.StartDate;
              DateTime dateTime9 = nullable.Value;
              if (!(dateTime8 < dateTime9))
                goto label_29;
            }
            else
              goto label_29;
          }
          else
            goto label_29;
label_28:
          return true;
        }
label_29:
        return false;
      })).ToList<TaskBaseViewModel>();
      Dictionary<string, Node<TaskBaseViewModel>> taskTree = TaskCache.GetAllTaskTree();
      HashSet<string> taskIdSet = new HashSet<string>((IEnumerable<string>) list.Select<TaskBaseViewModel, string>((Func<TaskBaseViewModel, string>) (t => t.Id)).ToList<string>());
      Dictionary<string, TaskBaseViewModel> extraTasks = new Dictionary<string, TaskBaseViewModel>();
      foreach (TaskBaseViewModel taskBaseViewModel in list.Where<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (task => !string.IsNullOrEmpty(task.Id) && taskTree.ContainsKey(task.Id))))
      {
        Node<TaskBaseViewModel> node = taskTree[taskBaseViewModel.Id];
        List<Node<TaskBaseViewModel>> children = node.Children;
        // ISSUE: explicit non-virtual call
        if ((children != null ? (__nonvirtual (children.Count) > 0 ? 1 : 0) : 0) != 0 && !taskIdSet.Contains(node.Value.ParentId) && !string.IsNullOrEmpty(node.Value.Id) && !extraTasks.ContainsKey(node.Value.Id))
          node.GetAllChildrenValue().ForEach((Action<TaskBaseViewModel>) (child =>
          {
            if (string.IsNullOrEmpty(child.Id) || taskIdSet.Contains(child.Id) || child.Deleted != 0 || child.Status != 0 || extraTasks.ContainsKey(child.Id))
              return;
            extraTasks[child.Id] = child;
          }));
      }
      list.AddRange((IEnumerable<TaskBaseViewModel>) extraTasks.Values);
      return list;
    }

    private static bool CheckPriority(TaskBaseViewModel task, List<int> priorities)
    {
      // ISSUE: explicit non-virtual call
      if (priorities == null || __nonvirtual (priorities.Count) <= 0)
        return true;
      return task.Kind != "NOTE" && priorities.Contains(task.Priority);
    }

    private static bool CheckAssignTo(
      TaskBaseViewModel task,
      ProjectModel project,
      List<string> assignTo)
    {
      // ISSUE: explicit non-virtual call
      return assignTo == null || __nonvirtual (assignTo.Count) <= 0 || !string.IsNullOrEmpty(task.Assignee) && task.Assignee != "-1" && (assignTo.Contains(task.Assignee) || assignTo.Contains("anyone")) || assignTo.Contains("me") && task.Assignee == LocalSettings.Settings.LoginUserId && project.IsShareList() || project.IsShareList() && assignTo.Contains("other") && !string.IsNullOrEmpty(task.Assignee) && task.Assignee != "-1" && task.Assignee != LocalSettings.Settings.LoginUserId || assignTo.Contains("noassignee") && (string.IsNullOrEmpty(task.Assignee) || task.Assignee == "-1");
    }

    public static List<TaskBaseViewModel> GetClosedModels(
      DateTime? fromTime,
      DateTime toTime,
      List<string> projectIds,
      int limit,
      bool showCheckItem = false,
      int closedType = 0,
      bool onlyClosedByMe = false)
    {
      IOrderedEnumerable<TaskBaseViewModel> orderedEnumerable;
      switch (closedType)
      {
        case -1:
        case 2:
          orderedEnumerable = TaskCache.LocalTaskViewModels.Values.Where<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (task =>
          {
            if (task == null || task.Status != closedType || task.Deleted != 0)
              return false;
            return !onlyClosedByMe || task.CompletedUser == LocalSettings.Settings.LoginUserId;
          })).OrderByDescending<TaskBaseViewModel, DateTime?>((Func<TaskBaseViewModel, DateTime?>) (t => t.CompletedTime));
          break;
        default:
          orderedEnumerable = TaskCache.LocalTaskViewModels.Values.Where<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (task =>
          {
            if (task == null || task.Status == 0 || task.Deleted != 0)
              return false;
            return !onlyClosedByMe || task.CompletedUser == LocalSettings.Settings.LoginUserId;
          })).OrderByDescending<TaskBaseViewModel, DateTime?>((Func<TaskBaseViewModel, DateTime?>) (t => t.CompletedTime));
          break;
      }
      List<TaskBaseViewModel> taskBaseViewModelList = new List<TaskBaseViewModel>();
      DateTime now = DateTime.Now;
      foreach (TaskBaseViewModel taskBaseViewModel in (IEnumerable<TaskBaseViewModel>) orderedEnumerable)
      {
        ProjectModel projectById = CacheManager.GetProjectById(taskBaseViewModel.ProjectId);
        if (projectById != null && projectById.IsValid())
        {
          List<string> stringList = projectIds;
          // ISSUE: explicit non-virtual call
          if ((stringList != null ? (__nonvirtual (stringList.Count) > 0 ? 1 : 0) : 0) == 0 || projectIds.Contains(taskBaseViewModel.ProjectId))
          {
            DateTime? nullable;
            if (fromTime.HasValue)
            {
              DateTime? completedTime = taskBaseViewModel.CompletedTime;
              nullable = fromTime;
              if ((completedTime.HasValue & nullable.HasValue ? (completedTime.GetValueOrDefault() < nullable.GetValueOrDefault() ? 1 : 0) : 0) != 0)
                continue;
            }
            nullable = taskBaseViewModel.CompletedTime;
            DateTime dateTime1 = toTime;
            if ((nullable.HasValue ? (nullable.GetValueOrDefault() > dateTime1 ? 1 : 0) : 0) == 0)
            {
              taskBaseViewModelList.Add(taskBaseViewModel);
              if (taskBaseViewModelList.Count >= limit)
              {
                nullable = taskBaseViewModel.CompletedTime;
                if (nullable.HasValue)
                {
                  DateTime dateTime2 = now;
                  nullable = taskBaseViewModel.CompletedTime;
                  DateTime dateTime3 = nullable.Value;
                  if ((dateTime2 - dateTime3).TotalSeconds >= 1.0)
                    break;
                }
              }
            }
          }
        }
      }
      List<TaskBaseViewModel> source = taskBaseViewModelList;
      if (showCheckItem && closedType != -1)
      {
        if (!fromTime.HasValue && source.Count > 0)
          fromTime = (DateTime?) source.LastOrDefault<TaskBaseViewModel>()?.CompletedTime;
        IEnumerable<TaskBaseViewModel> collection = TaskDetailItemCache.LocalCheckItemViewModels.Values.ToList<TaskBaseViewModel>().Where<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (item =>
        {
          if (Utils.IsEmptyDate(item.StartDate) || Utils.IsEmptyDate(item.CompletedTime) || string.IsNullOrEmpty(item.ParentId) || !TaskCache.LocalTaskViewModels.ContainsKey(item.ParentId))
            return false;
          TaskBaseViewModel localTaskViewModel = TaskCache.LocalTaskViewModels[item.ParentId];
          if (localTaskViewModel.Status != 0 || item.Status == 0 || Utils.IsEmptyDate(item.StartDate) || !TaskViewModelHelper.CheckTask(localTaskViewModel))
            return false;
          ProjectModel projectById = CacheManager.GetProjectById(localTaskViewModel.ProjectId);
          if (projectById == null)
            return false;
          List<string> stringList = projectIds;
          // ISSUE: explicit non-virtual call
          if ((stringList != null ? (__nonvirtual (stringList.Count) > 0 ? 1 : 0) : 0) != 0 && !projectIds.Contains(projectById.id))
            return false;
          if (fromTime.HasValue)
          {
            DateTime? completedTime = item.CompletedTime;
            DateTime? nullable = fromTime;
            if ((completedTime.HasValue & nullable.HasValue ? (completedTime.GetValueOrDefault() < nullable.GetValueOrDefault() ? 1 : 0) : 0) != 0)
              return false;
          }
          DateTime? completedTime1 = item.CompletedTime;
          DateTime dateTime = toTime;
          return (completedTime1.HasValue ? (completedTime1.GetValueOrDefault() >= dateTime ? 1 : 0) : 0) == 0;
        }));
        source.AddRange(collection);
      }
      return source;
    }

    public static bool CheckTeamTrash(string taskTeamId, bool isPerson)
    {
      int num = UserManager.IsTeamUser() ? 1 : 0;
      string id = num != 0 ? CacheManager.GetTeam()?.id : "";
      bool flag = !isPerson;
      if (num == 0 || flag && taskTeamId == id)
        return true;
      return !flag && taskTeamId != id;
    }

    public static List<TaskBaseViewModel> GetTrashModels(bool isPerson)
    {
      List<TaskBaseViewModel> list = TaskCache.LocalTaskViewModels.Values.Where<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (task => task != null && task.Deleted == 1 && TaskViewModelHelper.CheckTeamTrash(task.TeamId, isPerson))).ToList<TaskBaseViewModel>().OrderByDescending<TaskBaseViewModel, DateTime?>((Func<TaskBaseViewModel, DateTime?>) (t => t.ModifiedTime)).ToList<TaskBaseViewModel>();
      if (TrashSyncService.IsDrainOff(isPerson))
        return list;
      List<TaskBaseViewModel> trashModels = new List<TaskBaseViewModel>();
      int num = TrashSyncService.TrashPage(isPerson) * 50;
      foreach (TaskBaseViewModel taskBaseViewModel in list)
      {
        trashModels.Add(taskBaseViewModel);
        if (trashModels.Count >= num)
          break;
      }
      return trashModels;
    }

    private static bool CheckTask(TaskBaseViewModel task, bool showComplete = true)
    {
      return task != null && task.Deleted == 0 && (showComplete || task.Status == 0);
    }

    private static bool CheckProjectAndGroups(
      ProjectModel project,
      List<string> projects,
      List<string> groups,
      bool checkInAll,
      LogicType type = LogicType.Or,
      bool isMe = true)
    {
      // ISSUE: explicit non-virtual call
      // ISSUE: explicit non-virtual call
      if (!project.IsValid() || ((projects == null || __nonvirtual (projects.Count) <= 0 ? ((groups != null ? (__nonvirtual (groups.Count) > 0 ? 1 : 0) : 0) == 0 ? 1 : 0) : 0) & (checkInAll ? 1 : 0)) != 0 && !project.inAll && !isMe)
        return false;
      // ISSUE: explicit non-virtual call
      // ISSUE: explicit non-virtual call
      // ISSUE: explicit non-virtual call
      // ISSUE: explicit non-virtual call
      if (projects != null && __nonvirtual (projects.Contains("#alllists")) || projects != null && __nonvirtual (projects.Contains("ProjectAll2e4c103c57ef480997943206")) || (projects == null || __nonvirtual (projects.Count) <= 0) && (groups == null || __nonvirtual (groups.Count) <= 0))
        return true;
      // ISSUE: explicit non-virtual call
      // ISSUE: explicit non-virtual call
      bool flag = projects != null && __nonvirtual (projects.Count) > 0 && projects.Contains(project.id) || groups != null && __nonvirtual (groups.Count) > 0 && groups.Contains(project.groupId);
      switch (type)
      {
        case LogicType.Or:
          if (!flag)
            break;
          goto default;
        case LogicType.Not:
          if (!flag)
            goto default;
          else
            break;
        default:
          return true;
      }
      return false;
    }

    private static bool CheckTag(TaskBaseViewModel task, List<string> tags, LogicType type = LogicType.Or)
    {
      string[] tags1 = task.Tags;
      List<string> taskTags = (tags1 != null ? ((IEnumerable<string>) tags1).Select<string, string>((Func<string, string>) (t => t.ToLower())).ToList<string>() : (List<string>) null) ?? new List<string>();
      // ISSUE: explicit non-virtual call
      if (tags != null && __nonvirtual (tags.Count) > 0)
      {
        switch (type)
        {
          case LogicType.And:
            if (tags.Contains("*withtags"))
            {
              tags.Remove("*withtags");
              if (taskTags.Count == 0)
                return false;
            }
            if (tags.Contains("!tag"))
            {
              tags.Remove("!tag");
              if (taskTags.Count > 0)
                return false;
            }
            if (tags.Any<string>((Func<string, bool>) (tag => !taskTags.Contains(tag.ToLower()))))
              return false;
            break;
          case LogicType.Or:
            return tags.Contains("*withtags") && taskTags.Count > 0 || tags.Contains("!tag") && taskTags.Count == 0 || !tags.All<string>((Func<string, bool>) (tag => !taskTags.Contains(tag.ToLower())));
          case LogicType.Not:
            if (tags.Contains("*withtags") && taskTags.Count > 0 || tags.Contains("!tag") && taskTags.Count == 0 || tags.Any<string>((Func<string, bool>) (tag => taskTags.Contains(tag.ToLower()))))
              return false;
            break;
        }
      }
      return true;
    }

    public static async Task<List<SearchContent>> GetTaskSearchContents(
      SearchFilterModel filter,
      List<string> tags)
    {
      Dictionary<string, string> dict = new Dictionary<string, string>();
      foreach (TaskBaseViewModel taskBaseViewModel in TaskCache.GetAllTask().Where<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (t => t != null && t.Deleted == 0)))
      {
        TaskBaseViewModel task = taskBaseViewModel;
        if ((!filter.Status.HasValue || filter.Status.Value == task.Status) && CacheManager.CheckProjectValid(task.ProjectId))
        {
          List<string> projectIds = filter.ProjectIds;
          // ISSUE: explicit non-virtual call
          if ((projectIds != null ? (__nonvirtual (projectIds.Count) > 0 ? 1 : 0) : 0) == 0 || filter.ProjectIds.Contains(task.ProjectId))
          {
            List<int> priorities = filter.Priorities;
            // ISSUE: explicit non-virtual call
            if (((priorities != null ? (__nonvirtual (priorities.Count) > 0 ? 1 : 0) : 0) == 0 || filter.Priorities.Contains(task.Priority)) && (filter.TaskType != TaskType.Note || !(task.Kind != "NOTE")) && (filter.TaskType != TaskType.Task || !(task.Kind == "NOTE")))
            {
              List<string> assignees = filter.Assignees;
              // ISSUE: explicit non-virtual call
              if ((assignees != null ? (__nonvirtual (assignees.Count) > 0 ? 1 : 0) : 0) != 0)
              {
                ProjectModel projectById = CacheManager.GetProjectById(task.ProjectId);
                if (projectById == null || !TaskViewModelHelper.CheckAssignTo(task, projectById, filter.Assignees))
                  continue;
              }
              List<string> stringList = tags;
              // ISSUE: explicit non-virtual call
              if ((stringList != null ? (__nonvirtual (stringList.Count) > 0 ? 1 : 0) : 0) != 0)
              {
                tags = tags.Select<string, string>((Func<string, string>) (t => t.ToLower())).ToList<string>();
                string[] tags1 = task.Tags;
                if ((tags1 != null ? tags1.Length : 0) < tags.Count || ((IEnumerable<string>) task.Tags).Intersect<string>((IEnumerable<string>) tags).Count<string>() < tags.Count)
                  continue;
              }
              List<string> tags2 = filter.Tags;
              // ISSUE: explicit non-virtual call
              if ((tags2 != null ? (__nonvirtual (tags2.Count) > 0 ? 1 : 0) : 0) != 0)
              {
                bool flag = false;
                if (filter.Tags.Contains("!tag") && (string.IsNullOrEmpty(task.Tag) || task.Tag == "[]"))
                  flag = true;
                if (!flag && filter.Tags.Any<string>((Func<string, bool>) (t => task.Tags != null && ((IEnumerable<string>) task.Tags).Any<string>((Func<string, bool>) (tt => string.Equals(tt, t, StringComparison.CurrentCultureIgnoreCase))))))
                  flag = true;
                if (!flag)
                  continue;
              }
              if (filter.Start.HasValue && filter.End.HasValue)
              {
                DateTime? nullable1 = task.StartDate;
                if (nullable1.HasValue)
                {
                  nullable1 = task.StartDate;
                  DateTime? nullable2 = filter.End;
                  if ((nullable1.HasValue & nullable2.HasValue ? (nullable1.GetValueOrDefault() >= nullable2.GetValueOrDefault() ? 1 : 0) : 0) == 0)
                  {
                    nullable2 = task.DueDate;
                    if (nullable2.HasValue)
                    {
                      nullable2 = task.DueDate;
                      nullable1 = filter.Start;
                      if ((nullable2.HasValue & nullable1.HasValue ? (nullable2.GetValueOrDefault() <= nullable1.GetValueOrDefault() ? 1 : 0) : 0) != 0)
                        continue;
                    }
                    nullable1 = task.DueDate;
                    if (!nullable1.HasValue)
                    {
                      nullable1 = task.StartDate;
                      nullable2 = filter.Start;
                      if ((nullable1.HasValue & nullable2.HasValue ? (nullable1.GetValueOrDefault() < nullable2.GetValueOrDefault() ? 1 : 0) : 0) != 0)
                        continue;
                    }
                  }
                  else
                    continue;
                }
                else
                  continue;
              }
              if (!string.IsNullOrEmpty(task.Id) && dict.ContainsKey(task.Id))
              {
                Dictionary<string, string> dictionary = dict;
                string id = task.Id;
                dictionary[id] = dictionary[id] + "\r\n" + task.Title?.ToLower() + "\r\n" + task.Content?.ToLower() + "\r\n" + task.Desc?.ToLower();
              }
              else
                dict[task.Id] = "\r\n" + task.Title?.ToLower() + "\r\n" + task.Content?.ToLower() + "\r\n" + task.Desc?.ToLower();
            }
          }
        }
      }
      foreach (TaskBaseViewModel allCheckItem in TaskDetailItemCache.GetAllCheckItems())
      {
        if (!string.IsNullOrEmpty(allCheckItem.ParentId) && dict.ContainsKey(allCheckItem.ParentId))
        {
          TaskBaseViewModel localTaskViewModel = TaskCache.LocalTaskViewModels[allCheckItem.ParentId];
          if (localTaskViewModel != null && localTaskViewModel.Kind == "CHECKLIST")
          {
            Dictionary<string, string> dictionary = dict;
            string parentId = allCheckItem.ParentId;
            dictionary[parentId] = dictionary[parentId] + "\r\n" + allCheckItem.Title?.ToLower();
          }
        }
      }
      ConcurrentDictionary<string, List<CommentModel>> taskCommentDict = await TaskCommentCache.GetTaskCommentDict();
      foreach (string key1 in dict.Keys.ToList<string>())
      {
        if (taskCommentDict.ContainsKey(key1))
        {
          foreach (CommentModel commentModel in taskCommentDict[key1])
          {
            Dictionary<string, string> dictionary = dict;
            string key2 = key1;
            dictionary[key2] = dictionary[key2] + "\r\n" + commentModel.title?.ToLower();
          }
        }
      }
      List<SearchContent> searchContentList = new List<SearchContent>();
      if (dict.Any<KeyValuePair<string, string>>())
      {
        foreach (KeyValuePair<string, string> keyValuePair in dict)
          searchContentList.Add(new SearchContent()
          {
            TaskId = keyValuePair.Key,
            Content = keyValuePair.Value
          });
      }
      List<SearchContent> taskSearchContents = searchContentList;
      dict = (Dictionary<string, string>) null;
      return taskSearchContents;
    }

    public static async Task<List<TaskSearchModel>> GetTaskSearchModels(
      SearchFilterModel filter,
      List<string> tags)
    {
      Dictionary<string, TaskSearchModel> dict = new Dictionary<string, TaskSearchModel>();
      Dictionary<string, string> dictionaryEx = AttachmentCache.GetAllAttachments().Where<AttachmentModel>((Func<AttachmentModel, bool>) (a => a.IsActive())).GroupBy<AttachmentModel, string>((Func<AttachmentModel, string>) (model => model.taskId)).ToDictionaryEx<IGrouping<string, AttachmentModel>, string, string>((Func<IGrouping<string, AttachmentModel>, string>) (group => group.Key), (Func<IGrouping<string, AttachmentModel>, string>) (group => string.Join(", ", group.Select<AttachmentModel, string>((Func<AttachmentModel, string>) (model => model.fileName)))));
      foreach (TaskBaseViewModel taskBaseViewModel in TaskCache.GetAllTask().Where<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (t => t != null && t.Deleted == 0)))
      {
        TaskBaseViewModel task = taskBaseViewModel;
        if ((!filter.Status.HasValue || filter.Status.Value == task.Status) && CacheManager.CheckProjectValid(task.ProjectId))
        {
          List<string> projectIds = filter.ProjectIds;
          // ISSUE: explicit non-virtual call
          if ((projectIds != null ? (__nonvirtual (projectIds.Count) > 0 ? 1 : 0) : 0) == 0 || filter.ProjectIds.Contains(task.ProjectId))
          {
            List<int> priorities = filter.Priorities;
            // ISSUE: explicit non-virtual call
            if (((priorities != null ? (__nonvirtual (priorities.Count) > 0 ? 1 : 0) : 0) == 0 || filter.Priorities.Contains(task.Priority)) && (filter.TaskType != TaskType.Note || !(task.Kind != "NOTE")) && (filter.TaskType != TaskType.Task || !(task.Kind == "NOTE")))
            {
              List<string> assignees = filter.Assignees;
              // ISSUE: explicit non-virtual call
              if ((assignees != null ? (__nonvirtual (assignees.Count) > 0 ? 1 : 0) : 0) != 0)
              {
                ProjectModel projectById = CacheManager.GetProjectById(task.ProjectId);
                if (projectById == null || !TaskViewModelHelper.CheckAssignTo(task, projectById, filter.Assignees))
                  continue;
              }
              List<string> stringList = tags;
              // ISSUE: explicit non-virtual call
              if ((stringList != null ? (__nonvirtual (stringList.Count) > 0 ? 1 : 0) : 0) != 0)
              {
                tags = tags.Select<string, string>((Func<string, string>) (t => t.ToLower())).ToList<string>();
                string[] tags1 = task.Tags;
                if ((tags1 != null ? tags1.Length : 0) < tags.Count || ((IEnumerable<string>) task.Tags).Intersect<string>((IEnumerable<string>) tags).Count<string>() < tags.Count)
                  continue;
              }
              List<string> tags2 = filter.Tags;
              // ISSUE: explicit non-virtual call
              if ((tags2 != null ? (__nonvirtual (tags2.Count) > 0 ? 1 : 0) : 0) != 0)
              {
                bool flag = false;
                if (filter.Tags.Contains("!tag") && (string.IsNullOrEmpty(task.Tag) || task.Tag == "[]"))
                  flag = true;
                if (!flag && filter.Tags.Any<string>((Func<string, bool>) (t => task.Tags != null && ((IEnumerable<string>) task.Tags).Any<string>((Func<string, bool>) (tt => string.Equals(tt, t, StringComparison.CurrentCultureIgnoreCase))))))
                  flag = true;
                if (!flag)
                  continue;
              }
              if (filter.Start.HasValue && filter.End.HasValue)
              {
                DateTime? nullable1 = task.StartDate;
                if (nullable1.HasValue)
                {
                  nullable1 = task.StartDate;
                  DateTime? nullable2 = filter.End;
                  if ((nullable1.HasValue & nullable2.HasValue ? (nullable1.GetValueOrDefault() >= nullable2.GetValueOrDefault() ? 1 : 0) : 0) == 0)
                  {
                    nullable2 = task.DueDate;
                    if (nullable2.HasValue)
                    {
                      nullable2 = task.DueDate;
                      nullable1 = filter.Start;
                      if ((nullable2.HasValue & nullable1.HasValue ? (nullable2.GetValueOrDefault() <= nullable1.GetValueOrDefault() ? 1 : 0) : 0) != 0)
                        continue;
                    }
                    nullable1 = task.DueDate;
                    if (!nullable1.HasValue)
                    {
                      nullable1 = task.StartDate;
                      nullable2 = filter.Start;
                      if ((nullable1.HasValue & nullable2.HasValue ? (nullable1.GetValueOrDefault() < nullable2.GetValueOrDefault() ? 1 : 0) : 0) != 0)
                        continue;
                    }
                  }
                  else
                    continue;
                }
                else
                  continue;
              }
              string empty = string.Empty;
              dictionaryEx.TryGetValue(task.Id, out empty);
              if (!string.IsNullOrEmpty(task.Id) && dict.ContainsKey(task.Id))
              {
                TaskSearchModel taskSearchModel = dict[task.Id];
                taskSearchModel.SourceModel = task;
                taskSearchModel.Content = task.Content;
                taskSearchModel.Attachment = empty;
              }
              else
              {
                TaskSearchModel taskSearchModel = new TaskSearchModel()
                {
                  Content = task.Content,
                  Attachment = empty,
                  SourceModel = task
                };
                dict[task.Id] = taskSearchModel;
              }
            }
          }
        }
      }
      foreach (TaskBaseViewModel allCheckItem in TaskDetailItemCache.GetAllCheckItems())
      {
        if (!string.IsNullOrEmpty(allCheckItem.ParentId) && dict.ContainsKey(allCheckItem.ParentId))
        {
          TaskBaseViewModel localTaskViewModel = TaskCache.LocalTaskViewModels[allCheckItem.ParentId];
          if (localTaskViewModel != null && localTaskViewModel.Kind == "CHECKLIST")
          {
            TaskSearchModel taskSearchModel1 = dict[allCheckItem.ParentId];
            TaskSearchModel taskSearchModel2 = taskSearchModel1;
            taskSearchModel2.CheckItems = taskSearchModel2.CheckItems + (!string.IsNullOrEmpty(taskSearchModel1.CheckItems) ? "\r\n" : "") + allCheckItem.Title?.ToLower();
          }
        }
      }
      ConcurrentDictionary<string, List<CommentModel>> taskCommentDict = await TaskCommentCache.GetTaskCommentDict();
      foreach (string key in dict.Keys.ToList<string>())
      {
        TaskSearchModel taskSearchModel3 = dict[key];
        if (taskCommentDict.ContainsKey(key))
        {
          foreach (CommentModel commentModel in taskCommentDict[key])
          {
            TaskSearchModel taskSearchModel4 = taskSearchModel3;
            taskSearchModel4.Comment = taskSearchModel4.Comment + "\r\n" + commentModel.title?.ToLower();
          }
        }
      }
      List<TaskSearchModel> result = dict.Values.ToList<TaskSearchModel>();
      if ((tags == null || tags.Count == 0) && filter.SearchEvent())
      {
        List<CalendarEventModel> events = await CalendarEventDao.GetAllShowEvents();
        List<CalendarEventModel> skipEvents = events.Where<CalendarEventModel>((Func<CalendarEventModel, bool>) (e => e.OriginalStartTime.HasValue)).ToList<CalendarEventModel>();
        List<string> archivedKeys = await ArchivedDao.GetArchivedKeys();
        foreach (CalendarEventModel model in events)
        {
          TaskSearchModel taskSearchModel = new TaskSearchModel()
          {
            Content = model.Content
          };
          DateTime? nullable3;
          DateTime? nullable4;
          DateTime today;
          if (!string.IsNullOrEmpty(model.RepeatFlag))
          {
            DateTime? eventNextRepeat = CalendarEventDao.GetEventNextRepeat(model, skipEvents, archivedKeys);
            if (eventNextRepeat.HasValue)
            {
              nullable3 = model.DueEnd;
              nullable4 = model.DueStart;
              TimeSpan? nullable5 = nullable3.HasValue & nullable4.HasValue ? new TimeSpan?(nullable3.GetValueOrDefault() - nullable4.GetValueOrDefault()) : new TimeSpan?();
              ref TimeSpan? local = ref nullable5;
              double? nullable6 = local.HasValue ? new double?(local.GetValueOrDefault().TotalMinutes) : new double?();
              model.DueStart = eventNextRepeat;
              if (nullable6.HasValue)
              {
                CalendarEventModel calendarEventModel = model;
                today = eventNextRepeat.Value;
                DateTime? nullable7 = new DateTime?(today.AddMinutes(nullable6.Value));
                calendarEventModel.DueEnd = nullable7;
              }
            }
          }
          else
          {
            string eventKey = ArchivedEventDao.GenerateEventKey(new EventArchiveArgs(model));
            if (archivedKeys.Contains(eventKey))
              continue;
          }
          nullable3 = model.DueEnd;
          if (nullable3.HasValue)
          {
            nullable3 = model.DueEnd;
            today = DateTime.Today;
            if ((nullable3.HasValue ? (nullable3.GetValueOrDefault() <= today ? 1 : 0) : 0) != 0)
              continue;
          }
          nullable3 = model.DueEnd;
          if (!nullable3.HasValue)
          {
            nullable3 = model.DueStart;
            today = DateTime.Today;
            if ((nullable3.HasValue ? (nullable3.GetValueOrDefault() < today ? 1 : 0) : 0) != 0)
              continue;
          }
          if (filter.Start.HasValue && filter.End.HasValue)
          {
            nullable3 = model.DueStart;
            if (nullable3.HasValue)
            {
              nullable3 = model.DueStart;
              nullable4 = filter.End;
              if ((nullable3.HasValue & nullable4.HasValue ? (nullable3.GetValueOrDefault() >= nullable4.GetValueOrDefault() ? 1 : 0) : 0) == 0)
              {
                nullable4 = model.DueEnd;
                if (nullable4.HasValue)
                {
                  nullable4 = model.DueEnd;
                  nullable3 = filter.Start;
                  if ((nullable4.HasValue & nullable3.HasValue ? (nullable4.GetValueOrDefault() <= nullable3.GetValueOrDefault() ? 1 : 0) : 0) != 0)
                    continue;
                }
                nullable3 = model.DueEnd;
                if (!nullable3.HasValue)
                {
                  nullable3 = model.DueStart;
                  nullable4 = filter.Start;
                  if ((nullable3.HasValue & nullable4.HasValue ? (nullable3.GetValueOrDefault() < nullable4.GetValueOrDefault() ? 1 : 0) : 0) != 0)
                    continue;
                }
              }
              else
                continue;
            }
            else
              continue;
          }
          taskSearchModel.SourceModel = new TaskBaseViewModel(model);
          result.Add(taskSearchModel);
        }
        events = (List<CalendarEventModel>) null;
        skipEvents = (List<CalendarEventModel>) null;
      }
      List<TaskSearchModel> taskSearchModels = result;
      dict = (Dictionary<string, TaskSearchModel>) null;
      result = (List<TaskSearchModel>) null;
      return taskSearchModels;
    }

    public static List<TaskBaseViewModel> GetMatchedItems(
      ProjectIdentity projectIdentity,
      List<string> ids)
    {
      List<TaskBaseViewModel> checkItemByIds = TaskDetailItemCache.GetCheckItemByIds(ids);
      List<TaskBaseViewModel> matchedItems = (List<TaskBaseViewModel>) null;
      switch (projectIdentity)
      {
        case TodayProjectIdentity _:
          matchedItems = TaskViewModelHelper.GetItems(checkItemByIds, startEndPairs: new List<FilterDatePair>()
          {
            new FilterDatePair(new DateTime?(DateTime.Today), new DateTime?(DateTime.Today.AddDays(1.0)))
          }, inTodayOrWeek: true).ToList<TaskBaseViewModel>();
          break;
        case TomorrowProjectIdentity _:
          matchedItems = TaskViewModelHelper.GetItems(checkItemByIds, startEndPairs: new List<FilterDatePair>()
          {
            new FilterDatePair(new DateTime?(DateTime.Today.AddDays(1.0)), new DateTime?(DateTime.Today.AddDays(2.0)))
          }).ToList<TaskBaseViewModel>();
          break;
        case WeekProjectIdentity _:
          matchedItems = TaskViewModelHelper.GetItems(checkItemByIds, startEndPairs: new List<FilterDatePair>()
          {
            new FilterDatePair(new DateTime?(DateTime.Today), new DateTime?(DateTime.Today.AddDays(7.0)))
          }, inTodayOrWeek: true).ToList<TaskBaseViewModel>();
          break;
        case FilterProjectIdentity filterProjectIdentity:
          matchedItems = TaskService.GetCheckItemsMatchedFilter(filterProjectIdentity.Filter, checkItemByIds);
          break;
        case MatrixQuadrantIdentity quadrantIdentity:
          matchedItems = TaskService.GetItemMatchedNormalFilter(Parser.ToNormalModel(quadrantIdentity.Quadrant.rule), checkItemByIds);
          break;
      }
      return matchedItems;
    }

    public static List<TaskBaseViewModel> GetMatchedTasks(
      ProjectIdentity projectIdentity,
      List<string> ids)
    {
      List<TaskBaseViewModel> tasksByIds = TaskCache.GetTasksByIds(ids);
      return TaskViewModelHelper.GetMatchedTasks(projectIdentity, tasksByIds);
    }

    public static List<TaskBaseViewModel> GetMatchedItemsInCal(
      DateTime startDate,
      DateTime endDate,
      List<string> ids)
    {
      if (!LocalSettings.Settings.ShowCheckListInCal)
        return new List<TaskBaseViewModel>();
      bool showCompletedInCal = LocalSettings.Settings.ShowCompletedInCal;
      List<TaskBaseViewModel> checkItemByIds = TaskDetailItemCache.GetCheckItemByIds(ids);
      ProjectExtra projectFilter = ProjectExtra.Deserialize(LocalSettings.Settings.CalendarFilterData);
      if (projectFilter.FilterIds.Any<string>())
      {
        FilterModel filter = CacheManager.GetFilters().FirstOrDefault<FilterModel>((Func<FilterModel, bool>) (f => f.id == projectFilter.FilterIds[0]));
        if (filter == null)
          return new List<TaskBaseViewModel>();
        List<TaskBaseViewModel> itemsMatchedFilter = TaskService.GetCheckItemsMatchedFilter(filter, checkItemByIds);
        return itemsMatchedFilter == null ? (List<TaskBaseViewModel>) null : itemsMatchedFilter.Where<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (t =>
        {
          if (t.StartDate.HasValue)
          {
            DateTime? startDate1 = t.StartDate;
            DateTime dateTime1 = startDate;
            if ((startDate1.HasValue ? (startDate1.GetValueOrDefault() >= dateTime1 ? 1 : 0) : 0) != 0)
            {
              DateTime? startDate2 = t.StartDate;
              DateTime dateTime2 = endDate;
              if ((startDate2.HasValue ? (startDate2.GetValueOrDefault() < dateTime2 ? 1 : 0) : 0) != 0)
                goto label_15;
            }
          }
          if (t.DueDate.HasValue)
          {
            DateTime? dueDate1 = t.DueDate;
            DateTime dateTime3 = startDate;
            if ((dueDate1.HasValue ? (dueDate1.GetValueOrDefault() > dateTime3 ? 1 : 0) : 0) != 0)
            {
              DateTime? dueDate2 = t.DueDate;
              DateTime dateTime4 = endDate;
              if ((dueDate2.HasValue ? (dueDate2.GetValueOrDefault() < dateTime4 ? 1 : 0) : 0) != 0)
                goto label_15;
            }
          }
          if (t.StartDate.HasValue && t.DueDate.HasValue)
          {
            DateTime? startDate3 = t.StartDate;
            DateTime dateTime5 = startDate;
            if ((startDate3.HasValue ? (startDate3.GetValueOrDefault() < dateTime5 ? 1 : 0) : 0) != 0)
            {
              DateTime? dueDate = t.DueDate;
              DateTime dateTime6 = endDate;
              if ((dueDate.HasValue ? (dueDate.GetValueOrDefault() >= dateTime6 ? 1 : 0) : 0) != 0)
                goto label_15;
            }
          }
          if (!t.StartDate.HasValue && !t.DueDate.HasValue && t.CompletedTime.HasValue && t.Status != 0)
          {
            DateTime? completedTime1 = t.CompletedTime;
            DateTime dateTime7 = startDate;
            if ((completedTime1.HasValue ? (completedTime1.GetValueOrDefault() >= dateTime7 ? 1 : 0) : 0) != 0)
            {
              DateTime? completedTime2 = t.CompletedTime;
              DateTime dateTime8 = endDate;
              return completedTime2.HasValue && completedTime2.GetValueOrDefault() < dateTime8;
            }
          }
          return false;
label_15:
          return true;
        })).ToList<TaskBaseViewModel>();
      }
      if ((projectFilter.SubscribeCalendars.Any<string>() || projectFilter.BindAccounts.Any<string>()) && projectFilter.GroupIds.Count == 0 && projectFilter.ProjectIds.Count == 0)
        projectFilter.ProjectIds.Add("none");
      List<FilterDatePair> startEndPairs = new List<FilterDatePair>()
      {
        new FilterDatePair(new DateTime?(startDate), new DateTime?(endDate))
      };
      return TaskViewModelHelper.GetItems(checkItemByIds, projectFilter.GroupIds, projectFilter.ProjectIds, projectFilter.Tags, startEndPairs, showCompletedInCal, checkInAll: projectFilter.GroupIds.Count == 0 && projectFilter.ProjectIds.Count == 0 && projectFilter.Tags.Count == 0, orTag: true).ToList<TaskBaseViewModel>();
    }

    public static async Task<List<TaskBaseViewModel>> GetMatchedTasksInCal(
      DateTime startDate,
      DateTime endDate,
      List<string> ids)
    {
      bool showCompletedInCal = LocalSettings.Settings.ShowCompletedInCal;
      bool showCheckListInCal = LocalSettings.Settings.ShowCheckListInCal;
      List<TaskBaseViewModel> tasksByIds = TaskCache.GetTasksByIds(ids);
      List<TaskBaseViewModel> repeats = new List<TaskBaseViewModel>();
      foreach (TaskBaseViewModel taskBaseViewModel1 in tasksByIds)
      {
        TaskBaseViewModel task = taskBaseViewModel1;
        if (task.StartDate.HasValue && !string.IsNullOrEmpty(task.RepeatFlag))
        {
          double diffMinutes = 0.0;
          if (task.DueDate.HasValue)
            diffMinutes = (task.DueDate.Value - task.StartDate.Value).TotalMinutes;
          bool? isAllDay = task.IsAllDay;
          if (isAllDay.HasValue)
          {
            isAllDay = task.IsAllDay;
            if (isAllDay.Value)
              goto label_8;
          }
          string timeZoneName;
          if (!task.IsFloating && task.TimeZoneName != TimeZoneData.LocalTimeZoneModel?.TimeZoneName)
          {
            timeZoneName = task.TimeZoneName;
            goto label_10;
          }
label_8:
          timeZoneName = TimeZoneData.LocalTimeZoneModel?.TimeZoneName;
label_10:
          string timeZone = timeZoneName;
          RepeatDao.GetRepeatFlagDate(task, startDate.AddDays((double) (-2 - (int) diffMinutes / 1440)), endDate.AddDays(1.0)).ForEach((Action<DateTime>) (date =>
          {
            DateTime dateTime1 = date;
            DateTime? nullable1 = new DateTime?();
            if (task.DueDate.HasValue)
              nullable1 = new DateTime?(TimeZoneUtils.ToLocalTime(TimeZoneUtils.LocalToTargetTzTime(date, timeZone).AddMinutes(diffMinutes), timeZone));
            if (!(dateTime1 >= startDate) || !(dateTime1 < endDate))
            {
              DateTime? nullable2 = nullable1;
              DateTime dateTime2 = startDate;
              if ((nullable2.HasValue ? (nullable2.GetValueOrDefault() > dateTime2 ? 1 : 0) : 0) != 0)
              {
                DateTime? nullable3 = nullable1;
                DateTime dateTime3 = endDate;
                if ((nullable3.HasValue ? (nullable3.GetValueOrDefault() <= dateTime3 ? 1 : 0) : 0) != 0)
                  goto label_6;
              }
              DateTime? nullable4 = nullable1;
              DateTime dateTime4 = endDate;
              if ((nullable4.HasValue ? (nullable4.GetValueOrDefault() > dateTime4 ? 1 : 0) : 0) == 0 || !(dateTime1 < startDate))
                return;
            }
label_6:
            double totalDays = (date - (task.StartDate ?? date)).TotalDays;
            if (totalDays == 0.0)
              return;
            TaskBaseViewModel taskBaseViewModel2 = task.Copy();
            TaskBaseViewModel taskBaseViewModel3 = taskBaseViewModel2;
            DateTime? startDate1 = taskBaseViewModel2.StartDate;
            ref DateTime? local1 = ref startDate1;
            DateTime? nullable5 = local1.HasValue ? new DateTime?(local1.GetValueOrDefault().AddDays(totalDays)) : new DateTime?();
            taskBaseViewModel3.StartDate = nullable5;
            TaskBaseViewModel taskBaseViewModel4 = taskBaseViewModel2;
            DateTime? dueDate = taskBaseViewModel2.DueDate;
            ref DateTime? local2 = ref dueDate;
            DateTime? nullable6 = local2.HasValue ? new DateTime?(local2.GetValueOrDefault().AddDays(totalDays)) : new DateTime?();
            taskBaseViewModel4.DueDate = nullable6;
            repeats.Add(taskBaseViewModel2);
          }));
        }
      }
      tasksByIds.AddRange((IEnumerable<TaskBaseViewModel>) repeats);
      List<TaskBaseViewModel> checkItemsInTaskIds = TaskDetailItemCache.GetCheckItemsInTaskIds(tasksByIds.Select<TaskBaseViewModel, string>((Func<TaskBaseViewModel, string>) (t => t.Id)).ToList<string>());
      ProjectExtra projectFilter = ProjectExtra.Deserialize(LocalSettings.Settings.CalendarFilterData);
      if (projectFilter.FilterIds.Any<string>())
      {
        FilterModel filter = CacheManager.GetFilters().FirstOrDefault<FilterModel>((Func<FilterModel, bool>) (f => f.id == projectFilter.FilterIds[0]));
        if (filter == null)
          return new List<TaskBaseViewModel>();
        List<TaskBaseViewModel> source = TaskService.GetTasksMatchedFilter(filter, tasksByIds) ?? new List<TaskBaseViewModel>();
        source.AddRange((IEnumerable<TaskBaseViewModel>) TaskService.GetCheckItemsMatchedFilter(filter, checkItemsInTaskIds));
        return source.Where<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (t =>
        {
          if (t.StartDate.HasValue)
          {
            DateTime? startDate2 = t.StartDate;
            DateTime dateTime5 = startDate;
            if ((startDate2.HasValue ? (startDate2.GetValueOrDefault() >= dateTime5 ? 1 : 0) : 0) != 0)
            {
              DateTime? startDate3 = t.StartDate;
              DateTime dateTime6 = endDate;
              if ((startDate3.HasValue ? (startDate3.GetValueOrDefault() < dateTime6 ? 1 : 0) : 0) != 0)
                goto label_15;
            }
          }
          if (t.DueDate.HasValue)
          {
            DateTime? dueDate1 = t.DueDate;
            DateTime dateTime7 = startDate;
            if ((dueDate1.HasValue ? (dueDate1.GetValueOrDefault() > dateTime7 ? 1 : 0) : 0) != 0)
            {
              DateTime? dueDate2 = t.DueDate;
              DateTime dateTime8 = endDate;
              if ((dueDate2.HasValue ? (dueDate2.GetValueOrDefault() < dateTime8 ? 1 : 0) : 0) != 0)
                goto label_15;
            }
          }
          if (t.StartDate.HasValue && t.DueDate.HasValue)
          {
            DateTime? startDate4 = t.StartDate;
            DateTime dateTime9 = startDate;
            if ((startDate4.HasValue ? (startDate4.GetValueOrDefault() < dateTime9 ? 1 : 0) : 0) != 0)
            {
              DateTime? dueDate = t.DueDate;
              DateTime dateTime10 = endDate;
              if ((dueDate.HasValue ? (dueDate.GetValueOrDefault() >= dateTime10 ? 1 : 0) : 0) != 0)
                goto label_15;
            }
          }
          if (!t.StartDate.HasValue && !t.DueDate.HasValue && t.CompletedTime.HasValue && t.Status != 0)
          {
            DateTime? completedTime1 = t.CompletedTime;
            DateTime dateTime11 = startDate;
            if ((completedTime1.HasValue ? (completedTime1.GetValueOrDefault() >= dateTime11 ? 1 : 0) : 0) != 0)
            {
              DateTime? completedTime2 = t.CompletedTime;
              DateTime dateTime12 = endDate;
              return completedTime2.HasValue && completedTime2.GetValueOrDefault() < dateTime12;
            }
          }
          return false;
label_15:
          return true;
        })).ToList<TaskBaseViewModel>();
      }
      if ((projectFilter.SubscribeCalendars.Any<string>() || projectFilter.BindAccounts.Any<string>()) && projectFilter.GroupIds.Count == 0 && projectFilter.ProjectIds.Count == 0)
        projectFilter.ProjectIds.Add("none");
      List<FilterDatePair> startEndPairs = new List<FilterDatePair>()
      {
        new FilterDatePair(new DateTime?(startDate), new DateTime?(endDate))
      };
      IEnumerable<TaskBaseViewModel> tasks = TaskViewModelHelper.GetTasks(tasksByIds, projectFilter.GroupIds, projectFilter.ProjectIds, projectFilter.Tags, startEndPairs, showCompletedInCal, showCheckListInCal, projectFilter.GroupIds.Count == 0 && projectFilter.ProjectIds.Count == 0 && projectFilter.Tags.Count == 0, inCal: true, orTag: true);
      List<TaskBaseViewModel> matchedTasksInCal = (tasks != null ? tasks.ToList<TaskBaseViewModel>() : (List<TaskBaseViewModel>) null) ?? new List<TaskBaseViewModel>();
      if (showCheckListInCal)
      {
        IEnumerable<TaskBaseViewModel> items = TaskViewModelHelper.GetItems(checkItemsInTaskIds, projectFilter.GroupIds, projectFilter.ProjectIds, projectFilter.Tags, startEndPairs, showCompletedInCal, checkInAll: projectFilter.GroupIds.Count == 0 && projectFilter.ProjectIds.Count == 0 && projectFilter.Tags.Count == 0, orTag: true);
        matchedTasksInCal.AddRange(items);
      }
      return matchedTasksInCal;
    }

    public static List<TaskBaseViewModel> GetMatchedTasks(
      ProjectIdentity projectIdentity,
      List<TaskBaseViewModel> tasks,
      bool checkCheckItem = false)
    {
      if (tasks == null || tasks.Count == 0)
        return (List<TaskBaseViewModel>) null;
      List<TaskBaseViewModel> matchedTasks = (List<TaskBaseViewModel>) null;
      List<TaskBaseViewModel> collection = (List<TaskBaseViewModel>) null;
      switch (projectIdentity)
      {
        case AllProjectIdentity _:
          matchedTasks = tasks.Where<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (t =>
          {
            if (t == null || t.Deleted != 0)
              return false;
            ProjectModel projectById = CacheManager.GetProjectById(t.ProjectId);
            return projectById != null && projectById.IsValid() && (projectById.inAll || t.IsAssignToMe());
          })).ToList<TaskBaseViewModel>();
          break;
        case TodayProjectIdentity _:
          List<TaskBaseViewModel> allTasks1 = tasks;
          List<FilterDatePair> startEndPairs1 = new List<FilterDatePair>();
          DateTime? start1 = new DateTime?(DateTime.Today);
          DateTime today1 = DateTime.Today;
          DateTime? end1 = new DateTime?(today1.AddDays(1.0));
          startEndPairs1.Add(new FilterDatePair(start1, end1));
          matchedTasks = TaskViewModelHelper.GetTasks(allTasks1, startEndPairs: startEndPairs1, inTodayOrWeek: true).ToList<TaskBaseViewModel>();
          if (checkCheckItem)
          {
            List<TaskBaseViewModel> checkItemsInTaskIds = TaskDetailItemCache.GetCheckItemsInTaskIds(tasks.Select<TaskBaseViewModel, string>((Func<TaskBaseViewModel, string>) (t => t.Id)).ToList<string>());
            List<FilterDatePair> startEndPairs2 = new List<FilterDatePair>();
            DateTime? start2 = new DateTime?(DateTime.Today);
            today1 = DateTime.Today;
            DateTime? end2 = new DateTime?(today1.AddDays(1.0));
            startEndPairs2.Add(new FilterDatePair(start2, end2));
            collection = TaskViewModelHelper.GetItems(checkItemsInTaskIds, startEndPairs: startEndPairs2, inTodayOrWeek: true).ToList<TaskBaseViewModel>();
            break;
          }
          break;
        case TomorrowProjectIdentity _:
          List<TaskBaseViewModel> allTasks2 = tasks;
          List<FilterDatePair> startEndPairs3 = new List<FilterDatePair>();
          DateTime today2 = DateTime.Today;
          DateTime? start3 = new DateTime?(today2.AddDays(1.0));
          today2 = DateTime.Today;
          DateTime? end3 = new DateTime?(today2.AddDays(2.0));
          startEndPairs3.Add(new FilterDatePair(start3, end3));
          matchedTasks = TaskViewModelHelper.GetTasks(allTasks2, startEndPairs: startEndPairs3).ToList<TaskBaseViewModel>();
          if (checkCheckItem)
          {
            List<TaskBaseViewModel> checkItemsInTaskIds = TaskDetailItemCache.GetCheckItemsInTaskIds(tasks.Select<TaskBaseViewModel, string>((Func<TaskBaseViewModel, string>) (t => t.Id)).ToList<string>());
            List<FilterDatePair> startEndPairs4 = new List<FilterDatePair>();
            today2 = DateTime.Today;
            DateTime? start4 = new DateTime?(today2.AddDays(1.0));
            today2 = DateTime.Today;
            DateTime? end4 = new DateTime?(today2.AddDays(2.0));
            startEndPairs4.Add(new FilterDatePair(start4, end4));
            collection = TaskViewModelHelper.GetItems(checkItemsInTaskIds, startEndPairs: startEndPairs4).ToList<TaskBaseViewModel>();
            break;
          }
          break;
        case WeekProjectIdentity _:
          List<TaskBaseViewModel> allTasks3 = tasks;
          List<FilterDatePair> startEndPairs5 = new List<FilterDatePair>();
          DateTime? start5 = new DateTime?(DateTime.Today);
          DateTime today3 = DateTime.Today;
          DateTime? end5 = new DateTime?(today3.AddDays(7.0));
          startEndPairs5.Add(new FilterDatePair(start5, end5));
          matchedTasks = TaskViewModelHelper.GetTasks(allTasks3, startEndPairs: startEndPairs5, inTodayOrWeek: true).ToList<TaskBaseViewModel>();
          if (checkCheckItem)
          {
            List<TaskBaseViewModel> checkItemsInTaskIds = TaskDetailItemCache.GetCheckItemsInTaskIds(tasks.Select<TaskBaseViewModel, string>((Func<TaskBaseViewModel, string>) (t => t.Id)).ToList<string>());
            List<FilterDatePair> startEndPairs6 = new List<FilterDatePair>();
            DateTime? start6 = new DateTime?(DateTime.Today);
            today3 = DateTime.Today;
            DateTime? end6 = new DateTime?(today3.AddDays(7.0));
            startEndPairs6.Add(new FilterDatePair(start6, end6));
            collection = TaskViewModelHelper.GetItems(checkItemsInTaskIds, startEndPairs: startEndPairs6, inTodayOrWeek: true).ToList<TaskBaseViewModel>();
            break;
          }
          break;
        case GroupProjectIdentity groupProjectIdentity:
          matchedTasks = TaskViewModelHelper.GetTasks(tasks, new List<string>()
          {
            groupProjectIdentity.GroupId
          }).ToList<TaskBaseViewModel>();
          break;
        case AssignToMeProjectIdentity _:
          matchedTasks = TaskViewModelHelper.GetTasks(tasks, assignTo: new List<string>()
          {
            "me"
          }).ToList<TaskBaseViewModel>();
          break;
        default:
          TagProjectIdentity tagProjectIdentity = projectIdentity as TagProjectIdentity;
          if (tagProjectIdentity == null)
          {
            switch (projectIdentity)
            {
              case FilterProjectIdentity filterProjectIdentity:
                matchedTasks = TaskService.GetTasksMatchedFilter(filterProjectIdentity.Filter, tasks);
                if (checkCheckItem)
                {
                  List<TaskBaseViewModel> checkItemsInTaskIds = TaskDetailItemCache.GetCheckItemsInTaskIds(tasks.Select<TaskBaseViewModel, string>((Func<TaskBaseViewModel, string>) (t => t.Id)).ToList<string>());
                  collection = TaskService.GetCheckItemsMatchedFilter(filterProjectIdentity.Filter, checkItemsInTaskIds);
                  break;
                }
                break;
              case MatrixQuadrantIdentity quadrantIdentity:
                matchedTasks = TaskService.GetTaskMatchedNormalFilter(Parser.ToNormalModel(quadrantIdentity.Quadrant.rule), tasks);
                if (checkCheckItem)
                {
                  List<TaskBaseViewModel> checkItemsInTaskIds = TaskDetailItemCache.GetCheckItemsInTaskIds(tasks.Select<TaskBaseViewModel, string>((Func<TaskBaseViewModel, string>) (t => t.Id)).ToList<string>());
                  collection = TaskService.GetItemMatchedNormalFilter(Parser.ToNormalModel(quadrantIdentity.Quadrant.rule), checkItemsInTaskIds);
                  break;
                }
                break;
              default:
                ColumnProjectIdentity columnProjectIdentity = projectIdentity as ColumnProjectIdentity;
                if (columnProjectIdentity == null)
                {
                  NormalProjectIdentity normalProjectIdentity = projectIdentity as NormalProjectIdentity;
                  if (normalProjectIdentity != null)
                  {
                    matchedTasks = tasks.Where<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (p => p.ProjectId == normalProjectIdentity.Id && p.Deleted == 0)).ToList<TaskBaseViewModel>();
                    break;
                  }
                  break;
                }
                matchedTasks = tasks.Where<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (p => p.ProjectId == columnProjectIdentity.GetProjectId() && p.ColumnId == columnProjectIdentity.Id && p.Deleted == 0)).ToList<TaskBaseViewModel>();
                break;
            }
          }
          else
          {
            List<string> list = CacheManager.GetTags().Where<TagModel>((Func<TagModel, bool>) (t => t.name == tagProjectIdentity.Tag || t.parent == tagProjectIdentity.Tag)).Select<TagModel, string>((Func<TagModel, string>) (t => t.name)).ToList<string>();
            matchedTasks = TaskViewModelHelper.GetTasks(tasks, tags: list, showComplete: !LocalSettings.Settings.HideComplete).ToList<TaskBaseViewModel>();
            break;
          }
          break;
      }
      if (matchedTasks == null)
        return collection;
      if (collection != null)
        matchedTasks.AddRange((IEnumerable<TaskBaseViewModel>) collection);
      return matchedTasks;
    }

    public static void OnProjectChanged(ProjectModel project)
    {
      foreach (TaskBaseViewModel taskBaseViewModel in TaskCache.LocalTaskViewModels.Values.ToList<TaskBaseViewModel>())
      {
        if (taskBaseViewModel.ProjectId == project.id)
          taskBaseViewModel.SetProject(project.id);
      }
    }

    public static void OnProjectDeleted(ProjectModel project)
    {
      foreach (TaskBaseViewModel taskBaseViewModel in TaskCache.LocalTaskViewModels.Values.ToList<TaskBaseViewModel>())
      {
        if (taskBaseViewModel.ProjectId == project.id)
        {
          taskBaseViewModel.Color = string.Empty;
          taskBaseViewModel.ProjectName = string.Empty;
          taskBaseViewModel.TeamId = string.Empty;
        }
      }
    }

    public static void ClearModels()
    {
      TaskCache.Clear();
      TaskDetailItemCache.Clear();
    }
  }
}
