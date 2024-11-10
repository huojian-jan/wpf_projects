// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Service.TaskDetailItemService
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Undo;

#nullable disable
namespace ticktick_WPF.Service
{
  public class TaskDetailItemService
  {
    public static async Task SaveModel(
      string itemId,
      DateTime? startDate,
      DateTime? completeDate,
      int status)
    {
      TaskBaseViewModel viewModel = (TaskBaseViewModel) null;
      if (!string.IsNullOrEmpty(itemId) && TaskDetailItemCache.LocalCheckItemViewModels.ContainsKey(itemId))
      {
        TaskBaseViewModel checkItemViewModel = TaskDetailItemCache.LocalCheckItemViewModels[itemId];
        if (checkItemViewModel.IsCheckItem)
        {
          checkItemViewModel.StartDate = startDate;
          checkItemViewModel.CompletedTime = completeDate;
          checkItemViewModel.Status = status;
          viewModel = checkItemViewModel;
        }
      }
      TaskDetailItemModel checkItemById = await TaskDetailItemDao.GetCheckItemById(itemId);
      if (checkItemById == null)
      {
        viewModel = (TaskBaseViewModel) null;
      }
      else
      {
        checkItemById.startDate = startDate;
        checkItemById.completedTime = completeDate;
        checkItemById.status = status;
        TaskDetailItemDao.SaveChecklistItem(checkItemById);
        if (viewModel == null)
          TaskDetailItemCache.AddCheckItemToDict(checkItemById);
        ProjectAndTaskIdsCache.OnCheckItemsChanged(new List<TaskBaseViewModel>()
        {
          viewModel
        });
        viewModel = (TaskBaseViewModel) null;
      }
    }

    public static async Task<bool> UncheckDetailItemsByTaskId(string taskId)
    {
      List<TaskBaseViewModel> checkItemsByTaskId1 = TaskDetailItemCache.GetCheckItemsByTaskId(taskId);
      bool changed = false;
      // ISSUE: explicit non-virtual call
      if (checkItemsByTaskId1 != null && __nonvirtual (checkItemsByTaskId1.Count) > 0)
      {
        foreach (TaskBaseViewModel taskBaseViewModel in checkItemsByTaskId1)
        {
          if (taskBaseViewModel.Status != 0)
          {
            taskBaseViewModel.Status = 0;
            changed = true;
          }
        }
        ProjectAndTaskIdsCache.OnCheckItemsChanged(checkItemsByTaskId1);
      }
      List<TaskDetailItemModel> checkItemsByTaskId2 = await TaskDetailItemDao.GetCheckItemsByTaskId(taskId);
      // ISSUE: explicit non-virtual call
      if (checkItemsByTaskId2 != null && __nonvirtual (checkItemsByTaskId2.Count) > 0)
      {
        foreach (TaskDetailItemModel taskDetailItemModel in checkItemsByTaskId2)
        {
          if (taskDetailItemModel.status != 0)
            taskDetailItemModel.status = 0;
        }
      }
      await TaskDetailItemDao.BatchUpdateChecklists(checkItemsByTaskId2);
      return changed;
    }

    public static async Task CompleteCheckItem(
      string itemId,
      bool handleTask = true,
      bool completeOnly = false,
      bool needUndo = false,
      IToastShowWindow window = null)
    {
      TaskDetailItemModel item = await TaskDetailItemDao.GetChecklistItemById(itemId);
      if (item == null)
        item = (TaskDetailItemModel) null;
      else if (completeOnly && item.status == 1)
      {
        item = (TaskDetailItemModel) null;
      }
      else
      {
        if (needUndo)
          CloseUndoHandler.AddUndoCheckItem(item.TaskServerId, item);
        item.status = item.status != 1 ? 1 : 0;
        item.completedTime = item.status == 1 ? new DateTime?(DateTime.Now) : new DateTime?();
        await TaskDetailItemDao.SaveChecklistItem(item);
        CheckItemCompleteExtra itemCompleteExtra = await TaskService.SyncTaskChange(item, handleTask, needUndo, window);
        ItemChangeNotifier.NotifyItemStatusChanged(item.id);
        item = (TaskDetailItemModel) null;
      }
    }
  }
}
