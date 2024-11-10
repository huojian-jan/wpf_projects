// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Dal.TaskDetailItemDao
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ticktick_WPF.Cache;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Dal
{
  public static class TaskDetailItemDao
  {
    public static async Task<List<TaskDetailItemModel>> GetCheckItemsByTaskId(string taskId)
    {
      return await App.Connection.Table<TaskDetailItemModel>().Where((Expression<Func<TaskDetailItemModel, bool>>) (item => item.TaskServerId == taskId)).ToListAsync();
    }

    public static async Task<List<TaskDetailItemModel>> GetCheckItemsInTaskIds(
      ICollection<string> taskIds)
    {
      return await App.Connection.Table<TaskDetailItemModel>().Where((Expression<Func<TaskDetailItemModel, bool>>) (item => taskIds.Contains(item.TaskServerId))).ToListAsync();
    }

    public static async Task SaveChecklistItem(
      TaskDetailItemModel model,
      bool updateCache = true,
      bool checkCount = true)
    {
      if (model == null)
        return;
      if (updateCache || !TaskDetailItemCache.ExistItem(model.id))
        TaskDetailItemCache.OnCheckItemUpdated(model, checkCount);
      TaskDetailItemModel checkItemById = await TaskDetailItemDao.GetCheckItemById(model.id);
      if (checkItemById != null)
      {
        model._Id = checkItemById._Id;
        int num = await App.Connection.UpdateAsync((object) model);
      }
      else
      {
        int num1 = await App.Connection.InsertAsync((object) model);
      }
    }

    public static async Task TrySaveChecklistItem(TaskDetailItemModel model, bool checkCount = true)
    {
      await TaskDetailItemDao.SaveChecklistItem(model, checkCount: checkCount);
    }

    public static async Task<List<TaskDetailItemModel>> DeleteCheckItemsByTaskId(
      string taskId,
      List<string> ignore = null)
    {
      List<TaskDetailItemModel> items = await TaskDetailItemDao.GetCheckItemsByTaskId(taskId);
      if (items != null && items.Any<TaskDetailItemModel>())
      {
        List<TaskBaseViewModel> vms = new List<TaskBaseViewModel>();
        foreach (TaskDetailItemModel taskDetailItemModel in items)
        {
          TaskDetailItemModel item = taskDetailItemModel;
          if ((ignore == null || !ignore.Contains(item.id)) && !string.IsNullOrEmpty(item.id))
          {
            int num = await App.Connection.DeleteAsync((object) item);
            TaskBaseViewModel taskBaseViewModel;
            TaskDetailItemCache.LocalCheckItemViewModels.TryRemove(item.id, out taskBaseViewModel);
            if (taskBaseViewModel != null)
              vms.Add(taskBaseViewModel);
            item = (TaskDetailItemModel) null;
          }
        }
        if (vms.Count > 0)
          ProjectAndTaskIdsCache.OnCheckItemsRemoved(vms);
        vms = (List<TaskBaseViewModel>) null;
      }
      List<TaskDetailItemModel> taskDetailItemModelList = items;
      items = (List<TaskDetailItemModel>) null;
      return taskDetailItemModelList;
    }

    public static async Task InsertChecklistItem(TaskDetailItemModel model)
    {
      if (model == null)
        return;
      int num = await App.Connection.InsertAsync((object) model);
      TaskDetailItemCache.OnCheckItemAdded(model);
    }

    public static async Task DeleteById(string id)
    {
      TaskDetailItemModel checkItemById = await TaskDetailItemDao.GetCheckItemById(id);
      if (checkItemById == null)
        return;
      UtilLog.Info("DeleteTaskCheckItem taskId " + checkItemById.TaskServerId);
      int num = await App.Connection.DeleteAsync((object) checkItemById);
      TaskDetailItemCache.DeleteCheckItemById(id);
    }

    public static async Task BatchInsertChecklists(List<TaskDetailItemModel> models)
    {
      int num = await App.Connection.InsertAllAsync((IEnumerable) models);
      TaskDetailItemCache.BatchAddCheckItems(models);
    }

    public static async Task BatchUpdateChecklists(List<TaskDetailItemModel> models)
    {
      int num = await App.Connection.UpdateAllAsync((IEnumerable) models);
      TaskDetailItemCache.OnCheckItemsBatchUpdated(models);
    }

    public static async Task<List<TaskDetailItemModel>> BatchGetItems(List<string> itemIds)
    {
      if (itemIds == null || itemIds.Count <= 0)
        return new List<TaskDetailItemModel>();
      return await App.Connection.Table<TaskDetailItemModel>().Where((Expression<Func<TaskDetailItemModel, bool>>) (item => itemIds.Contains(item.id))).ToListAsync();
    }

    public static async Task<TaskDetailItemModel> GetChecklistItemById(string id)
    {
      return await TaskDetailItemDao.GetCheckItemById(id);
    }

    public static async Task<TaskDetailItemModel> GetCheckItemById(string id)
    {
      return await App.Connection.Table<TaskDetailItemModel>().Where((Expression<Func<TaskDetailItemModel, bool>>) (v => v.id == id)).FirstOrDefaultAsync();
    }

    public static async Task<Dictionary<string, long>> GetCheckItemSortOrderInTask(
      List<string> taskIds)
    {
      Dictionary<string, long> dict = new Dictionary<string, long>();
      List<TaskModel> thinTasksInBatch = await TaskDao.GetThinTasksInBatch(taskIds);
      if (thinTasksInBatch == null || thinTasksInBatch.Count == 0)
        return dict;
      foreach (TaskModel taskModel in thinTasksInBatch)
      {
        TaskModel task = taskModel;
        List<TaskDetailItemModel> checkItemsByTaskId = await TaskDetailItemDao.GetCheckItemsByTaskId(task.id);
        List<TaskDetailItemModel> list = checkItemsByTaskId != null ? checkItemsByTaskId.OrderBy<TaskDetailItemModel, long>((Func<TaskDetailItemModel, long>) (v => v.sortOrder)).ToList<TaskDetailItemModel>() : (List<TaskDetailItemModel>) null;
        if (list != null && list.Count != 0)
        {
          for (int index = 0; index < list.Count; ++index)
          {
            if (list[index] != null && !string.IsNullOrEmpty(list[index].id) && !dict.ContainsKey(list[index].id))
              dict.Add(list[index].id, task.sortOrder + (long) index + 1L);
          }
          task = (TaskModel) null;
        }
      }
      return dict;
    }

    public static async Task<TaskDetailItemModel> GetPrimarySubtaskInTask(string taskId)
    {
      List<TaskDetailItemModel> checkItemsByTaskId = await TaskDetailItemDao.GetCheckItemsByTaskId(taskId);
      return checkItemsByTaskId == null || !checkItemsByTaskId.Any<TaskDetailItemModel>() ? (TaskDetailItemModel) null : checkItemsByTaskId.OrderBy<TaskDetailItemModel, int>((Func<TaskDetailItemModel, int>) (item => item.status)).ThenBy<TaskDetailItemModel, long>((Func<TaskDetailItemModel, long>) (item => item.sortOrder)).FirstOrDefault<TaskDetailItemModel>();
    }

    public static async Task<Dictionary<string, TaskDetailItemModel>> GetPrimarySubtaskInTaskIds(
      List<string> taskIds)
    {
      Dictionary<string, TaskDetailItemModel> result = new Dictionary<string, TaskDetailItemModel>();
      if (taskIds != null && taskIds.Any<string>())
      {
        List<TaskDetailItemModel> checkItemsInTaskIds = await TaskDetailItemDao.GetCheckItemsInTaskIds((ICollection<string>) taskIds);
        if (checkItemsInTaskIds != null && checkItemsInTaskIds.Any<TaskDetailItemModel>())
        {
          foreach (string taskId1 in taskIds)
          {
            string taskId = taskId1;
            TaskDetailItemModel taskDetailItemModel = checkItemsInTaskIds.Where<TaskDetailItemModel>((Func<TaskDetailItemModel, bool>) (item => item.TaskServerId == taskId)).OrderBy<TaskDetailItemModel, int>((Func<TaskDetailItemModel, int>) (item => item.status)).ThenBy<TaskDetailItemModel, long>((Func<TaskDetailItemModel, long>) (item => item.sortOrder)).FirstOrDefault<TaskDetailItemModel>();
            if (taskDetailItemModel != null && !result.ContainsKey(taskId))
              result.Add(taskId, taskDetailItemModel);
          }
        }
      }
      Dictionary<string, TaskDetailItemModel> subtaskInTaskIds = result;
      result = (Dictionary<string, TaskDetailItemModel>) null;
      return subtaskInTaskIds;
    }

    public static async void ClearSameItemById(string itemId)
    {
      if (string.IsNullOrEmpty(itemId))
        return;
      List<TaskDetailItemModel> items = await App.Connection.Table<TaskDetailItemModel>().Where((Expression<Func<TaskDetailItemModel, bool>>) (v => v.id == itemId)).ToListAsync();
      if (items != null && items.Count > 1)
      {
        for (int i = 1; i < items.Count; ++i)
        {
          int num = await App.Connection.DeleteAsync((object) items[i]);
        }
      }
      items = (List<TaskDetailItemModel>) null;
    }

    public static async Task<TaskDetailItemModel> CopyItem(string originItemId)
    {
      TaskDetailItemModel item = await TaskDetailItemDao.GetChecklistItemById(originItemId);
      item.sortOrder += 24L;
      item.id = Utils.GetGuid();
      await TaskDetailItemDao.InsertChecklistItem(item);
      TaskDetailItemModel taskDetailItemModel = item;
      item = (TaskDetailItemModel) null;
      return taskDetailItemModel;
    }

    public static async Task<List<TaskDetailItemModel>> GetAllCheckItems()
    {
      return await App.Connection.QueryAsync<TaskDetailItemModel>("select * from TaskDetailItemModel where taskserverid in (select id from taskModel where userid='" + LocalSettings.Settings.LoginUserId + "')");
    }

    public static void SortItems(List<TaskBaseViewModel> items)
    {
      items.Sort((Comparison<TaskBaseViewModel>) ((a, b) =>
      {
        if (a.Status == 0 && b.Status != 0)
          return -1;
        if (a.Status != 0 && b.Status == 0)
          return 1;
        return a.Status != 0 && b.Status != 0 && a.CompletedTime.HasValue && b.CompletedTime.HasValue ? a.CompletedTime.Value.CompareTo(b.CompletedTime.Value) : a.SortOrder.CompareTo(b.SortOrder);
      }));
    }
  }
}
