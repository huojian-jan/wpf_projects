// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Cache.TaskCache
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Service;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Sync.ClosedLoader;
using ticktick_WPF.ViewModels;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Cache
{
  public static class TaskCache
  {
    public static readonly ConcurrentDictionary<string, TaskBaseViewModel> LocalTaskViewModels = new ConcurrentDictionary<string, TaskBaseViewModel>();
    private static int _num;

    public static async Task<HashSet<string>> InitLocalTasks()
    {
      HashSet<string> ignoreTasks = (HashSet<string>) null;
      List<TaskModel> taskModels = await TaskDao.GetAllTask();
      if (ABTestManager.IsCleanTask() && LocalSettings.Settings.ExtraSettings.LastClearTaskTime < DateUtils.GetDateNum(DateTime.Today.AddDays(-7.0)))
      {
        LocalSettings.Settings.ExtraSettings.LastClearTaskTime = DateUtils.GetDateNum(DateTime.Today);
        List<string> syncStatusTaskIds = new HashSet<string>((await SyncStatusDao.GetAllSyncStatus()).Select<SyncStatusModel, string>((Func<SyncStatusModel, string>) (s => s.EntityId))).ToList<string>();
        DateTime time = DateTime.Today.AddDays(-180.0);
        List<TaskModel> list = taskModels.Where<TaskModel>((Func<TaskModel, bool>) (t =>
        {
          if (syncStatusTaskIds.Contains(t.id))
            return false;
          DateTime? nullable;
          if (t.status != 0)
          {
            nullable = t.completedTime;
            DateTime dateTime = time;
            if ((nullable.HasValue ? (nullable.GetValueOrDefault() < dateTime ? 1 : 0) : 0) != 0)
              return true;
          }
          if (t.deleted == 0)
            return false;
          nullable = t.modifiedTime;
          DateTime dateTime1 = time;
          return nullable.HasValue && nullable.GetValueOrDefault() < dateTime1;
        })).ToList<TaskModel>();
        ignoreTasks = new HashSet<string>(list.Select<TaskModel, string>((Func<TaskModel, string>) (t => t.id)));
        TaskService.DeleteTasksFromDb(list);
        UtilLog.Info(LocalSettings.Settings.ExtraSettings.LastClearTaskTime.ToString() + " ClearHistoryTaskCount " + list.Count.ToString());
        CompletionLoadStatusDao.ResetPullTime(time);
        taskModels.RemoveAll((Predicate<TaskModel>) (t => ignoreTasks.Contains(t.id)));
      }
      taskModels.ForEach((Action<TaskModel>) (task => TaskCache.Insert(task, false)));
      await AttachmentCache.ResetDictItems();
      HashSet<string> stringSet = ignoreTasks;
      taskModels = (List<TaskModel>) null;
      return stringSet;
    }

    public static void AddToDict(TaskBaseViewModel task, bool checkNum = true)
    {
      if (task.IsTaskOrNote && !string.IsNullOrEmpty(task.Id) && !TaskCache.LocalTaskViewModels.ContainsKey(task.Id))
      {
        if (!task.CreatedTime.HasValue)
          task.CreatedTime = new DateTime?(DateTime.Now);
        TaskCache.LocalTaskViewModels[task.Id] = task;
      }
      if (!checkNum)
        return;
      ProjectAndTaskIdsCache.OnTaskChanged(task, CheckMatchedType.All);
    }

    public static Dictionary<string, Node<TaskBaseViewModel>> GetAllTaskTree()
    {
      Dictionary<string, Node<TaskBaseViewModel>> dictionary = TaskCache.LocalTaskViewModels.Values.Where<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (t => t.Deleted == 0)).ToList<TaskBaseViewModel>().ToDictionary<TaskBaseViewModel, string, Node<TaskBaseViewModel>>((Func<TaskBaseViewModel, string>) (task => task.Id), (Func<TaskBaseViewModel, Node<TaskBaseViewModel>>) (task => (Node<TaskBaseViewModel>) new TaskNode(task)));
      TaskNodeUtils.BuildNodeTree<TaskBaseViewModel>(dictionary, true);
      return dictionary;
    }

    public static Dictionary<string, Node<TaskBaseViewModel>> GetTaskTreeInProject(
      string projectId,
      bool withCompleted = true)
    {
      return TaskCache.GetTaskNodeDict(TaskCache.LocalTaskViewModels.Values.ToList<TaskBaseViewModel>().Where<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (t =>
      {
        if (t == null || !(t.ProjectId == projectId) || t.Deleted != 0)
          return false;
        return withCompleted || t.Status == 0;
      })).ToList<TaskBaseViewModel>());
    }

    public static Dictionary<string, Node<TaskBaseViewModel>> GetTaskNodeDict(
      List<TaskBaseViewModel> tasks)
    {
      IEnumerable<TaskBaseViewModel> taskBaseViewModels = tasks.Where<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (t => t.Kind == "NOTE" || t.IsCheckItem));
      Dictionary<string, Node<TaskBaseViewModel>> dictionaryEx = tasks.Where<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (t => t.Kind != "NOTE" && !t.IsCheckItem)).ToDictionaryEx<TaskBaseViewModel, string, Node<TaskBaseViewModel>>((Func<TaskBaseViewModel, string>) (task => task.Id), (Func<TaskBaseViewModel, Node<TaskBaseViewModel>>) (task => (Node<TaskBaseViewModel>) new TaskNode(task)));
      TaskNodeUtils.BuildNodeTree<TaskBaseViewModel>(dictionaryEx, true);
      foreach (TaskBaseViewModel task in taskBaseViewModels)
      {
        if (!string.IsNullOrEmpty(task.Id) && !dictionaryEx.ContainsKey(task.Id))
          dictionaryEx[task.Id] = (Node<TaskBaseViewModel>) new TaskNode(task);
      }
      return dictionaryEx;
    }

    public static List<TaskBaseViewModel> GetRepeatTasks(bool inAll)
    {
      return TaskCache.LocalTaskViewModels.Values.Where<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (task => task != null && task.Deleted == 0 && task.Status == 0 && CacheManager.CheckProjectValid(task.ProjectId, inAll) && task.Kind != "NOTE" && !string.IsNullOrEmpty(task.RepeatFlag))).ToList<TaskBaseViewModel>().ToList<TaskBaseViewModel>();
    }

    internal static List<TaskBaseViewModel> GetTaskInQuickLink()
    {
      return TaskCache.GetAllTask().Where<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (t =>
      {
        ProjectModel projectById = CacheManager.GetProjectById(t.ProjectId);
        return projectById != null && projectById.IsValid() && t.Deleted == 0 && t.Status == 0 && !string.IsNullOrWhiteSpace(t.Title);
      })).ToList<TaskBaseViewModel>();
    }

    public static List<TaskBaseViewModel> GetModelsByIds(List<string> ids)
    {
      if (ids == null || ids.Count == 0)
        return new List<TaskBaseViewModel>();
      List<TaskBaseViewModel> modelsByIds = new List<TaskBaseViewModel>();
      foreach (TaskBaseViewModel taskBaseViewModel in (IEnumerable<TaskBaseViewModel>) TaskCache.LocalTaskViewModels.Values)
      {
        if (taskBaseViewModel != null && taskBaseViewModel.Deleted == 0 && ids.Contains(taskBaseViewModel.Id) && CacheManager.CheckProjectValid(taskBaseViewModel.ProjectId))
          modelsByIds.Add(taskBaseViewModel);
      }
      return modelsByIds;
    }

    private static IEnumerable<TaskBaseViewModel> GetTaskInProject(string projectId)
    {
      return (IEnumerable<TaskBaseViewModel>) TaskCache.LocalTaskViewModels.Values.ToList<TaskBaseViewModel>().Where<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (t => t.ProjectId == projectId && t.Deleted == 0)).ToList<TaskBaseViewModel>();
    }

    public static async Task<List<TaskBaseViewModel>> GetTasksInProjectOrColumn(
      string projectId,
      string columnId = null,
      bool isDefaultColumn = false)
    {
      Dictionary<string, Node<TaskBaseViewModel>> nodes = TaskCache.GetTaskTreeInProject(projectId);
      if (CacheManager.GetProjectById(projectId) == null)
        return new List<TaskBaseViewModel>();
      List<string> columnIds = (List<string>) null;
      if (isDefaultColumn)
      {
        List<ColumnModel> columnsByProjectId = await ColumnDao.GetColumnsByProjectId(projectId);
        columnIds = columnsByProjectId != null ? columnsByProjectId.Select<ColumnModel, string>((Func<ColumnModel, string>) (c => c.id)).ToList<string>() : (List<string>) null;
      }
      return nodes.Values.Where<Node<TaskBaseViewModel>>((Func<Node<TaskBaseViewModel>, bool>) (node =>
      {
        if (string.IsNullOrEmpty(columnId))
          return true;
        string str1 = node.Value.ParentId;
        TaskBaseViewModel taskBaseViewModel = (TaskBaseViewModel) null;
        HashSet<string> stringSet = new HashSet<string>()
        {
          node.NodeId
        };
        while (!string.IsNullOrEmpty(str1))
        {
          if (stringSet.Contains(str1))
          {
            TaskDao.UpdateParent(str1, "", false);
            break;
          }
          if (nodes.ContainsKey(str1))
          {
            stringSet.Add(str1);
            taskBaseViewModel = nodes[str1].Value;
            str1 = taskBaseViewModel.ParentId;
          }
          else
          {
            str1 = "";
            taskBaseViewModel = (TaskBaseViewModel) null;
          }
        }
        string str2 = taskBaseViewModel?.ColumnId ?? node.Value.ColumnId;
        if (str2 == columnId)
          return true;
        if (!isDefaultColumn)
          return false;
        if (string.IsNullOrEmpty(str2))
          return true;
        List<string> stringList = columnIds;
        // ISSUE: explicit non-virtual call
        return (stringList != null ? (__nonvirtual (stringList.Contains(str2)) ? 1 : 0) : 0) == 0;
      })).Select<Node<TaskBaseViewModel>, TaskBaseViewModel>((Func<Node<TaskBaseViewModel>, TaskBaseViewModel>) (t => t.Value)).ToList<TaskBaseViewModel>();
    }

    public static List<TaskBaseViewModel> GetModelsInProject(string projectId)
    {
      return CacheManager.GetProjectById(projectId) == null ? new List<TaskBaseViewModel>() : TaskCache.GetTaskInProject(projectId).Where<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (t => t != null)).ToList<TaskBaseViewModel>();
    }

    public static async Task<List<TaskBaseViewModel>> GetModelsInProjectColumn(
      string projectId,
      string columnId = null,
      bool isDefaultColumn = false)
    {
      return CacheManager.GetProjectById(projectId) == null ? new List<TaskBaseViewModel>() : await TaskCache.GetTasksInProjectOrColumn(projectId, columnId, isDefaultColumn);
    }

    public static List<string> GetAllTaskIds()
    {
      return TaskCache.LocalTaskViewModels.Keys.ToList<string>();
    }

    public static HashSet<string> GetAllTaskIdMap()
    {
      return new HashSet<string>((IEnumerable<string>) TaskCache.GetAllTaskIds());
    }

    public static TaskBaseViewModel GetTaskById(string taskId)
    {
      return !string.IsNullOrEmpty(taskId) && TaskCache.LocalTaskViewModels.ContainsKey(taskId) ? TaskCache.LocalTaskViewModels[taskId] : (TaskBaseViewModel) null;
    }

    public static TaskBaseViewModel SafeGetTaskViewModel(TaskModel task)
    {
      if (task == null)
      {
        UtilLog.Info("task is null");
        return new TaskBaseViewModel();
      }
      if (!string.IsNullOrEmpty(task.id))
        return TaskCache.Insert(task, false);
      UtilLog.Info("taskId is null");
      return new TaskBaseViewModel(task);
    }

    public static TaskBaseViewModel Insert(TaskModel task, bool notify = true)
    {
      if (!string.IsNullOrEmpty(task?.id) && !string.IsNullOrEmpty(task.projectId))
      {
        TaskBaseViewModel taskBaseViewModel;
        if (TaskCache.LocalTaskViewModels.TryGetValue(task.id, out taskBaseViewModel))
          return taskBaseViewModel;
        TaskBaseViewModel task1 = new TaskBaseViewModel(task);
        if (TaskCache.LocalTaskViewModels.TryAdd(task.id, task1))
        {
          if (notify && task1.Status == 0)
            ProjectAndTaskIdsCache.OnTaskChanged(task1, CheckMatchedType.All);
          return task1;
        }
      }
      return new TaskBaseViewModel(task);
    }

    public static List<TaskBaseViewModel> InsertAll(List<TaskModel> tasks)
    {
      return tasks == null ? (List<TaskBaseViewModel>) null : tasks.Select<TaskModel, TaskBaseViewModel>((Func<TaskModel, TaskBaseViewModel>) (t => TaskCache.Insert(t))).Where<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (t => t != null)).ToList<TaskBaseViewModel>();
    }

    public static void DeleteTask(string taskId)
    {
      TaskBaseViewModel task;
      if (string.IsNullOrEmpty(taskId) || !TaskCache.LocalTaskViewModels.ContainsKey(taskId) || !TaskCache.LocalTaskViewModels.TryRemove(taskId, out task))
        return;
      ProjectAndTaskIdsCache.OnTaskRemoved(task);
    }

    public static List<long> GetTaskSortOrdersInProject(string projectId, string parentId)
    {
      return TaskCache.LocalTaskViewModels.Values.Where<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (task => task.ProjectId == projectId && Utils.IsEqualString(parentId, task.ParentId) && task.ProjectId == projectId)).Select<TaskBaseViewModel, long>((Func<TaskBaseViewModel, long>) (task => task.SortOrder)).ToList<long>();
    }

    public static List<string> GetDeletedTaskIds()
    {
      return TaskCache.LocalTaskViewModels.Values.Where<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (task => task.Deleted == 1)).Select<TaskBaseViewModel, string>((Func<TaskBaseViewModel, string>) (task => task.Id)).ToList<string>();
    }

    public static List<TaskBaseViewModel> GetAllTask()
    {
      return TaskCache.LocalTaskViewModels.Values.ToList<TaskBaseViewModel>();
    }

    public static List<TaskDao.TitleModel> GetTaskTitleModels()
    {
      return TaskCache.LocalTaskViewModels.Values.Where<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (task => task.Deleted != 2)).Select<TaskBaseViewModel, TaskDao.TitleModel>((Func<TaskBaseViewModel, TaskDao.TitleModel>) (task => new TaskDao.TitleModel()
      {
        TaskId = task.Id,
        Title = task.Title,
        Kind = task.Kind
      })).ToList<TaskDao.TitleModel>();
    }

    public static List<TaskBaseViewModel> GetPinCheckChildren(List<string> taskIds)
    {
      Dictionary<string, Node<TaskBaseViewModel>> taskTree = TaskCache.GetAllTaskTree();
      HashSet<string> taskIdSet = new HashSet<string>((IEnumerable<string>) taskIds);
      Dictionary<string, TaskBaseViewModel> extraTasks = new Dictionary<string, TaskBaseViewModel>();
      foreach (string key in taskIds.Where<string>((Func<string, bool>) (id => !string.IsNullOrEmpty(id) && taskTree.ContainsKey(id))))
      {
        Node<TaskBaseViewModel> node = taskTree[key];
        bool isNodePinned = node.Value.PinnedTime > 0L;
        List<Node<TaskBaseViewModel>> children = node.Children;
        // ISSUE: explicit non-virtual call
        if ((children != null ? (__nonvirtual (children.Count) > 0 ? 1 : 0) : 0) != 0 && !taskIdSet.Contains(node.Value.ParentId) && !string.IsNullOrEmpty(node.Value.Id) && !extraTasks.ContainsKey(node.Value.Id))
        {
          List<string> failedIds = new List<string>();
          node.GetAllChildrenValue().ForEach((Action<TaskBaseViewModel>) (child =>
          {
            if (taskIdSet.Contains(child.Id) || string.IsNullOrEmpty(child.Id) || child.Deleted != 0 || child.Status != 0 || extraTasks.ContainsKey(child.Id))
              return;
            if (isNodePinned == child.PinnedTime > 0L && !failedIds.Contains(child.ParentId))
              extraTasks[child.Id] = child;
            else
              failedIds.Add(child.Id);
          }));
        }
      }
      return extraTasks.Values.ToList<TaskBaseViewModel>();
    }

    public static Node<TaskBaseViewModel> GetTaskNode(string id, string projectId)
    {
      if (string.IsNullOrEmpty(id))
        return (Node<TaskBaseViewModel>) null;
      TaskBaseViewModel task1;
      if (TaskCache.LocalTaskViewModels.TryGetValue(id, out task1) && task1.Kind == "NOTE")
        return (Node<TaskBaseViewModel>) new TaskNode(task1);
      Dictionary<string, Node<TaskBaseViewModel>> dictionary = TaskCache.LocalTaskViewModels.Values.ToList<TaskBaseViewModel>().Where<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (t => t.ProjectId == projectId && t.Deleted == 0)).ToList<TaskBaseViewModel>().ToDictionary<TaskBaseViewModel, string, Node<TaskBaseViewModel>>((Func<TaskBaseViewModel, string>) (task => task.Id), (Func<TaskBaseViewModel, Node<TaskBaseViewModel>>) (task => (Node<TaskBaseViewModel>) new TaskNode(task)));
      TaskNodeUtils.BuildNodeTree<TaskBaseViewModel>(dictionary);
      return dictionary.ContainsKey(id) ? dictionary[id] : (Node<TaskBaseViewModel>) null;
    }

    public static List<TaskBaseViewModel> GetAllSubTasksById(string id, string projectId)
    {
      Node<TaskBaseViewModel> taskNode = TaskCache.GetTaskNode(id, projectId);
      return taskNode == null ? (List<TaskBaseViewModel>) null : taskNode.GetAllChildrenValue().Where<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (t => t != null)).ToList<TaskBaseViewModel>();
    }

    public static bool CanTaskCompletedByCheckItem(string taskId)
    {
      List<TaskBaseViewModel> allTask = TaskCache.GetAllTask();
      return (allTask != null ? (allTask.Any<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (t => t.Deleted == 0 && t.ParentId == taskId && t.Status == 0)) ? 1 : 0) : 0) == 0;
    }

    public static List<TaskBaseViewModel> GetAllParentTasksById(string id)
    {
      List<TaskBaseViewModel> tasks = new List<TaskBaseViewModel>();
      if (!string.IsNullOrEmpty(id) && TaskCache.LocalTaskViewModels.ContainsKey(id))
        GetParent(TaskCache.LocalTaskViewModels[id].ParentId);
      return tasks;

      void GetParent(string taskId)
      {
        TaskBaseViewModel localTaskViewModel;
        for (; !string.IsNullOrEmpty(taskId) && TaskCache.LocalTaskViewModels.ContainsKey(taskId); taskId = localTaskViewModel.ParentId)
        {
          localTaskViewModel = TaskCache.LocalTaskViewModels[taskId];
          tasks.Add(localTaskViewModel);
        }
      }
    }

    public static List<TaskBaseViewModel> GetTaskAndChildrenInBatch(
      List<string> taskIds,
      bool checkExist = true,
      string projectId = null)
    {
      if (taskIds == null || taskIds.Count == 0)
        return new List<TaskBaseViewModel>();
      Dictionary<string, Node<TaskBaseViewModel>> dictionary = string.IsNullOrEmpty(projectId) ? TaskCache.GetAllTaskTree() : TaskCache.GetTaskTreeInProject(projectId);
      if (checkExist)
      {
        Dictionary<string, TaskBaseViewModel> taskDict = new Dictionary<string, TaskBaseViewModel>();
        foreach (string taskId in taskIds)
        {
          if (!string.IsNullOrEmpty(taskId) && dictionary.ContainsKey(taskId))
          {
            Node<TaskBaseViewModel> node = dictionary[taskId];
            List<Node<TaskBaseViewModel>> children = node.Children;
            // ISSUE: explicit non-virtual call
            if ((children != null ? (__nonvirtual (children.Count) > 0 ? 1 : 0) : 0) != 0 && (string.IsNullOrEmpty(node.Value.ParentId) || !taskDict.ContainsKey(node.Value.ParentId)) && !string.IsNullOrEmpty(node.Value.Id) && !taskDict.ContainsKey(node.Value.Id))
            {
              taskDict[node.Value.Id] = node.Value;
              node.GetAllChildrenValue()?.ForEach((Action<TaskBaseViewModel>) (child =>
              {
                if (string.IsNullOrEmpty(child?.Id) || taskDict.ContainsKey(child.Id) || child.Deleted != 0)
                  return;
                taskDict[child.Id] = child;
              }));
            }
            else
              taskDict[node.Value.Id] = node.Value;
          }
        }
        return taskDict.Values.ToList<TaskBaseViewModel>();
      }
      List<TaskBaseViewModel> andChildrenInBatch = new List<TaskBaseViewModel>();
      foreach (string taskId in taskIds)
      {
        if (!string.IsNullOrEmpty(taskId) && dictionary.ContainsKey(taskId))
        {
          Node<TaskBaseViewModel> node = dictionary[taskId];
          andChildrenInBatch.Add(node.Value);
          andChildrenInBatch.AddRange((IEnumerable<TaskBaseViewModel>) node.GetAllChildrenValue());
        }
      }
      return andChildrenInBatch;
    }

    public static bool ExistTask(string id)
    {
      return !string.IsNullOrEmpty(id) && TaskCache.LocalTaskViewModels.ContainsKey(id);
    }

    public static void Clear() => TaskCache.LocalTaskViewModels.Clear();

    public static bool IsParentTask(string taskId)
    {
      return !string.IsNullOrEmpty(taskId) && TaskCache.LocalTaskViewModels.Values.Any<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (t => t.Deleted == 0 && t.ParentId == taskId));
    }

    public static bool ExistParentTask(List<string> taskIds)
    {
      return TaskCache.LocalTaskViewModels.Values.Any<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (t => !string.IsNullOrEmpty(t.ParentId) && taskIds.Contains(t.ParentId) && t.Deleted == 0));
    }

    public static bool IsChildTask(string parentId, string projectId)
    {
      if (string.IsNullOrEmpty(parentId) || !TaskCache.LocalTaskViewModels.ContainsKey(parentId))
        return false;
      TaskBaseViewModel localTaskViewModel = TaskCache.LocalTaskViewModels[parentId];
      return localTaskViewModel.Deleted == 0 && localTaskViewModel.ProjectId == projectId;
    }

    public static int GetTaskLevel(string taskId)
    {
      if (!TaskCache.LocalTaskViewModels.ContainsKey(taskId))
        return 0;
      TaskBaseViewModel taskBaseViewModel = TaskCache.LocalTaskViewModels[taskId];
      int taskLevel = 0;
      HashSet<string> stringSet = new HashSet<string>()
      {
        taskId
      };
      while (!string.IsNullOrEmpty(taskBaseViewModel.ParentId) && TaskCache.LocalTaskViewModels.ContainsKey(taskBaseViewModel.ParentId))
      {
        if (stringSet.Contains(taskBaseViewModel.ParentId))
        {
          TaskDao.UpdateParent(taskBaseViewModel.Id, "", false);
          UtilLog.Info("SetTaskParent empty " + taskBaseViewModel.Id + " Getlevel loop");
          break;
        }
        TaskBaseViewModel localTaskViewModel = TaskCache.LocalTaskViewModels[taskBaseViewModel.ParentId];
        if (localTaskViewModel.ProjectId == taskBaseViewModel.ProjectId && localTaskViewModel.Deleted == 0)
        {
          stringSet.Add(localTaskViewModel.Id);
          taskBaseViewModel = localTaskViewModel;
          ++taskLevel;
        }
        else
          break;
      }
      return taskLevel;
    }

    public static TaskBaseViewModel GetTopParentByTaskId(string taskId)
    {
      if (!TaskCache.LocalTaskViewModels.ContainsKey(taskId))
        return (TaskBaseViewModel) null;
      TaskBaseViewModel topParentByTaskId = TaskCache.LocalTaskViewModels[taskId];
      HashSet<string> stringSet = new HashSet<string>()
      {
        taskId
      };
      while (!string.IsNullOrEmpty(topParentByTaskId.ParentId) && TaskCache.LocalTaskViewModels.ContainsKey(topParentByTaskId.ParentId))
      {
        string parentId = topParentByTaskId.ParentId;
        if (stringSet.Contains(parentId))
        {
          TaskDao.UpdateParent(topParentByTaskId.Id, "", false);
          break;
        }
        TaskBaseViewModel localTaskViewModel = TaskCache.LocalTaskViewModels[parentId];
        if (localTaskViewModel.ProjectId == topParentByTaskId.ProjectId && localTaskViewModel.Deleted == 0)
        {
          topParentByTaskId = localTaskViewModel;
          stringSet.Add(parentId);
        }
        else
          break;
      }
      return topParentByTaskId;
    }

    public static List<TaskBaseViewModel> GetTasksByIds(List<string> taskIds)
    {
      List<TaskBaseViewModel> tasksByIds = new List<TaskBaseViewModel>();
      if (taskIds == null)
        return tasksByIds;
      foreach (string taskId in taskIds)
      {
        TaskBaseViewModel taskById = TaskCache.GetTaskById(taskId);
        if (taskById != null)
          tasksByIds.Add(taskById);
      }
      return tasksByIds;
    }

    public static List<TaskBaseViewModel> GetChildren(string id, string projectId)
    {
      return TaskCache.LocalTaskViewModels.Values.Where<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (t => t != null && t.Deleted == 0 && t.ParentId == id && t.ProjectId == projectId)).ToList<TaskBaseViewModel>();
    }

    public static bool ExistParent(string id, string parentId)
    {
      bool flag = TaskCache.ExistTask(id);
      string key = id;
      HashSet<string> stringSet = new HashSet<string>();
      TaskBaseViewModel localTaskViewModel;
      for (; flag; flag = !stringSet.Contains(localTaskViewModel.ParentId) && TaskCache.ExistTask(localTaskViewModel.ParentId))
      {
        localTaskViewModel = TaskCache.LocalTaskViewModels[key];
        stringSet.Add(key);
        key = localTaskViewModel.ParentId;
        if (key == parentId)
          return true;
      }
      return false;
    }

    public static List<TaskBaseViewModel> GetAllOutDateTasks()
    {
      HashSet<string> hideProjectIds = new HashSet<string>(CacheManager.GetProjects().Where<ProjectModel>((Func<ProjectModel, bool>) (p => !p.inAll)).Select<ProjectModel, string>((Func<ProjectModel, string>) (p => p.id)));
      return TaskCache.LocalTaskViewModels.Values.ToList<TaskBaseViewModel>().Where<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (t => t.Editable && t.Status == 0 && t.Deleted == 0 && !t.IsNote && (!hideProjectIds.Contains(t.ProjectId) || !(t.Assignee != LocalSettings.Settings.LoginUserId)) && t.OutDate())).ToList<TaskBaseViewModel>();
    }

    public static bool FindParent(string taskId, string parentId)
    {
      if (string.IsNullOrEmpty(taskId) || string.IsNullOrEmpty(parentId) || !TaskCache.LocalTaskViewModels.ContainsKey(taskId))
        return false;
      TaskBaseViewModel taskBaseViewModel = TaskCache.LocalTaskViewModels[taskId];
      HashSet<string> stringSet = new HashSet<string>()
      {
        taskId
      };
      while (!string.IsNullOrEmpty(taskBaseViewModel.ParentId) && TaskCache.LocalTaskViewModels.ContainsKey(taskBaseViewModel.ParentId))
      {
        string parentId1 = taskBaseViewModel.ParentId;
        if (parentId1 == parentId)
          return true;
        if (stringSet.Contains(parentId1))
        {
          TaskDao.UpdateParent(taskBaseViewModel.Id, "", false);
          UtilLog.Info("SetTaskParent empty " + taskBaseViewModel.Id + " FindParent loop");
          break;
        }
        TaskBaseViewModel localTaskViewModel = TaskCache.LocalTaskViewModels[parentId1];
        if (localTaskViewModel.ProjectId == taskBaseViewModel.ProjectId && localTaskViewModel.Deleted == 0)
        {
          taskBaseViewModel = localTaskViewModel;
          stringSet.Add(parentId1);
        }
        else
          break;
      }
      return false;
    }

    public static List<TaskBaseViewModel> GetEmptyColumnTaskInProject(string projectId)
    {
      return TaskCache.LocalTaskViewModels.Values.ToList<TaskBaseViewModel>().Where<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (t => t.ProjectId == projectId && t.Deleted == 0 && t.Status == 0 && string.IsNullOrEmpty(t.ColumnId))).ToList<TaskBaseViewModel>();
    }

    public static List<string> GetAllTaskAndSubTaskIdsByIds(List<string> ids)
    {
      HashSet<string> source = new HashSet<string>((IEnumerable<string>) ids);
      using (List<TaskBaseViewModel>.Enumerator enumerator = TaskCache.LocalTaskViewModels.Values.ToList<TaskBaseViewModel>().GetEnumerator())
      {
label_11:
        while (enumerator.MoveNext())
        {
          TaskBaseViewModel current = enumerator.Current;
          if (!source.Contains(current.Id) && !string.IsNullOrEmpty(current.Id) && !string.IsNullOrEmpty(current.ParentId) && current.Deleted != 2)
          {
            TaskBaseViewModel taskBaseViewModel = current;
            HashSet<string> stringSet = new HashSet<string>()
            {
              current.Id
            };
            while (true)
            {
              if (!string.IsNullOrEmpty(taskBaseViewModel.ParentId) && TaskCache.LocalTaskViewModels.ContainsKey(taskBaseViewModel.ParentId))
              {
                string parentId = taskBaseViewModel.ParentId;
                if (!source.Contains(parentId))
                {
                  if (!stringSet.Contains(parentId))
                  {
                    TaskBaseViewModel localTaskViewModel = TaskCache.LocalTaskViewModels[parentId];
                    if (localTaskViewModel.Deleted != 2 && localTaskViewModel.ProjectId == taskBaseViewModel.ProjectId)
                    {
                      taskBaseViewModel = localTaskViewModel;
                      stringSet.Add(parentId);
                    }
                    else
                      goto label_11;
                  }
                  else
                    goto label_7;
                }
                else
                  break;
              }
              else
                goto label_11;
            }
            source.Add(current.Id);
            continue;
label_7:
            TaskDao.UpdateParent(taskBaseViewModel.Id, "", false);
            UtilLog.Info("SetTaskParent empty " + taskBaseViewModel.Id + " FindParent loop");
          }
        }
      }
      return source.ToList<string>();
    }
  }
}
