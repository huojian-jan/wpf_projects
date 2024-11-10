// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Cache.TaskDetailItemCache
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
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Cache
{
  public class TaskDetailItemCache
  {
    public static readonly ConcurrentDictionary<string, TaskBaseViewModel> LocalCheckItemViewModels = new ConcurrentDictionary<string, TaskBaseViewModel>();

    public static async Task InitLocalCheckItems(HashSet<string> cleanedTaskIds)
    {
      (await TaskDetailItemDao.GetAllCheckItems()).ForEach((Action<TaskDetailItemModel>) (item =>
      {
        if (cleanedTaskIds != null && cleanedTaskIds.Contains(item.TaskServerId))
          return;
        TaskDetailItemCache.AddCheckItemToDict(item);
      }));
    }

    public static TaskBaseViewModel AddCheckItemToDict(TaskDetailItemModel item)
    {
      if (string.IsNullOrEmpty(item?.id))
        return (TaskBaseViewModel) null;
      if (!TaskDetailItemCache.LocalCheckItemViewModels.ContainsKey(item.id))
      {
        TaskBaseViewModel dict = new TaskBaseViewModel(item);
        TaskDetailItemCache.LocalCheckItemViewModels[item.id] = dict;
        return dict;
      }
      TaskBaseViewModel checkItemViewModel = TaskDetailItemCache.LocalCheckItemViewModels[item.id];
      if (checkItemViewModel.OwnerTask == null)
        checkItemViewModel.SetOwnerTask(item.TaskServerId, true);
      return checkItemViewModel;
    }

    public static List<TaskBaseViewModel> GetCheckItemsByTaskId(string taskId)
    {
      return TaskCache.GetTaskById(taskId)?.CheckItems?.ToList();
    }

    public static List<TaskBaseViewModel> GetAllCheckItems()
    {
      return TaskDetailItemCache.LocalCheckItemViewModels.Values.ToList<TaskBaseViewModel>();
    }

    private static TaskBaseViewModel UpdateItem(TaskDetailItemModel item)
    {
      TaskBaseViewModel taskBaseViewModel;
      if (!string.IsNullOrEmpty(item.id) && TaskDetailItemCache.LocalCheckItemViewModels.ContainsKey(item.id))
      {
        taskBaseViewModel = TaskDetailItemCache.LocalCheckItemViewModels[item.id];
        taskBaseViewModel.BuildCheckItem(item);
      }
      else
        taskBaseViewModel = TaskDetailItemCache.AddCheckItemToDict(item);
      return taskBaseViewModel;
    }

    public static void OnCheckItemUpdated(TaskDetailItemModel item, bool checkCount)
    {
      if (string.IsNullOrEmpty(item?.id))
        return;
      TaskBaseViewModel taskBaseViewModel = TaskDetailItemCache.UpdateItem(item);
      if (!(taskBaseViewModel != null & checkCount))
        return;
      ProjectAndTaskIdsCache.OnCheckItemsChanged(new List<TaskBaseViewModel>()
      {
        taskBaseViewModel
      });
    }

    public static void OnCheckItemsBatchUpdated(List<TaskDetailItemModel> items)
    {
      // ISSUE: explicit non-virtual call
      if (items == null || __nonvirtual (items.Count) <= 0)
        return;
      List<TaskBaseViewModel> models = new List<TaskBaseViewModel>();
      foreach (TaskDetailItemModel taskDetailItemModel in items.Where<TaskDetailItemModel>((Func<TaskDetailItemModel, bool>) (i => !string.IsNullOrEmpty(i?.id))))
      {
        TaskBaseViewModel taskBaseViewModel = TaskDetailItemCache.UpdateItem(taskDetailItemModel);
        if (taskBaseViewModel != null)
          models.Add(taskBaseViewModel);
      }
      ProjectAndTaskIdsCache.OnCheckItemsChanged(models);
    }

    public static async Task DeleteCheckItemById(string id)
    {
      TaskBaseViewModel taskBaseViewModel;
      if (string.IsNullOrEmpty(id) || !TaskDetailItemCache.LocalCheckItemViewModels.TryRemove(id, out taskBaseViewModel))
        return;
      taskBaseViewModel.OwnerTask?.RemoveCheckItem(taskBaseViewModel);
      ProjectAndTaskIdsCache.OnCheckItemsRemoved(new List<TaskBaseViewModel>()
      {
        taskBaseViewModel
      });
    }

    public static async Task BatchAddCheckItems(List<TaskDetailItemModel> models)
    {
      if (models == null || models.Count <= 0)
        return;
      List<TaskBaseViewModel> models1 = new List<TaskBaseViewModel>();
      foreach (TaskDetailItemModel model in models)
      {
        TaskBaseViewModel dict = TaskDetailItemCache.AddCheckItemToDict(model);
        if (dict != null)
          models1.Add(dict);
      }
      ProjectAndTaskIdsCache.OnCheckItemsChanged(models1);
    }

    public static TaskBaseViewModel GetCheckItemById(string id)
    {
      return string.IsNullOrEmpty(id) || !TaskDetailItemCache.LocalCheckItemViewModels.ContainsKey(id) ? (TaskBaseViewModel) null : TaskDetailItemCache.LocalCheckItemViewModels[id];
    }

    public static List<TaskBaseViewModel> GetCheckItemsInTaskIds(List<string> taskIds)
    {
      List<TaskBaseViewModel> checkItemsInTaskIds = new List<TaskBaseViewModel>();
      if (taskIds == null || !taskIds.Any<string>())
        return checkItemsInTaskIds;
      foreach (string taskId in taskIds)
      {
        List<TaskBaseViewModel> checkItemsByTaskId = TaskDetailItemCache.GetCheckItemsByTaskId(taskId);
        if (checkItemsByTaskId != null)
          checkItemsInTaskIds.AddRange((IEnumerable<TaskBaseViewModel>) checkItemsByTaskId);
      }
      return checkItemsInTaskIds;
    }

    public static bool ExistItem(string modelId)
    {
      return !string.IsNullOrEmpty(modelId) && TaskDetailItemCache.LocalCheckItemViewModels.ContainsKey(modelId);
    }

    public static TaskBaseViewModel GetPrimarySubtaskInTask(string taskId)
    {
      List<TaskBaseViewModel> checkItemsByTaskId = TaskDetailItemCache.GetCheckItemsByTaskId(taskId);
      return checkItemsByTaskId != null && checkItemsByTaskId.Any<TaskBaseViewModel>() ? checkItemsByTaskId.OrderBy<TaskBaseViewModel, int>((Func<TaskBaseViewModel, int>) (item => item == null ? 2 : item.Status)).ThenBy<TaskBaseViewModel, long>((Func<TaskBaseViewModel, long>) (item => item == null ? long.MaxValue : item.SortOrder)).FirstOrDefault<TaskBaseViewModel>() : (TaskBaseViewModel) null;
    }

    public static TaskBaseViewModel SafeGetViewModel(TaskDetailItemModel item)
    {
      if (item == null)
        throw new Exception("item is null");
      if (string.IsNullOrEmpty(item.id))
        throw new Exception("itemId is null");
      if (!TaskDetailItemCache.LocalCheckItemViewModels.ContainsKey(item.id))
        TaskDetailItemCache.AddCheckItemToDict(item);
      return TaskDetailItemCache.LocalCheckItemViewModels[item.id];
    }

    public static void OnCheckItemAdded(TaskDetailItemModel model)
    {
      TaskBaseViewModel dict = TaskDetailItemCache.AddCheckItemToDict(model);
      if (dict == null)
        return;
      ProjectAndTaskIdsCache.OnCheckItemsChanged(new List<TaskBaseViewModel>()
      {
        dict
      });
    }

    public static List<TaskBaseViewModel> GetCheckItemByIds(List<string> ids)
    {
      List<TaskBaseViewModel> checkItemByIds = new List<TaskBaseViewModel>();
      // ISSUE: explicit non-virtual call
      if (ids != null && __nonvirtual (ids.Count) > 0)
        checkItemByIds.AddRange(ids.Where<string>((Func<string, bool>) (id => !string.IsNullOrEmpty(id) && TaskDetailItemCache.LocalCheckItemViewModels.ContainsKey(id))).Select<string, TaskBaseViewModel>((Func<string, TaskBaseViewModel>) (id => TaskDetailItemCache.LocalCheckItemViewModels[id])));
      return checkItemByIds;
    }

    public static void Clear() => TaskDetailItemCache.LocalCheckItemViewModels.Clear();

    public static List<TaskBaseViewModel> GetAllOutDateItems()
    {
      return TaskDetailItemCache.LocalCheckItemViewModels.Values.ToList<TaskBaseViewModel>().Where<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (t =>
      {
        TaskBaseViewModel ownerTask = t.OwnerTask;
        return ownerTask != null && ownerTask.Editable && ownerTask.Status == 0 && ownerTask.Deleted == 0 && t.Status == 0 && t.Deleted == 0 && t.OutDate();
      })).ToList<TaskBaseViewModel>();
    }
  }
}
